/*****************************************************************************************
 * 
 * Générateur de bordereaux de transfert guidé par le profil d'archivage
 *                         -----------------------
 * Département du Morbihan
 * Direction des systèmes d'information
 * Service Études
 * Patrick Percot
 * Mars 2015
 * 
 * Réutilisation du code autorisée dans l'intérêt des collectivités territoriales
 * 
 * ****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Data;
using System.Collections.Specialized;
using System.Collections;


namespace SedaSummaryGenerator {
    public class RngProfileController {
        private String profileFile = String.Empty;
        private String dataFile = null;
        private XmlDocument docIn;
        private XmlNamespaceManager docInXmlnsManager;
        private XmlNode grammarNode;
        private String SEDA_version;    // 0.2 ou 1.0
        private String rootContains = "Contains";
        private String descendantContains = "Contains";
        private String stDocumentIdentification = "Identification";

        private ContainsNode rootContainsNode;
        private ContainsNode currentContainsNode;
        private String currentDocumentTypeId;
        private String missingString = "!!! DOCLIST manquant !!!";

        // TODO: Attention, ici on en gère pas encote le fait que des xxOrMore puissent être imbriqués
        //private bool inXxOrMore = false;

        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        protected StringCollection errorsList;
        protected bool errorsListCompleted = false;

        protected StringCollection treeList;
        protected bool treeListCompleted = false;

        protected StringCollection expectedTagsList;
        protected bool expectedTagsListCompleted = false;

        public RngProfileController() {
            errorsList = new StringCollection();
            unsetTracesWriter();
            treeList = new StringCollection();
            expectedTagsList = new StringCollection();
        }

        public void setDataFile(String dataFile) {
            this.dataFile = dataFile;
        }

        /*
         * La liste des documents à archiver est définie dans un fichier qui 
         * contient le nom des fichiers et leurs métadonnées
         * */
        public void controlProfileFile(String profileFile) {
             Action<Exception> eh = (ex) => {
                if (traceActions) tracesWriter.WriteLine(ex.GetType().Name + " while reading: " + profileFile);
                if (traceActions) tracesWriter.WriteLine(ex.Message);
                errorsList.Add("Erreur lors de la préparation du profil d'archivage '" + profileFile + "' " + ex.GetType().Name + " : " + ex.Message);
            };

            docIn = new XmlDocument();
            try {
                using (StreamReader sr = new StreamReader(profileFile)) {
                    String line = sr.ReadToEnd();
                    //Console.WriteLine(line);
                    docIn.LoadXml(line);
                    //Instantiate an XmlNamespaceManager object. 
                    docInXmlnsManager = new System.Xml.XmlNamespaceManager(docIn.NameTable);
                    //Add the namespaces used in books.xml to the XmlNamespaceManager.
                    docInXmlnsManager.AddNamespace("rng", "http://relaxng.org/ns/structure/1.0");
                }
            }
            catch (ArgumentException e) { eh(e); }
            catch (DirectoryNotFoundException e) { eh(e); }
            catch (FileNotFoundException e) { eh(e); }
            catch (OutOfMemoryException e) { eh(e); } 
            catch (IOException e) { eh(e); } 
            catch (XmlException e) { eh(e);  }

            // Annuler les opérations si le fichier n'existe pas
            if (errorsList.Count > 0)
                return;

            if (traceActions) tracesWriter.WriteLine("\n-----------------------------------------\n");
            if (traceActions) tracesWriter.WriteLine("Début de la vérification du profil");
            if (traceActions) tracesWriter.WriteLine("\n-----------------------------------------\n");
            //Console.WriteLine("Début de la vérification du profil");
            String xPath;
            try {
                grammarNode = docIn.SelectSingleNode("rng:grammar", docInXmlnsManager);
                if (grammarNode == null) {
                    errorsList.Add("Le nœud '" + "rng:grammar" + "' n'a pas été trouvé dans le profil '" + profileFile + "'. Le fichier n'est probablement pas de type RelaxNG");
                } else {
                    // SEDA 1.0 "fr:gouv:culture:archivesdefrance:seda:v1.0"
                    // SEDA 0.2 "fr:gouv:ae:archive:draft:standard_echange_v0.2"
                    String sTestSeda = grammarNode.Attributes.GetNamedItem("ns").Value;
                    if (sTestSeda == "fr:gouv:ae:archive:draft:standard_echange_v0.2") {
                        rootContains = "Contains";
                        descendantContains = "Contains";
                        stDocumentIdentification = "Identification";
                        SEDA_version = "0.2";
                    }
                    else
                        if (sTestSeda == "fr:gouv:culture:archivesdefrance:seda:v1.0") {
                            rootContains = "Archive";
                            descendantContains = "ArchiveObject";
                            stDocumentIdentification = "ArchivalAgencyDocumentIdentifier";
                            SEDA_version = "1.0";
                        }
                        else {
                            SEDA_version = "Version du SEDA inconnue";
                            errorsList.Add("Version du SEDA inconnue : '" + sTestSeda + "'");
                        }

                    XmlNode startNode = docIn.SelectSingleNode("rng:grammar/rng:start/rng:ref", docInXmlnsManager);
                    if (startNode == null) {
                        errorsList.Add("Le nœud '" + "rng:grammar/rng:start/rng:ref" + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                    } else {
                        String startNodeName = startNode.Attributes.GetNamedItem("name").Value;
                        if (startNodeName == null) {
                            errorsList.Add("Le nœud '" + "rng:grammar/rng:start/rng:ref" + "' n'a pas d'attribut name.");
                        } else {
                            xPath = "rng:define[@name='" + startNodeName + "']/rng:element[@name='" + startNodeName + "']/rng:ref";
                            XmlNode archiveNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (archiveNode == null) {
                                errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                            } else {
                                String archiveNodeName = archiveNode.Attributes.GetNamedItem("name").Value;
                                if (archiveNodeName == null) {
                                    errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                                } else {
                                    loadExpectedTagsInArchiveNode( archiveNodeName );
                                    xPath = "rng:define[@name='" + archiveNodeName + "']/descendant::rng:element[@name='" + rootContains + "']/rng:ref";
                                    XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                    if (containsNode == null) {
                                        errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                                    } else {
                                        String containsNodeName = containsNode.Attributes.GetNamedItem("name").Value;
                                        if (containsNodeName == null) {
                                            errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                                        } else {
                                            currentDocumentTypeId = "root";
                                            loadExpectedTagsInMainContainsNode( containsNodeName, "rootContains" );
											checkForDocumentInArchive(containsNodeName, "rootContains");
                                            checkForContentDescription(containsNodeName, "rootContains");
                                            checkForOptionalOriginatingAgency(containsNodeName, "rootContains");
                                            checkForTagInContains("Keyword", containsNodeName, "rootContains");
                                            // checkForTagInContains("FilePlanPosition", containsNodeName, "rootContains");
                                            checkForFilePlanPositionTag(containsNodeName, "rootContains");
                                            recurseContainsDefine(containsNodeName, String.Empty, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (XPathException e) {
                if (traceActions) tracesWriter.WriteLine("Exception XPath...");
                if (traceActions) tracesWriter.WriteLine(e.Message);
                Console.WriteLine("Exception XPath...");
                Console.WriteLine(e.Message);
                errorsList.Add("Exception XPath... " + e.Message);
            }

            if (traceActions) tracesWriter.WriteLine("\n-----------------------------------------\n");
            if (traceActions) tracesWriter.WriteLine("Fin de la vérification du profil");
            if (traceActions) tracesWriter.WriteLine("\n-----------------------------------------\n");
            //Console.WriteLine("Fin de la vérification du profil");

            if (rootContainsNode != null) {
                //if (traceActions) tracesWriter.WriteLine(rootContainsNode.dump(false));
                String arborescence = rootContainsNode.dump(false);
                foreach (String str in arborescence.Split('\n')) {
                    treeList.Add(str);
                }
                StringCollection errLst = rootContainsNode.checkDuplicates(rootContainsNode, missingString);
                foreach (String err in errLst) {
                    errorsList.Add(err);
                }
            }

            Action<Exception, String> ehd = (ex, str) => {
                if (traceActions) tracesWriter.WriteLine(ex.GetType().Name + " while writing: " + dataFile);
                if (traceActions) tracesWriter.WriteLine(ex.Message);
                errorsList.Add("Erreur lors de la création des données métier (" + str + ") '" + dataFile + "' " + ex.GetType().Name + " : " + ex.Message);
            };

            if (dataFile != null && ! dataFile.Equals(String.Empty)) {
                try {
                    StreamWriter dataStream = new StreamWriter(dataFile);
                    dataStream.WriteLine("# Liste de tags attendus par le profil");
                    dataStream.WriteLine("# Les tags de la forme TAG[#1] sont des tags répétables");
                    dataStream.WriteLine("# \t#1 est un indice qui peut prendre des valeurs entières");
                    dataStream.WriteLine("# \n");
                    dataStream.WriteLine("# Les documents peuvent prendre trois formes");
                    dataStream.WriteLine("# Les trois derniers éléments peuvent être produits par le générateur");
                    dataStream.WriteLine("# Dans les données qui suivent, seule la première forme est fournie\n");
                    dataStream.WriteLine("# Document, 1ère forme (chemin et nom du fichier) (TAG) (Description) (date)");
                    dataStream.WriteLine("# Document, 2ème forme (chemin et nom du fichier) (TAG) (Description) (date) (algorithme de l'empreinte) (empreinte)");
                    dataStream.WriteLine("# Document, 3ème forme (chemin et nom du fichier) (TAG) (Description) (date) (algorithme de l'empreinte) (empreinte) (taille)");
                    dataStream.WriteLine("# Exemple complet : ,fichier.txt,TAG,Description du document,13/03/2017 14:31:27,http://www.w3.org/2001/04/xmlenc#sha256,f81ba5573d70bb23c5510237208e2965bd87a389623c985cff341879e373c4b7,29516");
                    dataStream.WriteLine("# \n\n");
                    if (expectedTagsList != null && expectedTagsList.Count != 0) {
                        foreach (String str in expectedTagsList) {
                            if (str.StartsWith("#")) {
                                dataStream.WriteLine("," + str + ",Texte à personnaliser");
                            } else {
                                dataStream.WriteLine(",fichier.txt," + str.Replace("document: ", "") + ",Description du document,13/03/2017 14:31:27");
                            }
                        }
                    }
                    dataStream.Close();
                }
                catch (IOException e) { ehd(e, "Mauvaise syntaxe de nom de fichier"); }
                catch (UnauthorizedAccessException e) { ehd(e, "Droits d'accès à corriger"); }
                catch (System.Security.SecurityException e) { ehd(e, "Droits d'accès à corriger"); }

            }
        }


        private void loadExpectedTagsInArchiveNode(String activeNodeName) {
            XmlNode node;
            String xPath, nodeName, nodeContent;

            // Recherche du commentaire
            nodeName = "Comment";
            xPath = "rng:define[@name='" + activeNodeName + "']/descendant::rng:element[@name='" + nodeName + "']/rng:ref";
            node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (node != null) {
                nodeContent = node.Attributes.GetNamedItem("name").Value;
                if (nodeContent != null) {
                    xPath = "rng:define[@name='" + nodeContent + "']/rng:data[@type='string']";
                    node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (node != null) {
                        expectedTagsList.Add("#" + nodeName);
                    }
                }
            }

        }

        private void loadExpectedTagsInMainContainsNode(String activeNodeName, String context) {
            XmlNode node, parentNode;
            String xPath, nodeName, parentNodeName, nodeContent;

            // Recherche du nom du transfert
            xPath = "rng:define[@name='" + activeNodeName + "']/descendant::rng:element[@name='" + "Name" + "']/rng:ref";
            node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (node != null) {
                nodeContent = node.Attributes.GetNamedItem("name").Value;
                if (nodeContent != null) {
                    xPath = "rng:define[@name='" + nodeContent + "']/rng:data[@type='string']";
                    node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (node != null) {
                        expectedTagsList.Add("#TransferName");
                    }
                }
            }

            // Recherche de CustodialHistory et des OriginatingAgency des unités documentaires
            xPath = "rng:define[@name='" + activeNodeName + "']/descendant::rng:element[@name='" + "ContentDescription" + "']/rng:ref";
            parentNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (parentNode != null) {
                parentNodeName = parentNode.Attributes.GetNamedItem("name").Value;
                if (parentNodeName != null) {
                    String oldParentNodeName = parentNodeName;

                    nodeName = "CustodialHistory";
                    xPath = "rng:define[@name='" + parentNodeName + "']/descendant::rng:element[@name='" + nodeName + "']/rng:ref";
                    node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (node != null) {
                        nodeContent = node.Attributes.GetNamedItem("name").Value;
                        if (nodeContent != null) {
                            xPath = "rng:define[@name='" + nodeContent + "']/rng:data[@type='string']";
                            node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (node != null) {
                                expectedTagsList.Add("#" + nodeName);
                            }
                        }
                    }
                    nodeName = "OriginatingAgency";
                    xPath = "rng:define[@name='" + parentNodeName + "']/descendant::rng:element[@name='" + nodeName + "']/rng:ref";
                    parentNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (parentNode != null) {
                        parentNodeName = parentNode.Attributes.GetNamedItem("name").Value;
                        if (parentNodeName != null) {
                            String[] nodes = { "BusinessType", "Description", "LegalClassification", "Name", "Identification" };
                            foreach (String name in nodes) {
                                xPath = "rng:define[@name='" + parentNodeName + "']/descendant::rng:element[@name='" + name + "']/rng:ref";
                                node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                if (node != null) {
                                    nodeContent = node.Attributes.GetNamedItem("name").Value;
                                    if (nodeContent != null) {
                                        xPath = "rng:define[@name='" + nodeContent + "']/rng:data[@type='string']";
                                        node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                        if (node != null) {
                                            expectedTagsList.Add("#OriginatingAgency." + name);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Recherche de Description
                    xPath = "rng:define[@name='" + parentNodeName + "']/descendant::rng:element[@name='" + "Description" + "']/rng:ref";
                    parentNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (parentNode != null) {
                        parentNodeName = parentNode.Attributes.GetNamedItem("name").Value;
                        if (parentNodeName != null) {
                            xPath = "rng:define[@name='" + parentNodeName + "']/rng:data[@type='string']";
                            node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (node != null) {
                                expectedTagsList.Add("#ContentDescription.Description");
                            }
                        }
                    }

                    // Recherche de KeywordContent
                    addExpectedKeywordTags(oldParentNodeName, context, null);

                    // Recherche de FilePlanPosition
                    addExpectedFilePlanPositionTags(oldParentNodeName, context, null);
                }
            }
        }


            /*
             * Forme d'un ContentDexcriptive ou Keyword
             * 
                <rng:define name="ContentDescriptive_N66524">
                    <rng:element name="KeywordContent">
                        <rng:ref name="KeywordContent_N66529"/>
                    </rng:element>
                    <rng:element name="KeywordReference">
                        <rng:ref name="KeywordReference_N66541"/>
                    </rng:element>
                    <rng:element name="KeywordType">
                        <rng:ref name="KeywordType_N66578"/>
                    </rng:element>
                    <rng:optional>
                        <rng:attribute name="Id">
                            <rng:data type="string"/>
                        </rng:attribute>
                    </rng:optional>
                </rng:define>
             * 
             * Forme n° 1
             * On a une rng:value sur KeywordContent
             * 
                <rng:define name="KeywordContent_N66299">
                    <rng:value>collectivité locale</rng:value>
                </rng:define>
                <rng:define name="KeywordReference_N66311">
                     * 
                     * On peut avoir ou pas un schemeID, KeywordReference n'est pas obligatoire non plus
                     * 
                </rng:define>


             * Forme N° 2
             * On attend une valeur (on a une rng:data)
             * 
                <rng:define name="KeywordContent_N66529">
                    <rng:data type="string"/>
                </rng:define>
                <rng:define name="KeywordReference_N66541">
             * 
             * On doit avoir un schemeID
             * 
                    <rng:attribute name="schemeID">
                        <rng:value>CG56_DOCLIST_2015 / KWGEO</rng:value>
                    </rng:attribute>
                </rng:define>
             * */
            // Recherche de KeywordContent
        protected void addExpectedKeywordTags(String contentDescriptionDefineName, String context, String relativeContext) {
            String stKeyword = SEDA_version == "0.2" ? "ContentDescriptive" : "Keyword";
            String xPath = "rng:define[@name='" + contentDescriptionDefineName + "']/descendant::rng:element[@name='" + stKeyword + "']/rng:ref";
            XmlNodeList kwList = grammarNode.SelectNodes(xPath, docInXmlnsManager);
            XmlNode parentNode, node;
            String parentNodeName;
            int numkw = 0;
            if (kwList != null && kwList.Count >= 1) {
                foreach (XmlNode descriptiveNode in kwList) {
                    ++numkw;
                    // On récupère le ContentDescriptive_N?????
                    parentNodeName = descriptiveNode.Attributes.GetNamedItem("name").Value;
                    if (parentNodeName != null) {
                        Boolean schemeIdRequired = false;
                        String stScheme = null;
                        String keywordContentNodeName = getDefinedNodeName(parentNodeName, context, "KeywordContent", false);
                        if (keywordContentNodeName != String.Empty) {
                            xPath = "rng:define[@name='" + keywordContentNodeName + "']/rng:data[@type='string']";
                            XmlNode dataNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (dataNode != null) // schemeID n'est requis que si aucune valeur du mot-clé n'est forunie
                                schemeIdRequired = true;
                        }
                        if (schemeIdRequired == true) {
                            String keywordReferenceNodeName = getDefinedNodeName(parentNodeName, context, "KeywordReference", false);
                            if (keywordReferenceNodeName != String.Empty) {
                                xPath = "rng:define[@name='" + keywordReferenceNodeName + "']";
                                XmlNode keywordReferenceNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                stScheme = getSchemeIdValue(keywordReferenceNode, xPath, "", numkw);
                            }

                            // xPath = "rng:define[@name='" + parentNodeName + "']/descendant::rng:element[@name='" + "KeywordContent" + "']/rng:ref";
                            xPath = "rng:define[@name='" + contentDescriptionDefineName + "']/descendant::rng:element[@name='" + stKeyword + "']/rng:ref[@name='" + parentNodeName + "']";
                            XmlNode referenceNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (referenceNode != null) {
                                Boolean bRepeatable = false;
                                Boolean boptional = false;
                                xPath = "../..";
                                parentNode = referenceNode.SelectSingleNode(xPath);
                                String parentNodeStr = parentNode.Name;
                                if (parentNodeStr == "rng:zeroOrMore" || parentNodeStr == "rng:oneOrMore")
                                    bRepeatable = true;
                                if (parentNodeStr == "rng:zeroOrMore")
                                    boptional = true;

                                if (stScheme != null && stScheme != String.Empty) {
                                    String stEnd = bRepeatable ? "[#1]]" : "]";
                                    if (relativeContext == null)
                                        expectedTagsList.Add("#KeywordContent[{" + stScheme + "}" + stEnd);
                                    else
                                        expectedTagsList.Add("#KeywordContent[" + relativeContext + "{" + stScheme + "}" + stEnd);
                                }
                            } // if (referenceNode != null)
                        } // if (schemeIdRequired == true)
                    } // if (parentNodeName != null)
                } // foreach
            } // if (fppList != null && fppList.Count >= 1)

        }

        protected void addExpectedFilePlanPositionTags(String contentDescriptionDefineName, String context, String relativeContext) {
            String stFPP = "FilePlanPosition";
            String xPath = "rng:define[@name='" + contentDescriptionDefineName + "']/descendant::rng:element[@name='" + stFPP + "']/rng:ref";
            XmlNodeList fppList = grammarNode.SelectNodes(xPath, docInXmlnsManager);
            XmlNode parentNode, node;
            String stFilePlanPosition;
            int numfpp = 0;
            if (fppList == null)
            return;

            if (fppList.Count >= 1) {
                foreach (XmlNode filePlanPositionNode in fppList) {
                    ++numfpp;
                    // On récupère le ContentDescriptive_N?????
                    stFilePlanPosition = filePlanPositionNode.Attributes.GetNamedItem("name").Value;
                    if (stFilePlanPosition != null) {
                        String stScheme = null;
                        xPath = "rng:define[@name='" + stFilePlanPosition + "']/rng:data[@type='string']";
                        if (grammarNode.SelectSingleNode(xPath, docInXmlnsManager) != null) {
                            xPath = "rng:define[@name='" + stFilePlanPosition + "']/descendant::rng:attribute[@name='schemeID']/rng:value";
                            XmlNode schemeIdNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (schemeIdNode != null)
                                stScheme = getDocumentTypeId(schemeIdNode.InnerText, context);
                            Boolean bRepeatable = false;
                            /*  Pas d'utilisation à ce jour de optional */
                            /* Boolean boptional = false;  */
                            xPath = "../..";
                            parentNode = filePlanPositionNode.SelectSingleNode(xPath);
                            String parentNodeStr = parentNode.Name;
                            if (parentNodeStr == "rng:zeroOrMore" || parentNodeStr == "rng:oneOrMore")
                                bRepeatable = true;
                            /*  Pas d'utilisation à ce jour de optional */
                            /* if (parentNodeStr == "rng:zeroOrMore")
                                boptional = true; */
                            String stEnd = bRepeatable ? "[#1]]" : "]";

                            if (stScheme != null && stScheme != String.Empty) {
                                if (relativeContext == null)
                                    expectedTagsList.Add("#FilePlanPosition[{" + stScheme + "}" + stEnd);
                                else
                                    expectedTagsList.Add("#FilePlanPosition[" + relativeContext + "{" + stScheme + "}" + stEnd);
                            } else {
                                expectedTagsList.Add("#FilePlanPosition[" + relativeContext + stEnd);
                            } // if (stScheme != null && stScheme != String.Empty)
                        }
                    } // if (stFilePlanPosition != null)
                } // foreach
            } // if (fppList != null && fppList.Count >= 1)

        }


        private void loadExpectedTagsInContainsNode(String activeNodeName, String docTypeId, String context) {
            XmlNode node, parentNode;
            String xPath, parentNodeName, nodeContent;

            // Recherche du nom des unités documentaires
            xPath = "rng:define[@name='" + activeNodeName + "']/descendant::rng:element[@name='" + "Name" + "']/rng:ref";
            node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (node != null) {
                nodeContent = node.Attributes.GetNamedItem("name").Value;
                if (nodeContent != null) {
                    xPath = "rng:define[@name='" + nodeContent + "']/rng:data[@type='string']";
                    node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (node != null) {
                        expectedTagsList.Add("#ContainsName[" + currentContainsNode.getRelativeContext() + "]");
                    }
                }
            }

            // Recherche de la description des unités documentaires
            xPath = "rng:define[@name='" + activeNodeName + "']/descendant::rng:element[@name='" + "ContentDescription" + "']/rng:ref";
            parentNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (parentNode != null) {
                parentNodeName = parentNode.Attributes.GetNamedItem("name").Value;
                if (parentNodeName != null) {
                    String contentDescriptionDefineName = parentNodeName;

                    String nodeName = "CustodialHistory";
                    xPath = "rng:define[@name='" + parentNodeName + "']/descendant::rng:element[@name='" + nodeName + "']/rng:ref";
                    node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (node != null) {
                        nodeContent = node.Attributes.GetNamedItem("name").Value;
                        if (nodeContent != null) {
                            xPath = "rng:define[@name='" + nodeContent + "']/rng:data[@type='string']";
                            node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (node != null) {
                                expectedTagsList.Add("#" + nodeName + "[" + currentContainsNode.getRelativeContext() + "]");
                            }
                        }
                    }

                    // Recherche de Description
                    xPath = "rng:define[@name='" + parentNodeName + "']/descendant::rng:element[@name='" + "Description" + "']/rng:ref";
                    parentNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (parentNode != null) {
                        parentNodeName = parentNode.Attributes.GetNamedItem("name").Value;
                        if (parentNodeName != null) {
                            xPath = "rng:define[@name='" + parentNodeName + "']/rng:data[@type='string']";
                            node = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (node != null) {
                                expectedTagsList.Add("#ContentDescription.Description[" + currentContainsNode.getRelativeContext() + "]");
                            }
                        }
                    }

                    // Recherche de KeywordContent
                    addExpectedKeywordTags(contentDescriptionDefineName, context, currentContainsNode.getRelativeContext());
                    // Recherche de FilePlanPosition
                    addExpectedFilePlanPositionTags(contentDescriptionDefineName, context, currentContainsNode.getRelativeContext());
                }
            }

            // Recherche des documents des unités documentaires
            xPath = "rng:define[@name='" + activeNodeName + "']";
            XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (containsNode != null) {
                xPath = "descendant::rng:element[@name='Document']/rng:ref";
                XmlNodeList documentNodesList = containsNode.SelectNodes(xPath, docInXmlnsManager);
                // On peut ne pas avoir de Document
                if (documentNodesList != null) {
                    int compteur = 0;
                    foreach (XmlNode docNode in documentNodesList) {
                        compteur++;
                        parentNodeName = docNode.Attributes.GetNamedItem("name").Value;
                        if (parentNodeName != null) {
                            xPath = "rng:define[@name='" + parentNodeName + "']/descendant::rng:element[@name='" 
                                + stDocumentIdentification
                                + "']/rng:ref";
                            parentNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (parentNode != null) {
                                parentNodeName = parentNode.Attributes.GetNamedItem("name").Value;
                                if (parentNodeName != null) {
                                    xPath = "rng:define[@name='" + parentNodeName + "']/rng:attribute[@name='schemeID']/rng:value";
                                    parentNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                    if (parentNode != null) {
                                        expectedTagsList.Add("document: " + currentContainsNode.getRelativeContext() +
                                            "{" + getDocumentTypeId(parentNode.InnerText, String.Empty) + "}");
                                    }
                                }
                            } else {
                                expectedTagsList.Add("document: " + currentContainsNode.getRelativeContext());
                            }
                        }
                    }
                }
            }
        }

        /*
         * Méthode utilitaire pour récupérer le DOCLIST
         * 
         * TODO: ce code est dupliqué entre deux classes !!!! Il faut le factoriser
         * */
        private String getDocumentTypeId(String value, String context) {
            String docId = String.Empty;
            int pos = value.IndexOf(" / ");
            if (pos != -1) {
                docId = value.Substring(pos + 3);
            } else { // pos != -1
                if (context != String.Empty)
                    errorsList.Add("Identifiant DOCLIST malformé, on attend 'DOCLIST / identifier' et on a '"
                        + value + "' dans le contexte '" + context + "' dans l'attribut schemeID de ArchivalAgencyObjectIdentifier");
            }
            return docId;
        }

        // Récupère l'attribut schemeID
        // Si numfpp != 0, le message émis indique que l'on travaille sur un Keywrod
        protected String getSchemeIdValue(XmlNode aaoirefNode, String currentXpath, String context, int numKeyword) {
            String ret = null;
            String aaoiNodeName = aaoirefNode.Attributes.GetNamedItem("name").Value;
            if (aaoiNodeName == null) {
                errorsList.Add("Le nœud '" + currentXpath + "' n'a pas d'attribut name.");
            } else {
                currentXpath = "rng:define[@name='" + aaoiNodeName + "']/rng:attribute[@name='schemeID']/rng:value";
                XmlNode schemeIdNode = grammarNode.SelectSingleNode(currentXpath, docInXmlnsManager);
                if (schemeIdNode == null) {
                    currentXpath = "rng:define[@name='" + aaoiNodeName + "']/rng:optional/rng:attribute[@name='schemeID']/rng:value";
                    schemeIdNode = grammarNode.SelectSingleNode(currentXpath, docInXmlnsManager);
                    if (schemeIdNode != null) {
                        ret = getDocumentTypeId(schemeIdNode.InnerText, context);
                        if (numKeyword == 0)
                            errorsList.Add("L'attribut schemeID de la balise ArchivalAgencyObjectIdentifier de l'unité documentaire '" + ret + "' ne doit pas être optionnel. Il faut le rendre obligatoire.");
                        else
                            errorsList.Add("L'attribut schemeID de la balise KeywordReference du mot clé '" + numKeyword + "' ne doit pas être optionnel. Il faut le rendre obligatoire.");
                    } else {
                        if (numKeyword == 0)
                            errorsList.Add("L'attribut schemeID de la balise ArchivalAgencyObjectIdentifier de l'unité documentaire '" + currentDocumentTypeId + "' n'existe pas. Il faut le créer, le rendre obligatoire et lui donner un nom de tag unique.");
                        else
                            errorsList.Add("L'attribut schemeID de la balise KeywordReference du mot clé '" + numKeyword + "' n'existe pas. Il faut le créer, le rendre obligatoire et lui donner un nom de tag unique.");
                    }
                } else {
                    ret = getDocumentTypeId(schemeIdNode.InnerText, context);
                }
            }
            return ret;
        }

        // Récupère l'attribut schemeID des FilePlanPosition
        protected String getFPPSchemeIdValue(String stFilePlanPosition, String stLocalisation, String context, int numFPP) {
            String ret = null;
            String xPath = "rng:define[@name='" + stFilePlanPosition + "']/rng:attribute[@name='schemeID']/rng:value";
            XmlNode schemeIdNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (schemeIdNode == null) {
                xPath = "rng:define[@name='" + stFilePlanPosition + "']/rng:optional/rng:attribute[@name='schemeID']/rng:value";
                schemeIdNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                if (schemeIdNode != null) {
                    ret = getDocumentTypeId(schemeIdNode.InnerText, context);
                    errorsList.Add("L'attribut schemeID de la balise FilePlanPosition n° '" + numFPP + "' " + stLocalisation + " ne doit pas être optionnel. Il faut le rendre obligatoire.");
                } else {
                    errorsList.Add("L'attribut schemeID de la balise FilePlanPosition n° '" + numFPP + "' " + stLocalisation + " n'existe pas. Il faut le créer, le rendre obligatoire et lui donner un nom de tag unique.");
                }
            } else {
                ret = getDocumentTypeId(schemeIdNode.InnerText, context);
            }
            return ret;
        }

		/// <summary>
		/// On cherche à savoir s'il y a des filles/documents de l'unité documentaire unique au même seuil que root.
		/// Répond rien dans ce cas, sinon renvoie une erreur.
		/// </summary>
		/// <param name="activeNodeName">Lieu de l'unité documentaire</param>
		/// <param name="context"></param>
		protected void checkContainsHasContainsOrDocuments(String activeNodeName, String context) {
			// Recherche des filles d'unités documentaires
			if (traceActions) tracesWriter.WriteLine("checkContainsHasContainsOrDocuments ('" + activeNodeName + "', '" + context + "', '" + currentDocumentTypeId + "')");
			String xPath = "rng:define[@name='" + activeNodeName + "']";
			XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
			xPath = "descendant::rng:element[@name='Document']/rng:ref";
			XmlNodeList documentNodeList = containsNode.SelectNodes(xPath, docInXmlnsManager);
			if (documentNodeList != null && documentNodeList.Count > 0)	{
			} else {
				xPath = "descendant::rng:element[@name='" + descendantContains + "']/rng:ref";
				XmlNodeList containsNodesList = containsNode.SelectNodes(xPath, docInXmlnsManager);
				xPath = "descendant::rng:element[@name='Contains']/rng:ref";
				containsNodesList = containsNode.SelectNodes(xPath, docInXmlnsManager);
				if (containsNodesList != null && containsNodesList.Count > 0) {
				} else {
					errorsList.Add("La présence d'un document est obligatoire dans l'unité documentaire '" + currentDocumentTypeId + "' car elle n'a pas d'unité documentaire fille");
				}
			}
		}

		/// <summary>
		/// Recherche des UD qui sont sans balises et donc obligatoire, si pas trouvé renvoie une erreur
		/// </summary>
		/// <param name="activeNodeName"></param>
		/// <param name="context"></param>
		protected void checkContainsIsObligatory(String activeNodeName, String context) {
			if (traceActions) tracesWriter.WriteLine("checkContainsIsObligatory ('" + activeNodeName + "', '" + context + "', '" + currentDocumentTypeId + "')");
			String xPath = "rng:define[@name='" + activeNodeName + "']/rng:optional/rng:element[@name='Contains']";
			XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
			if (containsNode != null) {
				errorsList.Add("L'unité documentaire étant seule dans l'archive, il faut qu'elle soit rendue obligatoire");
			}
		}


		/// <summary>
		/// On recherche la balise ArchivalAgreement qui se situe dans Archive.
		/// Répond rien dans le cas où elle est présente, sinon renvoie une erreur.
		/// </summary>
		/// <param name="activeNodeName"></param>
		/// <param name="context"></param>
		protected void checkArchivalAgreement(String activeNodeName, String context)
		{
			if (traceActions) tracesWriter.WriteLine("checkArchivalAgreement ('" + activeNodeName + "', '" + context + "', '" + currentDocumentTypeId + "')");
			String xPath = "rng:define[@name='" + activeNodeName + "']/rng:element[@name='ArchivalAgreement']";
			XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
			if (containsNode == null) {
				xPath = "rng:define[@name='" + activeNodeName + "']/rng:optional/rng:element[@name='ArchivalAgreement']";
				containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
				if (containsNode != null) {
					errorsList.Add("(--) La balise ArchivalAgreement est optionnelle et ne sera pas générée. Il est conseillé de la rendre obligatoire");
				} else {
					errorsList.Add("(--) La balise ArchivalAgreement est absente, il est conseillé de la mettre et la rendre obligatoire");
				}
			}
		}

        protected void recurseContainsDefine(String defineNodeName, String context, bool isRepeatable) {
            if (traceActions) tracesWriter.WriteLine("recurseContainsDefine ('" + defineNodeName + "', '" + context + "', '" + currentDocumentTypeId + "')");
            String xPath;

            // Récupérer l'attribut schemeID de la balise ArchivalAgencyObjectIdentifier
            // et la mettre dans un arbre
            currentDocumentTypeId = missingString;
            if (context.Equals(String.Empty)) {
                currentDocumentTypeId = "root";
            } else {
                // Tester la présence de ArchivalAgencyObjectIdentifier dans l'unité documentaire
                xPath = "rng:define[@name='" + defineNodeName + "']/rng:element[@name='ArchivalAgencyObjectIdentifier']/rng:ref";
                XmlNode aaoirefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                if (aaoirefNode == null) {
                    xPath = "rng:define[@name='" + defineNodeName + "']/rng:optional/rng:element[@name='ArchivalAgencyObjectIdentifier']/rng:ref";
                    aaoirefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (aaoirefNode != null) {
                        String ret = getSchemeIdValue(aaoirefNode, xPath, context, 0);
                        if (ret != null) {
                            errorsList.Add("La balise ArchivalAgencyObjectIdentifier de l'unité documentaire '" + ret + "' est optionnelle. Il faut la rendre obligatoire.");
                        }
                    }
                    else
                        errorsList.Add("La balise ArchivalAgencyObjectIdentifier de l'unité documentaire '" + currentDocumentTypeId + "' n'existe pas. Il faut la créer et la rendre obligatoire.");
                } else {
                    String ret = getSchemeIdValue(aaoirefNode, xPath, context, 0);
                    if (ret != null)
                        currentDocumentTypeId = ret;
                }

				checkContainsHasContainsOrDocuments(defineNodeName, context);
                checkForMultipleDocument(defineNodeName, context);
                checkForContentDescription(defineNodeName, context);
                checkForOptionalOriginatingAgency(defineNodeName, context);
                checkForTagInContains("Keyword", defineNodeName, context);
                // checkForTagInContains("FilePlanPosition", defineNodeName, context);
                checkForFilePlanPositionTag(defineNodeName, context);
                checkForFilename(defineNodeName, context);
                checkForDocType(defineNodeName, context);
                // TODO: Désactivation du contrôle de la langue en attendant une meilleure gestion
                // checkForLanguage(defineNodeName, context);
                checkForDescriptionLevel(defineNodeName, context);

            }
			if (currentDocumentTypeId == "root") {
				checkContainsIsObligatory(defineNodeName, context);
				checkArchivalAgreement(defineNodeName, context);
                rootContainsNode = new ContainsNode(currentDocumentTypeId, null, true);
                currentContainsNode = rootContainsNode;
            } else {
                currentContainsNode = currentContainsNode.addNewNode(currentDocumentTypeId, true);
                if (  isRepeatable && ! currentDocumentTypeId.EndsWith("+"))
                    errorsList.Add("L'unité documentaire '" + currentDocumentTypeId + "' peut être répétée, mais elle ne possède pas de TAG répétable (TAG+). Il faut ajouter un '+' au tag ou changer les cardinalités");
                if (! isRepeatable &&   currentDocumentTypeId.EndsWith("+"))
                    errorsList.Add("L'unité documentaire '" + currentDocumentTypeId + "' est unique ou optionnelle, mais elle possède un TAG répétable (TAG+). Il faut supprimer le '+' du tag ou changer les cardinalités");
                loadExpectedTagsInContainsNode(defineNodeName, currentDocumentTypeId, context);
            }


            xPath = "rng:define[@name='" + defineNodeName + "']";
            XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (containsNode == null) {
                errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
            } else {
                // Parcourir récursivement toutes les balises Contains filles
                xPath = "descendant::rng:element[@name='" + descendantContains + "']/rng:ref";
                XmlNodeList containsNodesList = containsNode.SelectNodes(xPath, docInXmlnsManager);
				if (containsNodesList != null && containsNodesList.Count > 0)
				{
                    foreach (XmlNode node in containsNodesList) {
                        String nodeName = node.Attributes.GetNamedItem("name").Value;
                        if (nodeName == null) {
                            errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                        } else {
                            bool bRepeatableContains = false;
                            //<rng:element name="Contains">
                            //    <rng:ref name="Contains_N68245"/>
                            //</rng:element>
                            //<rng:zeroOrMore>
                            //    <rng:element name="Contains">
                            //        <rng:ref name="Contains_N68650"/>
                            //    </rng:element>
                            //</rng:zeroOrMore>
                            // on est sur le rng:ref, on remonte sur le parent rng:element
                            // et on regarde si on a un parent nommé rng:zeroOrMore ou rng:oneOrMore
                            xPath = "../..";
                            XmlNode parentNode = node.SelectSingleNode(xPath);
                            String parentNodeStr = parentNode.Name;
                            if (parentNodeStr == "rng:zeroOrMore" || parentNodeStr == "rng:oneOrMore")
                                bRepeatableContains = true;
                            recurseContainsDefine(nodeName, context + "/Contains", bRepeatableContains);
                        }
                    }
                } else {
					if (currentDocumentTypeId == "root")
						errorsList.Add("La présence d'une unité documentaire est obligatoire dans l'archive");
				}
                currentContainsNode = currentContainsNode.getParent();
            }
        }

        protected void checkForDescriptionLevel(String defineNodeName, String context) {
            String xPath = "rng:define[@name='" + defineNodeName + "']/rng:element[@name='DescriptionLevel']/rng:ref";
            XmlNode descNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (descNode != null) {
                String descNodeName = descNode.Attributes.GetNamedItem("name").Value;
                if (descNodeName == null) {
                    errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                } else {
                    xPath = "rng:define[@name='" + descNodeName + "']/rng:value";
                    XmlNode levelNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (levelNode == null) {
                        errorsList.Add("Le niveau de description de l'unité documentaire '" + currentDocumentTypeId + "' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.");
                    }
                }
            }
        }

        protected void checkForLanguage(String defineNodeName, String context) {
            String xPath = "rng:define[@name='" + defineNodeName + "']/rng:element[@name='ContentDescription']/rng:ref";
            XmlNode descNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (descNode != null) {
                String descNodeName = descNode.Attributes.GetNamedItem("name").Value;
                if (descNodeName == null) {
                    errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                } else {
                    xPath = "rng:define[@name='" + descNodeName + "']/rng:element[@name='Language']/rng:ref";
                    XmlNode langNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (langNode == null) {
                        xPath = "rng:define[@name='" + descNodeName + "']/rng:oneOrMore/rng:element[@name='Language']/rng:ref";
                        langNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                        if (langNode != null) {
                            errorsList.Add("La langue de la description de l'unité documentaire '" + currentDocumentTypeId + "' peut être répétée plusieurs fois. Le générateur ne permet pas de donner une valeur à ces éléments. Le bordereau ne sera pas conforme.");
                        }
                    } else {
                        String langNodeName = langNode.Attributes.GetNamedItem("name").Value;
                        if (langNodeName == null) {
                            errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                        } else {
                            xPath = "rng:define[@name='" + langNodeName + "']/rng:value";
                            XmlNode attrNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (attrNode == null) {
                                errorsList.Add("La langue de la description de l'unité documentaire '" + currentDocumentTypeId + "' n'a pas de valeur. C'est une donnée qui dans cette version doit être fournie par le profil.");
                            }
                        }
                    }
                }
            }
        }

        protected XmlNode getDocumentNode(String defineNodeName, int nDoc, ref int[] cards) {
            String xPath = "rng:define[@name='" + defineNodeName + "']/descendant::rng:element[@name='Document']";
            XmlNodeList nodes = grammarNode.SelectNodes(xPath, docInXmlnsManager);

            //XmlNode docNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            XmlNode docNode = null;
            /*
            String xPath = "rng:define[@name='" + defineNodeName + "']/rng:element[@name='Document'][" + cards[0] + "]/rng:ref";
            XmlNode docNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
             * */
            if (docNode == null) {
                xPath = "rng:define[@name='" + defineNodeName + "']/rng:optional[" + cards[1] + "]/rng:element[@name='Document']/rng:ref";
                docNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                if (docNode == null) {
                    xPath = "rng:define[@name='" + defineNodeName + "']/rng:zeroOrMore[" + cards[2] + "]/rng:element[@name='Document']/rng:ref";
                    docNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (docNode == null) {
                        xPath = "rng:define[@name='" + defineNodeName + "']/rng:oneOrMore[" + cards[3] + "]/rng:element[@name='Document']/rng:ref";
                        docNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                        if (docNode != null)
                            ++cards[3];
                    } else
                        ++cards[2];
                } else
                    ++cards[1];
            } else
                ++cards[0];
            return docNode;
        }

        protected void checkForDocType(String defineNodeName, String context) {
            // Tester la présence de la balise Type de Attachment
            int nDoc = 0;
            bool bContinuer = true;
            int[] cards = new int[] {1,1,1,1};
            String xPath = "rng:define[@name='" + defineNodeName + "']/descendant::rng:element[@name='Document']/rng:ref";
            XmlNodeList nodes = grammarNode.SelectNodes(xPath, docInXmlnsManager);

            IEnumerator nodesEnum = nodes.GetEnumerator();
            while (bContinuer) {
                nDoc++;
                //XmlNode docNode = getDocumentNode(defineNodeName, nDoc, ref cards);
                nodesEnum.MoveNext();
                XmlNode docNode = (XmlNode)nodesEnum.Current;

                if (docNode == null) { // Il n'y a pas ou plus de balise Document
                    bContinuer = false;
                } else {
                    String docNodeName = docNode.Attributes.GetNamedItem("name").Value;
                    if (docNodeName == null) {
                        errorsList.Add("Le nœud RNG du document n°'" + nDoc + "' n'a pas d'attribut name.");
                    } else {
                        xPath = "rng:define[@name='" + docNodeName + "']/rng:oneOrMore/rng:element[@name='Type']/rng:ref";
                        XmlNode typeNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                        if (typeNode != null) {
                            errorsList.Add("Le type de document de la balise Document[" + nDoc + "] de l'unité documentaire '" + currentDocumentTypeId + "' ne doit pas être multiple. La cardinalité doit être modifiée.");
                        } else {
                            xPath = "rng:define[@name='" + docNodeName + "']/rng:element[@name='Type']/rng:ref";
                            typeNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (typeNode != null) {
                                String typeNodeName = typeNode.Attributes.GetNamedItem("name").Value;
                                if (typeNodeName == null) {
                                    errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                                } else {
                                    xPath = "rng:define[@name='" + typeNodeName + "']/rng:value";
                                    XmlNode attrNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                    if (attrNode == null) {
                                        errorsList.Add("Le type de document de la balise Document[" + nDoc + "] de l'unité documentaire '" + currentDocumentTypeId + "' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void checkForFilename(String defineNodeName, String context) {
            // Tester la présence de l'attribut filename de Attachment
            int nDoc = 0;
            bool bContinuer = true;
            int[] cards = new int[] { 1, 1, 1, 1 };
            String xPath = "rng:define[@name='" + defineNodeName + "']/descendant::rng:element[@name='Document']/rng:ref";
            XmlNodeList nodes = grammarNode.SelectNodes(xPath, docInXmlnsManager);

            IEnumerator nodesEnum = nodes.GetEnumerator();
            while (bContinuer) {
                nDoc++;
                //XmlNode docNode = getDocumentNode(defineNodeName, nDoc, ref cards);
                nodesEnum.MoveNext();
                XmlNode docNode = (XmlNode)nodesEnum.Current;

                if (docNode == null) { // Il n'y a pas ou plus de balise Document
                    bContinuer = false;
                } else {
                    String docNodeName = docNode.Attributes.GetNamedItem("name").Value;
                    if (docNodeName == null) {
                        errorsList.Add("Le nœud RNG du document n°'" + nDoc + "' n'a pas d'attribut name.");
                    } else { // if (docNodeName == null)
                        xPath = "rng:define[@name='" + docNodeName + "']/rng:element[@name='Attachment']/rng:ref";
                        XmlNode attachNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                        if (attachNode != null) {
                            String attachNodeName = attachNode.Attributes.GetNamedItem("name").Value;
                            if (attachNodeName == null) {
                                errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                            } else { // if (attachNodeName == null)
                                xPath = "rng:define[@name='" + attachNodeName + "']/rng:optional/rng:attribute[@name='filename']";
                                XmlNode attrNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                if (attrNode != null) {
                                    errorsList.Add("L'attribut filename de la balise Document[" + nDoc + "]/Attachment de l'unité documentaire '" + currentDocumentTypeId + "' est facultatif alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.");
                                } else { // f (attrNode != null)
                                    xPath = "rng:define[@name='" + attachNodeName + "']/rng:attribute[@name='filename']";
                                    attrNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                    if (attrNode == null) {
                                        errorsList.Add("L'attribut filename de la balise Document[" + nDoc + "]/Attachment de l'unité documentaire '" + currentDocumentTypeId + "' est interdit alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.");
                                    }
                                } // f (attrNode != null)
                            } // if (attachNodeName == null)
                        } // if (attachNodeName == null)
                    } // if (docNodeName == null)
                }  // if (docNode != null)
            }
        }

        protected void checkForDocumentInArchive(String defineNodeName, String context) {
            // Tester la présence de Document dans le Contains (ou Archive) de départ 
            // et lancer une alerte
            bool error = false;
            String xPath = "rng:define[@name='" + defineNodeName + "']/rng:element[@name='Document']/rng:ref";
            XmlNode cdrefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (cdrefNode != null) {
                error = true;
            } else {
                xPath = "rng:define[@name='" + defineNodeName + "']/rng:optional/rng:element[@name='Document']/rng:ref";
                cdrefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                if (cdrefNode != null) {
                    error = true;
                } else {
                    xPath = "rng:define[@name='" + defineNodeName + "']/rng:oneOrMore/rng:element[@name='Document']/rng:ref";
                    cdrefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (cdrefNode != null) {
                        error = true;
                    } else {
                        xPath = "rng:define[@name='" + defineNodeName + "']/rng:zeroOrMore/rng:element[@name='Document']/rng:ref";
                        cdrefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                        if (cdrefNode != null) {
                            error = true;
                        }
                    }
                }
            }
            if (error) {
                errorsList.Add("La balise Document de Archive (ou Contains premier niveau) ne peut pas recevoir de documents. Tous les documents doivent être situés dans des unités documentaires.");
            }
        }

        protected void checkForContentDescription(String defineNodeName, String context) {
            // Tester la présence de ContentDescription dans rng:optional avec un KeywordContent
            // imbriqué dans un ContentDescriptive et lancer une alerte
            String xPath = "rng:define[@name='" + defineNodeName + "']/rng:optional/rng:element[@name='ContentDescription']/rng:ref";
            XmlNode cdrefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (cdrefNode != null) {
                String cdrefNodeName = cdrefNode.Attributes.GetNamedItem("name").Value;
                if (cdrefNodeName == null) {
                    errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                } else {
                    String stKeyword = SEDA_version == "0.2" ? "ContentDescriptive" : "Keyword";
                    xPath = "rng:define[@name='" + cdrefNodeName + "']/rng:element[@name='" + stKeyword + "']/rng:ref";
                    XmlNode cdeNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (cdeNode != null) {
                        String cdeNodeName = cdeNode.Attributes.GetNamedItem("name").Value;
                        if (cdeNodeName == null) {
                            errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                        } else {
                            xPath = "rng:define[@name='" + cdeNodeName + "']/rng:element[@name='KeywordContent']";
                            XmlNode kwNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (kwNode != null) {
                                errorsList.Add("Les mots-clés de l'unité documentaire '" + currentDocumentTypeId + "' ne pourront pas être produits car la description du contenu est optionnelle.");
                            }
                        }
                    }
                }
            }
        }

        protected void checkForOptionalOriginatingAgency(String defineNodeName, String context) {
            // Recherche de OriginatingAgency alerte si optionnel
            String xPath = "rng:define[@name='" + defineNodeName + "']/rng:element[@name='ContentDescription']/rng:ref";
            XmlNode cdrefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (cdrefNode != null) {
                String cdrefNodeName = cdrefNode.Attributes.GetNamedItem("name").Value;
                if (cdrefNodeName == null) {
                    errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                } else {
                    Boolean bProducteurOptionnel = false;
                    xPath = "rng:define[@name='" + cdrefNodeName + "']/rng:optional/rng:element[@name='OriginatingAgency']/rng:ref";
                    XmlNode cdeNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                    if (cdeNode != null)
                        bProducteurOptionnel = true;
                    else {
                        xPath = "rng:define[@name='" + cdrefNodeName + "']/rng:zeroOrMore/rng:element[@name='OriginatingAgency']/rng:ref";
                        cdeNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                        if (cdeNode != null)
                            bProducteurOptionnel = true;
                    }
                    if (bProducteurOptionnel == true) {
                        if (context.Equals("rootContains"))
                            errorsList.Add("(--) La balise OriginatingAgency est optionnelle et ne sera pas générée. Elle pourrait être rendue obligatoire");
                        else
                            errorsList.Add("(--) La balise OriginatingAgency de l'unité documentaire '" + currentDocumentTypeId + "' est optionnelle et ne sera pas générée. Elle pourrait être rendue obligatoire");
                    }
                }
            }
        }

        /*
         * Recherche la définition d'un nœud de nom searchedNodename
         * dans la définition du nœud defineNodeName 
         * dans le contexte context
         * 
         * Exemple :
         * on recherche la définition de ContentDescriptive dans ContentDescription_N65703
         * le retour est la déinition du nœud située ici : <rng:define name="ContentDescriptive_N65720">
         * 
          	<rng:define name="ContentDescription_N65703">
		        <rng:element name="Language">
			        <rng:ref name="Language_N65707"/>
		        </rng:element>
		        <rng:element name="ContentDescriptive">
			        <rng:ref name="ContentDescriptive_N65720"/>
		        </rng:element>
	        </rng:define>
	        <rng:define name="ContentDescriptive_N65720">
		        <rng:element name="KeywordContent">
			        <rng:ref name="KeywordContent_N65725"/>
		        </rng:element>
		        <rng:element name="KeywordReference">
			        <rng:ref name="KeywordReference_N65736"/>
		        </rng:element>
	        </rng:define>
         * 
         * */
        protected String getDefinedNodeName(String defineNodeName, String context, String searchedNodename, bool optional) {
            String searchedNodeName = String.Empty;
            String xPath;
            String stOptional = optional ? "//" : "";
            xPath = "rng:define[@name='" + defineNodeName + "']" 
                + stOptional 
                + "/rng:element[@name='" + searchedNodename + "']/rng:ref";

            XmlNode searchedRefNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (searchedRefNode != null) {
                searchedNodeName = searchedRefNode.Attributes.GetNamedItem("name").Value;
                if (searchedNodeName == null)
                    errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
            }
            return searchedNodeName;
        }

        /*
         * on recherche la définition de ContentDescription dans le Contains
         * le retour est la déinition du nœud située ici : <rng:define name="ContentDescriptive_N65720">
         * 
          	<rng:define name="ContentDescription_N65703">
		        <rng:element name="Language">
			        <rng:ref name="Language_N65707"/>
		        </rng:element>
		        <rng:element name="ContentDescriptive">
			        <rng:ref name="ContentDescriptive_N65720"/>
		        </rng:element>
	        </rng:define>
	        <rng:define name="ContentDescriptive_N65720">
		        <rng:element name="KeywordContent">
			        <rng:ref name="KeywordContent_N65725"/>
		        </rng:element>
		        <rng:element name="KeywordReference">
			        <rng:ref name="KeywordReference_N65736"/>
		        </rng:element>
	        </rng:define>
         * 
         * */
        protected void checkForTagInContains(String tag2Search, String containsNodeName, String context) {
            // Pour contrôler l'unicité des keyword ou FilePlanPosition
            StringCollection listXmlTags = new StringCollection();
            String contentDescriptionNodeName = getDefinedNodeName(containsNodeName, context, "ContentDescription", false);
            if (contentDescriptionNodeName != String.Empty) {
                String stTag2Search;
                if (tag2Search.Equals("Keyword"))
                    stTag2Search = SEDA_version == "0.2" ? "ContentDescriptive" : "Keyword";
                else
                    if (tag2Search.Equals("FilePlanPosition"))
                        stTag2Search = tag2Search;
                    else
                        stTag2Search = "UnknowTag";

                String xPath = "rng:define[@name='" + contentDescriptionNodeName + "']/descendant::rng:element[@name='" + stTag2Search + "']/rng:ref";
                XmlNodeList searchedTagsList = grammarNode.SelectNodes(xPath, docInXmlnsManager);
                int numTagInList = 0;
                if (searchedTagsList == null)
                    return;
                // Si la balise recherchée est unique, elle n'a pas à avoir un identifiant
                // Sauf si elle est dans l'archive
                if (searchedTagsList.Count > 1 || (context.Equals("rootContains") && searchedTagsList.Count >= 1)) {
                    foreach (XmlNode searchedTagNode in searchedTagsList) {
                        ++numTagInList;
                        String stSearchedTagDefinition;
                        stSearchedTagDefinition = searchedTagNode.Attributes.GetNamedItem("name").Value;
                        if (stSearchedTagDefinition == null)
                            errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                        else {
                            String stScheme = String.Empty;
                            bool bSearchedTagRequiresTag = true;
                            String keywordContentNodeName = getDefinedNodeName(stSearchedTagDefinition, context, "KeywordContent", false);
                            if (keywordContentNodeName != String.Empty) {
                                xPath = "rng:define[@name='" + keywordContentNodeName + "']/rng:value";
                                XmlNode valueNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                if (valueNode != null)
                                    bSearchedTagRequiresTag = false;
                            }
                            if (bSearchedTagRequiresTag) {
                                bool bEmettreWarning = true, bErreurDuplication = false;
                                String keywordReferenceNodeName = getDefinedNodeName(stSearchedTagDefinition, context, "KeywordReference", false);
                                if (keywordReferenceNodeName != String.Empty) {
                                    xPath = "rng:define[@name='" + keywordReferenceNodeName + "']";
                                    XmlNode keywordReferenceNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                    stScheme = getSchemeIdValue(keywordReferenceNode, xPath, context, numTagInList);
                                    if (stScheme != null && stScheme != String.Empty) {
                                        bEmettreWarning = false;
                                        if (listXmlTags.Contains(stScheme))
                                            bErreurDuplication = true;
                                        listXmlTags.Add(stScheme);
                                    }
                                }
                                if (bEmettreWarning || bErreurDuplication) {
                                    String stLocalisation = String.Empty;
                                    if (context.Equals("rootContains"))
                                        stLocalisation = "de l'archive";
                                    else
                                        stLocalisation = "de l'unité documentaire '" + currentDocumentTypeId + "'";
                                    if (bEmettreWarning)
                                        errorsList.Add("Le mot-clé n° '" + numTagInList + "' " + stLocalisation + " doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'");
                                    if (bErreurDuplication)
                                        errorsList.Add("Le mot-clé n° '" + numTagInList + "' " + stLocalisation + " a un tag '" + stScheme + "' qui n'est pas unique");
                                }
                            }
                        }
                    }
                }
            }
        }

        /*
         * on recherche la définition de ContentDescription dans le Contains
         * On recherche ensuite les FilePlanPosition
         * 
	        <rng:define name="ContentDescription_N65615">
		        <rng:element name="DescriptionLevel">
			        <rng:ref name="DescriptionLevel_N65618"/>
		        </rng:element>
		        <rng:zeroOrMore>
			        <rng:element name="FilePlanPosition">
				        <rng:ref name="FilePlanPosition_N65629"/>
			        </rng:element>
		        </rng:zeroOrMore>
		        <rng:zeroOrMore>
			        <rng:element name="FilePlanPosition">
				        <rng:ref name="FilePlanPosition_N65662"/>
			        </rng:element>
		        </rng:zeroOrMore>
		        <rng:oneOrMore>
			        <rng:element name="Language">
				        <rng:ref name="Language_N65695"/>
			        </rng:element>
		        </rng:oneOrMore>
	        </rng:define>
	        <rng:define name="FilePlanPosition_N65629">
		        <rng:data type="string"/>
		        <rng:attribute name="schemeID">
			        <rng:value>DOCLIST / FPP1</rng:value>
		        </rng:attribute>
	        </rng:define>
         * 
         * */
        protected void checkForFilePlanPositionTag(String containsNodeName, String context) {
            // Pour contrôler l'unicité des doclist des FilePlanPosition
            StringCollection listFPP = new StringCollection();
            String contentDescriptionNodeName = getDefinedNodeName(containsNodeName, context, "ContentDescription", false);
            if (contentDescriptionNodeName != String.Empty) {
                String stFPP = "FilePlanPosition";
                String xPath = "rng:define[@name='" + contentDescriptionNodeName + "']/descendant::rng:element[@name='" + stFPP + "']/rng:ref";
                XmlNodeList fppList = grammarNode.SelectNodes(xPath, docInXmlnsManager);
                int numfpp = 0;
                if (fppList == null)
                    return;
                // Si la balise recherchée est unique, elle n'a pas à avoir un identifiant
                // Sauf si elle est dans l'archive
                if (fppList.Count > 1 || (context.Equals("rootContains") && fppList.Count >= 1)) {
                    foreach (XmlNode filePlanPosition in fppList) {
                        ++numfpp;
                        String stFilePlanPosition;
                        stFilePlanPosition = filePlanPosition.Attributes.GetNamedItem("name").Value;
                        if (stFilePlanPosition == null)
                            errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                        else {
                            xPath = "rng:define[@name='" + stFilePlanPosition + "']/rng:data";
                            XmlNode dataNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (dataNode != null) {
                                String stLocalisation = String.Empty;
                                if (context.Equals("rootContains"))
                                    stLocalisation = "de l'archive";
                                else
                                    stLocalisation = "de l'unité documentaire '" + currentDocumentTypeId + "'";
                                bool /* bEmettreWarning = true, */ bErreurDuplication = false;
                                String stScheme = getFPPSchemeIdValue(stFilePlanPosition, stLocalisation, context, numfpp);
                                if (stScheme != null && stScheme != String.Empty) {
                                    // bEmettreWarning = false;
                                    if (listFPP.Contains(stScheme))
                                        bErreurDuplication = true;
                                    listFPP.Add(stScheme);
                                }

                                if (/* bEmettreWarning || */ bErreurDuplication) {
                                    /* if (bEmettreWarning)
                                        errorsList.Add("La position dans le plan de classement '" + numfpp + "' " + stLocalisation + " doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'"); */
                                    if (bErreurDuplication)
                                        errorsList.Add("La balise FilePlanPosition n° '" + numfpp + "' " + stLocalisation + " a un tag '" + stScheme + "' qui n'est pas unique");
                                }
                            }
                        }
                    }
                }
            }
        }

        /*
         * Génération d'alertes si des Document multiples existent et qu'il n'y a pas de balise Identification ou ArchivalAgencyDocumentIdentifier
         */
        protected void checkForMultipleDocument(String defineNodeName, String context) {
            if (traceActions) tracesWriter.WriteLine("checkForMultipleDocument ('" + defineNodeName + "', '" + context + "', '" + currentDocumentTypeId + "')");
            String xPath;
            // TODO
            // Tester l'existence de balise Document multiples et signaler l'absence de Identification ou ArchivalAgencyDocumentIdentifier
            xPath = "rng:define[@name='" + defineNodeName + "']";
            XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (containsNode == null) {
                errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
            } else {
                xPath = "descendant::rng:element[@name='Document']/rng:ref";
                XmlNodeList documentNodesList = containsNode.SelectNodes(xPath, docInXmlnsManager);
                // On peut ne pas avoir de Document
                if (documentNodesList != null && documentNodesList.Count > 1) { // Uniquement si il y a des documents multiples
                    int compteur = 0;
                    foreach (XmlNode node in documentNodesList) {
                        compteur++;
                        String nodeName = node.Attributes.GetNamedItem("name").Value;
                        if (nodeName == null) {
                            errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                        } else {
                            xPath = "rng:define[@name='" + nodeName + "']";
                            XmlNode documentNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                            if (documentNode == null) {
                                errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                            } else {
                                xPath = "rng:element[@name='"
                                    + stDocumentIdentification
                                    + "']/rng:ref";
                                XmlNode identificationNode = documentNode.SelectSingleNode(xPath, docInXmlnsManager);
                                if (identificationNode == null) {
                                    errorsList.Add("Dans l'unité documentaire '" + currentDocumentTypeId + "', la balise Document n°'" + compteur + "' doit contenir une balise " + stDocumentIdentification + " de cardinalités 1-1");
                                } else {
                                    String identificationName = identificationNode.Attributes.GetNamedItem("name").Value;
                                    if (identificationName == null) {
                                        errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                                    } else {
                                        xPath = "rng:define[@name='" + identificationName + "']";
                                        XmlNode identificationDefinitionNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                        if (identificationDefinitionNode == null) {
                                            errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                                        } else {
                                            String identificationDefinitionNodeName = identificationDefinitionNode.Attributes.GetNamedItem("name").Value;
                                            if (identificationDefinitionNodeName == null) {
                                                errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                                            } else {
                                                xPath = "rng:define[@name='" + identificationDefinitionNodeName + "']/rng:attribute[@name='schemeID']/rng:value";
                                                XmlNode schemeIdNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                                if (schemeIdNode == null) {
                                                    errorsList.Add("Dans l'unité documentaire '" + currentDocumentTypeId + "', la balise " + stDocumentIdentification + " de balise Document n°'" + compteur + "' doit avoir un attribut schemeID obligatoire");
                                                } else {
                                                    String docTypeId = getDocumentTypeId(schemeIdNode.InnerText, context + "/Document[" + compteur + "]");
                                                    //Console.WriteLine("Type de document '" + docTypeId + "'");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        /*
         * Toutes les erreurs détectées peuvent être récupérées dans la liste
         * */
        public StringCollection getErrorsList() {
            return errorsList;
        }

        /*
         * La structure de l'arbre peut être récupéré dans la liste
         * */
        public StringCollection getTreeList() {
            return treeList;
        }

        /*
         * La structure de l'arbre peut être récupéré dans la liste
         * */
        public StringCollection getExpectedTagsListList() {
            return expectedTagsList;
        }


        /*
         * Gestion des traces de débigage
         * */
        public void setTracesWriter(TextWriter tracesWriter) {
            if (tracesWriter != null) {
                this.tracesWriter = tracesWriter;
                traceActions = true;
            } else
                unsetTracesWriter();
        }

        /*
         * Gestion des traces de débigage
         * */
        public void unsetTracesWriter() {
            tracesWriter = null;
            traceActions = false;
        }

    }
}
