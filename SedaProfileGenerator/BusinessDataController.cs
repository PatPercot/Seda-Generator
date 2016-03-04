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

        /*
         * Gestion des traces de débigage
         * */
        public void setTracesWriter(TextWriter tracesWriter) {
            if (tracesWriter != null) {
                this.tracesWriter = tracesWriter;
                traceActions = true;
            } else
                unsetTracesWriter();
        }

        /*
         * Gestion des traces de débigage
         * */
        public void unsetTracesWriter() {
            tracesWriter = null;
            traceActions = false;
        }

    }
}
