/*****************************************************************************************
 * 
 * Générateur de bordereaux de transfert guidé par le profil d'archivage
 *                         -----------------------
 * Département du Morbihan
 * Direction des systèmes d'information
 * Service Études
 * Patrick Percot
 * Mars 2015
 * 
 * Réutilisation du code autorisée dans l'intérêt des collectivités territoriales
 * 
 * ****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using SedaSummaryGenerator;
using CommonClassesLibrary;

namespace RngProfileControllerLauncher {
    class Program {
        static int Main(string[] args) {
            StreamWriter streamWriter = null;

            String jobName;
            if (args.Length < 1) {
                System.Console.WriteLine("Syntaxe attendue : RngProfileControllerLauncher nom-job-controle");
                System.Console.WriteLine("nom-job-controle est une section dans le fichier job.config");
                System.Console.WriteLine("Une section de contrôle de profil a la forme :");
                System.Console.WriteLine("[profile-control : nom-job-controle]");
                System.Console.WriteLine("  trace = chemin/vers/fichier-de-trace.txt");
                System.Console.WriteLine("  profil = chemin/vers/fichier-de-profil.rng");
                System.Console.WriteLine("");
                System.Console.WriteLine("Aucun job demandé, le premier job sera exécuté");
                System.Console.WriteLine("");
                jobName = String.Empty;
            } else {
                jobName = args[0];
            }

            SimpleConfig config = new SimpleConfig();
            String erreur = config.loadFile("./job.config");
            if (erreur != String.Empty) // on tient compte du fait qu'en environnement de développement, l'exe est dans bin/Release
                erreur = config.loadFile("../../job.config");

            if (erreur != String.Empty) {
                System.Console.WriteLine(erreur);
                System.Environment.Exit(-1);
            }

            ProfileControlConfig control = config.getProfileConfig(jobName);
            if (control == null) {
                System.Console.WriteLine("Aucun job 'profile-control: " + jobName + "' trouvé dans le fichier job.config. Vérifiez la syntaxe ou créez une tâche.");
                System.Environment.Exit(-1);
            }

            System.Console.WriteLine("Contrôle profil du job '" + control.nomJob +  "' du profil '" + control.profileFile + "'");

            String profileFile = control.profileFile;
            String traceFile = control.traceFile;

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                System.Environment.Exit(-1);
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            RngProfileController rpc = new RngProfileController();
            rpc.setTracesWriter(streamWriter);

            rpc.setDataFile(control.dataFile);
            rpc.controlProfileFile(profileFile);
            
            StringCollection arbre = rpc.getTreeList();

            if (arbre != null && arbre.Count != 0) {
                Console.WriteLine("\nArbre des unités documentaires.\n");
                Console.WriteLine("Les unités répétées sont présentées sous la forme UNITE[#1].");
                foreach (String str in arbre) {
                    Console.WriteLine(str);
                    streamWriter.WriteLine(str);
                }
            }

            StringCollection expectedTagsList = rpc.getExpectedTagsList();

            if (expectedTagsList != null && expectedTagsList.Count != 0) {
                Console.WriteLine("\nTags attendus par le profil.\n");
                foreach (String str in expectedTagsList) {
                    Console.WriteLine(str);
                    streamWriter.WriteLine(str);
                }
            }

            StringCollection errors = rpc.getErrorsList();

            if (errors != null && errors.Count != 0) {
                Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!\nIl y a eu des erreurs.\n");
                foreach (String str in errors) {
                    Console.WriteLine(str);
                    streamWriter.WriteLine(str);
                }
            } else {
                Console.WriteLine("\nAucune erreur détectée\n");
            }

            streamWriter.Close();

            return errors.Count;
        }

    }
}
