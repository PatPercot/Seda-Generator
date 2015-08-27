using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SedaSummaryGenerator;
using System.Collections.Specialized;

namespace RngProfileControllerUnitTest {
    [TestClass]
    public class ProfileControllerUnitTest {
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
        public void TestErreursTagDoclistIdentification() {
            StreamWriter streamWriter = null;
            String traceFile = @"../../../TestCases/ProfileController/profil_erreurs_tags_doclist_identification-traces.txt";

            String profileFile = @"../../../TestCases/ProfileController/profiles/profil_erreurs_tags_doclist_identification.rng";

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

            StringCollection arbre = rpc.getTreeList();
            String[] branchesAttendues = { "\troot", "\t\tOFFRES_RETENUES[#1]", "\t\t\tOR_ETP[#1]", "\t\tMACHIN", ""};
            if (arbre != null && arbre.Count != 0) {
                int branche = 0;
                foreach (String str in arbre) {
                    Assert.AreEqual(branchesAttendues[branche], str, "Comparaison du nom des branches" );
                    branche++;
                }
            }

            StringCollection errors = rpc.getErrorsList();
            String[] erreursAttendues = 
                { 
                "L'unité documentaire 'OFFRES_RETENUES+' est unique ou optionnelle, mais elle possède un TAG répétable (TAG+). Il faut supprimer le '+' du tag ou changer les cardinalités", 
                "Identifiant DOCLIST malformé, on attend 'DOCLIST / identifier' et on a 'DOCLIST /OR_ETP_DOCA' dans le contexte '/Contains/Contains/Document[1]'", 
                "Erreur dans le contexte '/Contains/Contains' sur le DOCLIST 'OR_ETP+', la balise Document n°'2' doit contenir une balise Identification", 
                "Erreur dans le contexte '/Contains' sur le DOCLIST 'MACHIN', la balise Document n°'1' doit contenir une balise Identification",
                "L'unité documentaire 'MACHIN' peut être répétée, mais elle ne possède pas de TAG répétable (TAG+). Il faut ajouter un '+' au tag ou changer les cardinalités"
                };
            if (errors != null && errors.Count != 0) {
                int erreur = 0;
                foreach (String str in errors) {
                    Assert.AreEqual(erreursAttendues[erreur], str, "Comparaison du nom des branches");
                    erreur++;
                }
            }

            streamWriter.Close();
        }
    }
}
