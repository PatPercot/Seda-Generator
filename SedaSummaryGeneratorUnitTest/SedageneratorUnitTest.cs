using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SedaSummaryGenerator;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Specialized;

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
        // XmlNamespaceManager docInXmlnsManager;

        protected void executeGenerator() {
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
                    // docInXmlnsManager = new System.Xml.XmlNamespaceManager(docBordereau.NameTable);
                    // Add the namespaces used in xml to the XmlNamespaceManager.
                    // docInXmlnsManager.AddNamespace("xmlns", namespace_seda);
                    // docInXmlnsManager.AddNamespace(String.Empty, namespace_seda);
                }
            }
            catch (ArgumentException e) { ehb(e); }
            catch (DirectoryNotFoundException e) { ehb(e); }
            catch (FileNotFoundException e) { ehb(e); }
            catch (OutOfMemoryException e) { ehb(e); }
            catch (IOException e) { ehb(e); }
        }


        [TestMethod]
        public void TestRepetitionUneUnite() {
            streamWriter = null;

            accordVersement   = "CG56_ACCORD_MARCHE_TEST_1";
            fichier_metier    = @"../../../TestCases/ProfileGenerator/datafiles/liste_repetition_une_unite-1.txt";
            path_datafiles    = @"../../../TestCases/ProfileGenerator/datafiles/";
            fichier_bordereau = @"../../../TestCases/ProfileGenerator/results/repetition_une_unite-1-bordereau.xml";
            traceFile         = @"../../../TestCases/ProfileGenerator/traces/repetition_une_unite-1-traces.txt";
            baseURI           = "http://test";
            // namespace_seda    = "fr:gouv:ae:archive:draft:standard_echange_v0.2";

            informationsDatabase = "Server=VM-PSQL02\\OUTILS_P;Database=BW_DEV;User=App-Blueway-Exploitation;Password=Pi=3.14159;";

            executeGenerator();

            // Début de vérification de bon déroulement
            errors = ssg.getErrorsList();
            Assert.AreEqual(false, errors != null && errors.Count != 0, "Aucune erreur ne doit avoir été détectée");
            int numerror = 0;
            if (errors != null && errors.Count != 0) {
                Assert.AreEqual("", errors[numerror],"Cette erreur n'aurait pas dû se produire");
                numerror++;
            }

            XmlNode node;
            String xPath;
            XmlNodeList nodeList;

            xPath = "/*[local-name()='ArchiveTransfer']/Contains[1]/Contains[1]/ArchivalAgencyObjectIdentifier[1]";
            nodeList = docBordereau.SelectNodes(xPath);
            Assert.IsNotNull(nodeList, "Le nœud '" + xPath + "' devrait exister");
            String schemeID = "DOCLIST / OR_ETP+";
            foreach (XmlNode nodeItem in nodeList) {
                Assert.AreEqual(schemeID, nodeItem.Attributes.GetNamedItem("schemeID"), "Le xPath '" + xPath + "' doit avoir un attribut schemeID = à '" + schemeID + "'");
            }

            xPath = "/*[local-name()='ArchiveTransfer']/Contains[1]/Contains[1]/ArchivalAgencyObjectIdentifier[1]/Document[1]/Attachment[1]";
            nodeList = docBordereau.SelectNodes(xPath);
            Assert.IsNotNull(nodeList, "Le nœud '" + xPath + "' devrait exister");
            String filename = "document1e2.txt";
            foreach (XmlNode nodeItem in nodeList) {
                Assert.AreEqual(schemeID, nodeItem.Attributes.GetNamedItem("filename"), "Le xPath '" + xPath + "' doit avoir un attribut filename = à '" + filename + "'");
            }
        }
    }
}
