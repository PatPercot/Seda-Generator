using System;
using System.Collections.Specialized;
using System.IO;

namespace SedaSummaryGenerator {
    public class BusinessDataController {
        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        public StringCollection controlDataFormat(String dataFile) {
            CsvArchiveDocuments ad = new CsvArchiveDocuments();
            if (traceActions)
                ad.setTracesWriter(tracesWriter);
            return ad.checkFile(dataFile);
        }

        public StringCollection controlMatchingBetweenDataAndProfile(String dataFile, String profileFile) {
            StringCollection errors = new StringCollection();
            CsvArchiveDocuments ad = new CsvArchiveDocuments();
            if (traceActions)
                ad.setTracesWriter(tracesWriter);
            ad.loadFile(dataFile);

            StringCollection tagsForKeys = ad.getTagsListForKeys();
            StringCollection tagsForDocs = ad.getTagsListForDocuments();

            return errors;
        }
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
