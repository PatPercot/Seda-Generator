using System;
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

        GeneratorConfig configLoader(String configName) {
            SimpleConfig config = new SimpleConfig();
            String erreur = config.loadFile("./job.config");
            if (erreur != String.Empty) // on tient compte du fait qu'en environnement de développement, l'exe est dans bin/Release
                erreur = config.loadFile("../../job.config");

            if (erreur != String.Empty) {
                System.Console.WriteLine(erreur);
                Assert.Fail(erreur);
            }

            return config.getGeneratorConfig(configName);
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
                errors.Add("Erreur lors de la préparation du bordereau pour le test '" + fichier_bordereau + "' " + ex.GetType().Name);
            };

            GeneratorConfig control = configLoader(jobName);

            accordVersement = control.accordVersement;
            fichier_metier = control.dataFile;
            path_datafiles = control.repDocuments;
            fichier_bordereau = control.bordereauFile;
            traceFile = control.traceFile;
            baseURI = control.baseURI;

            informationsDatabase = ConfigurationManager.AppSettings["databaseConnexion"];

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            ssg = new SedaSummaryRngGenerator();
            ssg.setTracesWriter(streamWriter);

            ssg.prepareInformationsWithDatabase(informationsDatabase, baseURI, accordVersement);

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

            streamWriter.Close();

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
                , "fb20d26a36b8368ea31695298ca0222a31968847");

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
                , "fb20d26a36b8368ea31695298ca0222a31968847");
        }

        [TestMethod]
        public void W02_TestGenerateur_2_1_01() {
            executeGenerator("liste-fichiers_2-1-01", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");

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

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // TODO: À rediscuter, ce test ne semble pas cohérent avec les erreurs qui sont détectées.
        // TODO: De plus, je ne sais pas quoi vérifier
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [TestMethod]
        public void W04_TestGenerateur_2_1_03() {
            executeGenerator("liste-fichiers_2-1-03", "0.2");

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

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // TODO: À rediscuter, ce test est en échec, mais il me semble que le résultat attendu ne
        // TODO: peut pas être obtenu, car le générateur recherche un tag non numéroté or il l'est
        // TODO: dans les données.
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [TestMethod]
        public void W10_TestGenerateur_2_3_03() {
            executeGenerator("liste-fichiers_2-3-03", "0.2");

            checkForNoErrors();
            checkInnerText("/s:ArchiveTransfer/s:Comment"
               , "Transfert de pièces de marché public de la salle régionale des marchés publics marches.e-megalisbretagne.org. La procédure dématérialisée pouvant ne pas être complète, certaines pièces du dossier n'existent qu'au format papier (notification, registres, courriers, offres, etc.)");
            // On vérifie que l'unité documentaire existe
            checkExists("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Docs_Ext']");
            // On vérifie qu'il y a bien une première unité documentaire avec un document
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Docs_Ext']/../../s:Contains[4]/s:Document[1]/s:Attachment", "filename", "MP_Cons_Dossier_Docs_Ext_1.pdf");
            // On vérifie qu'il y a bien une première unité documentaire avec un document
            checkAttribute("//s:ArchivalAgencyObjectIdentifier[@schemeID='CG56_DOCLIST_2015 / MP_Cons_Dossier_Docs_Ext']/../../s:Contains[5]/s:Document[1]/s:Attachment", "filename", "MP_Cons_Dossier_Docs_Ext_2.pdf");
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


    }
}
