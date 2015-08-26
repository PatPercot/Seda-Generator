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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using SedaSummaryGenerator;

namespace WsSoapSedaGeneratorAspWebApp {
    /// <summary>
    /// Description résumée de WsSoapGenerator
    /// </summary>
    [WebService(Namespace = "http://morbihan.fr/SAE-WS/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

   
    // Pour autoriser l'appel de ce service Web depuis un script à l'aide d'ASP.NET AJAX, supprimez les marques de commentaire de la ligne suivante. 
    // [System.Web.Script.Services.ScriptService]
    public class WsSoapGenerator : System.Web.Services.WebService {


        // retourne des erreurs s'il s'en produit
        // sinon retourne une chaîne vide
        [WebMethod]
        public string checkProfileFile(string repBase, string filename) {
            String traceFile = buildTraceFileName(repBase, "CPF");
            StreamWriter streamWriter = null;

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            streamWriter.WriteLine("Appel de rpc.controlProfileFile(\"" + filename + "\") dans repBase : '" + repBase + "'");

            RngProfileController rpc = new RngProfileController();
            rpc.setTracesWriter(streamWriter);

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
        [WebMethod]
        public string espaceCoConvertToCsvFile(string repBase, string title, string repFichiers, string urlBase, string filename) {
            String traceFile = buildTraceFileName(repBase, "ECCTCF");
            StreamWriter streamWriter = null;

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            ConvertLstDocEspaceCo2LstDocSAE converter = new ConvertLstDocEspaceCo2LstDocSAE(title);
            converter.setTracesWriter(streamWriter);

            StringCollection errors = converter.convert(repFichiers, filename, urlBase);
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
        // produireBordereauVersement(repBaseTraces, repBaseDocumennts, "ACCORD_TEST_ESCOAD"
        //                            , "\\vm-devshare\d$\DEV_PPE\tmp/esco-ad/liste-fichiers-archive-escoad-02.txt"
        //                            , "\\vm-devshare\d$\DEV_PPE\tmp/esco-ad/bordereau-ESCOAD.xml");
        [WebMethod]
        public string produireBordereauVersement(string repLog, string repDocuments, string baseURI, string accordVersement, string documentsFile, string bordereauFile)
        {
            String traceFile = buildTraceFileName(repLog, "PBV");
            StreamWriter streamWriter = null;
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

            ssg.prepareInformationsWithDatabase(informationsDatabase, baseURI, accordVersement);

            ssg.prepareArchiveDocumentsWithFile(repDocuments, documentsFile);

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

        // Envoie le bordereau et le ZIP à une instance d'Asalaé
        // Appel avec :
        // asalaeArchiveZipFile(repBaseTraces, "https://sae.morbihan.fr", "web-service", "mdp-web-service", "mail@morbihan.fr"
        //                            , @"\\cg56.fr\dfs2\BW\SAE\WORK\bordereau-PESV2_MANUEL-1.xml"
        //                            , @"\\cg56.fr\dfs2\BW\SAE\WORK\mandat-et-ar-1.zip");
        [WebMethod]
        public string asalaeArchiveZipFile(string repLog, string baseURI, string user, string pwd, string mail, string bordereauFileName, string zipFileName) {
            String traceFile = buildTraceFileName(repLog, "AAZF");
            StreamWriter streamWriter = null;
            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            streamWriter.WriteLine("Création Asalaé {0} {1} {2} ", baseURI, user, pwd);
            Asalae asalae = new Asalae("restservices/sedaMessages", user, pwd, baseURI);
            asalae.setTracesWriter(streamWriter);

            streamWriter.WriteLine("Appel ArchiveZipFile {0} {1} {2} ", bordereauFileName, zipFileName, mail);
            string retour;
            retour = asalae.ArchiveZipFile(bordereauFileName, zipFileName, mail);

            streamWriter.WriteLine("Retour du transfert Asalaé : " + retour);
            streamWriter.Close();

            return retour;
        }


        [WebMethod]
        public string HelloWorld(string repLog, string name, UInt32 value) {
            String traceFile = buildTraceFileName(repLog, "HW");
            StreamWriter streamWriter = null;

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + traceFile + ". Complementary message: " + str);
                throw ex;
            };

            try {
                streamWriter = new StreamWriter(traceFile);
            } catch (IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            streamWriter.WriteLine("Hello in this marvelous World " + name + ", it will explode in " + value + " seconds");
            streamWriter.Close();

            return "Welcome in this marvelous World " + name + ", it will explode in " + value + " seconds";
        }

        protected String buildTraceFileName(String baseStr, String wsName) {
            String dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd--HH-mm-ss");
            if (baseStr.EndsWith(@"\") == false)
                baseStr += @"\";
            return baseStr + wsName + "-traces-" + dateStr + ".txt";
        }


    }
}
