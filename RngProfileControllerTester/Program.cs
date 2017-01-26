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

            // String traceFile = @"D:\DEV_PPE\devel\gw-quimper\trace-control.txt";
            // String profileFile = @"D:\DEV_PPE\devel\gw-quimper\repetition_une_unite_deux_documents_schema.rng";

            //String profileFile = @"D:\DEV_PPE\tests\marches\20150619Profil_avec_doc_falcultatif_donne_avant_engagement\EMEG_PROFIL_MP_0002_schema.rng";
            //String profileFile = @"D:\DEV_PPE\tests\marches\repetition_une_unite-1-v1_0_schema.rng";

            //String profileFile = @"D:\DEV_PPE\tests\marches\repetition_une_unite-trois_documents-optionnels_schema.rng";

            //String profileFile = @"D:\DEV_PPE\tests\CGI\Test_MARCHES-V5bis\EMEG_PROFIL_MP_0002-PPE_schema.rng";

            //String profileFile = @"D:\DEV_PPE\tests\marches\repetition_une_unite_schema.rng";
            //String profileFile = @"D:\DEV_PPE\tests\marches\repetition_une_unite_avec_sous_unites_schema.rng";
            //String profileFile = @"D:\DEV_PPE\tests\marches\repetition_deux_unites_avec_sous_unites_schema.rng";

            //String profileFile = @"\\cg56.fr\dfs2\BW\DEVT\ArcEspCo\DATA\CG56_PROFIL_PES_0001_v1_schema-duplication.rng";
            //String profileFile = "D:/DEV_PPE/devel/RNG/esco-ad/SAE-INT-PROFIL-ESPACE-CO_schema.rng";
            //String profileFile = @"\\vm-devshare\d$\DEV_PPE\devel\CG56_PES-transfert-manuel\CG56_PROFIL_PES_0001_v1_schema.rng";


            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                System.Environment.Exit(-1);
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            RngProfileController rpc = new RngProfileController();
            rpc.setTracesWriter(streamWriter);

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
