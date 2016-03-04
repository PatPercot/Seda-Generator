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
                "ERR: dans la ligne '33' le 2ème champ ne correspond pas à un des formats possibles : tagname, tagname{doc}, tagname[#num] ou tagname{#num]{doc}",
                "ERR: dans la ligne '34' le 2ème champ ne correspond pas à un des formats possibles : tagname, tagname{doc}, tagname[#num] ou tagname{#num]{doc}",
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

    }
}
