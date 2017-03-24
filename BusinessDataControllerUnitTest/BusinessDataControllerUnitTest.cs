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

        void declencherAnalyseProfil(String jobName, String[] erreursAttendues, String[] tagsAttendus, Boolean bWithWarns = false) {
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
                /*
                int erreur = 0;
                if (erreurs != null && erreurs.Count != 0) {
                    foreach (String str in erreurs) {
                        if (erreursAttendues.Length > erreur)
                            StringAssert.StartsWith(str, erreursAttendues[erreur], "Comparaison des erreurs");
                        erreur++;
                    }
                }

                Assert.AreEqual(erreursAttendues.Length, erreurs.Count, "Le nombre d'erreurs attendues et obtenues diffère");
                 * */
                StringCollection errors = rpc.getErrorsList();
                int erreur = 0;
                if (errors != null && errors.Count != 0) {
                    foreach (String str in errors) {
                        if (bWithWarns) {
                            if (erreursAttendues.Length > erreur)
                                StringAssert.StartsWith(str, erreursAttendues[erreur], "Comparaison des erreurs");
                            erreur++;
                        } else {
                            if (str.StartsWith("(--) ") == false) {
                                if (erreursAttendues.Length > erreur)
                                    StringAssert.StartsWith(str, erreursAttendues[erreur], "Comparaison des erreurs");
                                erreur++;
                            }
                        }
                    }
                }
                Assert.AreEqual(erreursAttendues.Length, erreur, "Le nombre d'erreurs attendues et obtenues diffère");

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
                "ERR: dans la ligne '13' la date '32/12/2014 12:00:00' a un format non reconnu (formats autorisés AAAA-MM-JJ ou AAAA-MM-JJ HH:MM:SS)",
                "ERR: dans la ligne '14' la date '' a un format non reconnu",
                "ERR: la ligne '16' contient une balise '#BlaBlaBla[ENRSON[#1]]' qui n'est pas reconnue et ne sera pas traitée",
                "ERR: dans la ligne '17' le 1er champ ne contient pas de nom de balise",
                "ERR: dans la ligne '27' la taille '150 Ko' a un format non reconnu",
                "ERR: dans la ligne '28' le 2ème champ ne contient pas de nom de tag",
                "ERR: dans la ligne '33' le 2ème champ ne correspond pas à un des formats possibles : tagname, tagname{doc}, tagname[#num] ou tagname[#num]{doc}",
                "ERR: dans la ligne '34' le 2ème champ ne correspond pas à un des formats possibles : tagname, tagname{doc}, tagname[#num] ou tagname[#num]{doc}",
                "ERR: dans la ligne '43' la date '1er juin 2013 12:00:00' a un format non reconnu",
                "ERR: dans la ligne '55' le 1er champ ne correspond pas à un des formats possibles : #tagname, #tagname[TAG] ou #tagname[TAG[#num]]",
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
                "#TransferName",
                "#KeywordContent[{KW_COMMUNE}]",
                "#KeywordContent[{KW_EVT}]",
                "#ContainsName[ENRSON[#1]]",
                "#ContentDescription.Description[ENRSON[#1]]",
                "document: ENRSON[#1]{PDF}",
                "document: ENRSON[#1]{MP3}",
                "document: ENRSON[#1]{WAV}",

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
                "#Comment",
                "#TransferName",
                "#CustodialHistory",
                "#OriginatingAgency.BusinessType",
                "#OriginatingAgency.Description",
                "#OriginatingAgency.LegalClassification",
                "#OriginatingAgency.Name",
                "#OriginatingAgency.Identification",
                "#ContentDescription.Description",
                "#KeywordContent[{KW_COMMUNE}]",
                "#KeywordContent[{KW_EVT}]",
                "#ContainsName[ENRSON[#1]]",
                "#CustodialHistory[ENRSON[#1]]",
                "#ContentDescription.Description[ENRSON[#1]]",
                "#KeywordContent[ENRSON[#1]{KW1}]",
                "document: ENRSON[#1]{PDF}",
                "document: ENRSON[#1]{MP3}",
                "document: ENRSON[#1]{WAV}",
                "#ContainsName[ENRSON[#1]//SAMPLES[#1]]",
                "#ContentDescription.Description[ENRSON[#1]//SAMPLES[#1]]",
                "#KeywordContent[ENRSON[#1]//SAMPLES[#1]{KW2}]",
                "document: ENRSON[#1]//SAMPLES[#1]",
                "#ContainsName[ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "#ContentDescription.Description[ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "#KeywordContent[ENRSON[#1]//SAMPLES[#1]//DESCRIPTION{KW3}]",
                "document: ENRSON[#1]//SAMPLES[#1]//DESCRIPTION",
                "#ContainsName[ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "#ContentDescription.Description[ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "#KeywordContent[ENRSON[#1]//SAMPLES[#1]//ANNEXE{KW4}]",
                "document: ENRSON[#1]//SAMPLES[#1]//ANNEXE",
                };
            declencherAnalyseProfil("tags_profil_complet_profil", erreursAttendues, tagsAttendus);
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
                "#ContentDescription.Description",
                "#OriginatingAgency.BusinessType",
                "#OriginatingAgency.Description",
                "#OriginatingAgency.LegalClassification",
                "#OriginatingAgency.Name",
                "#OriginatingAgency.Identification",
                "#KeywordContent[{KW_COMMUNE}]",
                "#KeywordContent[{KW_EVT}]",
                "#ContainsName[ENRSON[#1]]",
                "#CustodialHistory[ENRSON[#1]]",
                "#ContentDescription.Description[ENRSON[#1]]",
                "#KeywordContent[ENRSON[#1]{KW1}]",
                "#ContainsName[ENRSON[#1]//SAMPLES[#1]]",
                "#ContentDescription.Description[ENRSON[#1]//SAMPLES[#1]]",
                "#KeywordContent[ENRSON[#1]//SAMPLES[#1]{KW2}]",
                "#ContainsName[ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "#ContentDescription.Description[ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]",
                "#KeywordContent[ENRSON[#1]//SAMPLES[#1]//DESCRIPTION{KW3}]",
                "#ContainsName[ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "#ContentDescription.Description[ENRSON[#1]//SAMPLES[#1]//ANNEXE]",
                "#KeywordContent[ENRSON[#1]//SAMPLES[#1]//ANNEXE{KW4}]",
                };
            String[] tagsListeDocumentsAttendus = 
                { 
                "ENRSON[#1]{PDF}",
                "ENRSON[#1]{MP3}",
                "ENRSON[#1]{WAV}",
                "ENRSON[#1]//SAMPLES[#1]",
                "ENRSON[#1]//SAMPLES[#1]//DESCRIPTION",
                "ENRSON[#1]//SAMPLES[#1]//ANNEXE",
                };

            DataControlConfig control = configLoader("tags_profil_complet_donnees");

            StreamWriter streamWriter = null;
            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + control.traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(control.traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            CsvArchiveDocuments ad = new CsvArchiveDocuments();
            ad.setTracesWriter(streamWriter);
            ad.loadFile(control.dataFile);

            StringCollection adErrors = ad.getErrorsList();
            streamWriter.WriteLine("\nErreurs dans la lecture du fichier de données");
            foreach (String str in adErrors) {
                streamWriter.WriteLine(str);
            }
            streamWriter.Flush();

            Assert.AreEqual(0, adErrors.Count, "Aucune erreur n'était attendue");

            StringCollection listForKeys = ad.getTagsListForKeys();
            StringCollection listForDocuments = ad.getTagsListForDocuments();

            int key = 0;
            if (listForKeys != null && listForKeys.Count != 0) {
                streamWriter.WriteLine("\nListe des clés lues");
                foreach (String str in listForKeys) {
                    streamWriter.WriteLine(str);
                }
                streamWriter.Flush();
                foreach (String str in listForKeys) {
                    if (tagsListeClesAttendus.Length > key)
                        StringAssert.StartsWith(str, tagsListeClesAttendus[key], "Comparaison des tags de clés");
                    key++;
                }
            }

            Assert.AreEqual(tagsListeClesAttendus.Length, listForKeys.Count, "Le nombre de clés attendues et obtenues diffère");

            key = 0;
            if (listForDocuments != null && listForDocuments.Count != 0) {
                streamWriter.WriteLine("\nListe des documents lus");
                foreach (String str in listForDocuments) {
                    streamWriter.WriteLine(str);
                }
                streamWriter.Flush();
                foreach (String str in listForDocuments) {
                    if (tagsListeClesAttendus.Length > key)
                        StringAssert.StartsWith(str, tagsListeDocumentsAttendus[key], "Comparaison des tags de documents");
                    key++;
                }
            }

            Assert.AreEqual(tagsListeDocumentsAttendus.Length, listForDocuments.Count, "Le nombre de documents attendus et obtenus diffère");

            streamWriter.Flush();
            streamWriter.Close();
        }


        private void ConfrontationDonneesEtProfil(String testAExecuter, String[] erreursAttendues, Boolean bWithWarns = false) {
            StreamWriter streamWriter = null;
            DataControlConfig control = configLoader(testAExecuter);
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

            BusinessDataController bc = new BusinessDataController();
            bc.setTracesWriter(streamWriter);
            StringCollection erreurs = bc.controlMatchingBetweenDataAndProfile(dataFile, profileFile);

            foreach (String err in erreurs) {
                streamWriter.WriteLine(err);
            }
            streamWriter.Flush();

            int erreur = 0;
            if (erreurs != null && erreurs.Count != 0) {
                foreach (String str in erreurs) {
                    if (bWithWarns) {
                        if (erreursAttendues.Length > erreur)
                            StringAssert.StartsWith(str, erreursAttendues[erreur], "Comparaison des erreurs");
                        erreur++;
                    } else {
                        if (str.StartsWith("(--) ") == false) {
                            if (erreursAttendues.Length > erreur)
                                StringAssert.StartsWith(str, erreursAttendues[erreur], "Comparaison des erreurs");
                            erreur++;
                        }
                    }
                }
            }
            Assert.AreEqual(erreursAttendues.Length, erreur, "Le nombre d'erreurs attendues et obtenues diffère");
        }



        [TestMethod]
        /*
         * 
         * */
        public void H05_ConfrontationDonneesEtProfilCorrects() {
            String[] erreursAttendues = { };
            ConfrontationDonneesEtProfil("tags_profil_complet_comparaison", erreursAttendues);
        }

        [TestMethod]
        /*
         * 
         * */
        public void H06_ConfrontationDonneesIncorrectesAvecProfil() {
            String[] erreursAttendues = {
                "Dans le profil, le tag '#Comment' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#TransferName' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#CustodialHistory' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#OriginatingAgency.Description' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#OriginatingAgency.Name' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#ContentDescription.Description' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#KeywordContent[{KW_COMMUNE}]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#KeywordContent[{KW_EVT}]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#CustodialHistory[ENRSON[#1]]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#KeywordContent[ENRSON[#1]{KW1}]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#ContentDescription.Description[ENRSON[#1]//SAMPLES[#1]//DESCRIPTION]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#ContainsName[ENRSON[#1]//SAMPLES[#1]//ANNEXE]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag de document 'ENRSON[#1]{MP3}' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag de document 'ENRSON[#1]{WAV}' ne trouve pas de correspondance dans les données métier",
            };
            ConfrontationDonneesEtProfil("tags_donnees_incompletes_comparaison", erreursAttendues);
        }

        [TestMethod]
        /*
         *  Tests des Keyword OK SEDA 0.2
         * */
        public void H07_TestKeyword02Tagged() {
            String[] erreursAttendues = { };
            declencherControleDonnees("keywords02_tagged", erreursAttendues);
        }

        [TestMethod]
        /*
         *  Tests des Keyword OK SEDA 1.0
         * */
        public void H08_TestKeyword10Tagged() {
            String[] erreursAttendues = { };
            declencherControleDonnees("keywords10_tagged", erreursAttendues);
        }

        [TestMethod]
        /*
         * 
         * */
        public void H09_TestDonneesProfilEtDonnees() {
            String[] erreursAttendues = 
                { 
                "La clé '#CustodialHistory' fournie par les données métier n'est pas attendue par le profil",
                "La clé '#ContentDescription.Description' fournie par les données métier n'est pas attendue par le profil",
                "La clé '#CustodialHistory[UD1]' fournie par les données métier n'est pas attendue par le profil",
                "La clé '#ContentDescription.Description[UD1]' fournie par les données métier n'est pas attendue par le profil",
                };
            ConfrontationDonneesEtProfil("donnees_profil_et_donnees", erreursAttendues, true);
        }

        [TestMethod]
        /*
         * 
         * */
        public void I01_TestFilePlanPosition() {
            String[] erreursAttendues = 
                { 
                };
            ConfrontationDonneesEtProfil("FilePlanPositionOK", erreursAttendues, true);
        }

        [TestMethod]
        /*
         * 
         * */
        public void I02_TestFilePlanPositionErreur() {
            String[] erreursAttendues = 
                { 
                "Dans le profil, le tag '#FilePlanPosition[{FPPARCHIVE}]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#FilePlanPosition[UD1]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#FilePlanPosition[UD11[#1][#1]]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#FilePlanPosition[UD12[#1]{FPP121}]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#FilePlanPosition[UD2{FPP21}]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#FilePlanPosition[UD2{FPP22}]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#FilePlanPosition[UD2{FPP23}]' ne trouve pas de correspondance dans les données métier",
                "Dans le profil, le tag '#FilePlanPosition[UD21]' ne trouve pas de correspondance dans les données métier",
                };
            ConfrontationDonneesEtProfil("FilePlanPositionERREUR", erreursAttendues, true);
        }

        
    }
}
