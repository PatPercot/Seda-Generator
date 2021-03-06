﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SedaSummaryGenerator;
using System.Collections.Specialized;
using CommonClassesLibrary;

namespace RngProfileControllerUnitTest {
    [TestClass]
    public class ProfileControllerUnitTest {
        ProfileControlConfig control;

        ProfileControlConfig configLoader(String configName) {
            SimpleConfig config = new SimpleConfig();
            String erreur = config.loadFile("./job.config");
            if (erreur != String.Empty) // on tient compte du fait qu'en environnement de développement, l'exe est dans bin/Release
                erreur = config.loadFile("../../job.config");

            if (erreur != String.Empty) {
                System.Console.WriteLine(erreur);
                Assert.Fail(erreur);
            }

            return config.getProfileConfig(configName);
        }

        private void declencherTestProfilEtGeneration(String jobName, String[] branchesAttendues
            , String[] erreursAttendues, String[] tagsExemplesAttendus) {
                Action<Exception> eh = (ex) => {
                    Console.WriteLine(ex.GetType().Name + " en comparaison des tags attendus : " + control.dataFile);
                    throw ex;
                };


            declencherTestProfil(jobName, branchesAttendues, erreursAttendues, false);
            try {
                StreamReader reader = new StreamReader(control.dataFile);
                String strLine;
                int numLine = 0;
                while ((strLine = reader.ReadLine()) != null) {
                    strLine = strLine.Trim();
                    if (strLine.StartsWith("#")) {

                    } else if (strLine.Equals(String.Empty)) {

                    } else {
                        Assert.AreEqual(tagsExemplesAttendus[numLine], strLine, "Comparaison des exemples de tags attendus");
                        numLine++;
                    }
                }
                reader.Close();

                Assert.AreEqual(tagsExemplesAttendus.Length, numLine, "Le nombre de tags attendus ne correpond pas");

            }
            catch (FileNotFoundException e) { eh(e); }
            catch (DirectoryNotFoundException e) { eh(e); }
            catch (IOException e) { eh(e); }

        }

