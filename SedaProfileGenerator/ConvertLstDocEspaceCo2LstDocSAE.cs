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
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.Specialized;

namespace SedaSummaryGenerator {
    public class ConvertLstDocEspaceCo2LstDocSAE {
        protected String transferName = null;
        protected bool traceActions = false;
        protected TextWriter tracesWriter = null;
        // Liste des erreurs rencontrées
        protected StringCollection errorsList;
        SqlConnection connection;
        protected String SAE_EspCoUrl;
        protected String espCoDirectory;

        // Informations contenues dans les fichiers de métadonnées
        protected String curFileDescription;
        protected String curFileDate;
        protected String curFileDocList;

        /*
         * Cette classe permet de convertir la liste des fichiers sortant d'un espace collaboratif
         * Sharepoint dans le format attendu par la classe CsvArchiveDocuments
         * */
        public ConvertLstDocEspaceCo2LstDocSAE(String transferName) {
            this.transferName = transferName;
            this.traceActions = false;
            this.tracesWriter = null;
            errorsList = new StringCollection();
            connection = null;
        }

/*
        public void prepareInformationsWithDatabase(String informationsDatabase, String accordVersement) {
            connection = new SqlConnection(informationsDatabase);
            try {
                connection.Open();
                if (traceActions) tracesWriter.WriteLine("ServerVersion: {0}", connection.ServerVersion);
                if (traceActions) tracesWriter.WriteLine("State: {0}", connection.State);
                try {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT SAE_EspCoUrl FROM SAE WHERE SAE_AccordVersement='" + accordVersement + "'";
                    command.CommandTimeout = 15;
                    command.CommandType = CommandType.Text;
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read()) {
                        SAE_EspCoUrl = (String)reader[0];
                        if (traceActions) tracesWriter.WriteLine(String.Format("{0}", reader[0]));
                    }
                    reader.Dispose();
                } catch (InvalidOperationException e) {
                    String strError = e.GetType().Name + " while querying '" + accordVersement + "'";
                    errorsList.Add(strError);
                    if (traceActions) tracesWriter.WriteLine(strError);
                    if (traceActions) tracesWriter.WriteLine(e.Message);
                }

            } catch (SqlException e) {
                String strError = e.GetType().Name + " while opening database: " + informationsDatabase + "";
                errorsList.Add(strError);
                if (traceActions) tracesWriter.WriteLine(strError);
                if (traceActions) tracesWriter.WriteLine(e.Message);
                connection = null;
            }
        }
 * 
 * */
        /*
        protected String getDocListCode(String directoryName) {
            String str_result = String.Empty;
            try {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT DocList_Id FROM SAE_convert WHERE CheminFichier='" + directoryName + "'";
                command.CommandTimeout = 15;
                command.CommandType = CommandType.Text;
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) {
                    //if (traceActions) tracesWriter.WriteLine(String.Format("{0}", reader[0]));
                    str_result = (String)reader[0];
                } else {
                    String strError = "TODO: créer un DOCLIST dans SAE_convert pour '" + directoryName + "'";
                    errorsList.Add(strError);
                    if (traceActions) tracesWriter.WriteLine(strError);
                }
                reader.Dispose();
            } catch (InvalidOperationException e) {
                String strError = e.GetType().Name + " while querying '" + directoryName + "'";
                errorsList.Add(strError);
                if (traceActions) tracesWriter.WriteLine(strError);
                if (traceActions) tracesWriter.WriteLine(e.Message);
            } catch (SqlException e) {
                String strError = e.GetType().Name + " while querying '" + directoryName + "'";
                errorsList.Add(strError);
                if (traceActions) tracesWriter.WriteLine(strError);
                if (traceActions) tracesWriter.WriteLine(e.Message);
            }
            return str_result;
        }
         * */

