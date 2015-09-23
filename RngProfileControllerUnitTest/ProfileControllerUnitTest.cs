using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SedaSummaryGenerator;
using System.Collections.Specialized;
using CommonClassesLibrary;

namespace RngProfileControllerUnitTest {
    [TestClass]
    public class ProfileControllerUnitTest {

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

        void declencherTestProfil(String jobName, String[] branchesAttendues, String[] erreursAttendues) {
            StreamWriter streamWriter = null;
            ProfileControlConfig control = configLoader(jobName);
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

            rpc.controlProfileFile(profileFile);
            {
                streamWriter.WriteLine("-------------------------------");
                streamWriter.WriteLine("Arbre des unités documentaires.");
                StringCollection arbre = rpc.getTreeList();
                if (arbre != null && arbre.Count != 0) {
                    foreach (String str in arbre) {
                        streamWriter.WriteLine(str);
                    }
                }
                streamWriter.WriteLine("-----------------");

                streamWriter.WriteLine("-----------------");
                streamWriter.WriteLine("Liste des erreurs.");
                StringCollection erreurs = rpc.getErrorsList();
                if (erreurs != null && erreurs.Count != 0) {
                    foreach (String str in erreurs) {
                        streamWriter.WriteLine(str);
                    }
                }
                streamWriter.WriteLine("-----------------");
            }
            streamWriter.Flush();

            if (branchesAttendues != null) {
                StringCollection arbre = rpc.getTreeList();

                Assert.AreEqual(branchesAttendues.Length, arbre.Count, "La taille des arbres diffère");

                if (arbre != null && arbre.Count != 0) {
                    int branche = 0;
                    foreach (String str in arbre) {
                        Assert.AreEqual(branchesAttendues[branche], str, "Comparaison du nom des branches");
                        branche++;
                    }
                }
            }

            if (erreursAttendues != null) {
                StringCollection errors = rpc.getErrorsList();

                Assert.AreEqual(erreursAttendues.Length, errors.Count, "Le nombre d'erreurs attendues et obtenues diffère");

                if (errors != null && errors.Count != 0) {
                    int erreur = 0;
                    foreach (String str in errors) {
                        StringAssert.StartsWith(str, erreursAttendues[erreur], "Comparaison es erreurs");
                        erreur++;
                    }
                }
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
                "Erreur dans le contexte '/Contains/Contains' sur le DOCLIST 'OR_ETP+', la balise Document n°'2' doit contenir une balise Identification", 
                "Erreur dans le contexte '/Contains' sur le DOCLIST 'MACHIN', la balise Document n°'1' doit contenir une balise Identification",
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
            String[] erreursAttendues = { "L'identifiant MP_Cons n'est pas unique, il est utilisé 2 fois." };
            declencherTestProfil("profil_test_1-06", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M07_TestProfil07() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "Identifiant DOCLIST malformé, on attend 'DOCLIST / identifier' et on a 'CG56_DOCLIST_2015 MP_Cons'" };
            declencherTestProfil("profil_test_1-07", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M08_TestProfil08() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "Le nœud 'rng:define[@name='ArchivalAgencyObjectIdentifier_N66838']/rng:attribute[@name='schemeID']/rng:value' n'a pas été trouvé dans le profil" };
            declencherTestProfil("profil_test_1-08", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M10_TestProfil10() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "La balise ArchivalAgencyObjectIdentifier de l'unité documentaire 'MP_Cons' est optionnelle. Il faut la rendre obligatoire." };
            declencherTestProfil("profil_test_1-10", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M11_TestProfil11() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "L'unité documentaire 'MP_OetD_Analyse' peut être répétée, mais elle ne possède pas de TAG répétable (TAG+)." };
            declencherTestProfil("profil_test_1-11", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M12_TestProfil12() {
            String[] branchesAttendues = null;
            String[] erreursAttendues = { "L'unité documentaire 'MP_OetD_Analyse+' est unique ou optionnelle, mais elle possède un TAG répétable (TAG+)." };
            declencherTestProfil("profil_test_1-12", branchesAttendues, erreursAttendues);
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
                "Les mots-clés de l'unité documentaire 'KEYWORDUNIT_NOTOK' ne pourront pas produits car la description du contenu est optionnelle.", 
                };
            declencherTestProfil("KeywordInOptionalDescription", branchesAttendues, erreursAttendues);
        }

        [TestMethod]
        public void M15_TestErreurFilenameCdoLangueTypeDocNivdesc() {
            String[] branchesAttendues = { "\troot", "\t\tFILENAME_NOTOKINTERDIT", "\t\tFILENAME_NOTOKFACULTATIF", "\t\tTYPEDOCUMENT_INEXISTANT", "\t\tLANGUE_INEXISTANTE", "\t\tNIVEAUDESCRIPTION_INEXISTANT", "\t\tLANGUESMULTIPLES", "" };
            String[] erreursAttendues = 
                { 
                "L'attribut filename de la balise Document/Attachment de l'unité documentaire 'FILENAME_NOTOKINTERDIT' est interdit alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.", 
                "L'attribut filename de la balise Document/Attachment de l'unité documentaire 'FILENAME_NOTOKFACULTATIF' est facultatif alors qu'il est obligatoire. Les documents ne pourront pas y être stockés.", 
                "Le type de document de la balise Document de l'unité documentaire 'TYPEDOCUMENT_INEXISTANT' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.", 
                "La langue de la description de l'unité documentaire 'LANGUE_INEXISTANTE' n'a pas de valeur. C'est une donnée qui dans cette version doit être fournie par le profil.", 
                "Le niveau de description de l'unité documentaire 'NIVEAUDESCRIPTION_INEXISTANT' n'a pas de valeur. C'est une donnée archivistique qui doit être fournie par le profil.", 
                "La langue de la description de l'unité documentaire 'LANGUESMULTIPLES' peut être répétée plusieurs fois. Le générateur ne permet pas de donner une valeur à ces éléments. Le bordereau ne sera pas conforme.", 
                };
            declencherTestProfil("filenameCdoTypedocNivdesc", branchesAttendues, erreursAttendues);
        }

    }
}
