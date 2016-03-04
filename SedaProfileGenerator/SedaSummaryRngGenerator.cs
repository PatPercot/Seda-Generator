/*****************************************************************************************
 * 
 * Générateur de bordereaux de transfert guidé par le profil d'archivage
 *                         -----------------------
 * Département du Morbihan
 * Direction des systèmes d'information
 * Service Études
 * Patrick Percot
 * Mars-mai 2015
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
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.Collections.Specialized;
using CommonClassesLibrary;

namespace SedaSummaryGenerator {
    /*
     * SedaSummaryRngGenerator est une implémentation de SedaSummaryGenerator qui
     * s'appuie sur un profil au format RNG
     * */
    public class SedaSummaryRngGenerator : SedaSummaryGenerator {
        private String profileFile = String.Empty;
        private XmlDocument docIn;
        private XmlWriter docOut;
        private XmlNamespaceManager docInXmlnsManager;
        private XmlNode grammarNode;
        // La génération se fait en deux passes
        // Durant la première on calcule le nombre de doucments par unité documentaire
        // les erreurs sont inhibées durant cette phase
        private int currentPass;
        private ContainsNode rootContainsNode;
        private ContainsNode currentContainsNode;
        // Le premier nœud Contains doit être généré indépendamment du fait qu'il y ait ou pas des documents
        // Ce premier nœud ne contient pas de balise ArchivalAgencyObjectIdentifier et doit être traité de façon exceptionnelle
        private bool firstContainsNode = true;
        private String currentDocumentTypeId;

        // TODO: Attention, ici on en gère pas encote le fait que des xxOrMore puissent être imbriqués
        //private bool inXxOrMore = false;

        // Données utilisées pour générer le bordereau
        private String TransferId;
        private ulong objectIdentifier = 0;
        private String SAE_FilePath;
        private String TransferringAgencyId = String.Empty;
        private String TransferringAgencyName = String.Empty;
        private String TransferringAgencyDesc = String.Empty;
        private String ArchivalAgencyId = String.Empty;
        private String ArchivalAgencyName = String.Empty;
        private String ArchivalAgencyDesc = String.Empty;

        // L'algorithme courant est SHA256
        // Pour SHA1 était : http://www.w3.org/2000/09/xmldsig#sha1
        private String getCurrentHashURI() {
            String retour = archiveDocuments.getDocumentHashAlgorithm();
            if (retour == String.Empty)
                retour = "http://www.w3.org/2001/04/xmlenc#sha256";
            return retour;
        }

        public SedaSummaryRngGenerator() : base() {
            // rootContainsNode est marqué avec l'ID "root"
            rootContainsNode = new ContainsNode("root", null, true);
            currentContainsNode = rootContainsNode;
            currentPass = 1;
        }

        /*
         * La liste des documents à archiver est définie dans un fichier qui 
         * contient le nom des fichiers et leurs métadonnées
         * */
        override protected void prepareProfileWithFile(String profileFile) {
            Action<Exception> eh = (ex) => {
                if (traceActions) tracesWriter.WriteLineFlush(ex.GetType().Name + " while reading: " + profileFile);
                if (traceActions) tracesWriter.WriteLineFlush(ex.Message);
                errorsList.Add("Erreur lors de la préparation du profil d'archivage '" + profileFile + "' " + ex.GetType().Name + " : " + ex.Message);
            };
            profileLoaded = false;
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
                    profileLoaded = true;
                }
            }
            catch (ArgumentException e) { eh(e); }
            catch (DirectoryNotFoundException e) { eh(e); }
            catch (FileNotFoundException e) { eh(e); }
            catch (OutOfMemoryException e) { eh(e); } 
            catch (IOException e) { eh(e); } 
            catch (XmlException e) { eh(e);  }
        }

        /*
         * La liste des documents à archiver est définie dans un fichier qui contient
         * les noms des fichiers et leurs métadonnées
         * Les documents sont dans le répertoire documentsFilePath
         * */
        override public void prepareArchiveDocumentsWithFile(String documentsFilePath, String archiveDocumentsFile) {
            SAE_FilePath = documentsFilePath;
            archiveDocumentsLoaded = false;
            CsvArchiveDocuments ad = new CsvArchiveDocuments();
            ad.setTracesWriter(tracesWriter);
            try {
                ad.loadFile(archiveDocumentsFile);
                // La variable membre héritée contient la valeur de l'objet créé
                archiveDocuments = ad;
                archiveDocumentsLoaded = true;
            } catch (Exception e) {
                errorsList.Add("Erreur lors de la préparation du fichiers de documents '" + archiveDocumentsFile + "' " + e.GetType().Name);
            }
        }

        /*
         * Les informations sont toutes les données qui ne sont pas contenues dans le
         * profil. Ces informations sont variables d'un versement ou d'un producteur
         * du versement à un autre
         * Elles sont fournies dans un fichier
         * */
        override public void prepareInformationsWithFile(String informationsFile) {
            errorsList.Add("SedaSummaryGenerator.SedaSummaryRngGenerator.prepareInformationsWithFile is not yet implemented");
            throw new NotImplementedException("SedaSummaryGenerator.SedaSummaryRngGenerator.prepareInformationsWithFile is not yet implemented");
        }

        /*
         * informationsDatabase contient une chaîne de connexion
         * accordVersement + baseURI permettent de trouver toutes les informations relatives au versement (profil, producteur, ...)
         * */
        override public void prepareInformationsWithDatabase(String informationsDatabase, String baseURI, String accordVersement)
        {
            informationsLoaded = false;
            if (traceActions) tracesWriter.WriteLineFlush("informationsDatabase='" + informationsDatabase +"'");
            using (SqlConnection connection = new SqlConnection(informationsDatabase)) {
                try {
                    connection.Open();
                    if (traceActions) tracesWriter.WriteLineFlush("ServerVersion: " + connection.ServerVersion);
                    if (traceActions) tracesWriter.WriteLineFlush("State: " + connection.State);
                    SqlTransaction transaction = connection.BeginTransaction();
                    try {
                        SqlCommand command = connection.CreateCommand();
                        command.CommandText = "SELECT TransferIdPrefix, TransferIdValue, SAE_ProfilArchivage, "
                            + " TransferringAgencyId, TransferringAgencyName, TransferringAgencyDesc, ArchivalAgencyId, ArchivalAgencyName, ArchivalAgencyDesc " 
                            + " "
                            + " FROM SAE WHERE SAE_AccordVersement='" + accordVersement + "' and SAE_Serveur='" + baseURI + "'";
                        command.CommandTimeout = 15;
                        command.CommandType = CommandType.Text;
                        command.Transaction = transaction;
                        SqlDataReader reader = command.ExecuteReader();
                        ulong Id = 0;
                        if (reader.Read()) {
                            Id = Convert.ToUInt64((String)reader[1]) + 1;
                            TransferId = reader[0] + String.Format("{0:0000000000}", Id);
                            profileFile = (String)reader[2];
                            TransferringAgencyId = (String)reader[3];
                            TransferringAgencyName = (String)reader[4];
                            TransferringAgencyDesc = (String)reader[5];
                            ArchivalAgencyId = (String)reader[6];
                            ArchivalAgencyName = (String)reader[7];
                            ArchivalAgencyDesc = (String)reader[8];
                            if (traceActions) tracesWriter.WriteLineFlush(String.Format("{0}, {1}, {2}, {3}"
                                , reader[0], reader[1], reader[2], reader[3]));
                            informationsLoaded = true;
                        } else {
                            errorsList.Add("Impossible de trouver l'accord de versement '" + accordVersement + "' dans la table SAE");
                        }
                        reader.Dispose();
                        command.CommandText = "UPDATE SAE SET TransferIdValue='" + Id + "' WHERE SAE_AccordVersement='" + accordVersement + "' and SAE_Serveur='" + baseURI + "'";
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    } catch (InvalidOperationException e) {
                        String strError = e.GetType().Name + " while query";
                        errorsList.Add(strError);
                        if (traceActions) tracesWriter.WriteLineFlush(strError);
                        if (traceActions) tracesWriter.WriteLineFlush(e.Message);
                        transaction.Rollback();
                    }
                    transaction.Dispose();

                    connection.Close();
                } catch (SqlException e) {
                    String strError = e.GetType().Name + " while opening database: " + informationsDatabase + " with accord: " + accordVersement;
                    errorsList.Add(strError);
                    if (traceActions) tracesWriter.WriteLineFlush(strError);
                    if (traceActions) tracesWriter.WriteLineFlush(e.Message);
                }
                prepareProfileWithFile(profileFile);
            }
        }

        /*
         * accordVersementConfig contient une configuration d'accord de versement
         * qui définit toutes les informations relatives au versement (profil, producteur, ...)
         * */
        override public void prepareInformationsWithConfigFile(SimpleConfig config, String baseURI, String accordVersement, String dataSha1) {
            AccordVersementConfig accordVersementConfig = config.getAccordVersementConfig(accordVersement, baseURI);
            if (accordVersementConfig == null) {
                errorsList.Add("Impossible de trouver l'accord de versement '" + accordVersement + "' dans la configuration");
            } else {
                TransferId = accordVersementConfig.TransferIdPrefix + dataSha1 + "@" + DateTime.UtcNow.ToString("o");
                profileFile = accordVersementConfig.SAE_ProfilArchivage;
                TransferringAgencyId = accordVersementConfig.TransferringAgencyId;
                TransferringAgencyName = accordVersementConfig.TransferringAgencyName;
                TransferringAgencyDesc = accordVersementConfig.TransferringAgencyDesc;
                ArchivalAgencyId = accordVersementConfig.ArchivalAgencyId;
                ArchivalAgencyName = accordVersementConfig.ArchivalAgencyName;
                ArchivalAgencyDesc = accordVersementConfig.ArchivalAgencyDesc;
                if (traceActions) tracesWriter.WriteLineFlush(String.Format("{0}, {1}, {2}"
                    , accordVersementConfig.TransferIdPrefix, TransferId, accordVersementConfig.SAE_ProfilArchivage));

                informationsLoaded = true;
                prepareProfileWithFile(profileFile);
            }
        }


        /*
         * C'est le fichier de sortie qui contient le bordereau de transfert construit avec :
         * Exceptions :
         * - SedaSumGenNoInformationsException
         * - SedaSumGenNoArchiveDocumentsException
         * - SedaSumGenNoProfileException
         * 
         * La séquence d'appels pour une production de bordereau est la suivante :
         * 
         *      SedaSummaryGenerator ssg = new SedaSummaryRngGenerator();
         *      ssg.setTracesWriter(streamWriter);
         *      
         *      ssg.prepareInformationsWithDatabase(informationsDatabase, accordVersement);
         *      
         *      ssg.prepareArchiveDocumentsWithFile("liste-fichiers.txt");
         *      
         *      ssg.generateSummaryFile("bordereau.xml");
         *      
         *      ssg.close();
         *      
         *      errors = ssg.getErrorsList();
         *      
         * */
        override public void generateSummaryFile(String summaryFile) {
            if (archiveDocumentsLoaded == false || profileLoaded == false || informationsLoaded == false)
                return;
            if (currentPass == 1) {
                if (traceActions) tracesWriter.WriteLineFlush("\n-----------------------------------------\n");
                if (traceActions) tracesWriter.WriteLineFlush("Début de l'évaluation du nombre de documents");
                if (traceActions) tracesWriter.WriteLineFlush("\n-----------------------------------------\n");
                Console.WriteLine("Début de l'évaluation du nombre de documents");               
            }
            if (currentPass == 2) {
                if (traceActions) tracesWriter.WriteLineFlush("\n-----------------------------------------\n");
                if (traceActions) tracesWriter.WriteLineFlush("Début de génération du bordereau");
                if (traceActions) tracesWriter.WriteLineFlush("\n-----------------------------------------\n");
                Console.WriteLine("Début de génération du bordereau");
                // Create the XmlDocument.
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                docOut = XmlWriter.Create(summaryFile, settings);
            }

            try {
                if (currentPass == 2) docOut.WriteStartDocument();
                grammarNode = docIn.SelectSingleNode("rng:grammar", docInXmlnsManager);
                XmlNode startNode = docIn.SelectSingleNode("rng:grammar/rng:start/rng:ref", docInXmlnsManager);
                if (grammarNode != null && startNode != null) {
                    recurseDefine(startNode.Attributes.GetNamedItem("name").Value, "");
                } else {
                    if (currentPass == 2) {
                        if (grammarNode == null) {
                            errorsList.Add("Le nœud '" + "rng:grammar" + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                        } else {
                            errorsList.Add("Le nœud '" + "rng:grammar/rng:start/rng:ref" + "' n'a pas été trouvé dans le profil '" + profileFile + "'");
                        }
                    }
                }
            }
            catch (XPathException e) {
                if (traceActions) tracesWriter.WriteLineFlush("Exception XPath...");
                if (traceActions) tracesWriter.WriteLineFlush(e.Message);
                if (currentPass == 2) errorsList.Add("generateSummaryFile :" + e.GetType().Name + " " + e.Message);
            }
            if (currentPass == 2) {
                docOut.WriteEndDocument();
                docOut.Close();
            }

            if (currentPass == 1)  {
                if (traceActions) tracesWriter.WriteLineFlush("\n-----------------------------------------\n");
                if (traceActions) tracesWriter.WriteLineFlush("Fin de l'évaluation du nombre de documents");
                if (traceActions) tracesWriter.WriteLineFlush("\n-----------------------------------------\n");
                Console.WriteLine("Fin de l'évaluation du nombre de documents");
                rootContainsNode.computeNbDocuments();
                if (traceActions) tracesWriter.WriteLineFlush(rootContainsNode.dump());
                bool bAllMandatoryContainsHaveDocuments = true;
                String sListMandatoryContainsInError = String.Empty;
                if (traceActions) {
                    ContainsNode node = rootContainsNode;
                    while (node != null) {
                        if (node.getMandatory() == true && node.getNbDocuments() == 0) {
                            bAllMandatoryContainsHaveDocuments = false;
                            sListMandatoryContainsInError += node.getName();
                            sListMandatoryContainsInError += "\t";
                        }
                        tracesWriter.WriteLineFlush("next node = " + node.getName() + " docs = " + node.getNbDocuments() + " mandatory = " + node.getMandatory() );
                        node = node.next();
                    }
                }
                if (bAllMandatoryContainsHaveDocuments == false) {
                    if (traceActions) tracesWriter.WriteLineFlush("Il existe des unités documentaires obligatoires sans documents : " + sListMandatoryContainsInError);
                    errorsList.Add("Il existe des unités documentaires obligatoires sans documents : " + sListMandatoryContainsInError);

                } else {
                    currentPass = 2;
                    rootContainsNode.trunkChildrenOfEmptyBranches();
                    if (traceActions) tracesWriter.WriteLineFlush(rootContainsNode.dump());
                    currentContainsNode = rootContainsNode;
                    firstContainsNode = true; // Indique que le prochain nœud Contains est le premier
                    currentDocumentTypeId = "root";
                    generateSummaryFile(summaryFile);
                }
            } else {
                if (traceActions) tracesWriter.WriteLineFlush("\n-----------------------------------------\n");
                if (traceActions) tracesWriter.WriteLineFlush("Fin de génération du bordereau");
                if (traceActions) tracesWriter.WriteLineFlush("\n-----------------------------------------\n");
                Console.WriteLine("Fin de génération du bordereau");
            }
            
        }

        // Cette méthode recherche le contenu de l'attribut schemeID de la balise ArchivalAgencyObjectIdentifier
        // de la balise containsNode
        /* Arborescence dans laquelle on recherche le ArchivalAgencyObjectIdentifier
            <rng:optional>
              <rng:element name="Contains">             => on part d'ici
                <rng:ref name="Contains_N66109"/>
              </rng:element>
            </rng:optional>

            <rng:define name="Contains_N66109">
             <rng:optional>
              <rng:element name=
              "ArchivalAgencyObjectIdentifier">
                <rng:ref name=
                "ArchivalAgencyObjectIdentifier_N66114"/>
              </rng:element>
              <rng:element name="DescriptionLevel">
                <rng:ref name="DescriptionLevel_N66149"/>
             </rng:optional>
            </rng:element>

            <rng:define name=
            "ArchivalAgencyObjectIdentifier_N66114">
              <rng:data type="string"/>
              <rng:attribute name="schemeID">
                <rng:value>
                DOCLIST_20140604 / DOCLIST_ESPCO_CODIR
                </rng:value>
              </rng:attribute>
         * */
        protected String lookupForContainsIdentifier(XmlNode containsNode) {
            String containsIdentifier = String.Empty;
            if (firstContainsNode == true) {
                containsIdentifier = "root";
                firstContainsNode = false;
            } else {
                XmlNode firstStep = containsNode.SelectSingleNode("rng:ref", docInXmlnsManager);
                if (firstStep != null) {
                    String firstStepName = firstStep.Attributes.GetNamedItem("name").Value;
                    XmlNode secondStep = grammarNode.SelectSingleNode("rng:define[@name='" + firstStepName +
                        "']", docInXmlnsManager);
                    if (secondStep != null) {
                        XmlNode thirdStep = secondStep.SelectSingleNode("descendant::rng:element[@name='ArchivalAgencyObjectIdentifier']/rng:ref", docInXmlnsManager);
                        if (thirdStep != null) {
                            String fourthStepName = thirdStep.Attributes.GetNamedItem("name").Value;
                            XmlNode fourthStep = grammarNode.SelectSingleNode("rng:define[@name='" + fourthStepName +
                                "']/rng:attribute[@name='schemeID']/rng:value", docInXmlnsManager);
                            containsIdentifier = getDocumentTypeId(fourthStep.InnerText, String.Empty);
                        }
                    }
                }
            }
            return containsIdentifier;
        }

        /*
         * Méthode utilitaire pour récupérer le DOCLIST
         * */
        private String getDocumentTypeId(String value, String context) {
            String docId = String.Empty;
            int pos = value.IndexOf(" / ");
            if (pos != -1) {
                docId = value.Substring(pos + 3);
            } else { // pos != -1
                if (context != String.Empty)
                    errorsList.Add("L'identifiant DOCLIST est malformé, on attend : 'DOCLIST / identifier', on a : '"
                        + value + "' dans le contexte '" + context + "'");
            }
            return docId;
        }

        void doContains(XmlNode currentNode, ref int bGenererElement, ref bool boucleTags, int numeroTag, bool bContainsIsMandatory) {
            currentDocumentTypeId = lookupForContainsIdentifier(currentNode);
            if (traceActions) tracesWriter.WriteLineFlush("lookupForContainsIdentifier trouve currentDocumentTypeId " + currentDocumentTypeId + " en passe " + currentPass);
            if (currentDocumentTypeId.EndsWith("+")) {
                boucleTags = true;
                currentDocumentTypeId = formatContainsIdentifier(currentDocumentTypeId, numeroTag);
                if (traceActions) tracesWriter.WriteLineFlush("currentDocumentTypeId à répétition : " + currentDocumentTypeId);
            }
            bGenererElement = 1;
            // si on est sur une boucleTags, il faut s'arrêter au nombre maximum d'itérations
            // donc demander à ArchiveDocuments si il y a des références à e TAG numéroté
            if (boucleTags)
                if (archiveDocuments.IsThereDocumentsReferringToType(currentDocumentTypeId) == false) {
                    bGenererElement = 0;
                    boucleTags = false;
                }
            if (currentPass == 2) {
                if (bGenererElement > 0 && currentContainsNode != null) {
                    if (currentDocumentTypeId == "root")
                        currentContainsNode = rootContainsNode;
                    else
                        currentContainsNode = currentContainsNode.next();
                    if (currentContainsNode != null) { // Tant qu'on est pas rrivés au bout de l'arbre
                        if (traceActions) tracesWriter.WriteLineFlush("Selecting next currentContainsNode " + currentContainsNode.getName() + " that contains " + currentContainsNode.getNbDocuments() + " documents");
                        int nbDocs = currentContainsNode.getNbDocuments();
                        bGenererElement = nbDocs > 0 ? 1 : 0;
                        if (traceActions) tracesWriter.WriteLineFlush("DOCLIST docs " + currentDocumentTypeId + " contains " + nbDocs + " documents");
                    }
                }
            } else {
                if (bGenererElement != 0) {
                    if (currentDocumentTypeId == "root") {
                        currentContainsNode = rootContainsNode;
                    } else {
                        if (traceActions) tracesWriter.WriteLineFlush("création nouveau containsNode(" + currentDocumentTypeId + ")");
                        currentContainsNode = currentContainsNode.addNewNode(currentDocumentTypeId, bContainsIsMandatory);
                    }
                    int nbDocs = archiveDocuments.prepareListForType(currentContainsNode.getRelativeContext());
                    currentContainsNode.incNbDocs(nbDocs);
                }
            }
        }

        protected String lookupForDocumentIdentification(XmlNode documentNode) {
            String documentIdentification = String.Empty;
            XmlNode firstStep = documentNode.SelectSingleNode("rng:ref", docInXmlnsManager);
            if (firstStep != null) {
                String firstStepName = firstStep.Attributes.GetNamedItem("name").Value;
                XmlNode secondStep = grammarNode.SelectSingleNode("rng:define[@name='" + firstStepName +
                    "']", docInXmlnsManager);
                if (secondStep != null) {
                    XmlNode thirdStep = secondStep.SelectSingleNode("descendant::rng:element[@name='Identification']/rng:ref", docInXmlnsManager);
                    if (thirdStep != null) {
                        String fourthStepName = thirdStep.Attributes.GetNamedItem("name").Value;
                        XmlNode fourthStep = grammarNode.SelectSingleNode("rng:define[@name='" + fourthStepName +
                            "']/rng:attribute[@name='schemeID']/rng:value", docInXmlnsManager);
                        documentIdentification = getDocumentTypeId(fourthStep.InnerText, String.Empty);
                    }
                }
            }
            return documentIdentification;
        }



        /*
         * Un define est une structure qui définit le contenu d'un élément
         * Elle peut faire appel à des références (rng:ref) vers d'autres define
         * et doit donc être traité récursivement
         * 
            <rng:define name="Contains_N66109">
             <rng:optional>
              <rng:element name=
              "ArchivalAgencyObjectIdentifier">
                <rng:ref name=
                "ArchivalAgencyObjectIdentifier_N66114"/>
              </rng:element>
              <rng:element name="DescriptionLevel">
                <rng:ref name="DescriptionLevel_N66149"/>
             </rng:optional>
            </rng:element>

         * */
        protected String recurseDefine(String defineNodeName, String context) {
            if (traceActions) tracesWriter.WriteLineFlush("recurseDefine ('" + defineNodeName + "', '" + context + "', '" + currentDocumentTypeId + "')");
            String dataString = null;

            XmlNode defineNode = grammarNode.SelectSingleNode("rng:define[@name='"
                + defineNodeName + "']", docInXmlnsManager);
            if (defineNode.HasChildNodes) {
                foreach (XmlNode currentNode in defineNode.ChildNodes) {
                    if (traceActions) {
                        if (currentNode.Attributes != null && currentNode.Attributes.GetNamedItem("name") != null)
                            tracesWriter.WriteLineFlush("recurseDefine visiting '" + currentNode.Name
                                + "'[name='" + currentNode.Attributes.GetNamedItem("name").Value + "']");
                        else
                            tracesWriter.WriteLineFlush("recurseDefine visiting '" + currentNode.Name + "'");
                    }
                    switch (currentNode.Name) {
                        case "rng:element":
                            if (currentNode.Attributes != null) {
                                int bGenererElement = 1;
                                int numeroTag = 0; // Première boucle sur des tags numérotés
                                bool boucleTags = false;
                                do {
                                    numeroTag++;
                                    String elementName = currentNode.Attributes.GetNamedItem("name").Value;
                                    if (elementName == "Contains" // SEDA 0.2
                                        || elementName == "ArchiveObject" || elementName == "Archive") { // SEDA 1.0
                                        doContains(currentNode, ref bGenererElement, ref boucleTags, numeroTag, true);
                                    }
                                    if (bGenererElement > 0)
                                        genElement(currentNode, elementName, context);
                                } while (boucleTags);
                            }
                            break;
                        // Ici on va générer des documents. C'est là que l'on passe au pilotage par les documents
                        // au lieu du pilotage par le profil
                        case "rng:oneOrMore":
                        case "rng:zeroOrMore":
                        case "rng:optional": {
                                // ici on peut boucler si le containsIdentifier se termine par un plus
                                int numeroTag = 0; // Première boucle sur des tags numérotés
                                bool boucleTags = false;
                                do {
                                    numeroTag++;
                                    if (traceActions) tracesWriter.WriteLineFlush("recurseDefine ??OrMore for " + currentNode.Name + " in context " + context + " on defineNode " + defineNodeName);
                                    if (currentNode.HasChildNodes) {
                                        foreach (XmlNode zomNode in currentNode.ChildNodes) {
                                            if (zomNode.Name == "rng:element" && zomNode.Attributes != null) {
                                                int bGenererElement = 0;
                                                String elementName = zomNode.Attributes.GetNamedItem("name").Value;
                                                switch (elementName) {
                                                    default:
                                                        if (currentNode.Name == "rng:oneOrMore")
                                                            bGenererElement = 1;
                                                        break;
                                                    case "Document": // les documents doivent bien évidemment être générés
                                                        bGenererElement = 1;
                                                        break;
                                                    case "Contains": // SEDA 0.2
                                                    case "ArchiveObject": // SEDA 1.0
                                                    case "Archive": {
                                                        bool bContainsIsMandatory = false;
                                                        if (currentNode.Name == "rng:oneOrMore")
                                                            bContainsIsMandatory = true;
                                                        doContains(zomNode, ref bGenererElement, ref boucleTags, numeroTag, bContainsIsMandatory);
                                                        }
                                                        break;
                                                    case "KeywordContent":
                                                    case "Keyword": // SEDA 1.0
                                                    case "ContentDescriptive": // SEDA 0.2
                                                        if (currentPass == 2) {
                                                            String containsName = currentContainsNode.getRelativeContext();
                                                            if (containsName == "root")
                                                                containsName = null;
                                                            int nbKeywords = archiveDocuments.getNbkeys("KeywordContent", containsName);
                                                            // ContentDescriptive ou Keyword est généré plusieurs fois et contient un KeywordContent
                                                            bGenererElement = elementName == "KeywordContent" ? 1 : nbKeywords;
                                                            if (traceActions) tracesWriter.WriteLineFlush("KeywordContent(" + containsName + ") contains " + nbKeywords + " keywords");
                                                        }
                                                        break;
                                                }
                                                while (bGenererElement != 0) {
                                                    genElement(zomNode, elementName, context);
                                                    --bGenererElement;
                                                }
                                            } // if (zomNode.Name
                                        } // foreach (
                                    }  // if currentNode.HasChildNodes
                                } while (boucleTags);
                            }
                            break;
                        case "#comment":
                            if (traceActions) tracesWriter.WriteLineFlush("recurseDefine !!! " + currentNode.Name + " in context " + context + " on defineNode " + defineNodeName + " cancelled");
                            break;
                        case "rng:value":
                            dataString = currentNode.InnerText;
                            break;
                        case "rng:attribute":
                            if (currentNode.HasChildNodes) {
                                String value = "";
                                String attrName = "";
                                foreach (XmlNode attrNode in currentNode.ChildNodes) {
                                    attrName = currentNode.Attributes.GetNamedItem("name").Value;
                                    if (attrNode.Name == "rng:value") { 
                                        value = attrNode.InnerText;
                                    } else
                                    if (attrNode.Name == "rng:data") {
                                        if (currentDocumentTypeId != null
                                            && context.EndsWith("Document/Attachment")) {
                                            if (attrName == "filename") {
                                                value = archiveDocuments.getDocumentFilename();
                                                tracesWriter.WriteLineFlush("recurseDefine generating filename '" + value + "' for DOCLIST '" + currentDocumentTypeId + "'");
                                            }
                                            if (attrName == "mimeCode") {
                                                value = MimeMapping.GetMimeMapping(archiveDocuments.getDocumentFilename());
                                                tracesWriter.WriteLineFlush("recurseDefine generating mimeCode '" + value + "' for DOCLIST '" + currentDocumentTypeId + "'");
                                            }
                                        }
                                        if (context.EndsWith("Integrity") && attrName == "algorithme") {
                                            value = archiveDocuments.getDocumentHashAlgorithm();
                                            if (value == String.Empty)
                                                value = getCurrentHashURI();
                                        }
                                    }

                                    try {
                                        if (currentPass == 2) docOut.WriteAttributeString(attrName, value);
                                    }
                                    catch (InvalidOperationException e) {
                                        if (traceActions) tracesWriter.WriteLineFlush("Unable to write attr " + attrName + " with value " + value);
                                        if (traceActions) tracesWriter.WriteLineFlush(e.Message);
                                    }
                                }
                            }
                            break;
                        case "rng:data":
                            dataString = getData(context, currentNode);
                            break;
                        default:
                            if (traceActions) tracesWriter.WriteLineFlush("recurseDefine   ----  !!!! currentNode.Name '" + currentNode.Name + "' Unhandled");
                            break;
                    }
                }
                if (dataString != null)
                    if (currentPass == 2) docOut.WriteString(dataString);
            }
            return currentDocumentTypeId;
        }

        /*
         * Écriture des balisesUniques d'un rng:element
         * Si le contenu fait appel à des rng:define peut appeler recurseDefine
         * 
         * */
        protected bool genElement(XmlNode elemNode, String tagToWrite, String context) {
            String dataString = null;
            int counter = 1;
            bool callNextDocument = false;
            if (tagToWrite == "Document") {
                if (currentDocumentTypeId != null) {
                    String typeId = currentContainsNode.getRelativeContext();
                    String documentIdentification = lookupForDocumentIdentification(elemNode);
                    bool withDocumentIdentification = documentIdentification != String.Empty;
                    if (withDocumentIdentification) {
                        typeId += "{" + documentIdentification + "}";
                    }
                    counter = archiveDocuments.prepareListForType(typeId, withDocumentIdentification);
                    if (traceActions) tracesWriter.WriteLineFlush("genElement Document : typeId " + typeId + " has " + counter + " documents");
                    callNextDocument = true;
                }
            }

            for (int bcl = 0; bcl < counter; ++bcl) {
                if (callNextDocument)
                    archiveDocuments.nextDocument();

                if (traceActions) tracesWriter.WriteLineFlush("genElement (" + elemNode.Name + ", " + tagToWrite + ", " + context + ")");
                try {
                    if (currentPass == 2) {
                        if (tagToWrite == "ArchiveTransfer") {
                            // SEDA 1.0 "fr:gouv:culture:archivesdefrance:seda:v1.0"
                            // SEDA 0.2 "fr:gouv:ae:archive:draft:standard_echange_v0.2"
                            docOut.WriteStartElement(tagToWrite, grammarNode.Attributes.GetNamedItem("ns").Value);

                        }
                        else
                            docOut.WriteStartElement(tagToWrite);
                    }
                }
                catch (InvalidOperationException e) {
                    if (traceActions) tracesWriter.WriteLineFlush("Unable to write start element " + tagToWrite);
                    if (traceActions) tracesWriter.WriteLineFlush(e.Message);
                }

                if (elemNode.HasChildNodes) {
                    foreach (XmlNode node in elemNode.ChildNodes) {
                        switch (node.Name) {
                            case "rng:ref":
                                String newContext = context + "/" + tagToWrite;
                                if (node.Attributes != null) {
                                    if (node.Attributes.GetNamedItem("name").Value.Equals("anyElement"))
                                        dataString = getTag(tagToWrite, context);
                                    else
                                        recurseDefine(node.Attributes.GetNamedItem("name").Value, newContext);
                                }
                                break;
                            case "rng:value":
                                dataString = node.InnerText;
                                break;
                            case "rng:data":
                                dataString = getTag(tagToWrite, context);
                                break;
                            default:
                                if (traceActions) tracesWriter.WriteLineFlush("genElement  ----  !!!! currentNode.Name Unhandled '" + node.Name + "' in context '" + context + "' for tag2write '" + tagToWrite + "'");
                                break;
                        }
                    }
                }
                try {
                    if (dataString != null)
                        if (currentPass == 2) docOut.WriteString(dataString);
                }
                catch (InvalidOperationException e) {
                    if (traceActions) tracesWriter.WriteLineFlush("Unable to write string content " + dataString);
                    if (traceActions) tracesWriter.WriteLineFlush(e.Message);
                }
                try {
                    if (currentPass == 1) {
                        // On redescend dans l'arbre des ContainsNode
                        // en passe 2 on utilise la méthode next qui permet de parcurir l'arbre comme un vecteur
                        if ((tagToWrite == "Contains" // SEDA 0.2
                            || tagToWrite == "ArchiveObject" || tagToWrite == "Archive") // SEDA 1.0
                            && currentContainsNode != rootContainsNode)
                            currentContainsNode = currentContainsNode.getParent();
                    }
                    if (currentPass == 2) docOut.WriteEndElement();
                }
                catch (InvalidOperationException e) {
                    if (traceActions) tracesWriter.WriteLineFlush("Unable to write end element " + tagToWrite);
                    if (traceActions) tracesWriter.WriteLineFlush(e.Message);
                }
            }
            return true;
        }

        // Cette méthode recherche un rng:attribut du nœud désigné 
        // et retourne la valeur de rng:value ou null si l'attribut ou la valeur n'ont pas été trouvées
        /* Arborescence exemple
            * 	<rng:define name="Size_N66177"> <!-- On est ici, on cherche du rng:attribute avec un name == attributeName ->
            * 		<rng:data type="string"/> 
            * 		<rng:attribute name="unitCode">
            * 			<rng:value>2P</rng:value>
            * 		</rng:attribute>
            * 	</rng:define>
            * 											
         * */
        protected String lookupForAttribute(String attributeName, XmlNode node) {
            XmlNode attributeStep = node.SelectSingleNode("rng:attribute[@name='" + attributeName + "']", docInXmlnsManager);
            if (attributeStep != null) {
                if (traceActions) tracesWriter.WriteLineFlush("lookupForAttribute attributeStep = '" + attributeStep.Name + "'");
                XmlNode valueStep = attributeStep.SelectSingleNode("rng:value", docInXmlnsManager);
                if (valueStep != null) {
                    if (traceActions) tracesWriter.WriteLineFlush("lookupForAttribute valueStep = '" + valueStep.Name + "' = '" + valueStep.InnerText + "'");
                    return valueStep.InnerText;
                }
            }
            if (traceActions) tracesWriter.WriteLineFlush("lookupForAttribute attributeName '" + attributeName + "' non trouvé.");
            return null;
        }
        /*
         * Traitement associé aux balisesUniques rng:data
         * nodeName est utile seulement pour tracer les actions et situer le contexte
         * */
        private String getData(String context, XmlNode node) {
            if (currentPass == 1)
                return String.Empty;
            String dataString = null;
            switch (context) {
                // case "/ArchiveTransfer/Archive/ContentDescription/Size": // SEDA 1.0 Toutefois cette balise n'existe pas en SEDA 1.0
                case "/ArchiveTransfer/Contains/ContentDescription/Size": // SEDA 0.2
                    {
                        int nbDocuments = archiveDocuments.prepareCompleteList();
                        double sizeOfDocuments = 0.0;
                        while (archiveDocuments.nextDocument()) {
                            sizeOfDocuments += computeSizeOfCurrentDocument();
                        }
                        dataString = formatSizeForNode(sizeOfDocuments, node);
                    }
                    break;
                case "/ArchiveTransfer/TransferIdentifier":
                    dataString = TransferId;
                    break;
                case "/ArchiveTransfer/Archive/Name": // SEDA 1.0
                case "/ArchiveTransfer/Contains/Name": // SEDA 0.2
                    dataString = archiveDocuments.getKeyValue("TransferName");
                    break;
                case "/ArchiveTransfer/Comment":
                    dataString = archiveDocuments.getKeyValue("Comment");
                    break;
                case "/ArchiveTransfer/TransferringAgency/Description":
                    dataString = TransferringAgencyDesc;
                    break;
                case "/ArchiveTransfer/TransferringAgency/Identification":
                    dataString = TransferringAgencyId;
                    break;
                case "/ArchiveTransfer/TransferringAgency/Name":
                    dataString = TransferringAgencyName;
                    break;
                case "/ArchiveTransfer/ArchivalAgency/Description":
                    dataString = ArchivalAgencyDesc;
                    break;
                case "/ArchiveTransfer/ArchivalAgency/Identification":
                    dataString = ArchivalAgencyId;
                    break;
                case "/ArchiveTransfer/ArchivalAgency/Name":
                    dataString = ArchivalAgencyName;
                    break;
                case "/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/BusinessType": // SEDA 1.0
                case "/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/Identification":
                case "/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/Description":
                case "/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/LegalClassification":
                case "/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/Name":
                case "/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/BusinessType": // SEDA 0.2
                case "/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/Identification":
                case "/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/Description":
                case "/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/LegalClassification":
                case "/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/Name":
                    int posLastSlash = context.LastIndexOf('/') + 1;
                    String balise = context.Substring(posLastSlash, context.Length - posLastSlash);
                    dataString = archiveDocuments.getKeyValue("OriginatingAgency." + balise);
                    break;
                case "/ArchiveTransfer/Archive/ContentDescription/CustodialHistory": // SEDA 1.0
                case "/ArchiveTransfer/Contains/ContentDescription/CustodialHistory": // SEDA 0.2
                    dataString = archiveDocuments.getKeyValue("CustodialHistory");
                    break;
                case "/ArchiveTransfer/Archive/ContentDescription/Keyword/KeywordContent": // SEDA 1.0
                case "/ArchiveTransfer/Contains/ContentDescription/ContentDescriptive/KeywordContent": // SEDA 0.2
                    dataString = archiveDocuments.getNextKeyValue("KeywordContent", null);
                    break;
                case "/ArchiveTransfer/Archive/ArchiveObject/TransferringAgencyObjectIdentifier": // SEDA 1.0
                case "/ArchiveTransfer/Contains/Contains/TransferringAgencyObjectIdentifier": // SEDA 0.2
                    dataString = "TODO: '" + context + "'";
                    break;
                default:
                    if (context.EndsWith("Integrity")) {
                        dataString = computeHashOfCurrentDocument();
                    } else if (context.EndsWith("Contains/ContentDescription/Size")) {
                    {
                        int nbDocuments = archiveDocuments.prepareListForType(currentContainsNode.getRelativeContext());
                        double sizeOfDocuments = 0.0;
                        while (archiveDocuments.nextDocument()) {
                            sizeOfDocuments += computeSizeOfCurrentDocument();
                        }
                        dataString = formatSizeForNode(sizeOfDocuments, node);
                    }
                    } else if (context.EndsWith("Document/Size")) { // SEDA 1.0
                        double sizeOfDocument = computeSizeOfCurrentDocument();
                        dataString = formatSizeForNode(sizeOfDocument, node);
                    } else if (context.EndsWith("/Document/Description")) {
                        dataString = archiveDocuments.getDocumentName();
                    } else if (context.EndsWith("/ArchiveObject/Name") // SEDA 1.0
                        || context.EndsWith("/Contains/Contains/Name")) { // SEDA .02
                        dataString = archiveDocuments.getKeyValue("ContainsName[" + currentContainsNode.getRelativeContext() + "]");
                    } else if (context.EndsWith("/ArchiveObject/ArchivalAgencyObjectIdentifier") // SEDA 1.0
                        || context.EndsWith("/Contains/ArchivalAgencyObjectIdentifier") // SEDA 0.2
                        || context.EndsWith("/Document/Identification")) {
                        objectIdentifier++;
                        dataString = TransferId + "_" + String.Format("{0:00000}", objectIdentifier);
                    } else if (context.EndsWith("ArchiveObject/ContentDescription/Keyword/KeywordContent") // SEDA 1.0
                        || context.EndsWith("Contains/ContentDescription/ContentDescriptive/KeywordContent")) { // SEDA 0.2
                        dataString = archiveDocuments.getNextKeyValue("KeywordContent", currentDocumentTypeId == "root" ? null : currentContainsNode.getRelativeContext());
                    } else {
                        if (traceActions) tracesWriter.WriteLineFlush("getData  ----  !!!! context '" + context + "' Unhandled in '" + node.Name + "'");
                    }
                    break;
            }
            return dataString;
        }

        /*
         * fonction utilitaire utilisée par genElement
         * */
        private String getTag(String tag, String context) {
            DateTime date;
            String dateString = String.Empty;
            String dateTraitee;
            if (currentPass == 1)
                return String.Empty;
            switch (tag) {
                case "Receipt":
                    return "TODO: Receipt";
                case "Type":
                    return "TODO: Type";
                case "Issue":
                    return "TODO: Issue";
                case "Duration":
                    return "TODO: Duration";
                case "Creation":
                    return computeDateOfCurrentDocument();
                case "OldestDate":
                    dateTraitee = archiveDocuments.getOldestDate();
                    try {
                        date = DateTime.Parse(dateTraitee, new System.Globalization.CultureInfo("fr-FR", false));
                        int posT = dateString.IndexOf("T");
                        if (posT > 0)
                            dateString = date.ToString("o").Substring(0, posT);
                        else
                            dateString = date.ToString("o");
                    } catch (FormatException e) {
                        dateString = "#DATAERR: date " + dateTraitee;
                        errorsList.Add("#DATAERR: La date '" + dateTraitee + "' '" + tag + "' ne correspond pas à une date réelle ou son format est incorrect. Format attendu JJ/MM/AAAA hh:mm:ss");
                    }
                    return dateString;
                case "StartDate":
                case "LatestDate":
                    dateTraitee = archiveDocuments.getLatestDate();
                    try {
                        date = DateTime.Parse(archiveDocuments.getLatestDate(), new System.Globalization.CultureInfo("fr-FR", false));
                        int posT = dateString.IndexOf("T");
                        if (posT > 0)
                            dateString = date.ToString("o").Substring(0, posT);
                        else
                            dateString = date.ToString("o");
                    } catch (FormatException e) {
                        dateString = "#DATAERR: date " + dateTraitee;
                        errorsList.Add("#DATAERR: La date '" + dateTraitee + "' '" + tag + "' ne correspond pas à une date réelle ou son format est incorrect. Format attendu JJ/MM/AAAA hh:mm:ss");
                    }
                    return dateString;
                case "Date":
                    switch (context) {
                        case "/ArchiveTransfer":
                            return DateTime.UtcNow.ToString("o");
                        default:
                            if (traceActions) tracesWriter.WriteLineFlush("getTag  ----  !!!! context '" + context + "' Unhandled '" + tag + "'");
                            break;
                    }
                    break;
                case "Integrity":
                    switch (context) {
                        // À partir de la version 1.0 du SEDA, la balise Integrity est dans Document
                        default:
                            break;
                        // Version 0.2 du SEDA : la balise Integrity se trouve au niveau de ArchiveTransfer
                        // et son contenu n'est pas précisé
                        case "/ArchiveTransfer":
                            /*
                            <Contains algorithme="[currentHashURI]">52a354f92d4d8a1e1c714ec6cd6a6f1ae51f4a14</Contains>
                            <UnitIdentifier>056-225600014-20130924-0000008888-DE-1-1_1.PDF</UnitIdentifier>
                            */
                            // C'est pas très propre, mais s'il y a plus d'un document, on doit générer la balise "Integrity"
                            // autant de fois qu'il y a de documents, MAIS on doit tenir compte du fait que cette balise
                            // est générée une fois par le code appelant...
                            int nbDocuments = archiveDocuments.prepareCompleteList();
                            int curDocument = 0;
                            while (archiveDocuments.nextDocument()) {
                                docOut.WriteStartElement("Contains");
                                docOut.WriteAttributeString("algorithme", getCurrentHashURI());
                                docOut.WriteString(computeHashOfCurrentDocument());
                                docOut.WriteEndElement();
                                docOut.WriteStartElement("UnitIdentifier");
                                docOut.WriteString(archiveDocuments.getDocumentFilename());
                                docOut.WriteEndElement();
                                ++curDocument;
                                if (curDocument < nbDocuments) {
                                    docOut.WriteEndElement();
                                    docOut.WriteStartElement("Integrity");
                                }
                            }
                            break;
                        }
                    return "";
                default:
                    if (traceActions) tracesWriter.WriteLineFlush("getTag  ----  !!!! tag Unhandled '" + tag + "'");
                    break;
            }
            return null;
        }

        /* 
         * Computes the hash of the current document
         * */
        private String computeHashOfCurrentDocument() {
            String documentHash = String.Empty;
            documentHash = archiveDocuments.getDocumentHash();
            if (documentHash == String.Empty) {
                String file = SAE_FilePath + "/" + archiveDocuments.getDocumentFilename();
                try {
                    documentHash = Utils.computeSha256Hash(file);
                } catch (DirectoryNotFoundException e) {
                    errorsList.Add("#DATAERR: Integrity: répertoire '" + SAE_FilePath + "' inexistant. " + e.Message);
                    if (traceActions) tracesWriter.WriteLineFlush("#DATAERR: Integrity: répertoire '" + SAE_FilePath + "' inexistant. " + e.Message);
                } catch (FileNotFoundException e) {
                    errorsList.Add("#DATAERR: Integrity: Fichier '" + file + "' inexistant. " + e.Message);
                    if (traceActions) tracesWriter.WriteLineFlush("#DATAERR: Integrity: Fichier '" + file + "' inexistant. " + e.Message);
                }
            }
            return documentHash;
        }

        /*
         * Computes the size of the current document
         * */
        private double computeSizeOfCurrentDocument() {
            String dataString = String.Empty;
            double sizeOfDocument = 0L;
            Boolean sizeIsOK = false;
            String sizeStr = archiveDocuments.getDocumentSize();
            if (sizeStr != String.Empty) {
                try {
                    sizeOfDocument = Convert.ToDouble(sizeStr);
                    sizeIsOK = true;
                } catch (Exception e) {
                    String errorMsg = "#DATAERR: la taille '" + sizeStr + "' du fichier '" +
                        archiveDocuments.getDocumentFilename() + "' n'est pas une taille conforme. " + e.Message;
                    errorsList.Add(errorMsg);
                    if (traceActions) tracesWriter.WriteLineFlush(errorMsg);
                }
            }
            if (sizeIsOK == false) {
                try {
                    FileInfo f = new FileInfo(SAE_FilePath + "/" + archiveDocuments.getDocumentFilename());
                    sizeOfDocument = f.Length;
                } catch (System.IO.FileNotFoundException e) { // on se contente de ne pas calculer
                    e.ToString();
                }
            }
            if (traceActions) tracesWriter.WriteLineFlush("Size computed = '" + sizeOfDocument + "'");
            return sizeOfDocument;
        }

        /*
         * Computes the date of the current document
         * */
        private String computeDateOfCurrentDocument() {
            String dateString = String.Empty;
            DateTime date;
            Boolean dateIsOK = false;
            String dateStrTemp = archiveDocuments.getDocumentDate();
            if (dateStrTemp != String.Empty) {
                try {
                    date = DateTime.Parse(dateStrTemp, new System.Globalization.CultureInfo("fr-FR", false));
                    dateString = date.Date.ToString("o");
                    dateIsOK = true;
                } catch (FormatException e) {
                    String errorMsg = "#DATAERR: La date '" + dateStrTemp + "' du document '" +
                        archiveDocuments.getDocumentFilename() + "' ne correspond pas à une date réelle ou son format est incorrect. Format attendu JJ/MM/AAAA hh:mm:ss";
                    errorsList.Add(errorMsg);
                    if (traceActions) tracesWriter.WriteLineFlush(errorMsg);
                }
            }
            if (dateIsOK == false) {
                try {
                    FileInfo f = new FileInfo(SAE_FilePath + "/" + archiveDocuments.getDocumentFilename());
                    date = f.CreationTime;
                    dateString = date.Date.ToString("o");
                    dateIsOK = true;
                } catch (System.IO.FileNotFoundException e) { // on se contente de ne pas calculer
                    e.ToString();
                }
            }
            if (traceActions) tracesWriter.WriteLineFlush("Size computed = '" + dateString + "'");
            return dateString;
        }

        /*
         * Formats the size for an XmlNode
         * */
        private String formatSizeForNode(double sizeOfDocument, XmlNode node) {
            String dataString;
            String value = lookupForAttribute("unitCode", node.ParentNode);
            switch (value) {
                case "E36": sizeOfDocument /= (1024L ^ 5L); break; // petabyte
                case "E35": sizeOfDocument /= (1024L ^ 4L); break; // terabyte
                case "E34": sizeOfDocument /= (1024L ^ 3L); break; // gigabyte
                case "4L": sizeOfDocument /= (1024L ^ 2L); break; // megabyte
                case "2P": sizeOfDocument /= 1024L; break; // kilobyte
                case "null":
                case "AD":
                    break;
            }
            dataString = String.Format("{0:0.##}", (long)(sizeOfDocument + 0.5));

            return dataString;
        }

        private String formatContainsIdentifier(String containsIdentifier, int numeroTag) {
            // TODO: containsIdentifier - '+' + '[' + numéro d'ordre + ']'
            return containsIdentifier.Substring(0, containsIdentifier.Length - 1) + "[#" + numeroTag.ToString("D") + "]";
        }

        override public void close() {
            if (currentPass == 2) {
                // Save the document to a file.
                docOut.Flush();
                if (traceActions) tracesWriter.Flush();
                docOut.Close();
            }
        }
    }
}
