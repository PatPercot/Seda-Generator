using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SedaSummaryGenerator;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Specialized;
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

        protected void executeGenerator(String sedaVersion) {
            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };
            Action<Exception> ehb = (ex) => {
                errors.Add("Erreur lors de la préparation du bordereau pour le test '" + fichier_bordereau + "' " + ex.GetType().Name);
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            ssg = new SedaSummaryRngGenerator();
            ssg.setTracesWriter(streamWriter);

            ssg.prepareInformationsWithDatabase(informationsDatabase, baseURI, accordVersement);

            ssg.prepareArchiveDocumentsWithFile(path_datafiles, fichier_metier);

            ssg.generateSummaryFile(fichier_bordereau);

            ssg.close();
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
        public void TestRepetitionUneUniteSeda02() {
            streamWriter = null;
            String dir;
            dir = Directory.GetCurrentDirectory();
            System.Console.WriteLine(dir);
            GeneratorConfig control = configLoader("repetition_une_unite_seda02");

            accordVersement = control.accordVersement;
            fichier_metier = control.dataFile;
            path_datafiles = control.repDocuments;
            fichier_bordereau = control.bordereauFile;
            traceFile = control.traceFile;
            baseURI = control.baseURI;
            // namespace_seda    = "fr:gouv:ae:archive:draft:standard_echange_v0.2";

            informationsDatabase = "Server=VM-PSQL02\\OUTILS_P;Database=BW_DEV;User=App-Blueway-Exploitation;Password=Pi=3.14159;";

            executeGenerator("0.2");

            // Début de vérification de bon déroulement
            errors = ssg.getErrorsList();
            Assert.AreEqual(false, errors != null && errors.Count != 0, "Aucune erreur ne doit avoir été détectée");
            int numerror = 0;
            if (errors != null && errors.Count != 0) {
                Assert.AreEqual("", errors[numerror], "Cette erreur n'aurait pas dû se produire");
                numerror++;
            }

            String xPath;
            XmlNodeList nodeList = null; // nodeList = docBordereau.SelectNodes(xPath);
            XmlNode node;
            String contentToCheck, content;

            xPath = "/s:ArchiveTransfer/s:Contains/s:Contains[1]/s:Contains[1]/s:ArchivalAgencyObjectIdentifier";
            node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            contentToCheck = "DOCLIST / OR_ETP+";
            content = node.Attributes.GetNamedItem("schemeID").InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath + "' doit avoir un attribut schemeID = à '" + contentToCheck + "' au lieu de '" + content +"'");

            xPath = "/s:ArchiveTransfer/s:Contains/s:Contains[1]/s:Contains[1]/s:Document[2]/s:Attachment";
            node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            contentToCheck = "document2e1.txt";
            content = node.Attributes.GetNamedItem("filename").InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath + "' doit avoir un attribut filename = à '" + contentToCheck + "' au lieu de '" + content + "'");

            xPath = "/s:ArchiveTransfer/s:Integrity[1]/s:UnitIdentifier";
            node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            contentToCheck = "document1e1.txt";
            content = node.InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath + "' doit contenir '" + contentToCheck + "' au lieu de '" + content + "'");

            xPath = "/s:ArchiveTransfer/s:Integrity[1]/s:Contains";
            node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            contentToCheck = "fb20d26a36b8368ea31695298ca0222a31968847";
            content = node.InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath + "' doit contenir '" + contentToCheck + "' au lieu de '" + content + "'");

        }


        // Ce test correspond à la génération d'un bordereau au format SEDA 1.0, 
        // le test suivant vérifie avec les mêmes données la génération d'un bordereau au format SEDA 0.2
        [TestMethod]
        public void TestRepetitionUneUniteSeda10() {
            streamWriter = null;
            String dir;
            dir = Directory.GetCurrentDirectory();
            System.Console.WriteLine(dir);
            GeneratorConfig control = configLoader("repetition_une_unite_seda10");

            accordVersement = control.accordVersement;
            fichier_metier = control.dataFile;
            path_datafiles = control.repDocuments;
            fichier_bordereau = control.bordereauFile;
            traceFile = control.traceFile;
            baseURI = control.baseURI;
            // namespace_seda    = "fr:gouv:ae:archive:draft:standard_echange_v0.2";

            informationsDatabase = "Server=VM-PSQL02\\OUTILS_P;Database=BW_DEV;User=App-Blueway-Exploitation;Password=Pi=3.14159;";

            executeGenerator("1.0");

            // Début de vérification de bon déroulement
            errors = ssg.getErrorsList();
            Assert.AreEqual(false, errors != null && errors.Count != 0, "Aucune erreur ne doit avoir été détectée");
            int numerror = 0;
            if (errors != null && errors.Count != 0) {
                Assert.AreEqual("", errors[numerror],"Cette erreur n'aurait pas dû se produire");
                numerror++;
            }

            String xPath;
            XmlNodeList nodeList;
            XmlNode node;
            String contentToCheck, content;

            xPath = "/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:ArchivalAgencyObjectIdentifier";
            node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            contentToCheck = "DOCLIST / OR_ETP+";
            content = node.Attributes.GetNamedItem("schemeID").InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath + "' doit avoir un attribut schemeID = à '" + contentToCheck + "' au lieu de '" + content + "'");

            xPath = "/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:Document[1]/s:Attachment";
            node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            contentToCheck = "document1e1.txt";
            content = node.Attributes.GetNamedItem("filename").InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath + "' doit avoir un attribut filename = à '" + contentToCheck + "' au lieu de '" + content + "'");

            xPath = "/s:ArchiveTransfer/s:Archive/s:ArchiveObject[1]/s:ArchiveObject[1]/s:Document[1]/s:Integrity";
            node = docBordereau.SelectSingleNode(xPath, docInXmlnsManager);
            Assert.IsNotNull(node, "Le nœud '" + xPath + "' devrait exister");
            contentToCheck = "fb20d26a36b8368ea31695298ca0222a31968847";
            content = node.InnerText;
            Assert.AreEqual(contentToCheck, content, "Le xPath '" + xPath + "' doit contenir '" + contentToCheck + "' au lieu de '" + content + "'");
        }


    }
}
