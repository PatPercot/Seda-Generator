/*****************************************************************************************
 * 
 * Générateur de bordereaux de transfert guidé par le profil d'archivage
 *                         -----------------------
 * Département du Morbihan
 * Direction des systèmes d'information
 * Service Études
 * Patrick Percot
 * Mars-mai 2015
 * 
 * Réutilisation du code autorisée dans l'intérêt des collectivités territoriales
 * 
 * ****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;


namespace SedaSummaryGenerator {
    /*
     * Cette classe charge les informations concernant les fichiers à archiver
     * à partir d'un fichier CSV
     * Ce fichier doit resopecter les règles suivantes :
     * Chaque igne représente un document à archiver
     * Chaque ligne commence par un séparateur. Cette technique permet de changer de 
     * séparateur à chaque ligne
     * Les deux premiers champs sont obligatoires
     * Champ 0: spérateur de la ligne
     * Champ 1: nom de fichier relatif
     * Champ 2: identification du document (DOCLIST)
     * Champ 3: nom du document
     * Champ 4: date de production ou réception du document
     * 
     * Ce ficheir peut aussi contenir des informations de type
     * clé / valeur
     * Les identifiants des clés commencent par #
     * et sont suivis par un séparateur et une valeur
     * #TransferName, value
     * */

    public class CsvArchiveDocuments : ArchiveDocuments {
        private List<String[]> documentsList = null;
        private List<String[]> keyList = null;

        private List<String[]> partialDocumentsList;
        private List<String[]>.Enumerator partialDocumentsListEnumerator;
        private String lastError;

        private String oldestDate = null;
        private String latestDate = null;

        private String currentKey2search = String.Empty;
        private int currentKey2searchCounter = 1;

        public CsvArchiveDocuments() {
            documentsList = new List<String[]>();
            keyList = new List<String[]>();
            lastError = "Pas de données chargées";
        }

        /* 
         * Chargement des informations à partir du fichier
         * */
        public void loadFile(String csvFile) {
            Action<Exception> eh = (ex) => {
                if (traceActions) tracesWriter.WriteLine(ex.GetType().Name + " while reading: " + csvFile);
                if (traceActions) tracesWriter.WriteLine(ex.Message);
                throw (ex);
            };

            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.LoadFile");
            Regex rgxSeperator;
            String line;
            string[] elements;
            try {
                using (StreamReader reader = new StreamReader(csvFile)) {
                    while (reader.Peek() > -1) {
                        line = reader.ReadLine();
                        if (traceActions) tracesWriter.WriteLine(line);
                        if (line.Length > 0) {
                            rgxSeperator = new Regex("" + line[0]);
                            elements = rgxSeperator.Split(line);
                            foreach (string match in elements) {
                                if (traceActions) tracesWriter.Write("field: '" + match + "' ");
                            }
                            if (elements[1] != "" && elements[1][0] == '#') {
                                keyList.Add(elements);
                            } else {
                                documentsList.Add(elements);
                            }
                            if (traceActions) tracesWriter.WriteLine();
                        }
                        lastError = "";
                    }
                    reader.Close();
                }
            }
            catch (ArgumentException e) { eh(e); }
            catch (DirectoryNotFoundException e) { eh(e); }
            catch (FileNotFoundException e) { eh(e); }
            catch (OutOfMemoryException e) { eh(e); }
            catch (IOException e) { eh(e); }
        }


        protected String getTagWithoutDocumentIdentification(String part) {
            if (part.Contains("{")) { // marqueur des Identification de Document
                int pos = part.IndexOf("{");
                part = part.Substring(0, pos);
            }
            return part;
        }
        /*
         * Retourne le nombre de documents référençant le type docListType
         * */
        override public bool IsThereDocumentsReferringToType(String docListType) {
            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.IsThereDocumentsReferringToType('" + docListType + "')");
            bool atLeastOne = false;
            foreach (String[] elements in documentsList) {
                if (elements[2].Contains("/")) {
                    foreach (String part in elements[2].Split('/')) {
                        if (getTagWithoutDocumentIdentification(part) == docListType) {
                            atLeastOne = true;
                            break;
                        }
                    }
                    if (atLeastOne)
                        break;
                } else {
                    if (getTagWithoutDocumentIdentification(elements[2]) == docListType) {
                        atLeastOne = true;
                        break;
                    }
                }
            }
            if (traceActions) {
                String str_refer = atLeastOne ? " réfère " : " ne réfère pas ";
                tracesWriter.WriteLine("ArchiveDocuments.IsThereDocumentsReferringToType('" + docListType + "')" + str_refer + "à des documents");
            }
            return atLeastOne;
        }

        /*
         * Prépare une liste de documents qui sont identifiés comme docListType
         * retourne le nombre de documents
         * */
        override public int prepareListForType(String docListType, bool withDocumentIdentification = false) {
            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.prepareListForType('" + docListType + "')");
            int counter = 0;
            partialDocumentsList = new List<string[]>();
            foreach (String[] elements in documentsList) {
                if ((withDocumentIdentification == true && elements[2] == docListType) 
                    || (getTagWithoutDocumentIdentification(elements[2]) == docListType)) {
                    partialDocumentsList.Add(elements);
                    counter++;
                }
            }
            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.prepareListForType found '" + partialDocumentsList.Count + "' documents");
            if (partialDocumentsList.Count != 0) {
                partialDocumentsListEnumerator = partialDocumentsList.GetEnumerator();
                lastError = "";
                return counter;
            }
            lastError = "Pas de documents pour '" + docListType + "'";
            return 0;
        }

        /* 
         * Prépare une liste contenant tous les documents 
         */
        override public int prepareCompleteList() {
            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.prepareCompleteList()");
            partialDocumentsList = documentsList;
            if (partialDocumentsList.Count != 0) {
                partialDocumentsListEnumerator = partialDocumentsList.GetEnumerator();
                lastError = "";
                return partialDocumentsList.Count;
            }
            lastError = "Liste de documents vide";
            return 0;
        }

        /* 
         * Se positionne sur le document suivant dans la liste préparée 
         * par prepareListForType ou prepareCompleteList
         * */
        override public bool nextDocument() {
            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.nextDocument");
            if (lastError == "") {
                return partialDocumentsListEnumerator.MoveNext();
            }
            return false;
        }

        /* 
         * Donne le nom de fichier du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getFileName() {
            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.getFileName");
            if (lastError == "") {
                String[] tabCurrent = partialDocumentsListEnumerator.Current;
                if (tabCurrent == null) {
                    addActionError("#DATAERR: Le nom de fichier du document n'a pas été trouvé dans : '" + tabCurrent.ToString() + "'");
                    return "Error not found";
                }
                return tabCurrent[1];
            }
            return lastError;
        }

        /* 
         * Donne le nom du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getName() {
            if (lastError == "") {
                String[] tabCurrent = partialDocumentsListEnumerator.Current;
                if (tabCurrent == null) {
                    addActionError("#DATAERR: Le nom du document n'a pas été trouvé dans : '" + tabCurrent.ToString() + "'");
                    return "Error not found";
                }
                return tabCurrent[3];
            }
            return lastError;
        }

        /* 
         * Donne le date du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getDocumentDate() {
            if (lastError == "") {
                String[] tabCurrent = partialDocumentsListEnumerator.Current;
                if (tabCurrent == null) {
                    addActionError("#DATAERR: La date du document n'a pas été trouvée dans : '" + tabCurrent.ToString() + "'");
                    return "Error not found";
                }
                return tabCurrent[4];
            }
            return lastError;
        }

        /* 
         * Donne la date la plus récente de la liste 
         * préparée par prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getLatestDate() {
            if (latestDate == null) computeDates();
            return latestDate;
        }

        /* 
         * Donne la date la plus ancienne de la liste 
         * préparée par prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getOldestDate() {
            if (oldestDate == null) computeDates();
            return oldestDate;
        }


        /* 
         * Donne la valeur de la clé 
         * */
        override public String getKeyValue(String key) {
            key = "#" + key;
            bool bFound = false;
            String value = "#DATAERR: Le tag : '" + key + "' n'a pas été trouvé dans les données métier";
            if (keyList == null) {
                addActionError(value);
            } else {
                foreach (String[] elements in keyList) {
                    if (elements[1].CompareTo(key) == 0) {
                        value = elements[2];
                        bFound = true;
                    }
                }
            }
            if (!bFound) {
                addActionError(value);
            }
            return value;
        }

        /* 
         * Donne la valeur de la clé pour un tag de document (#cle_tag#num)
         * Si documentTag est vide ou null, seule la clé est cherchée (#cle#num)
         * */
        override public int getNbkeys(String key, String documentTag) {
            key = "#" + key;
            int nbKeys = 0;
            String key2search = key;
            if (documentTag != null && String.Empty != documentTag) {
                key2search += "_" + documentTag;
            }
            // On cherche la clé non suivie de _ pour s'assurer que la clé n'existe pas sous une forme
            // où elle est concaténée avec un nom d'unité documentaire
            // Ex : #KeywordContent#1 à différencier de #KeywordContent_UNITE#1
            Regex r = new Regex(key2search + @"[^_]");
            if (keyList != null) {
                foreach (String[] elements in keyList) {
                    /* Ce test simple non fonctionnel remplacé par la suite
                    if (elements[1].StartsWith(key2search)) {
                        nbKeys++;
                    }
                     * */
                    Match m = r.Match(elements[1]);
                    if (m.Success)
                        nbKeys++;
                }
            }
            if (key2search == currentKey2search) {
                nbKeys -= currentKey2searchCounter;
                if (nbKeys < 0)
                    nbKeys = 0;
            }
            return nbKeys;
        }

        /* 
         * Donne la valeur de la clé pour un tag de document (#cle_tag#num)
         * Si documentTag est vide ou null, seule la clé est cherchée (#cle#num)
         * */
        override public String getNextKeyValue(string key, string documentTag) {
            key = "#" + key;
            String key2search = key;
            if (documentTag != null && String.Empty != documentTag) {
                key2search += "_" + documentTag;
            }
            if (key2search != currentKey2search) {
                currentKey2search = key2search;
                currentKey2searchCounter = 1;
            } else
                currentKey2searchCounter++;
            key = key2search + "#" + currentKey2searchCounter.ToString("D");
            bool bFound = false;
            String value = "#DATAERR: Le tag : '" + key + "' n'a pas été trouvé dans les données métier";
            if (keyList == null) {
                addActionError(value);
            } else {
                foreach (String[] elements in keyList) {
                    if (elements[1].CompareTo(key) == 0) {
                        value = elements[2];
                        bFound = true;
                    }
                }
            }
            if (!bFound) {
                addActionError(value);
            }
            return value;
        }

        private void computeDates() {
            oldestDate = "9999-12-31";
            latestDate = "1970-01-01";
            String date;
            foreach (String[] elements in documentsList) {
                date = elements[4];
                if (date.CompareTo(oldestDate) < 0)
                    oldestDate = date;
                if (date.CompareTo(latestDate) > 0)
                    latestDate = date;
            }
        }
    }
}