        /*
         * convert réalise la conversion et retourne la liste des erreurs rencontrées 
         * durant la conversion.
         * Si la liste d'erreurs est vide, la conversion est réussie et le fichier csvListName
         * contient la liste des documents utilisable par ma classe CsvArchiveDocuments
         * */
        public StringCollection convert(String espCoDirectory, String csvListName, String SAE_EspCoUrl) {
            this.SAE_EspCoUrl = SAE_EspCoUrl;
            this.espCoDirectory = espCoDirectory;
            TextWriter csvWriter = null;
            String rapportName = espCoDirectory + "/" + "Rapport.xml";
            if (traceActions) tracesWriter.WriteLine("Conversion de '" + rapportName + "'");

            Action<Exception, String> eh = (ex, str) => {
                if (traceActions) tracesWriter.WriteLine(ex.GetType().Name + str);
                if (traceActions) tracesWriter.WriteLine(ex.Message);
            };

            bool thingsAreOk = false;
            XmlDocument docIn = new XmlDocument();
            XmlNamespaceManager docInXmlnsManager = null;
            String operation = "opening '" + rapportName + "'";
            try {
                using (StreamReader sr = new StreamReader(rapportName)) {
                    String line = sr.ReadToEnd();
                    //Console.WriteLine(line);
                    docIn.LoadXml(line);
                    //Instantiate an XmlNamespaceManager object. 
                    docInXmlnsManager = new System.Xml.XmlNamespaceManager(docIn.NameTable);
                    thingsAreOk = true;
                }
            } catch (ArgumentException e) { eh(e, operation); 
            } catch (DirectoryNotFoundException e) { eh(e, operation); 
            } catch (FileNotFoundException e) { eh(e, operation); 
            } catch (OutOfMemoryException e) { eh(e, operation);
            } catch (IOException e) { eh(e, operation); }

            if (thingsAreOk) {
                try {
                    csvWriter = new StreamWriter(csvListName);
                    Char separator = chooseSeparator(transferName);
                    csvWriter.WriteLine(separator + "#TransferName" + separator + transferName);
                } catch (IOException e) { 
                    eh(e, csvListName + ": Mauvaise syntaxe de nom de fichier"); 
                } catch (UnauthorizedAccessException e) {
                    eh(e, csvListName + ": Droits d'accès à corriger"); 
                } catch (System.Security.SecurityException e) {
                    eh(e, csvListName + ": Droits d'accès à corriger");
                }
            }

            if (thingsAreOk) {
                String expectedNodeName = "Archives/DetailFichiers";
                XmlNode startNode = docIn.SelectSingleNode(expectedNodeName, docInXmlnsManager);
                if (startNode != null) {
                    if (traceActions) tracesWriter.WriteLine("startNode : " + startNode.Name);
                    if (startNode.HasChildNodes) {
                        String fileRelativeName, pdcName;
                        foreach (XmlNode node in startNode.ChildNodes) {
                            if (node.Name == "Fichiers") {
                                if (traceActions) tracesWriter.WriteLine("Fichier : " + node.InnerText);
                                fileRelativeName = node.InnerText.Replace("\\", "/");
                                fileRelativeName = fileRelativeName.Substring(fileRelativeName.IndexOf(SAE_EspCoUrl));
                                //if (traceActions) tracesWriter.WriteLine("fileRelativeName : " + fileRelativeName);
                                pdcName = Path.GetDirectoryName(fileRelativeName).Substring(SAE_EspCoUrl.Length + 1).Replace("\\", "/");
                                //if (traceActions) tracesWriter.WriteLine("pdcName : " + pdcName);
                                /*
                                curFileDocList = getDocListCode(pdcName);
                                if (traceActions) tracesWriter.WriteLine("curFileDocList (" + pdcName + ") : " + curFileDocList);
                                 * */
                                getFileInformations(fileRelativeName);
                                Char separator = chooseSeparator(curFileDocList + fileRelativeName + curFileDate + curFileDescription);
                                csvWriter.WriteLine(separator + fileRelativeName
                                    + separator + curFileDocList + separator + curFileDescription
                                    + separator + curFileDate);
                            }
                        }
                    } else {
                        thingsAreOk = false;
                        String strError = "Le nœud '" + expectedNodeName + "' de '" + rapportName + "' n'a pas d'enfants";
                        errorsList.Add(strError);
                    }
                } else {
                    thingsAreOk = false;
                    String strError = "Le nœud '" + expectedNodeName + "' est introuvable dans '" + rapportName + "'";
                    errorsList.Add(strError);
                }
                if (csvWriter != null) {
                    csvWriter.Close();
                }
                    
            }

            if (traceActions) tracesWriter.WriteLine("Fin de conversion de '" + rapportName + "'\n");
            return errorsList;
        }

