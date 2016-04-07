using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SedaSummaryGenerator;
using CommonClassesLibrary;
using System.IO;
using System.Collections.Specialized;

namespace BusinessDataControllerUnitTest {
    [TestClass]
    public class BusinessDataControllerUnitTest {

        DataControlConfig configLoader(String configName) {
            SimpleConfig config = new SimpleConfig();
            String erreur = config.loadFile("./job.config");
            if (erreur != String.Empty) // on tient compte du fait qu'en environnement de développement, l'exe est dans bin/Release
                erreur = config.loadFile("../../job.config");

            if (erreur != String.Empty) {
                System.Console.WriteLine(erreur);
                Assert.Fail(erreur);
            }

            return config.getDatacontrolConfig(configName);
        }

        void declencherControleDonnees(String jobName, String[] erreursAttendues) {
            StreamWriter streamWriter = null;
            DataControlConfig control = configLoader(jobName);
            String traceFile = control.traceFile;
            String profileFile = control.profileFile;
            String dataFile = control.dataFile;

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            BusinessDataController bdc = new BusinessDataController();
            bdc.setTracesWriter(streamWriter);

            StringCollection erreurs = bdc.controlDataFormat(dataFile);
            {
                streamWriter.WriteLine("-----------------");
                streamWriter.WriteLine("Liste des erreurs.");
                if (erreurs != null && erreurs.Count != 0) {
                    foreach (String str in erreurs) {
                        streamWriter.WriteLine(str);
                    }
                }
                streamWriter.WriteLine("-----------------");
            }
            streamWriter.Flush();

            if (erreursAttendues != null) {
                int erreur = 0;
                if (erreurs != null && erreurs.Count != 0) {
                    foreach (String str in erreurs) {
                        if (erreursAttendues.Length > erreur)
                            StringAssert.StartsWith(str, erreursAttendues[erreur], "Comparaison des erreurs");
                        erreur++;
                    }
                }

                Assert.AreEqual(erreursAttendues.Length, erreurs.Count, "Le nombre d'erreurs attendues et obtenues diffère");
            }

            streamWriter.Close();
        }

        void declencherAnalyseProfil(String jobName, String[] erreursAttendues, String[] tagsAttendus) {
            StreamWriter streamWriter = null;
            DataControlConfig control = configLoader(jobName);
            String traceFile = control.traceFile;
            String profileFile = control.profileFile;
            String dataFile = control.dataFile;

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            RngProfileController rpc = new RngProfileController();
            rpc.setTracesWriter(streamWriter);
            rpc.controlProfileFile(profileFile);

            StringCollection erreurs = rpc.getErrorsList();
            {
                streamWriter.WriteLine("-----------------");
                streamWriter.WriteLine("Liste des erreurs.");
                if (erreurs != null && erreurs.Count != 0) {
                    foreach (String str in erreurs) {
                        streamWriter.WriteLine(str);
                    }
                }
                streamWriter.WriteLine("-----------------");
            }
            streamWriter.Flush();

            if (erreursAttendues != null) {
                int erreur = 0;
                if (erreurs != null && erreurs.Count != 0) {
                    foreach (String str in erreurs) {
                        if (erreursAttendues.Length > erreur)
                            StringAssert.StartsWith(str, erreursAttendues[erreur], "Comparaison des erreurs");
                        erreur++;
                    }
                }

                Assert.AreEqual(erreursAttendues.Length, erreurs.Count, "Le nombre d'erreurs attendues et obtenues diffère");
            }

            StringCollection tags = rpc.getExpectedTagsListList();
            {
                streamWriter.WriteLine("-----------------");
                streamWriter.WriteLine("Liste des tags.");
                if (tags != null && tags.Count != 0) {
                    foreach (String str in tags) {
                        streamWriter.WriteLine(str);
                    }
                }
                streamWriter.WriteLine("-----------------");
            }
            streamWriter.Flush();

            if (erreursAttendues != null) {
                int tag = 0;
                if (tags != null && tags.Count != 0) {
                    foreach (String str in tags) {
                        if (tagsAttendus.Length > tag)
                            StringAssert.StartsWith(str, tagsAttendus[tag], "Comparaison des tags");
                        tag++;
                    }
                }

                Assert.AreEqual(tagsAttendus.Length, tags.Count, "Le nombre de tags attendus et obtenus diffère");
            }


            streamWriter.Close();
        }


        [TestMethod]
        /*
         * 
         * */
        public void H00_TestErreursFormatDonnees() {
            String[] erreursAttendues = 
                { 
                "ERR: La ligne '1' contient '1' séparateurs (", 
                "ERR: La ligne '2' contient '3' séparateurs (", 
                "ERR: La ligne '3' contient '12' séparateurs (", 
                "ERR: le 1er champ de la ligne '5' commence par 'T' alors qu'il devrait commencer par #",
                "ERR: dans la ligne '13' la date '32/12/2014 12:00:00' a un format non reconnu",
                "ERR: dans la ligne '14' la date '' a un format non reconnu",
                "ERR: la ligne '16' contient une balise '#BlaBlaBla[ENRSON[#1]]' qui n'est pas reconnue et ne sera pas traitée",
                "ERR: dans la ligne '17' le 1er champ ne contient pas de nom de balise",
                "ERR: dans la ligne '27' la taille '150 Ko' a un format non reconnu",
                "ERR: dans la ligne '28' le 2ème champ ne contient pas de nom de tag",
                "ERR: dans la ligne '33' le 2ème champ ne correspond pas à un des formats possibles : tagname, tagname{doc}, tagname[#num] ou tagname[#num]{doc}",
                "ERR: dans la ligne '34' le 2ème champ ne correspond pas à un des formats possibles : tagname, tagname{doc}, tagname[#num] ou tagname[#num]{doc}",
                "ERR: dans la ligne '43' la date '1er juin 2013 12:00:00' a un format non reconnu",
                "ERR: la tagname 'TransferName' a été trouvée '2' fois alors qu'elle ne doit être présente qu'une seule fois",
                };
            declencherControleDonnees("caban_erreurs", erreursAttendues);
        }

