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
using System.IO;
using System.Collections.Specialized;

namespace SedaSummaryGenerator {
    /*
     * Classe abstraite qui permet de gérer la liste des documents à archiver
     * L'implémentation CsvArchiveDocuments est actuellement la seule
     * à fournir une classe dérivée
     * 
     * La liste peut aussi contenir des couples clés et valeur sous la forme 
     * #champ valeur variable selon l'implémentation de la liste
     * 
     * La liste de couples clés et valeur peut être utilisée avec des compteurs lorsque 
     * plusieurs éléments peuvent se succéder (exemple plusieurs KeywordContent)
     * Dans ce cas la forme de la clé est #cle#num où num vaut 1, 2... 10, 11...
     * De plus des tags de documents peuvent y être attachés pour les balises apparaissant
     * dans les unités documentaires
     * Dans ce cas, la forme de la clé est #cle_tag#num où num vaut 1, 2... 10, 11...
     * 
     * Les erreurs rencontrées sont conservées dans une liste qui peut être récupérée
     * en fin de traitement
     * */
    public abstract class ArchiveDocuments {
        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        protected StringCollection errorsList;

        public ArchiveDocuments() {
            errorsList = new StringCollection();
        }

        /*
         * Retourne le nombre de documents référençant le type docListType
         * */
        abstract public bool IsThereDocumentsReferringToType(String docListType);

        /*
         * Prépare une liste de documents qui sont identifiés comme docListType
         * retourne le nombre de documents
         * */
        abstract public int prepareListForType(String docListType, bool withDocumentIdentification = false);

        /* 
         * Prépare une liste contenant tous les documents 
         */
        abstract public int prepareCompleteList();

        /* 
         * Se positionne sur le document suivant dans la liste préparée 
         * par prepareListForType ou prepareCompleteList
         * */
        abstract public bool nextDocument();

        /* 
         * Donne le nom de fichier du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        abstract public String getFileName();

        /* 
         * Donne le nom du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        abstract public String getName();

        /* 
         * Donne le date du document courant 
         * (positionné par nextDocument ou prepareCompleteList 
         * ou prepareListForType
         * */
        abstract public String getDocumentDate();

        /* 
         * Donne la date la plus récente de la liste 
         * préparée par prepareCompleteList 
         * ou prepareListForType
         * */
        abstract public String getLatestDate();

        /* 
         * Donne la date la plus ancienne de la liste 
         * préparée par prepareCompleteList 
         * ou prepareListForType
         * */
        abstract public String getOldestDate();

        /* 
         * Donne la valeur de la clé 
         * */
        abstract public String getKeyValue(String key);

        /* 
         * Donne le nombre de clés pour un tag de document (#cle_tag#num)
         * Si documentTag est vide ou null, seule la clé est cherchée (#cle#num)
         * */
        abstract public int getNbkeys(string key, string documentTag);

        /* 
         * Donne la valeur de la clé pour un tag de document (#cle_tag#num)
         * Si documentTag est vide ou null, seule la clé est cherchée (#cle#num)
         * */
        abstract public String getNextKeyValue(string key, string documentTag);

        /*
         * Retourne la liste des erreurs rencontrées
         * */
        public StringCollection getErrorsList() {
            return errorsList;
        }

        /*
         * Permet d'ajouter une erreur
         * */
        protected void addActionError(String action) {
            errorsList.Add("Unable to perform action '" + action + "'. No matching element found in ArchiveDocuments");
        }

        /* 
         * Gestion des traces à fin de débigage
         * */
        public void setTracesWriter(TextWriter tracesWriter) {
            if (tracesWriter != null) {
                this.tracesWriter = tracesWriter;
                traceActions = true;
            }
            else
                unsetTracesWriter();
        }

        /* 
         * Gestion des traces à fin de débigage
         * */
        public void unsetTracesWriter() {
            tracesWriter = null;
            traceActions = false;
        }
    }
}