        public bool getFileInformations(String relativeFileName) {
            String pathName = Path.GetDirectoryName(relativeFileName);
            String nameWithoutExt = Path.GetFileNameWithoutExtension(relativeFileName);
            String fileInfoName = espCoDirectory + "/" + pathName + "/Infos" + nameWithoutExt + ".xml";
            if (traceActions) tracesWriter.WriteLine("Récupération des informations dans '" + fileInfoName + "'");

            Action<Exception, String> eh = (ex, str) => {
                String strError = "Erreur lors de l'accès à '" + fileInfoName + "'";
                errorsList.Add(strError);
                if (traceActions) tracesWriter.WriteLine(ex.GetType().Name + str);
                if (traceActions) tracesWriter.WriteLine(ex.Message);
            };

            bool thingsAreOk = false;
            XmlDocument docIn = new XmlDocument();
            XmlNamespaceManager docInXmlnsManager = null;
            String operation = "opening '" + fileInfoName + "'";
            try {
                using (StreamReader sr = new StreamReader(fileInfoName)) {
                    String line = sr.ReadToEnd();
                    //Console.WriteLine(line);
                    docIn.LoadXml(line);
                    //Instantiate an XmlNamespaceManager object. 
                    docInXmlnsManager = new System.Xml.XmlNamespaceManager(docIn.NameTable);
                    thingsAreOk = true;
                }
            } catch (ArgumentException e) {
                eh(e, operation);
            } catch (DirectoryNotFoundException e) {
                eh(e, operation);
            } catch (FileNotFoundException e) {
                eh(e, operation);
            } catch (OutOfMemoryException e) {
                eh(e, operation);
            } catch (IOException e) { eh(e, operation); }

            if (thingsAreOk) {
                String expectedNodeName = "FileToArchive";
                XmlNode startNode = docIn.SelectSingleNode(expectedNodeName, docInXmlnsManager);
                if (startNode != null) {
                    if (traceActions) tracesWriter.WriteLine("startNode : " + startNode.Name);
                    curFileDescription = curFileDate = String.Empty;
                    int variablesAffected = 0;
                    if (startNode.HasChildNodes) {
                        foreach (XmlNode node in startNode.ChildNodes) {
                            if (node.Name == "Title") {
                                if (traceActions) tracesWriter.WriteLine("Title : " + node.InnerText);
                                curFileDescription = node.InnerText;
                                variablesAffected++;
                            } else if (node.Name == "Write") {
                                if (traceActions) tracesWriter.WriteLine("Write : " + node.InnerText);
                                curFileDate = node.InnerText;
                                variablesAffected++;
                            } else if (node.Name == "DocList") {
                                if (traceActions) tracesWriter.WriteLine("DocList : " + node.InnerText);
                                curFileDocList = node.InnerText;
                                variablesAffected++;
                            }
                        }
                    } else {
                        thingsAreOk = false;
                        String strError = "Le nœud '" + expectedNodeName + "' de '" + fileInfoName + "' n'a pas d'enfants";
                        errorsList.Add(strError);
                    }
                    if (variablesAffected != 3) {
                        thingsAreOk = false;
                        String strError = "Toutes les informations attendues n'ont pas été trouvées dans '" + fileInfoName + "'";
                        errorsList.Add(strError);
                    }
                } else {
                    thingsAreOk = false;
                    String strError = "Le nœud '" + expectedNodeName + "' est introuvable dans '" + fileInfoName + "'";
                    errorsList.Add(strError);
                }
            }
            return thingsAreOk;
        }

        public void close() {
            if (connection != null)
                connection.Close();
        }

        protected Char chooseSeparator(String str) {
            String lstSeparators = ",.:/!|";
            Char separator = lstSeparators[0];
            for (int i = 0; i < lstSeparators.Length; ++i) {
                separator = lstSeparators[i];
                if (str.IndexOf(separator) == -1)
                    break;
            }
            return separator;
        }

        public void setTracesWriter(TextWriter tracesWriter) {
            if (tracesWriter != null) {
                this.tracesWriter = tracesWriter;
                traceActions = true;
            } else
                unsetTracesWriter();
        }

        public void unsetTracesWriter() {
            tracesWriter = null;
            traceActions = false;
        }
    }
}