        private void declencherTestProfil(String jobName, String[] branchesAttendues, String[] erreursAttendues, Boolean bWithWarns = false) {
            StreamWriter streamWriter = null;
            control = configLoader(jobName);
            String traceFile = control.traceFile;
            String profileFile = control.profileFile;

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            RngProfileController rpc = new RngProfileController();
            rpc.setTracesWriter(streamWriter);

            rpc.setDataFile(control.dataFile);
            rpc.controlProfileFile(profileFile);
            {
                streamWriter.WriteLine("-------------------------------");
                streamWriter.WriteLine("Arbre des unités documentaires.");
                StringCollection arbre = rpc.getTreeList();
                if (arbre != null && arbre.Count != 0) {
                    int numBranche = 1;
                    foreach (String str in arbre) {
                        streamWriter.WriteLine(String.Format("{0,3:G}\t", numBranche) + str);
                        ++numBranche;
                    }
                }
                streamWriter.WriteLine("-----------------");

                streamWriter.WriteLine("-----------------");
                streamWriter.WriteLine("Liste des erreurs.");
                StringCollection erreurs = rpc.getErrorsList();
                if (erreurs != null && erreurs.Count != 0) {
                    int numErreur = 1;
                    foreach (String str in erreurs) {
                        streamWriter.WriteLine(String.Format("{0,3:G}\t", numErreur) + str);
                        ++numErreur;
                    }
                }
                streamWriter.WriteLine("-----------------");
            }
            streamWriter.Flush();

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

            if (branchesAttendues != null) {
                StringCollection arbre = rpc.getTreeList();

                int branche = 0;
                if (arbre != null && arbre.Count != 0) {
                    foreach (String str in arbre) {
                        if (branchesAttendues.Length > branche)
                            Assert.AreEqual(branchesAttendues[branche], str, "Comparaison du nom des branches");
                        branche++;
                    }
                }

                Assert.AreEqual(branchesAttendues.Length, arbre.Count, "La taille des arbres diffère");
            }

            streamWriter.Close();
        }

        
        [TestMethod]
        /*
         * 
            Arbre des unités documentaires.
                    root
                            OFFRES_RETENUES[#1]
                                    OR_ETP[#1]
                            MACHIN


            Erreurs attendues

            L'unité documentaire 'OFFRES_RETENUES+' est unique ou optionnelle, mais elle 
            possède un TAG répétable (TAG+). Il faut supprimer le '+' du tag ou changer les
            cardinalités
            Identifiant DOCLIST malformé, on attend 'DOCLIST / identifier' et on a 'DOCLIST
            /OR_ETP_DOCA' dans le contexte '/Contains/Contains/Document[1]'
            Erreur dans le contexte '/Contains/Contains' sur le DOCLIST 'OR_ETP+', la balise
             Document n°'2' doit contenir une balise Identification
            Erreur dans le contexte '/Contains' sur le DOCLIST 'MACHIN', la balise Document
            n°'1' doit contenir une balise Identification
            L'unité documentaire 'MACHIN' peut être répétée, mais elle ne possède pas de TAG
             répétable (TAG+). Il faut ajouter un '+' au tag ou changer les cardinalités

         * */
        public void M00_TestErreursTagDoclistIdentification() {
            String[] branchesAttendues = { "\troot", "\t\tOFFRES_RETENUES[#1]", "\t\t\tOR_ETP[#1]", "\t\tMACHIN", "" };
            String[] erreursAttendues = 
                { 
                "L'unité documentaire 'OFFRES_RETENUES+' est unique ou optionnelle, mais elle possède un TAG répétable (TAG+). Il faut supprimer le '+' du tag ou changer les cardinalités", 
                "Identifiant DOCLIST malformé, on attend 'DOCLIST / identifier' et on a 'DOCLIST /OR_ETP_DOCA' dans le contexte '/Contains/Contains/Document[1]'", 
                "Dans l'unité documentaire 'OR_ETP+', la balise Document n°'2' doit contenir une balise Identification de cardinalités 1-1", 
                "Dans l'unité documentaire 'MACHIN', la balise Document n°'1' doit contenir une balise Identification de cardinalités 1-1",
                "L'unité documentaire 'MACHIN' peut être répétée, mais elle ne possède pas de TAG répétable (TAG+). Il faut ajouter un '+' au tag ou changer les cardinalités"
                };
            declencherTestProfil("tag_doclist", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M01_TestProfil01() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { };
            declencherTestProfil("profil_test_1-01", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M02_TestProfil02() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "Erreur lors de la préparation du profil d'archivage" };
            declencherTestProfil("profil_test_1-02", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M03_TestProfil03() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "Le nœud 'rng:grammar' n'a pas été trouvé dans le profil" };
            declencherTestProfil("profil_test_1-03", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M04_TestProfil04() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "Version du SEDA inconnue : '" };
            declencherTestProfil("profil_test_1-04", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M05_TestProfil05() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "Erreur lors de la préparation du profil d'archivage" };
            declencherTestProfil("profil_test_1-05", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M06_TestProfil06() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "Identifiant DOCLIST malformé, on attend 'DOCLIST / identifier' et on a 'CG56_DOCLIST_2015 MP_Cons_Dossier' dans le contexte '/Contains/Contains' dans l'attribut schemeID de ArchivalAgencyObjectIdentifier" };
            declencherTestProfil("profil_test_1-06", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M07_TestProfil07() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "L'identifiant MP_Cons n'est pas unique, il est utilisé 2 fois." };
            declencherTestProfil("profil_test_1-07", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M08_TestProfil08() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { 
                        "L'attribut schemeID de la balise ArchivalAgencyObjectIdentifier de l'unité documentaire 'MP_Cons' ne doit pas être optionnel. Il faut le rendre obligatoire.",
                        "Dans l'unité documentaire 'MP_Cons_Dossier_AncDCE', la balise Identification de balise Document n°'2' doit avoir un attribut schemeID obligatoire",
            };
            declencherTestProfil("profil_test_1-08", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M10_TestProfil10() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "L'unité documentaire 'MP_OetD_Analyse' peut être répétée, mais elle ne possède pas de TAG répétable (TAG+)." };
            declencherTestProfil("profil_test_1-10", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M11_TestProfil11() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "L'unité documentaire 'MP_OetD_Analyse+' est unique ou optionnelle, mais elle possède un TAG répétable (TAG+)." };
            declencherTestProfil("profil_test_1-11", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M12_TestProfil12() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { 
                        "La balise ArchivalAgencyObjectIdentifier de l'unité documentaire 'MP_Cons' est optionnelle. Il faut la rendre obligatoire.",
                        "Les mots-clés de l'unité documentaire 'MP_OetD_Analyse+' ne pourront pas être produits car la description du contenu est optionnelle."
                    };
            declencherTestProfil("profil_test_1-12", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M12b_TestProfil13() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { 
                "Dans l'unité documentaire 'MP_Cons_Dossier_AncDCE', la balise Identification de balise Document n°'1' doit avoir un attribut schemeID obligatoire",
                "Dans l'unité documentaire 'MP_Cons_Dossier_AncDCE', la balise Identification de balise Document n°'2' doit avoir un attribut schemeID obligatoire",
                "Le mot-clé n° '1' de l'unité documentaire 'MP_OetD_Analyse+' doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'",
                "Le mot-clé n° '2' de l'unité documentaire 'MP_OetD_Analyse+' doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'",
                "Le mot-clé n° '3' de l'unité documentaire 'MP_OetD_Analyse+' doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'",
                "Le mot-clé n° '4' de l'unité documentaire 'MP_OetD_Analyse+' doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'",
            };
            declencherTestProfil("profil_test_1-13", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M13_TestErreurArchivalAgencyObjectIdentifier() {
            String[] branchesAttendues = { "\troot", "\t\t!!! DOCLIST manquant !!!", "\t\tTAGOK", "" };
            String[] erreursAttendues = 
                { 
                "La balise ArchivalAgencyObjectIdentifier de l'unité documentaire 'TAGNOTOK' est optionnelle. Il faut la rendre obligatoire.", 
                };
            declencherTestProfil("ArchivalAgencyObjectIdentifier", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M14_TestErreurDescriptionContenuKeyword() {
            String[] branchesAttendues = { "\troot", "\t\tKEYWORDUNIT_OK", "\t\tKEYWORDUNIT_NOTOK", "" };
            String[] erreursAttendues = 
                {
				"La présence d'un document est obligatoire dans l'unité documentaire '",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                "Les mots-clés de l'unité documentaire 'KEYWORDUNIT_NOTOK' ne pourront pas être produits car la description du contenu est optionnelle.",
                };
            declencherTestProfil("KeywordInOptionalDescription", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M15_TestErreurFilenameCdoLangueTypeDocNivdesc() {
            String[] branchesAttendues = { "\troot", "\t\tFILENAME_NOTOKINTERDIT", "\t\tFILENAME_NOTOKFACULTATIF", "\t\tTYPEDOCUMENT_INEXISTANT", "\t\tLANGUE_INEXISTANTE", "\t\tNIVEAUDESCRIPTION_INEXISTANT", "\t\tLANGUESMULTIPLES", "" };
            String[] erreursAttendues = 
                { 
                "L'attribut filename de la balise Document[1]/Attachment de l'unité documentaire 'FILENAME_NOTOKINTERDIT' est interdit alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.", 
                "L'attribut filename de la balise Document[1]/Attachment de l'unité documentaire 'FILENAME_NOTOKFACULTATIF' est facultatif alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.", 
                "Le type de document de la balise Document[1] de l'unité documentaire 'TYPEDOCUMENT_INEXISTANT' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.", 
                // TODO: gestion de la langue à améliorer
                // "La langue de la description de l'unité documentaire 'LANGUE_INEXISTANTE' n'a pas de valeur. C'est une donnée qui dans cette version doit être fournie par le profil.", 
                "Le niveau de description de l'unité documentaire 'NIVEAUDESCRIPTION_INEXISTANT' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.", 
                // TODO: gestion de la langue à améliorer
                // "La langue de la description de l'unité documentaire 'LANGUESMULTIPLES' peut être répétée plusieurs fois. Le générateur ne permet pas de donner une valeur à ces éléments. Le bordereau ne sera pas conforme.", 
                };
            declencherTestProfil("filenameCdoTypedocNivdesc", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M16_TestProfilChapitre06() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { };
            declencherTestProfil("profil_test_chapitre_06", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M17_TestProfilMarches() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { };
            declencherTestProfil("profil_test_marches", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M18_TestUtilisationBaliseDocumentArchive() {
            String[] branchesAttendues = { "\troot", "\t\tUD1[#1]", "" };
            String[] erreursAttendues = 
                {
                "La balise Document de Archive (ou Contains premier niveau) ne peut pas recevoir de documents. Tous les documents doivent être situés dans des unités documentaires.", 
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                };
            declencherTestProfil("UtilisationBaliseDocumentArchive", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M19_TestMultipleDocumentSansTypeOuFilename() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = 
                { 
                "Dans l'unité documentaire 'UD1', la balise Document n°'5' doit contenir une balise Identification de cardinalités 1-1",
                "L'attribut filename de la balise Document[1]/Attachment de l'unité documentaire 'UD1' est interdit alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.",
                "L'attribut filename de la balise Document[4]/Attachment de l'unité documentaire 'UD1' est facultatif alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.",
                "L'attribut filename de la balise Document[5]/Attachment de l'unité documentaire 'UD1' est interdit alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.",
                "Le type de document de la balise Document[1] de l'unité documentaire 'UD1' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.",
                "Le type de document de la balise Document[2] de l'unité documentaire 'UD1' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.",
                "Le type de document de la balise Document[5] de l'unité documentaire 'UD1' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil."
                };
            declencherTestProfil("MultipleDocumentSansTypeOuFilename", branchesAttendues, erreursAttendues);
        }

        public void M20_TestMultipleDocumentSansTypeOuFilenamev10() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = 
                { 
                "Dans l'unité documentaire 'UD1', la balise Document n°'5' doit contenir une balise ArchivalAgencyDocumentIdentifier de cardinalités 1-1",
                "L'attribut filename de la balise Document[1]/Attachment de l'unité documentaire 'UD1' est interdit alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.",
                "L'attribut filename de la balise Document[4]/Attachment de l'unité documentaire 'UD1' est facultatif alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.",
                "L'attribut filename de la balise Document[5]/Attachment de l'unité documentaire 'UD1' est interdit alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.",
                "Le type de document de la balise Document[1] de l'unité documentaire 'UD1' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.",
                "Le type de document de la balise Document[2] de l'unité documentaire 'UD1' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.",
                "Le type de document de la balise Document[5] de l'unité documentaire 'UD1' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.",
                "Le type de document de la balise Document[6] de l'unité documentaire 'UD1' ne doit pas être multiple. La cardinalité doit être modifiée.",
                };
            declencherTestProfil("MultipleDocumentSansTypeOuFilenameV10", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M21_TestKeywordTagged() {
            String[] branchesAttendues = { "\troot", "\t\tKEYWORDUNIT_OK", "\t\tKEYWORDUNIT_NOTOK", "" };
            String[] erreursAttendues = 
                { 
				"La présence d'un document est obligatoire dans l'unité documentaire '",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                "Les mots-clés de l'unité documentaire 'KEYWORDUNIT_NOTOK' ne pourront pas être produits car la description du contenu est optionnelle.", 
                };
            declencherTestProfil("KeywordTagged", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M22_TestKeywordUnTagged() {
            String[] branchesAttendues = { "\troot", "\t\tKEYWORDUNIT_OK", "\t\tKEYWORDUNIT_NOTOK", "" };
            String[] erreursAttendues = 
                { 
                "Le mot-clé n° '1' de l'archive doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'",
                "Le mot-clé n° '2' de l'archive doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                "Le mot-clé n° '2' de l'unité documentaire 'KEYWORDUNIT_OK' doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                "Le mot-clé n° '2' de l'unité documentaire 'KEYWORDUNIT_NOTOK' doit avoir un identifiant de référentiel avec un attribut schemeID de la forme 'DOCLIST / TAG'",
                };
            declencherTestProfil("KeywordUnTagged", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M23_TestKeywordDuplicated() {
            String[] branchesAttendues = { "\troot", "\t\tKEYWORDUNIT_OK", "\t\tKEYWORDUNIT_NOTOK", "" };
            String[] erreursAttendues = 
                { 
                "Le mot-clé n° '2' de l'archive a un tag 'ARKW1' qui n'est pas unique",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                "Le mot-clé n° '2' de l'unité documentaire 'KEYWORDUNIT_NOTOK' a un tag 'KWNOTOK' qui n'est pas unique",
                };
            declencherTestProfil("KeywordDuplicated", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M24_TestAccordVersementProducteur() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = 
                { 
                "(--) La balise ArchivalAgreement est optionnelle ou absente et ne sera pas générée. Elle pourrait être rendue obligatoire",
                "(--) La balise OriginatingAgency de l'archive est optionnelle et ne sera pas générée. Elle pourrait être rendue obligatoire",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                "(--) La balise OriginatingAgency de l'unité documentaire 'UD1' est optionnelle et ne sera pas générée. Elle pourrait être rendue obligatoire",
                };
            declencherTestProfil("AccordVersement_Producteur", branchesAttendues, erreursAttendues, true);
        }

        [TestMethod]
        public void M25_TestFilePlanPosition() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "\t\t\tUD11[#1]", "\t\t\tUD12[#1]", "\t\tUD2", "\t\t\tUD21", "\t\tUD3", "\t\t\tUD31", "" };
            String[] erreursAttendues = 
                { 
                "L'attribut schemeID de la balise FilePlanPosition n° '1' de l'archive ne doit pas être optionnel. Il faut le rendre obligatoire.",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                "L'attribut schemeID de la balise FilePlanPosition n° '1' de l'unité documentaire 'UD31' n'existe pas. Il faut le créer, le rendre obligatoire et lui donner un nom de tag unique.",
                "L'attribut schemeID de la balise FilePlanPosition n° '2' de l'unité documentaire 'UD31' n'existe pas. Il faut le créer, le rendre obligatoire et lui donner un nom de tag unique.",
                "L'attribut schemeID de la balise FilePlanPosition n° '3' de l'unité documentaire 'UD31' ne doit pas être optionnel. Il faut le rendre obligatoire.",
                "La balise FilePlanPosition n° '4' de l'unité documentaire 'UD31' a un tag 'FPPOPTIONAL' qui n'est pas unique",
                };
            declencherTestProfil("FilePlanPosition", branchesAttendues, erreursAttendues, true);
        }

        [TestMethod]
        public void M26_TestDonneesProfilMarches() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = { };
            String[] tagsExemplesAttendus = { 
                ",#Comment,Texte à personnaliser",
                ",#TransferName,Texte à personnaliser",
                ",#CustodialHistory,Texte à personnaliser",
                ",#OriginatingAgency.Description,Texte à personnaliser",
                ",#OriginatingAgency.Name,Texte à personnaliser",
                ",#OriginatingAgency.Identification,Texte à personnaliser",
                ",#ContentDescription.Description,Texte à personnaliser",
                ",#FilePlanPosition[{FPPARCHIVE}[#1]],Texte à personnaliser",
                ",#ContainsName[UD1],Texte à personnaliser",
                ",#CustodialHistory[UD1],Texte à personnaliser",
                ",#KeywordContent[UD1{KW11}[#1]],Texte à personnaliser",
                ",#KeywordContent[UD1{KW2}[#1]],Texte à personnaliser",
                ",#FilePlanPosition[UD1{FPP11}[#1]],Texte à personnaliser",
                ",#FilePlanPosition[UD1{FPP12}[#1]],Texte à personnaliser",
                ",fichier.txt,UD1{FILE1},Description du document,13/03/2017 14:31:27",
                ",fichier.txt,UD1{FILE2},Description du document,13/03/2017 14:31:27",
            };
            declencherTestProfilEtGeneration("generation_donnees_metier", branchesAttendues, erreursAttendues, tagsExemplesAttendus);
		}

		[TestMethod]
		/*
		 * 
		 * */
		public void M27_TestSansUniteDocumentaireErreur()
		{
			String[] branchesAttendues = { "\troot", "" };
			String[] erreursAttendues = 
                { 
				"La présence d'une unité documentaire est obligatoire dans l'archive",
                };
			declencherTestProfil("SansUniteDocumentaireErreur", branchesAttendues, erreursAttendues, true);
		}

        [TestMethod]
        /*
         * 
         * */
        public void M28_TestUdSansFilleErreur() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = 
                { 
				"La présence d'un document est obligatoire dans l'unité documentaire '",
                };
            declencherTestProfil("UdSansFilleErreur", branchesAttendues, erreursAttendues, true);
        }

        [TestMethod]
        /*
         * 
         * */
        public void M29_TestUniteDocumentaireCardinalite01Erreur() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = 
                { 
				"Une unité documentaire unique ne peut pas être optionnelle. La cardinalité minimum doit être 1",
                };
            declencherTestProfil("UniteDocumentaireCardinalite01Erreur", branchesAttendues, erreursAttendues, true);
        }

        [TestMethod]
        /*
         * 
         * */
        public void M30_TestTransferringAgencyObjectOdentifier_02()
        {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "\t\t\tUD_TAOI", "\t\t\tUD_TAOI_OPT", "\t\t\tUD_SS_TAOI", "" };
            String[] erreursAttendues = 
                { 
				"La balise TransferringAgencyArchiveIdentifier de l'archive ne peut pas être optionnelle. Il faut la rendre obligatoire",
				"La balise TransferringAgencyObjectIdentifier de l'unité documentaire 'UD_TAOI_OPT' ne peut pas être optionnelle. Il faut la rendre obligatoire",
                };
            declencherTestProfil("TransferringAgencyObjectIdentifier_02", branchesAttendues, erreursAttendues, true);
        }

        [TestMethod]
        /*
         * 
         * */
        public void M31_TestTransferringAgencyObjectOdentifier_10()
        {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "\t\t\tUD_TAOI", "\t\t\tUD_TAOI_OPT", "\t\t\tUD_SS_TAOI", "" };
            String[] erreursAttendues = 
                { 
				"La balise TransferringAgencyArchiveIdentifier de l'archive ne peut pas être optionnelle. Il faut la rendre obligatoire",
				"La balise TransferringAgencyObjectIdentifier de l'unité documentaire 'UD_TAOI_OPT' ne peut pas être optionnelle. Il faut la rendre obligatoire",
                };
            declencherTestProfil("TransferringAgencyObjectIdentifier_10", branchesAttendues, erreursAttendues, true);
        }

        [TestMethod]
        /*
         * 
         * */
        public void M32_TestCycleVie_02()
        {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = 
                { 
                };
            declencherTestProfil("cycle_vie_02", branchesAttendues, erreursAttendues, true);
        }

        [TestMethod]
        /*
         * 
         * */
        public void M33_TestCycleVie_10()
        {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = 
                { 
                };
            declencherTestProfil("cycle_vie_10", branchesAttendues, erreursAttendues, true);
        }

        [TestMethod]
        public void M34_TestCustodialHistory_02() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = { };
            String[] tagsExemplesAttendus = { 
                ",#TransferName,Texte à personnaliser",
                ",#CustodialHistory,Texte à personnaliser",
                ",#ContainsName[UD1],Texte à personnaliser",
                ",#CustodialHistory[UD1],Texte à personnaliser",
                ",fichier.txt,UD1,Description du document,13/03/2017 14:31:27",
            };
            declencherTestProfilEtGeneration("custodial02", branchesAttendues, erreursAttendues, tagsExemplesAttendus);
        }

        [TestMethod]
        public void M35_TestCustodialHistory_10() {
            String[] branchesAttendues = { "\troot", "\t\tUD1", "" };
            String[] erreursAttendues = { };
            String[] tagsExemplesAttendus = { 
                ",#TransferName,Texte à personnaliser",
                ",#CustodialHistory,Texte à personnaliser",
                ",#ContainsName[UD1],Texte à personnaliser",
                ",#CustodialHistory[UD1],Texte à personnaliser",
                ",fichier.txt,UD1,Description du document,13/03/2017 14:31:27",
            };
            declencherTestProfilEtGeneration("custodial10", branchesAttendues, erreursAttendues, tagsExemplesAttendus);
        }


    }
}
