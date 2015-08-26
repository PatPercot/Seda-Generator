using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SedaSummaryGenerator;

namespace WcfSedaGeneratorServicesLib {
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" à la fois dans le code et le fichier de configuration.
    public class SedaGeneratorService : ISedaGeneratorService {
        // retourne des erreurs s'il s'en produit
        // sinon retourne une chaîne vide
        public string checkProfileFile(String filename) {
            StreamWriter streamWriter = null;
            DateTime date = DateTime.Now;
            String stdate = date.ToString();
            String traceFile = "D:/DEV_PPE/traces/trace-control-WS-" +  DateTime.Now.ToString() + ".txt";

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            RngProfileController rpc = new RngProfileController();
            rpc.setTracesWriter(streamWriter);

            streamWriter.WriteLine("Appel de rpc.controlProfileFile(filename)");

            rpc.controlProfileFile(filename);

            StringCollection errors = rpc.getErrorsList();
            StringBuilder Response = new StringBuilder();

            if (errors != null && errors.Count != 0) {
                Console.WriteLine("Il y a eu des erreurs.");
                foreach (String str in errors) {
                    Console.WriteLine(str);
                    streamWriter.WriteLine(str);
                    Response.Append(str);
                    Response.Append("\n");
                }
            }
            Console.WriteLine("Fin de la liste des erreurs du programme de contrôle du profil");

            streamWriter.Close();

            return Response.ToString();
        }

        // retourne des erreurs s'il s'en produit
        // sinon retourne une chaîne vide
        // Appel avec :
        // espaceCoConvertToCsvFile("Documents de l'espace collaboratif des archives départementales de septembre 2014"
        //                         , "\\vm-devshare\d$\DEV_PPE\tmp\esco-ad/esco-ad/fichiers"
        //                         , "preprodarchives.cg56.fr"
        //                         , "\\vm-devshare\d$\DEV_PPE\tmp/esco-ad/liste-fichiers-archive-escoad-02.txt");
        public string espaceCoConvertToCsvFile(String title, String repBase, String urlBase, String filename) {
            StreamWriter streamWriter = null;
            String traceFile = "D:/DEV_PPE/traces/trace-convert-WS-" + DateTime.Now.ToString() + ".txt";

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            ConvertLstDocEspaceCo2LstDocSAE converter = new ConvertLstDocEspaceCo2LstDocSAE(title);
            converter.setTracesWriter(streamWriter);

            StringCollection errors = converter.convert(repBase, filename, urlBase);
            StringBuilder Response = new StringBuilder();
            if (errors != null && errors.Count != 0) {
                streamWriter.WriteLine("Il y a eu des erreurs lors de la conversion.");
                Console.WriteLine("Il y a eu des erreurs lors de la conversion.");
                foreach (String str in errors) {
                    streamWriter.WriteLine(str);
                    Console.WriteLine(str);
                    Response.Append(str);
                }
            }
            streamWriter.Close();

            return Response.ToString();
        }


        // retourne des erreurs s'il s'en produit
        // sinon retourne une chaîne vide
        // Appel avec :
        // produireBordereauVersement("ACCORD_TEST_ENRSON"
        //                            , "\\vm-devshare\d$\DEV_PPE\tmp/esco-ad/liste-fichiers-archive-escoad-02.txt"
        //                            , "\\vm-devshare\d$\DEV_PPE\tmp/esco-ad/bordereau-ESCOAD.xml");
        [OperationContract]
        public string produireBordereauVersement(String accordVersement, String documentsFile, String bordereauFile) {
            StreamWriter streamWriter = null;
            String traceFile = "D:/DEV_PPE/traces/trace-bordereau-WS-" + DateTime.Now.ToString() + ".txt";
            String informationsDatabase = "Server=VM-PSQL02\\OUTILS_P;Database=BW_DEV;User=App-Blueway-Exploitation;Password=Pi=3.14159;";

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            SedaSummaryGenerator.SedaSummaryGenerator ssg = new SedaSummaryRngGenerator();
            ssg.setTracesWriter(streamWriter);

            ssg.prepareInformationsWithDatabase(informationsDatabase, accordVersement);

            ssg.prepareArchiveDocumentsWithFile(documentsFile);

            ssg.generateSummaryFile(bordereauFile);

            ssg.close();
            streamWriter.Close();

            StringCollection errors = ssg.getErrorsList();
            StringBuilder Response = new StringBuilder();
            if (errors != null && errors.Count != 0) {
                Console.WriteLine("Il y a eu des erreurs.");
                streamWriter.WriteLine("Il y a eu des erreurs.");
                foreach (String str in errors) {
                    streamWriter.WriteLine(str);
                    Console.WriteLine(str);
                    Response.Append(str);
                }
            }
            return Response.ToString();
        }

        /*
        public string GetData(int value) {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite) {
            if (composite == null) {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue) {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
         * */
    }
}
