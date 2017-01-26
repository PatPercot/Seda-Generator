using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SedaSummaryGenerator;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Specialized;
using System.Configuration;
using CommonClassesLibrary;

namespace SedaSummaryGeneratorUnitTest {
    [TestClass]
    public class SedageneratorUnitTest {
        // Variables de classe permettant de paramétrer le lancement d'une génération
        // de bordereau de versement
        StreamWriter streamWriter = null;

        String accordVersement;
        String fichier_metier;
        String path_datafiles;
        String fichier_bordereau;
        String traceFile;
        String baseURI;
        // String namespace_seda;

        String informationsDatabase;
        StringCollection errors;
        SedaSummaryGenerator.SedaSummaryGenerator ssg;
        XmlDocument docBordereau;
        XmlNamespaceManager docInXmlnsManager;
        String namespaceSEDA02 = "fr:gouv:ae:archive:draft:standard_echange_v0.2";
        String namespaceSEDA10 = "fr:gouv:culture:archivesdefrance:seda:v1.0";

        protected void checkForErrors(String[] erreursAttendues) {
            // Début de vérification de bon déroulement
            errors = ssg.getErrorsList();
            Assert.IsNotNull(errors, "La liste d'erreurs rencontrées doit exister");
            Assert.AreEqual(erreursAttendues.Length, errors.Count, "Le nombre d'erreurs attendues et rencontrées ne correspond pas");
            int numerror = 0;
            foreach (String error in errors) {
                Assert.AreEqual(erreursAttendues[numerror], error, "Comparaison des erreurs");
                numerror++;
            }
        }

        protected void checkForNoErrors() {
            // Début de vérification de bon déroulement
            errors = ssg.getErrorsList();
            Assert.AreEqual(false, errors != null && errors.Count != 0, "Aucune erreur ne doit avoir été détectée");
            int numerror = 0;
            if (errors != null && errors.Count != 0) {
                Assert.AreEqual("", errors[numerror], "Cette erreur n'aurait pas dû se produire");
                numerror++;
            }
            // Si on ne s'attend pas à des erreurs et qu'il n'y en a pas eu, le bordereau doit être créé
            Assert.IsNotNull(docBordereau, "Le fichier de bordereau de transfert aurait dû être créé");
        }

        protected void checkAttribute(String xPath, String attribute, String contentToCheck) {
            XmlNode node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            String content = node.Attributes.GetNamedItem(attribute).InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath 
                + "' doit avoir un attribut '" + attribute 
                + "' = à '" + contentToCheck 
                + "' au lieu de '" + content + "'");
        }

        protected void checkAttributeInList(String xPath, String attribute, String value) {
            XmlNodeList nodeList = docBordereau.SelectNodes(xPath, docInXmlnsManager);
            Assert.AreNotEqual(0, nodeList.Count, "Le nœud '" + xPath + "' devrait exister");
            Boolean found = false;
            XmlNode attr;
            foreach (XmlNode node in nodeList) {
                attr = node.Attributes.GetNamedItem(attribute);
                if (attr != null && value.Equals(attr.InnerText))
                    found = true;
            }
            Assert.AreNotEqual(false, found, "Il devrait exister un nœud '" + xPath + "' dont l'attribut '"
                + attribute + "' " );
        }

