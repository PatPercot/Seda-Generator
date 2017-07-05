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
using System.Collections.Specialized;
using CommonClassesLibrary;

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

        Dictionary<string, int> balisesUniques;
        Dictionary<string, int> balisesMultiples;

        public CsvArchiveDocuments() {
            documentsList = new List<String[]>();
            keyList = new List<String[]>();
            lastError = "Pas de données chargées";
        }

        /*
         * Indique si la ligne est une ligne de données ou une ligne de commentaires
         * Les lignes de commentaires commencent par une espace ou une tabulation
         * */
        private bool isThisLineALineOfData(String line) {
            return line.Length > 0 && line[0] != '#';
        }

        private int getNumberOfSeparator(String line) {
            return Utils.nbOccur(line[0], line);
        }
        /*
         * Vérifie si le nombre de séparateurs dans la ligne est correct
         * La ligne doit être une ligne de données isThisLineALineOfData
         * */
        private bool isNumberOfSeparatorCorrect(String line) {
            // Check: comptage du nombre de séparateurs
            int nbSeparator = getNumberOfSeparator(line);
            switch (nbSeparator) {
                case 2:
                case 4:
                case 7:
                    return true;
                default:
                    return false;
            }
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

            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.LoadFile '" + csvFile + "'");
            Regex rgxSeperator;
            String line;
            int nbLine = 0;
            string[] elements;
            try {
                using (StreamReader reader = new StreamReader(csvFile)) {
                    while (reader.Peek() > -1) {
                        nbLine++;
                        line = reader.ReadLine();
                        if (traceActions) tracesWriter.WriteLine(line);
                        if ( isThisLineALineOfData(line) ) {
                            if (! isNumberOfSeparatorCorrect(line)) {
                                addActionError("#DATAERR: Nombre de séparateurs incorrect en ligne '" + nbLine + "'");
                            } else {
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

        public StringCollection getTagsListForKeys() {
            StringCollection liste = new StringCollection();
            foreach (string[] elements in keyList) {
                liste.Add(elements[1]);
            }
            return liste;
        }

        public StringCollection getTagsListForDocuments() {
            StringCollection liste = new StringCollection();
            foreach (string[] elements in documentsList) {
                liste.Add(elements[2]);
            }
            return liste;
        }

        public StringCollection checkFile(String csvFile) {
            Action<Exception> eh = (ex) => {
                if (traceActions) tracesWriter.WriteLine(ex.GetType().Name + " while reading: " + csvFile);
                if (traceActions) tracesWriter.WriteLine(ex.Message);
                throw (ex);
            };

            StringCollection listeAvertissements;
            listeAvertissements = new StringCollection();

            balisesUniques = new Dictionary<string, int>();
            balisesUniques.Add("TransferName", 0);
            balisesUniques.Add("Comment", 0);
            balisesUniques.Add("CustodialHistory", 0);
            balisesUniques.Add("OriginatingAgency.BusinessType", 0);
            balisesUniques.Add("OriginatingAgency.LegalClassification", 0);
            balisesUniques.Add("OriginatingAgency.Description", 0);
            balisesUniques.Add("OriginatingAgency.Identification", 0);
            balisesUniques.Add("OriginatingAgency.Name", 0);

            balisesMultiples = new Dictionary<string, int>();
            balisesMultiples.Add("ContainsName", 0);
            balisesMultiples.Add("ContentDescription.Description", 0);
            balisesMultiples.Add("KeywordContent", 0);

            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.checkFile '" + csvFile + "'");
            Regex rgxSeperator;
            String line;
            int linenumber = 0;
            string[] elements;
            try {
                using (StreamReader reader = new StreamReader(csvFile)) {
                    while (reader.Peek() > -1) {
                        ++linenumber;
                        line = reader.ReadLine();
                        if (traceActions) tracesWriter.WriteLine(line);

                        if (isThisLineALineOfData(line)) {
                            int nbSeparator = getNumberOfSeparator(line);
                            if (isNumberOfSeparatorCorrect(line) == false)
                                    listeAvertissements.Add("ERR: La ligne '" + linenumber + "' contient '" + nbSeparator +
                                        "' séparateurs (" + line[0] + ") alors qu'elle ne peut en contenir que 2, 4 ou 7");

                            rgxSeperator = new Regex("" + line[0]);
                            elements = rgxSeperator.Split(line);
                            /*
                            if (elements[1].Length == 0)
                                listeAvertissements.Add("ERR: le 1er champ de la ligne '" + linenumber +
                                    "' ne doit pas être vide");
                            if (nbSeparator == 4 || nbSeparator == 7) {
                                if (elements[2].Length == 0)
                                    listeAvertissements.Add("ERR: le 2ème champ de la ligne '" + linenumber +
                                        "' ne doit pas être vide");
                            }
                            */
                            if (nbSeparator == 2)
                                checkTagFormat(elements[1], listeAvertissements, line, linenumber);

                            if (nbSeparator == 4 || nbSeparator == 7) {
                                checkDocumentTagFormat(elements[2], listeAvertissements, line, linenumber);

                                // Check: format de la date
                                try {
                                    DateTime date = DateTime.Parse(elements[4], new System.Globalization.CultureInfo("fr-FR", false));
                                } catch (Exception e) {
                                    listeAvertissements.Add("ERR: dans la ligne '" + linenumber +
                                        "' la date '" + elements[4] + "' a un format non reconnu (formats autorisés AAAA-MM-JJ ou AAAA-MM-JJ HH:MM:SS)");
                                }

                            }

                            if (nbSeparator == 7) {
                                // Check: format numérique de la taille
                                try {
                                    long size = long.Parse(elements[7]);
                                } catch (Exception e) {
                                    listeAvertissements.Add("ERR: dans la ligne '" + linenumber +
                                        "' la taille '" + elements[7] + "' a un format non reconnu (format attendu champ numérique sans espaces)");
                                }

                            }

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
            } catch (ArgumentException e) { eh(e); } catch (DirectoryNotFoundException e) { eh(e); } catch (FileNotFoundException e) { eh(e); } catch (OutOfMemoryException e) { eh(e); } catch (IOException e) { eh(e); }

            foreach (KeyValuePair<string, int> kvp in balisesUniques) {
                if (kvp.Value > 1) {
                    listeAvertissements.Add("ERR: la tagname '" + kvp.Key +
                        "' a été trouvée '" + kvp.Value + "' fois alors qu'elle ne doit être présente qu'une seule fois");
                }
            }

            foreach (String error in errorsList) {
                listeAvertissements.Add(error);
            }

            return listeAvertissements;
        }

        /*
         * Réalise une vérification complète du format d'un TAG pour une tagname
         * #TransferName pour TAG non répétable
         * #ContainsName[TAG] pour TAG non répétable
         * #ContainsName[TAG[#1]] pour TAG répétable
         * */
        protected void checkTagFormat(String tag, StringCollection listeAvertissements, String line, int linenumber) {
            // Check: vérification du premier caractère du nom de tagname
            if (tag.Length != 0 && tag[0] != '#') {
                listeAvertissements.Add("ERR: le 1er champ de la ligne '" + linenumber +
                    "' commence par '" + tag[0] + "' alors qu'il devrait commencer par #");
                return;
            }

            String balisename = String.Empty;
            Regex extractname = new Regex(@"^#(\w[\w\d_.]+)");
            MatchCollection matches = extractname.Matches(tag);
            if (matches.Count == 0) { // Pas de tagname trouvée
                listeAvertissements.Add("ERR: dans la ligne '" + linenumber +
                    "' le 1er champ ne contient pas de nom de balise");
                return;
            }
            if (matches.Count == 1) {
                foreach (Match match in matches) { // un seul passage
                    balisename = match.Groups[1].Value;
                }
            }
            bool formatOK = false;
            if (balisename.Equals("KeywordContent")) {
                Regex keywordFormat = new Regex(@"^#KeywordContent\[((\w[\w\d_]+)(\[#\d+\])?((//\w[\w\d_]+)(\[#\d+\])?)*)?({\w[\w\d_]+})(\[#\d+\])?\]$");
                matches = keywordFormat.Matches(tag);
                if (matches.Count == 1)
                    formatOK = true;
            } else {
                Regex simpleOrIndexedformat = new Regex ("^#" + balisename + @"(\[#\d+\])?$");
                /*
                Regex taggedFormat = new Regex("^#" + balisename + @"\[\w([\w\d_]+)?\]$");
                Regex taggedIndexedFormat = new Regex("^#" + balisename + @"\[\w([\w\d_]+)\[#\d+\]\]$");
                Regex taggedIndexedIndexedFormat = new Regex("^#" + balisename + @"\[\w([\w\d_]+)\[#\d+\]\[#\d+\]\]$");
                */

                String fullTagformat = @"(\w[\w\d_]+)(\[#\d+\])?((//\w[\w\d_]+)(\[#\d+\])?)*({\w[\w\d_]+})?";
                Regex globalFormat = new Regex("^#" + balisename + @"\[" + fullTagformat + @"(\[#\d+\])?\]$");
                matches = simpleOrIndexedformat.Matches(tag);
                if (matches.Count == 1)
                    formatOK = true;
                else {
                    matches = globalFormat.Matches(tag);
                    if (matches.Count == 1)
                        formatOK = true;
                }
                    /*
                    matches = simpleOrIndexedformat.Matches(tag);
                    if (matches.Count == 1) 
                        formatOK = true;
                    else {
                        matches = taggedFormat.Matches(tag);
                        if (matches.Count == 1)
                            formatOK = true;
                        else {
                            matches = taggedIndexedFormat.Matches(tag);
                            if (matches.Count == 1)
                                formatOK = true;
                            else {
                                matches = taggedIndexedIndexedFormat.Matches(tag);
                                if (matches.Count == 1)
                                    formatOK = true;
                            }
                        }
                    }
                     * */

            }
            if (formatOK != true) {
                listeAvertissements.Add("ERR: dans la ligne '" + linenumber +
                    "' le 1er champ ne correspond pas à un des formats possibles : #tagname, #tagname[TAG] ou #tagname[TAG[#num]]");
            }

            int nbOccurrences;
            if (balisesUniques.TryGetValue(balisename, out nbOccurrences)) {
                balisesUniques[balisename] = nbOccurrences + 1;
            } else {
				if (balisesMultiples.TryGetValue(balisename, out nbOccurrences)) {
					balisesMultiples[balisename] = nbOccurrences + 1;
				} else {
					if (tag.Equals("#ArchivalAgreement")) {
						listeAvertissements.Add("ERR: la ligne '" + linenumber +
							"' contient une balise '" + tag +
							"' qui ne doit pas se trouver dans les données métier, car son" +
							" contenu provient du fichier de configuration (job.config)");
					} else {

						listeAvertissements.Add("ERR: la ligne '" + linenumber +
							"' contient une balise '" + tag +
							"' qui n'est pas reconnue et ne sera pas traitée");
					}
				}
            }
        }

        /*
         * Réalise une vérification complète du format d'un TAG pour un document
         * TAG
         * TAG[#1]
         * TAG[#1]{DOC}
         * */
        protected void checkDocumentTagFormat(String tag, StringCollection listeAvertissements, String line, int linenumber) {
            String tagname = String.Empty;
            Regex extractname = new Regex(@"^(\w[\w\d_]+)");
            MatchCollection matches = extractname.Matches(tag);
            if (matches.Count == 0) { // Pas de tagname trouvée
                listeAvertissements.Add("ERR: dans la ligne '" + linenumber +
                    "' le 2ème champ ne contient pas de nom de tag");
                return;
            }
            if (matches.Count == 1) {
                foreach (Match match in matches) { // un seul passage
                    tagname = match.Groups[1].Value;
                }
            }
            Regex tagformat = new Regex("^" + tagname + @"(\[#\d+\])?((//\w[\w\d_]+)(\[#\d+\])?)*({\w[\w\d_]+})?$");
            bool formatOK = false;
            matches = tagformat.Matches(tag);
            if (matches.Count == 1)
                formatOK = true;
            if (formatOK != true) {
                listeAvertissements.Add("ERR: dans la ligne '" + linenumber +
                    "' le 2ème champ ne correspond pas à un des formats possibles : tagname, tagname{doc}, tagname[#num] ou tagname[#num]{doc}");
            }
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
        override public String getDocumentFilename() {
            if (traceActions) tracesWriter.WriteLine("ArchiveDocuments.getDocumentFilename");
            if (lastError == "") {
                String[] tabCurrent = partialDocumentsListEnumerator.Current;
                if (tabCurrent == null) {
                    addActionError("#DATAERR: Le nom de fichier du document n'a pas été trouvé dans partialDocumentsListEnumerator");
                    return "Error not found";
                }
                return tabCurrent.Length > 1 ? tabCurrent[1] : String.Empty;
            }
            return lastError;
        }

        /* 
         * Donne le nom du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getDocumentName() {
            if (lastError == "") {
                String[] tabCurrent = partialDocumentsListEnumerator.Current;
                if (tabCurrent == null) {
                    addActionError("#DATAERR: Le nom du document n'a pas été trouvé dans partialDocumentsListEnumerator");
                    return "Error not found";
                }
                return tabCurrent.Length > 3 ? tabCurrent[3] : String.Empty;
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
                    addActionError("#DATAERR: La date du document n'a pas été trouvée dans partialDocumentsListEnumerator");
                    return "Error not found";
                }
                return tabCurrent.Length > 4 ? tabCurrent[4] : String.Empty;
            }
            return lastError;
        }

        /* 
         * Donne l'algorithme de l'empreinte du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getDocumentHashAlgorithm() {
            if (lastError == "") {
                String[] tabCurrent = partialDocumentsListEnumerator.Current;
                if (tabCurrent == null) {
                    addActionError("#DATAERR: L'algorithme du hash du document n'a pas été trouvé dans partialDocumentsListEnumerator");
                    return "Error not found";
                }
                // Attention, ici on attend un algorithme et une empreinte, donc on impose la présence de deux champs à suivre
                // D'où la comparaison de la longueur avec 6 (le sixième champ qui contient l'empreinte)
                // On rappelle que le champ 0 est vide
                return tabCurrent.Length > 6 ? tabCurrent[5] : String.Empty;
            }
            return lastError;
        }

        /* 
         * Donne l'empreinte du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getDocumentHash() {
            if (lastError == "") {
                String[] tabCurrent = partialDocumentsListEnumerator.Current;
                if (tabCurrent == null) {
                    addActionError("#DATAERR: Le hash du document n'a pas été trouvé dans partialDocumentsListEnumerator");
                    return "Error not found";
                }
                // On rappelle que le champ 0 est vide
                return tabCurrent.Length > 6 ? tabCurrent[6] : String.Empty;
            }
            return lastError;
        }

        /* 
         * Donne la taille en octets du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        override public String getDocumentSize() {
            if (lastError == "") {
                String[] tabCurrent = partialDocumentsListEnumerator.Current;
                if (tabCurrent == null) {
                    addActionError("#DATAERR: La taille du document n'a pas été trouvée dans partialDocumentsListEnumerator");
                    return "Error not found";
                }
                // On rappelle que le champ 0 est vide
                return tabCurrent.Length > 7 ? tabCurrent[7] : String.Empty;
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
         * Donne la valeur de la clé pour un tag de document (#cle[tag[#num]])
         * Si documentTag est vide ou null, seule la clé est cherchée (#cle[#num])
         * */
        override public int getNbkeys(String key, String documentTag) {
            key = "#" + key;
            int nbKeys = 0;
            String key2search = key, reKey2search = key;
            if (documentTag != null && String.Empty != documentTag) {
                key2search += "[" + documentTag;
                reKey2search += @"\[" + documentTag;
            }
            // On cherche la clé suivie de [#compteur]
            Regex r = new Regex("^" + reKey2search + @"\[#\d+\]");
            if (keyList != null) {
                foreach (String[] elements in keyList) {
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
         * Donne la valeur de la clé pour un tag de document (#cle[tag[#num]])
         * Si documentTag est vide ou null, seule la clé est cherchée (#cle[#num])
         * */
        override public String getNextKeyValue(string key, string documentTag) {
            key = "#" + key;
            String key2search = key;
            if (documentTag != null && String.Empty != documentTag) {
                key2search += "[" + documentTag;
            }

            if (key2search != currentKey2search) {
                currentKey2search = key2search;
                currentKey2searchCounter = 1;
            } else
                currentKey2searchCounter++;

            if (documentTag != null && String.Empty != documentTag) {
                key = key2search + "[#" + currentKey2searchCounter.ToString("D") + "]]";
            } else {
                key = key2search + "[#" + currentKey2searchCounter.ToString("D") + "]";
            }

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
                if (elements.Length > 4) {
                    date = elements[4];
                    if (date.CompareTo(oldestDate) < 0)
                        oldestDate = date;
                    if (date.CompareTo(latestDate) > 0)
                        latestDate = date;
                }
            }
        }
    }
}

