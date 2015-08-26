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
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
//using Dit.ug;
using System.Web;
using System.Security.Cryptography;

namespace SedaSummaryGenerator {

    public static partial class Extension {
        /// <summary>
        /// hash la chaine avec l'algorithme de hachage SHA256
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string sha256(this string input) {
            SHA256Managed crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(input), 0, Encoding.UTF8.GetByteCount(input));
            foreach (byte bit in crypto) {
                hash += bit.ToString("x2");
            }
            return hash;
        }
    }



    public class Asalae {
        private string _urlSedaMessages;
        public string UrlSedaMessages {
            get { return _urlSedaMessages; }
            set { _urlSedaMessages = value; }
        }
        private string _user;
        public string User {
            get { return _user; }
            set { _user = value; }
        }
        private string _pwd;
        public string Pwd {
            get { return _pwd; }
            set { _pwd = value; }
        }
        private string _baseURI;
        public string BaseURI {
            get { return _baseURI; }
            set { _baseURI = value; }
        }

        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        public Asalae(string UrlSedaMessages, string User, string Pwd, string BaseURI) {
            this.UrlSedaMessages = UrlSedaMessages;
            this.User = User;
            this.Pwd = Pwd;
            this.BaseURI = BaseURI;
            unsetTracesWriter();
        }

        public string TestConnectionAsalae() {
            string result = "";
            try {
                // URL REST pour tester les web-méthodes asalaé
                String urlServicesPing = "restservices/ping";
                String urlServicesVersions = "restservices/versions";

                var retourPing = this.RunAsync(urlServicesPing, HttpMethod.Get, null);
                if (retourPing.IsFaulted)
                    throw new Exception(@"Une erreur a été détectée lors de l'appel d'une métode REST de Asal@e " + retourPing.Result, new Exception(retourPing.Result.Content.ReadAsStringAsync().Result));
                else
                    result = retourPing.Result.Content.ReadAsStringAsync().Result;
                var retourVersion = this.RunAsync(urlServicesVersions, HttpMethod.Get, null);
                if (retourVersion.IsFaulted)
                    throw new Exception(@"Une erreur a été détectée lors de l'appel d'une métode REST de Asal@e " + retourVersion.Result, new Exception(retourVersion.Result.Content.ReadAsStringAsync().Result));
                else
                    result += "\r\n" + retourVersion.Result.Content.ReadAsStringAsync().Result;
            } catch (Exception e) {
                throw new Exception("Erreur lors d'un appel à l'PAI REST Asal@ae", e);
            }
            return result;
        }

        public string ArchiveZipFile(string bordereauName, string FileNameToArchive, string accuseMailAdress) {
            if (traceActions)
                    tracesWriter.WriteLine("Envoi de " + bordereauName + " et " + FileNameToArchive + " à Asalaé : " + BaseURI);
            string boundary = "--------------------" + DateTime.Now.Ticks.ToString("x");
            FileNameToArchive = FileNameToArchive.Substring(0, FileNameToArchive.LastIndexOf('.')) + ".zip";
            byte[] bytes = Encoding.Default.GetBytes(FileNameToArchive);
            string uriRest = "restservices/sedaMessages";

            byte[] fileContent = File.ReadAllBytes(FileNameToArchive);
            string bordereauContent = File.ReadAllText(bordereauName);

            using (MemoryStream bordereauStream = new MemoryStream()) {
                using (MemoryStream fileContentStream = new MemoryStream()) {
                    using (Stream mailContentStream = new MemoryStream()) {
                        //bordereau
                        var bordereau = Encoding.UTF8.GetBytes(bordereauContent);
                        bordereauStream.Write(bordereau, 0, bordereau.Length);

                        //file
                        if (fileContent != null && fileContent.Length > 0 && FileNameToArchive != "") {
                            try {
                                ZipArchive archive = new ZipArchive(new MemoryStream(fileContent), ZipArchiveMode.Read);
                                fileContentStream.Write(fileContent, 0, fileContent.Length);
                            } catch (InvalidDataException e) {
                                string retError = "Le fichier n'est pas un ZIP ou est corrompu : " + e.Message;
                                if (traceActions)
                                    tracesWriter.WriteLine(retError);
                                throw new Exception(retError);
                            }
                        }

                        //email

                        if (accuseMailAdress.Length > 0) {
                            byte[] reply = System.Text.Encoding.ASCII.GetBytes("MAIL:" + accuseMailAdress);
                            mailContentStream.Write(reply, 0, (int)reply.Length);
                        }
                        //send
                        using (var content = new MultipartFormDataContent(boundary)) {
                            bordereauStream.Seek(0, SeekOrigin.Begin);
                            HttpContent bordereauHttpContent = new StreamContent(bordereauStream);
                            bordereauHttpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
                            content.Add(bordereauHttpContent, "\"seda_message\"", HttpUtility.UrlEncode(Path.GetFileName(bordereauName)));

                            fileContentStream.Seek(0, SeekOrigin.Begin);
                            HttpContent fileHttpContent = new StreamContent(fileContentStream);
                            fileHttpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-zip-compressed");
                            content.Add(fileHttpContent, "\"attachments\"", HttpUtility.UrlEncode(Path.GetFileName(FileNameToArchive)));

                            mailContentStream.Seek(0, SeekOrigin.Begin);
                            content.Add((new StreamContent(mailContentStream)), "reply");

                            Task<HttpResponseMessage> response = this.RunAsync(uriRest, HttpMethod.Post, content);
                            Console.WriteLine("Response status : " + response.Result.IsSuccessStatusCode);
                            Console.WriteLine("Response : " + response.Result.Content.ReadAsStringAsync().Result);
                            if (response.Result.IsSuccessStatusCode) {
                                return response.Result.Content.ReadAsStringAsync().Result;
                            } else {
                                string retError = "Une erreur s'est produite lors de l'appel REST " + response.Result.Content.ReadAsStringAsync().Result;
                                if (traceActions)
                                    tracesWriter.WriteLine(retError);
                                throw new Exception(retError, new Exception(response.Result.Content.ReadAsStringAsync().Result));
                            }
                        }
                    }
                }
            }
        }

        public string ArchiveFile(string bordereauName, string bordereauContent, string accuseMailAdress, string FileNameToArchive, byte[] FileContent) {
            string boundary = "--------------------" + DateTime.Now.Ticks.ToString("x");
            FileNameToArchive = FileNameToArchive.Substring(0, FileNameToArchive.LastIndexOf('.')) + ".zip";
            byte[] bytes = Encoding.Default.GetBytes(FileNameToArchive);
            string uriRest = "restservices/sedaMessages";
            using (MemoryStream bordereauStream = new MemoryStream()) {
                using (MemoryStream fileContentStream = new MemoryStream()) {
                    using (Stream mailContentStream = new MemoryStream()) {
                        //bordereau
                        var bordereau = Encoding.UTF8.GetBytes(bordereauContent);
                        bordereauStream.Write(bordereau, 0, bordereau.Length);

                        //file
                        if (FileContent != null && FileContent.Length > 0 && FileNameToArchive != "") {
                            try {
                                ZipArchive archive = new ZipArchive(new MemoryStream(FileContent), ZipArchiveMode.Read);
                                fileContentStream.Write(FileContent, 0, FileContent.Length);
                            } catch (InvalidDataException) { //il ne s'agit pas d'un ZIP, donc on le zip et on fait l'archivage
                                using (var memoryStream = new MemoryStream()) {
                                    using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) {
                                        //ecriture du fichier dans le zip
                                        var entry = archive.CreateEntry(FileNameToArchive);
                                        using (var entryStream = entry.Open())
                                        using (var streamWriter = new BinaryWriter(entryStream)) {
                                            streamWriter.Write(FileContent, 0, (int)FileContent.Length);
                                        }
                                    }
                                    memoryStream.Seek(0, SeekOrigin.Begin);
                                    memoryStream.WriteTo(fileContentStream);
                                }
                            }
                        }

                        //email

                        if (accuseMailAdress.Length > 0) {
                            byte[] reply = System.Text.Encoding.ASCII.GetBytes("MAIL:" + accuseMailAdress);
                            mailContentStream.Write(reply, 0, (int)reply.Length);
                        }
                        //send
                        using (var content = new MultipartFormDataContent(boundary)) {
                            bordereauStream.Seek(0, SeekOrigin.Begin);
                            HttpContent bordereauHttpContent = new StreamContent(bordereauStream);
                            bordereauHttpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
                            content.Add(bordereauHttpContent, "\"seda_message\"", HttpUtility.UrlEncode(bordereauName));

                            fileContentStream.Seek(0, SeekOrigin.Begin);
                            HttpContent fileHttpContent = new StreamContent(fileContentStream);
                            fileHttpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-zip-compressed");
                            content.Add(fileHttpContent, "\"attachments\"", HttpUtility.UrlEncode(FileNameToArchive));

                            mailContentStream.Seek(0, SeekOrigin.Begin);
                            content.Add((new StreamContent(mailContentStream)), "reply");

                            Task<HttpResponseMessage> response = this.RunAsync(uriRest, HttpMethod.Post, content);
                            if (response.Result.IsSuccessStatusCode) {
                                return response.Result.Content.ReadAsStringAsync().Result;
                            } else
                                throw new Exception("Une erreur s'est produite lors de l'appel REST " + uriRest, new Exception(response.Result.Content.ReadAsStringAsync().Result));
                        }
                    }
                }
            }
        }



        #region Methodes Async REST
        private async Task<HttpResponseMessage> RunAsync(string URI, HttpMethod verbe, HttpContent content) {
            // string pwdHash = sha256.getHashSha256(Pwd);
            string pwdHash = Pwd.sha256();
            var credentials = new NetworkCredential(User, pwdHash);
            using (HttpClientHandler handler = new HttpClientHandler { PreAuthenticate = true, Credentials = credentials })
            using (HttpClient clientREST = new HttpClient(handler) { MaxResponseContentBufferSize = 1000000 }) {
                clientREST.BaseAddress = new Uri(this.BaseURI);
                clientREST.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var byteArray = Encoding.ASCII.GetBytes(User + ":" + pwdHash);
                HttpRequestMessage request = new HttpRequestMessage(verbe, URI);
                request.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
                clientREST.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                if (content != null) {
                    request.Content = content;
                }
                var stp = request.ToString();
                HttpResponseMessage response = clientREST.SendAsync(request).Result;
                //retourne le contenu de la réponse, si il s'agit d'une erreur, elle doit être traîtée
                return response;
            }
        }
        #endregion

        /*
         * Gestion des traces de débogage
         * */
        public void setTracesWriter(TextWriter tracesWriter) {
            if (tracesWriter != null) {
                this.tracesWriter = tracesWriter;
                traceActions = true;
            } else
                unsetTracesWriter();
        }

        /*
         * Gestion des traces de débogage
         * */
        public void unsetTracesWriter() {
            tracesWriter = null;
            traceActions = false;
        }
    }
}

