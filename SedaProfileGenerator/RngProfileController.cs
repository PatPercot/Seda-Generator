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


namespace SedaSummaryGenerator {
    public class RngProfileController {
        private String profileFile = String.Empty;
        private XmlDocument docIn;
        private XmlNamespaceManager docInXmlnsManager;
        private XmlNode grammarNode;
        private String SEDA_version;    // 0.2 ou 1.0
        private String rootContains = "Contains";
        private String descendantContains = "Contains";
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

        public RngProfileController() {
            errorsList = new StringCollection();
            unsetTracesWriter();
            treeList = new StringCollection();
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

            if (traceActions) tracesWriter.WriteLine("\n-----------------------------------------\n");
            if (traceActions) tracesWriter.WriteLine("Début de la vérification du profil");
            if (traceActions) tracesWriter.WriteLine("\n-----------------------------------------\n");
            Console.WriteLine("Début de la vérification du profil");
            String xPath;
            try {
                grammarNode = docIn.SelectSingleNode("rng:grammar", docInXmlnsManager);
                if (grammarNode == null) {
                    errorsList.Add("Le nœud '" + "rng:grammar" + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                } else {
                    // SEDA 1.0 "fr:gouv:culture:archivesdefrance:seda:v1.0"
                    // SEDA 0.2 "fr:gouv:ae:archive:draft:standard_echange_v0.2"
                    String sTestSeda = grammarNode.Attributes.GetNamedItem("ns").Value;
                    if (sTestSeda == "fr:gouv:ae:archive:draft:standard_echange_v0.2") {
                        rootContains = "Contains";
                        descendantContains = "Contains";
                        SEDA_version = "0.2";
                    }
                    else
                        if (sTestSeda == "fr:gouv:culture:archivesdefrance:seda:v1.0") {
                            rootContains = "Archive";
                            descendantContains = "ArchiveObject";
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
                                    xPath = "rng:define[@name='" + archiveNodeName + "']/descendant::rng:element[@name='" + rootContains + "']/rng:ref";
                                    XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
                                    if (containsNode == null) {
                                        errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                                    } else {
                                        String containsNodeName = containsNode.Attributes.GetNamedItem("name").Value;
                                        if (containsNodeName == null) {
                                            errorsList.Add("Le nœud '" + xPath + "' n'a pas d'attribut name.");
                                        } else {
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
            }

            if (traceActions) tracesWriter.WriteLine("\n-----------------------------------------\n");
            if (traceActions) tracesWriter.WriteLine("Fin de la vérification du profil");
            if (traceActions) tracesWriter.WriteLine("\n-----------------------------------------\n");
            Console.WriteLine("Fin de la vérification du profil");

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
                        + value + "' dans le contexte '" + context + "'");
            }
            return docId;
        }

        protected String getSchemeIdValue(XmlNode aaoirefNode, String currentXpath, String context) {
            String ret = null;
            String aaoiNodeName = aaoirefNode.Attributes.GetNamedItem("name").Value;
            if (aaoiNodeName == null) {
                errorsList.Add("Le nœud '" + currentXpath + "' n'a pas d'attribut name.");
            } else {
                currentXpath = "rng:define[@name='" + aaoiNodeName + "']/rng:attribute[@name='schemeID']/rng:value";
                XmlNode schemeIdNode = grammarNode.SelectSingleNode(currentXpath, docInXmlnsManager);
                if (schemeIdNode == null) {
                    errorsList.Add("Le nœud '" + currentXpath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                } else {
                    ret = getDocumentTypeId(schemeIdNode.InnerText, context);
                }
            }
            return ret;
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
                        String ret = getSchemeIdValue(aaoirefNode, xPath, context);
                        if (ret != null) {
                            errorsList.Add("La balise ArchivalAgencyObjectIdentifier de l'unité documentaire '" + ret + "' est optionnelle. Il faut la rendre obligatoire.");
                        }
                    }
                    else
                        errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                } else {
                    String ret = getSchemeIdValue(aaoirefNode, xPath, context);
                    if (ret != null)
                        currentDocumentTypeId = ret;
                }
                checkForMultipleDocument(defineNodeName, context);
            }
            if (currentDocumentTypeId == "root") {
                rootContainsNode = new ContainsNode(currentDocumentTypeId, null, true);
                currentContainsNode = rootContainsNode;
            } else {
                currentContainsNode = currentContainsNode.addNewNode(currentDocumentTypeId, true);
                if (  isRepeatable && ! currentDocumentTypeId.EndsWith("+"))
                    errorsList.Add("L'unité documentaire '" + currentDocumentTypeId + "' peut être répétée, mais elle ne possède pas de TAG répétable (TAG+). Il faut ajouter un '+' au tag ou changer les cardinalités");
                if (! isRepeatable &&   currentDocumentTypeId.EndsWith("+"))
                    errorsList.Add("L'unité documentaire '" + currentDocumentTypeId + "' est unique ou optionnelle, mais elle possède un TAG répétable (TAG+). Il faut supprimer le '+' du tag ou changer les cardinalités");
            }

            xPath = "rng:define[@name='" + defineNodeName + "']";
            XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (containsNode == null) {
                errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
            } else {
                // Parcourir récursivement toutes les balises Contains filles
                xPath = "descendant::rng:element[@name='" + descendantContains + "']/rng:ref";
                XmlNodeList containsNodesList = containsNode.SelectNodes(xPath, docInXmlnsManager);
                if (containsNodesList != null) {
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
                }
                currentContainsNode = currentContainsNode.getParent();
            }
        }


        /*
         * Génération d'alertes si des Document multiples existent et qu'il n'y a psa de balise Identification
         */
        protected void checkForMultipleDocument(String defineNodeName, String context) {
            if (traceActions) tracesWriter.WriteLine("checkForMultipleDocument ('" + defineNodeName + "', '" + context + "', '" + currentDocumentTypeId + "')");
            String xPath;
            // TODO
            // Tester l'existence de balise Document multiples et signaler l'absence de Attachment/Identification
            xPath = "rng:define[@name='" + defineNodeName + "']";
            XmlNode containsNode = grammarNode.SelectSingleNode(xPath, docInXmlnsManager);
            if (containsNode == null) {
                errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
            } else {
                xPath = "descendant::rng:element[@name='Document']/rng:ref";
                XmlNodeList documentNodesList = containsNode.SelectNodes(xPath, docInXmlnsManager);
                // On peut ne pas avoir de Document
                if (documentNodesList != null && documentNodesList.Count > 1) {
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
                                xPath = "rng:element[@name='Identification']/rng:ref";
                                XmlNode identificationNode = documentNode.SelectSingleNode(xPath, docInXmlnsManager);
                                if (identificationNode == null) {
                                    errorsList.Add("Erreur dans le contexte '" + context + "' sur le DOCLIST '" + currentDocumentTypeId + "', la balise Document n°'" + compteur + "' doit contenir une balise Identification");
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
                                                    errorsList.Add("Le nœud '" + xPath + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                                                } else {
                                                    String docTypeId = getDocumentTypeId(schemeIdNode.InnerText, context + "/Document[" + compteur + "]");
                                                    Console.WriteLine("Type de document '" + docTypeId + "'");
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