        [TestMethod]
        /*
         *  Utiliser ce test pour analyser des erreurs non répertoriées
         * */
        public void H01_TestSansErreursFormatDonnees() {
            String[] erreursAttendues = 
                { 
                };
            declencherControleDonnees("sans_erreurs", erreursAttendues);
        }

        [TestMethod]
        /*
         * 
         * */
        public void H02_TestTagsProfil() {
            String[] erreursAttendues = 
                { 
                };
            String[] tagsAttendus = 
                { 
                "TransferName",
                "OriginatingAgency.Identification"
                };
            declencherAnalyseProfil("tags_profil", erreursAttendues, tagsAttendus);
        }

        [TestMethod]
        /*
         * 
         * */
        public void H03_TestTagsProfilComplet() {
            String[] erreursAttendues = 
                { 
                };
            String[] tagsAttendus = 
                { 
                "Comment",
                "TransferName",
                "CustodialHistory",
                "OriginatingAgency.BusinessType",
                "OriginatingAgency.Description",
                "OriginatingAgency.LegalClassification",
                "OriginatingAgency.Name",
                "OriginatingAgency.Identification",
                "ContainsName[#ENRSON[#1]]",
                "ContainsDescription[#ENRSON[#1]]",
                "KeywordContent[#ENRSON[#1]]",
                "document[#ENRSON[#1]]{PDF}",
                "document[#ENRSON[#1]]{MP3}",
                "document[#ENRSON[#1]]{WAV}",
                "ContainsName[#ENRSON[#1]//SAMPLES[#1]]",
                "ContainsDescription[#ENRSON[#1]//SAMPLES[#1]]",
                "KeywordContent[#ENRSON[#1]//SAMPLES[#1]]",
                "document[#ENRSON[#1]//SAMPLES[#1]]",
                "ContainsName[#ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "ContainsDescription[#ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "KeywordContent[#ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "document[#ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "ContainsName[#ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "ContainsDescription[#ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "KeywordContent[#ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "document[#ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                };
            declencherAnalyseProfil("tags_profil_complet", erreursAttendues, tagsAttendus);
        }

        [TestMethod]
        /*
         * 
         * */
        public void H04_TestContenuFichierDonnees() {
            String[] tagsListeClesAttendus = 
                { 
                "#Comment",
                "#TransferName",
                "#CustodialHistory",
                "#OriginatingAgency.BusinessType",
                "#OriginatingAgency.Description",
                "#OriginatingAgency.LegalClassification",
                "#OriginatingAgency.Name",
                "#OriginatingAgency.Identification",
                "#ContainsName[#ENRSON[#1]]",
                "#ContainsDescription[#ENRSON[#1]]",
                "#KeywordContent[#ENRSON[#1]]",
                "#ContainsName[#ENRSON[#1]//SAMPLES[#1]]",
                "#ContainsDescription[#ENRSON[#1]//SAMPLES[#1]]",
                "#KeywordContent[#ENRSON[#1]//SAMPLES[#1]]",
                "#ContainsName[#ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "#ContainsDescription[#ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "#KeywordContent[#ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "#ContainsName[#ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "#ContainsDescription[#ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "#KeywordContent[#ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                };
            String[] tagsListeDocumentsAttendus = 
                { 
                "[#ENRSON[#1]]{PDF}",
                "[#ENRSON[#1]]{MP3}",
                "[#ENRSON[#1]]{WAV}",
                "[#ENRSON[#1]//SAMPLES[#1]]",
                "[#ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "[#ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                };
            DataControlConfig control = configLoader("tags_profil_complet");

            CsvArchiveDocuments ad = new CsvArchiveDocuments();
            ad.loadFile(control.dataFile);
            StringCollection listForKeys = ad.getTagsListForKeys();
            StringCollection listForDocuments = ad.getTagsListForDocuments();

            int key = 0;
            if (listForKeys != null && listForKeys.Count != 0) {
                foreach (String str in listForKeys) {
                    if (tagsListeClesAttendus.Length > key)
                        StringAssert.StartsWith(str, tagsListeClesAttendus[key], "Comparaison des tags de clés");
                    key++;
                }
            }

            Assert.AreEqual(tagsListeClesAttendus.Length, listForKeys.Count, "Le nombre de clés attendues et obtenues diffère");

            key = 0;
            if (listForDocuments != null && listForDocuments.Count != 0) {
                foreach (String str in listForDocuments) {
                    if (tagsListeClesAttendus.Length > key)
                        StringAssert.StartsWith(str, tagsListeDocumentsAttendus[key], "Comparaison des tags de documents");
                    key++;
                }
            }

            Assert.AreEqual(tagsListeDocumentsAttendus.Length, listForDocuments.Count, "Le nombre de documents attendus et obtenus diffère");

        }

    }
}