        protected void checkInnerText(String xPath, String contentToCheck) {
            XmlNode node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            String content = node.InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath + "' doit contenir '" + contentToCheck + "' au lieu de '" + content + "'");
        }

        protected void checkExists(String xPath) {
            XmlNode node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
        }

        protected void checkNotExists(String xPath) {
            XmlNode node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNull(node, "Le nœud '" + xPath + "' ne devrait pas exister");
        }

        protected void executeGenerator(String jobName, String sedaVersion) {
            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };
            Action<Exception> ehb = (ex) => {
                streamWriter.WriteLine("Erreur lors de la préparation du bordereau pour le test '" + fichier_bordereau + "' " + ex.GetType().Name);
            };

            SimpleConfig config = new SimpleConfig();
            String erreur = config.loadFile("./job.config");
            if (erreur != String.Empty) // on tient compte du fait qu'en environnement de développement, l'exe est dans bin/Release
                erreur = config.loadFile("../../job.config");

            if (erreur != String.Empty) {
                System.Console.WriteLine(erreur);
                Assert.Fail(erreur);
            }

            GeneratorConfig control = config.getGeneratorConfig(jobName);

            accordVersement = control.accordVersement;
            fichier_metier = control.dataFile;
            path_datafiles = control.repDocuments;
            fichier_bordereau = control.bordereauFile;
            traceFile = control.traceFile;
            baseURI = control.baseURI;

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            ssg = new SedaSummaryRngGenerator();
            ssg.setTracesWriter(streamWriter);

            if (config.hasAccordVersementConfig()) {
                AccordVersementConfig accordVersementConfig = config.getAccordVersementConfig(accordVersement, baseURI);
                if (accordVersementConfig == null) {
                    Console.WriteLine("ATTENTION : Impossible de trouver l'accord de versement '" + accordVersement + "' dans la configuration");
                } else {
                    if (accordVersementConfig.SAE_ProfilArchivage.Length == 0)
                        Console.WriteLine("ATTENTION : Le profil d'archivage n'a pas de nom de fichier");
                }

                String dataSha1 = String.Empty;
                try {
                    dataSha1 = Utils.computeSha1Hash(fichier_metier);
                } catch (IOException e) {
                    // Ignorer les exceptions, car si le fichier de données n'est pas accessible, 
                    // une exception sera générée plus tard avec un contexte plus explicatif
                }

                ssg.prepareInformationsWithConfigFile(config, baseURI, accordVersement, dataSha1);
            } else {
                informationsDatabase = ConfigurationManager.AppSettings["databaseConnexion"];
                ssg.prepareInformationsWithDatabase(informationsDatabase, baseURI, accordVersement);
            }

            ssg.prepareArchiveDocumentsWithFile(path_datafiles, fichier_metier);

            ssg.generateSummaryFile(fichier_bordereau);

            ssg.close();

            streamWriter.WriteLine("\n---------- ERREURS ----------\n");
            StringCollection dumpErrors = ssg.getErrorsList();
            if (dumpErrors != null && dumpErrors.Count != 0) {
                foreach (String err in dumpErrors) {
                    streamWriter.WriteLine(err);
                }
            }
            streamWriter.WriteLine("\n---------- ^^^^^^^ ----------\n");

            docBordereau = new XmlDocument();
            try {
                using (StreamReader sr = new StreamReader(fichier_bordereau)) {
                    String line = sr.ReadToEnd();
                    //Console.WriteLine(line);
                    docBordereau.LoadXml(line);
                    //Instantiate an XmlNamespaceManager object. 
                    docInXmlnsManager = new System.Xml.XmlNamespaceManager(docBordereau.NameTable);
                    // Add the namespaces used in xml to the XmlNamespaceManager.
                    // docInXmlnsManager.AddNamespace(String.Empty, sedaVersion.Equals("1.0") ? namespaceSEDA10 : namespaceSEDA02);
                    docInXmlnsManager.AddNamespace("s", sedaVersion.Equals("1.0") ? namespaceSEDA10 : namespaceSEDA02);
                }
            }
            catch (ArgumentException e) { ehb(e); }
            catch (DirectoryNotFoundException e) { ehb(e); }
            catch (FileNotFoundException e) { ehb(e); }
            catch (OutOfMemoryException e) { ehb(e); }
            catch (IOException e) { ehb(e); }

            streamWriter.Close();
        }

        // Ce test correspond à la génération d'un bordereau au format SEDA 0.2, 
        // le test suivant vérifie avec les mêmes données la génération d'un bordereau au format SEDA 1.0
        [TestMethod]
        public void W00_TestRepetitionUneUniteSeda02() {
            executeGenerator("repetition_une_unite_seda02", "0.2");

            checkForNoErrors();
            checkAttribute("/s:ArchiveTransfer/s:Contains/s:Contains[1]/s:Contains[1]/s:ArchivalAgencyObjectIdentifier"
                , "schemeID", "DOCLIST / OR_ETP+");

            checkAttribute("/s:ArchiveTransfer/s:Contains/s:Contains[1]/s:Contains[1]/s:Document[2]/s:Attachment"
                , "filename", "document2e1.txt");

            checkInnerText("/s:ArchiveTransfer/s:Integrity[1]/s:UnitIdentifier"
                , "document1e1.txt");

            checkInnerText("/s:ArchiveTransfer/s:Integrity[1]/s:Contains"
                , "f81ba5573d70bb23c5510237208e2965bd87a389623c985cff341879e373c4b7");

        }


        // Ce test correspond à la génération d'un bordereau au format SEDA 1.0, 
        // le test précédent vérifie avec les mêmes données la génération d'un bordereau au format SEDA 0.2
        [TestMethod]
        public void W01_TestRepetitionUneUniteSeda10() {
            executeGenerator("repetition_une_unite_seda10", "1.0");

            checkForNoErrors();
            checkAttribute("/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:ArchivalAgencyObjectIdentifier"
                , "schemeID", "DOCLIST / OR_ETP+");

            checkAttribute("/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:Document[1]/s:Attachment"
                , "filename", "document1e1.txt");

            checkInnerText("/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:Document[1]/s:Integrity"
                , "f81ba5573d70bb23c5510237208e2965bd87a389623c985cff341879e373c4b7");
        }

        [TestMethod]
        public void W02_TestGenerateur_2_1_01() {
            executeGenerator("liste-fichiers_2-1-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");

            checkInnerText("/s:ArchiveTransfer/s:Contains/s:ContentDescription/s:LatestDate", "2014-12-27");
            checkInnerText("/s:ArchiveTransfer/s:Contains/s:ContentDescription/s:OldestDate", "2014-12-01");
            checkInnerText("/s:ArchiveTransfer/s:Contains/s:Appraisal/s:StartDate", "2014-12-27");
            
            /* Ce test et le suivant sont identiques */
            checkAttributeInList("//s:ArchivalAgencyObjectIdentifier"
                , "schemeID", "CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_Reponse");
            /* Ce test et le précédent sont identiques */
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_Reponse']");

            checkAttributeInList("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_Reponse']/../../s:Contains[1]/s:Document[1]/s:Attachment"
                , "filename", "MP_OetD_Analyse_Recept_ONR1_Reponse_1_1_1.pdf");
        }

        [TestMethod]
        public void W03_TestGenerateur_2_1_02() {
            executeGenerator("liste-fichiers_2-1-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");

            /* Ce test et le suivant sont identiques */
            checkAttributeInList("//s:ArchivalAgencyObjectIdentifier"
                , "schemeID", "CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_Reponse");
            /* Ce test et le précédent sont identiques */
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_Reponse']");

            checkNotExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_Reponse']/../../s:Contains[1]/s:Document[1]/s:Attachment");
        }

        [TestMethod]
        public void W04_TestGenerateur_2_1_03() {
            executeGenerator("liste-fichiers_2-1-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");

            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier']");

            checkNotExists("//s:Attachment[@filename='depot_DCE_horodatage.xml']");
            checkNotExists("//s:Attachment[@filename='depot_DCE_horodatage_2.xml']");
        }

        [TestMethod]
        public void W05_TestGenerateur_2_2_01() {
            executeGenerator("liste-fichiers_2-2-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_DAD_DocsExts+']");
            // On vérifie qu'il y a bien une première unité documentaire avec un document
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_DAD_DocsExts+']/../../s:Contains[1]/s:Document[1]/s:Attachment");
            // On vérifie qu'il n'y a pas de seconde unité documentaire avec un document
            checkNotExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_DAD_DocsExts+']/../../s:Contains[2]/s:Document[1]/s:Attachment");
        }

        [TestMethod]
        public void W06_TestGenerateur_2_2_02() {
            executeGenerator("liste-fichiers_2-2-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_DAD_DocsExts+']");
            // On vérifie qu'il y a bien une première unité documentaire avec un document
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_DAD_DocsExts+']/../../s:Contains[1]/s:Document[1]/s:Attachment", "filename", "MP_OetD_DAD_DocsExts_1.pdf");
            // On vérifie qu'il y a bien une seconde unité documentaire avec un document
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_DAD_DocsExts+']/../../s:Contains[2]/s:Document[1]/s:Attachment", "filename", "MP_OetD_DAD_DocsExts_2.pdf");
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // TODO: À rediscuter, est-ce qu'il ne serait pas préférable d'avoir une erreur générée
        // TODO: Par contre, il n'est pas évident de déterminer le niveau de difficulté
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [TestMethod]
        public void W07_TestGenerateur_2_2_03() {
            executeGenerator("liste-fichiers_2-2-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire n'existe pas
            checkNotExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_DAD_DocsExts+']");
        }

        [TestMethod]
        public void W08_TestGenerateur_2_3_01() {
            executeGenerator("liste-fichiers_2-3-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Docs_Ext']");
            // On vérifie qu'il y a bien une première unité documentaire avec un document
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Docs_Ext']/../../s:Contains[4]/s:Document[1]/s:Attachment", "filename", "MP_Cons_Dossier_Docs_Ext_1.pdf");
        }

        [TestMethod]
        public void W09_TestGenerateur_2_3_02() {
            executeGenerator("liste-fichiers_2-3-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkNotExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Docs_Ext']");
        }

        [TestMethod]
        public void W10_TestGenerateur_2_3_03() {
            executeGenerator("liste-fichiers_2-3-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire n'existe pas
            checkNotExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Docs_Ext']");
        }

        [TestMethod]
        public void W11_TestGenerateur_2_4_01() {
            executeGenerator("liste-fichiers_2-4-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']");
            // On vérifie qu'il y a bien une première unité documentaire avec deux documents
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']/../../s:Contains[3]/s:Document[1]/s:Attachment", "filename", "MP_OetD_Admissibilite_Compl2_1_1.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']/../../s:Contains[3]/s:Document[2]/s:Attachment", "filename", "MP_OetD_Admissibilite_Compl2_1_2.pdf");
        }

        [TestMethod]
        public void W12_TestGenerateur_2_4_02() {
            executeGenerator("liste-fichiers_2-4-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']");
            // On vérifie qu'il y a bien une première unité documentaire avec deux documents
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']/../../s:Contains[3]/s:Document[1]/s:Attachment", "filename", "MP_OetD_Admissibilite_Compl2_1_1.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']/../../s:Contains[3]/s:Document[2]/s:Attachment", "filename", "MP_OetD_Admissibilite_Compl2_1_2.pdf");
            // On vérifie qu'il y a bien une seconde unité documentaire avec deux documents
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']/../../s:Contains[4]/s:Document[1]/s:Attachment", "filename", "MP_OetD_Admissibilite_Compl2_2_1.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']/../../s:Contains[4]/s:Document[2]/s:Attachment", "filename", "MP_OetD_Admissibilite_Compl2_2_2.pdf");
        }

        [TestMethod]
        public void W13_TestGenerateur_2_4_03() {
            executeGenerator("liste-fichiers_2-4-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkNotExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Admissibilite_Compl2+']");
        }

        [TestMethod]
        public void W20_TestGenerateur_4_1_01() {
            executeGenerator("liste-fichiers_4-1-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']/../s:Attachment", "filename", "depot_DCE_horodatage.xml");
            checkAttribute("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment", "filename", "RC.pdf");
        }

        [TestMethod]
        public void W21_TestGenerateur_4_1_02() {
            executeGenerator("liste-fichiers_4-1-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']");
            checkExists("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']/../s:Attachment", "filename", "depot_DCE_horodatage.xml");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']/../s:Attachment", "filename", "depot_DCE_horodatage_2.xml");
            checkAttribute("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment", "filename", "RC.pdf");
        }

        [TestMethod]
        public void W22_TestGenerateur_4_1_03() {
            executeGenerator("liste-fichiers_4-1-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Document/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']");
            checkExists("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            checkExists("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']/../s:Attachment", "filename", "depot_DCE_horodatage.xml");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment", "filename", "RC.pdf");
            checkAttribute("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment", "filename", "RC_2.pdf");
        }

        [TestMethod]
        public void W23_TestGenerateur_4_1_04() {
            executeGenerator("liste-fichiers_4-1-04", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']");
            checkExists("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']");
            checkExists("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            checkExists("//s:Document[4]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']/../s:Attachment", "filename", "depot_DCE_horodatage.xml");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']/../s:Attachment", "filename", "depot_DCE_horodatage_2.xml");
            checkAttribute("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment", "filename", "RC.pdf");
            checkAttribute("//s:Document[4]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment", "filename", "RC_2.pdf");
        }

        [TestMethod]
        public void W24_TestGenerateur_4_1_05() {
            executeGenerator("liste-fichiers_4-1-05", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']");
            checkNotExists("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']/../s:Attachment", "filename", "depot_DCE_horodatage.xml");
        }

        [TestMethod]
        public void W25_TestGenerateur_4_1_06() {
            executeGenerator("liste-fichiers_4-1-06", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkNotExists("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Horodatage']");
            checkNotExists("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
        }

        [TestMethod]
        public void W26_TestGenerateur_4_2_01() {
            executeGenerator("liste-fichiers_4-2-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Horodatage']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']/../s:Attachment", "filename", "DCE_v0.1.pdf");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Horodatage']/../s:Attachment", "filename", "DCE_v0.1_horodatage.xml");
        }

        [TestMethod]
        public void W27_TestGenerateur_4_2_02() {
            executeGenerator("liste-fichiers_4-2-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Horodatage']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']/../s:Attachment", "filename", "DCE_v0.1.pdf");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']/../s:Attachment", "filename", "DCE_v0.2.pdf");
            checkAttribute("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Horodatage']/../s:Attachment", "filename", "DCE_v0.1_horodatage.xml");
            checkAttribute("//s:Document[4]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Horodatage']/../s:Attachment", "filename", "DCE_v0.2_horodatage.xml");
        }

        [TestMethod]
        public void W28_TestGenerateur_4_2_03() {
            executeGenerator("liste-fichiers_4-2-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']");
            checkNotExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Horodatage']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']/../s:Attachment", "filename", "DCE_v0.1.pdf");
        }

        [TestMethod]
        public void W29_TestGenerateur_4_2_04() {
            executeGenerator("liste-fichiers_4-2-04", "0.2");

            String[] erreursAttendues = { "Il existe des unités documentaires obligatoires sans documents : MP_Cons_Dossier_AncDCE\t" };
            checkForErrors(erreursAttendues);
        }

        [TestMethod]
        public void W30_TestGenerateur_4_3_01() {
            executeGenerator("liste-fichiers_4-3-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Acte_1_1.pdf");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Signature_1_1.xml");
        }

        [TestMethod]
        public void W31_TestGenerateur_4_3_02() {
            executeGenerator("liste-fichiers_4-3-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']");
            checkNotExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Acte_1_1.pdf");
        }

        [TestMethod]
        public void W32_TestGenerateur_4_3_03() {
            executeGenerator("liste-fichiers_4-3-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkNotExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']");
            checkNotExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']");
        }

        [TestMethod]
        public void W33_TestGenerateur_4_3_04() {
            executeGenerator("liste-fichiers_4-3-04", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Acte_1_1.pdf");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Acte_2_1.pdf");
            checkAttribute("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Signature_1_1.xml");
        }

        [TestMethod]
        public void W34_TestGenerateur_4_3_05() {
            executeGenerator("liste-fichiers_4-3-05", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']");
            // On vérifie qu'il y a bien une unité documentaire avec le document
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Acte_1_1.pdf");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Acte']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Acte_2_1.pdf");
            checkAttribute("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Signature_1_1.xml");
            checkAttribute("//s:Document[4]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_OetD_Analyse_Recept_ONR1_AE_Signature']/../s:Attachment", "filename", "MP_OetD_Analyse_Recept_ONR1_AE_Signature_2_1.xml");
        }

        [TestMethod]
        public void W35_TestGenerateur_4_4_01() {
            executeGenerator("liste-fichiers_4-4-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange_PJ_Msg']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange_Horodatage']");

            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']");

            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[1]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_1_1.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[2]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_1_1_Horodatage.xml");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../../s:Contains[2]/s:Document[1]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_2_1.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../../s:Contains[2]/s:Document[2]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_2_1_Horodatage.xml");
        }

        [TestMethod]
        public void W36_TestGenerateur_4_4_02() {
            executeGenerator("liste-fichiers_4-4-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange_PJ_Msg']");
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange_Horodatage']");

            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']");

            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[1]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_1_1.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[2]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_1_2.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[3]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_1_1_Horodatage.xml");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[4]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_1_2_Horodatage.xml");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../../s:Contains[2]/s:Document[1]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_2_1.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../../s:Contains[2]/s:Document[2]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_2_1_Horodatage.xml");
        }

        [TestMethod]
        public void W37_TestGenerateur_4_4_03() {
            executeGenerator("liste-fichiers_4-4-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange_PJ_Msg']");
            checkNotExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange_Horodatage']");

            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']");

            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[1]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_1_1.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[2]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_1_2.pdf");
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../../s:Contains[2]/s:Document[1]/s:Attachment"
                , "filename", "MP_Cons_Msg_Echange_Msg_2_1.pdf");
        }

        [TestMethod]
        public void W38_TestGenerateur_4_4_04() {
            executeGenerator("liste-fichiers_4-4-04", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkNotExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange_PJ_Msg']");
            checkNotExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange_Horodatage']");
            // Il y a d'autres documents dans l'unité documentaire
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']");
        }

        [TestMethod]
        public void W39_TestGenerateur_3_1_01() {
            executeGenerator("liste-fichiers_3-1-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            // On vérifie que le nom de fichier est le bon
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment"
                , "filename", "RC.pdf");
        }

        [TestMethod]
        public void W40_TestGenerateur_3_1_02() {
            executeGenerator("liste-fichiers_3-1-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkNotExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
        }

        [TestMethod]
        public void W41_TestGenerateur_3_1_03() {
            executeGenerator("liste-fichiers_3-1-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']");
            // On vérifie qu'il y a bien deux fichiers
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment"
                , "filename", "RC.pdf");
            checkAttribute("//s:Document[3]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_RC']/../s:Attachment"
                , "filename", "RC2.pdf");
        }

        [TestMethod]
        public void W42_TestGenerateur_3_2_01() {
            executeGenerator("liste-fichiers_3-2-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']");
            // On vérifie que le nom de fichier est le bon
            checkAttribute("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']/../s:Attachment"
                , "filename", "DCE_v0.1.pdf");
        }

        [TestMethod]
        public void W43_TestGenerateur_3_2_02() {
            executeGenerator("liste-fichiers_3-2-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']");
            // On vérifie que les deux fichiers existent
            checkAttribute("//s:Document[1]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']/../s:Attachment"
                , "filename", "DCE_v0.1.pdf");
            checkAttribute("//s:Document[2]/s:Identification[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_AncDCE_Fichier']/../s:Attachment"
                , "filename", "DCE_v0.2.pdf");
        }

        [TestMethod]
        public void W44_TestGenerateur_3_2_03() {
            executeGenerator("liste-fichiers_3-2-03", "0.2");

            String[] erreursAttendues = { "Il existe des unités documentaires obligatoires sans documents : MP_Cons_Dossier_AncDCE\t" };
            checkForErrors(erreursAttendues);
        }

        [TestMethod]
        public void W45_TestGenerateur_3_3_01() {
            executeGenerator("liste-fichiers_3-3-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']");

            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document/s:Attachment[@filename='MP_Cons_Msg_Echange_Msg_1_1_Horodatage.xml']");
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[2]/s:Attachment[@filename='MP_Cons_Msg_Echange_Msg_2_1_Horodatage.xml']");
        }

        [TestMethod]
        public void W46_TestGenerateur_3_3_02() {
            executeGenerator("liste-fichiers_3-3-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']");

            checkNotExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document/s:Attachment[@filename='MP_Cons_Msg_Echange_Msg_1_1_Horodatage.xml']");
        }

        [TestMethod]
        public void W47_TestGenerateur_3_3_03() {
            executeGenerator("liste-fichiers_3-3-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']");

            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document/s:Attachment[@filename='MP_Cons_Msg_Echange_Msg_1_1_Horodatage.xml']");
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document/s:Attachment[@filename='MP_Cons_Msg_Echange_Msg_1_2_Horodatage.xml']");
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_Echange+']/../s:Document[2]/s:Attachment[@filename='MP_Cons_Msg_Echange_Msg_2_1_Horodatage.xml']");
        }

        [TestMethod]
        public void W48_TestGenerateur_3_4_01() {
            executeGenerator("liste-fichiers_3-4-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_DocsExt']");

            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_DocsExt']/../s:Document/s:Attachment[@filename='MP_Cons_Msg_DocsExt_1.pdf']");
        }

        [TestMethod]
        public void W49_TestGenerateur_3_4_02() {
            executeGenerator("liste-fichiers_3-4-02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_DocsExt']");

            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_DocsExt']/../s:Document/s:Attachment[@filename='MP_Cons_Msg_DocsExt_1.pdf']");
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_DocsExt']/../s:Document/s:Attachment[@filename='MP_Cons_Msg_DocsExt_2.pdf']");
        }

        [TestMethod]
        public void W50_TestGenerateur_3_4_03() {
            executeGenerator("liste-fichiers_3-4-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que le document existe
            checkNotExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Msg_DocsExt']");
        }

        [TestMethod]
        public void W51_TestGenerateur_6_1() {
            executeGenerator("06_Chapitre1", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W52_TestGenerateur_6_2_01() {
            executeGenerator("06_Chapitre2-01", "0.2");
            String[] erreursAttendues = { "#DATAERR: Le tag : '#TransferName' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W53_TestGenerateur_6_2_02() {
            executeGenerator("06_Chapitre2-02", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#Comment' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
        }

        [TestMethod]
        public void W54_TestGenerateur_6_2_03() {
            executeGenerator("06_Chapitre2-03", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#OriginatingAgency.BusinessType' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W55_TestGenerateur_6_2_04() {
            executeGenerator("06_Chapitre2-04", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#OriginatingAgency.Identification' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W56_TestGenerateur_6_2_05() {
            executeGenerator("06_Chapitre2-05", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#OriginatingAgency.Description' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W57_TestGenerateur_6_2_06() {
            executeGenerator("06_Chapitre2-06", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#OriginatingAgency.LegalClassification' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W58_TestGenerateur_6_2_07() {
            executeGenerator("06_Chapitre2-07", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#OriginatingAgency.Name' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W59_TestGenerateur_6_2_08() {
            executeGenerator("06_Chapitre2-08", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#CustodialHistory' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W60_TestGenerateur_6_2_09() {
            executeGenerator("06_Chapitre2-09", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#KeywordContent[#1]' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W61_TestGenerateur_6_2_10() {
            executeGenerator("06_Chapitre2-10", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#KeywordContent[O6_Chapitre1[#1]]' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W62_TestGenerateur_6_2_11() {
            executeGenerator("06_Chapitre2-11", "0.2");

            String[] erreursAttendues = { "#DATAERR: Le tag : '#ContainsName[O6_Chapitre1]' n'a pas été trouvé dans les données métier" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W63_TestGenerateur_6_2_12() {
            executeGenerator("06_Chapitre2-12", "0.2");

            String[] erreursAttendues = { "#DATAERR: La date '1er juin 2013 12:00:00' du document 'Document1.pdf' ne correspond pas à une date réelle ou son format est incorrect. Format attendu JJ/MM/AAAA hh:mm:ss" };
            checkForErrors(erreursAttendues);
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Commentaire sur les tests de l'onglet 6");
        }

        [TestMethod]
        public void W64_TestGenerateur_empreinte_taille_seda02() {
            executeGenerator("empreinte_taille_seda02", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Integrity[1]/s:UnitIdentifier"
                , "document1e1.txt");

            checkInnerText("/s:ArchiveTransfer/s:Integrity[1]/s:Contains"
                , "Integrity en provenance application 1");

            checkInnerText("/s:ArchiveTransfer/s:Contains/s:ContentDescription/s:Size"
                , "3999996");

            checkInnerText("/s:ArchiveTransfer/s:Contains/s:Contains[1]/s:Contains[1]/s:ContentDescription/s:Size"
                , "333333");
        }

        [TestMethod]
        public void W65_TestGenerateur_empreinte_taille_seda10() {
            executeGenerator("empreinte_taille_seda10", "1.0");

            checkForNoErrors();
            checkAttribute("/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:Document[1]/s:Attachment"
                , "filename", "document1e1.txt");

            checkInnerText("/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:Document[1]/s:Integrity"
                , "Integrity en provenance application 1");

            checkInnerText("/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:Document[1]/s:Size"
                , "111111");
        }

        // Ce test correspond à la génération d'un bordereau au format SEDA 0.2, 
        // le test suivant vérifie avec les mêmes données la génération d'un bordereau au format SEDA 1.0
        [TestMethod]
        public void W66_TestDateManquanteSansErreur() {
            executeGenerator("date_manquante_sans_erreur", "0.2");

            checkForNoErrors();

        }

        // Ce test correspond à la génération d'un bordereau au format SEDA 0.2, 
        // le test suivant vérifie avec les mêmes données la génération d'un bordereau au format SEDA 1.0
        [TestMethod]
        public void W67_TestMultipleDocumentsV02() {
            executeGenerator("multiple_documents_V02", "0.2");

            checkForNoErrors();
            checkAttribute("/s:ArchiveTransfer/s:Contains/s:Contains/s:Document[1]/s:Attachment"
                , "filename", "document1e1.txt");

            checkAttribute("/s:ArchiveTransfer/s:Contains/s:Contains//s:Document[2]/s:Attachment"
                , "filename", "document2e1.txt");


        }


        // Ce test correspond à la génération d'un bordereau au format SEDA 1.0, 
        [TestMethod]
        public void W68_TestMultipleDocumentsV10() {
            executeGenerator("multiple_documents_V10", "1.0");

            checkForNoErrors();
            checkAttribute("/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:Document[1]/s:Attachment"
                , "filename", "document1e1.txt");

            checkAttribute("/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:Document[2]/s:Attachment"
                , "filename", "document2e1.txt");


        }



    }
}
