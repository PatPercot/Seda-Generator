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
using System.Collections.Specialized;
using CommonClassesLibrary;

/*
 * SedaSummaryGenerator est une classe abstraite qui offre les méthodes
 * nécessaires pour générer un bordereau de transfert SEDA
 * 
 * Trois types de données sont nécessaires pour générer un bordereau
 * - un profil (ce peut être un fichier RNG, un schéma XSD, un schematron, ...)
 * - un ensemeble de descriptions de documents avec quelques métadonnées et un identifiant
 *   d'unité documentaire du profil
 * - un ensemble d'informations : le producteur, les dates les plus anciennes et récentes,
 *   le cycle de vie des documents
 *   
 * Cette classe utilise un tracesWriter pour tracer les étapes du processus de transformation
 * 
 * Toutes les erreurs détectées sont mémorisées dans une liste
 *  
 * Plusieurs implémentations peuvent être écrites, par exemple RNG, XSD, SCHEMATRON
 *  
 * Comment utiliser cette classe :
 *  Créer un objet
 *  appeler setArchiveDocuments avec un objet dérivé de ArchiveDocuments
 *  éventuellement appeler setTracesWriter
 *  call generateElements()
 *  call close()
 *  getErrorList()
 *  
 * */
namespace SedaSummaryGenerator {
    public abstract class SedaSummaryGenerator {
        /*
         * Toutes ces informations sont nécessaires pour produire un bordereau
         * Des exceptions seront lancées si ces variables sont fausses
         * */
        protected bool profileLoaded = false;
        protected bool archiveDocumentsLoaded = false;
        protected bool informationsLoaded = false;

        protected ArchiveDocuments archiveDocuments;

        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        protected StringCollection errorsList;
        protected bool errorsListCompleted = false;

        public SedaSummaryGenerator() {
            errorsList = new StringCollection();
            archiveDocuments = null;
            unsetTracesWriter();
        }

        /* 
         * Exceptions :
         *  - SedaSummaryGeneratorException et ses sous classes
         *  - 
         * */
        /*
         * un profil est tyoujours contenu dans un fichier
         * */
        abstract protected void prepareProfileWithFile(String profileFile);

        /*
         * La liste des documents à archiver est définie dans un fichier qui 
         * contient le nom des fichiers et leurs métadonnées
         * Les documents sont dans le répertoire documentsFilePath
         * */
        abstract public void prepareArchiveDocumentsWithFile(String documentsFilePath, String archiveDocumentsFile);

        /*
         * Les informations sont toutes les données qui ne sont pas contenues dans le
         * profil. Ces informations sont variables d'un versement ou d'un producteur
         * du versement à un autre
         * Elles sont fournies dans un fichier
         * */
        abstract public void prepareInformationsWithFile(String informationsFile);

        /*
         * informationsDatabase contient une chaîne de connexion
         * accordVersement + baseURI permettent de trouver toutes les informations relatives au versement (profil, producteur, ...)
         * */
        abstract public void prepareInformationsWithDatabase(String informationsDatabase, String baseURI, String accordVersement);


        /*
         * accordVersementConfig contient une configuration d'accord de versement
         * qui définit toutes les informations relatives au versement (profil, producteur, ...)
         * */
        abstract public void prepareInformationsWithConfigFile(SimpleConfig config, String baseURI, String accordVersement, String dataSha1);


        /*
         * C'est le fichier de sortie qui contient le bordereau de transfert construit avec :
         * Exceptions :
         * - SedaSumGenNoInformationsException
         * - SedaSumGenNoArchiveDocumentsException
         * - SedaSumGenNoProfileException
         * 
         * La séquence d'appels pour une production de bordereau est la suivante :
         * 
         *      SedaSummaryGenerator ssg = new SedaSummaryRngGenerator();
         *      ssg.setTracesWriter(streamWriter);
         *      
         *      ssg.prepareInformationsWithDatabase(informationsDatabase, accordVersement);
         *      
         *      ssg.prepareArchiveDocumentsWithFile("liste-fichiers.txt");
         *      
         *      ssg.generateSummaryFile("bordereau.xml");
         *      
         *      ssg.close();
         *      
         *      errors = ssg.getErrorsList();
         *      
         * */
        abstract public void generateSummaryFile(String summaryFile);

        abstract public void close();

        /*
         * Toutes les erreurs détectées peuvent être récupérées dans la liste
         * */
        public StringCollection getErrorsList() {
            if (errorsListCompleted == false) {
                if (archiveDocuments != null) {
                    foreach (String str in archiveDocuments.getErrorsList()) {
                        errorsList.Add(str);
                    }
                }
                errorsListCompleted = true;
            }
            return errorsList;
        }

        /*
         * Gestion des traces de débogage
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
         * Gestion des traces de débogage
         * */
        public void unsetTracesWriter() {
            tracesWriter = null;
            traceActions = false;
        }
    }
}
