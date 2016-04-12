using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;

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
            /*
            StringCollection erreursDonnees = ad.getErrorsList();
            if (erreursDonnees.Count != 0) {
                foreach (String st in erreursDonnees) {
                    errors.Add(st);
                }
            }
            */
            RngProfileController rpc = new RngProfileController();
            if (traceActions)
                rpc.setTracesWriter(tracesWriter);
            rpc.controlProfileFile(profileFile);
            /*
            StringCollection erreursProfil = rpc.getErrorsList();
            if (erreursProfil.Count != 0) {
                foreach (String st in erreursProfil) {
                    errors.Add(st);
                }
            }
            */
            String str;
            StringCollection tagsForKeys = ad.getTagsListForKeys();
            StringCollection tagsForDocs = ad.getTagsListForDocuments();
            StringCollection expectedTags = rpc.getExpectedTagsListList();

            StringCollection expectedTagsForDocs = new StringCollection();
            foreach (String st in expectedTags) {
                if (st.StartsWith("document: ")) {
                    expectedTagsForDocs.Add(st.Substring(10));
                }
            }
            foreach (String st in expectedTagsForDocs) {
                expectedTags.Remove("document: " + st);
            }

            StringCollection tagsForKeysModified = new StringCollection();
            foreach (String st in tagsForKeys) {
                str = Regex.Replace(st, @"#KeywordContent\[#[0-9]+\]", "#KeywordContent");
                tagsForKeysModified.Add(str);
            }

            StringCollection tagsForDocsModified = new StringCollection();
            foreach (String st in tagsForDocs) {
                str = Regex.Replace(st, @"#KeywordContent(\[[^]]+\])?\[#[0-9]+\]", "#KeywordContent$1");
                tagsForDocsModified.Add(str);
            }

            if (traceActions) {
                if (tagsForKeysModified.Count != 0) {
                    tracesWriter.WriteLine("\ntagsForKeysModified");
                    foreach (String st in tagsForKeysModified) {
                        tracesWriter.WriteLine(st);
                    }
                }
                if (tagsForDocsModified.Count != 0) {
                    tracesWriter.WriteLine("\ntagsForDocsModified");
                    foreach (String st in tagsForDocsModified) {
                        tracesWriter.WriteLine(st);
                    }
                }
                if (expectedTags.Count != 0) {
                    tracesWriter.WriteLine("\nexpectedTags");
                    foreach (String st in expectedTags) {
                        tracesWriter.WriteLine(st);
                    }
                }
                if (expectedTagsForDocs.Count != 0) {
                    tracesWriter.WriteLine("\nexpectedTagsForDocs");
                    foreach (String st in expectedTagsForDocs) {
                        tracesWriter.WriteLine(st);
                    }
                }
                tracesWriter.WriteLine("");
            }

            
            // Test des clés
            foreach (String st in tagsForKeysModified) {
                str = Regex.Replace(st, @"\[#\d+\]", "[#1]");
                str = Regex.Replace(str, @"#KeywordContent\[#[0-9]+\]", "#KeywordContent");
                if (expectedTags.IndexOf(str) == -1)
                    errors.Add("La clé '" + st + "' fournie par les données métier n'est pas attendue par le profil");
            }

            // Test des documents
            foreach (String st in tagsForDocsModified) {
                str = Regex.Replace(st, @"\[#\d+\]", "[#1]");
                str = Regex.Replace(str, @"#KeywordContent(\[[^]]+\])?\[#[0-9]+\]", "#KeywordContent$1");
                if (expectedTagsForDocs.IndexOf(str) == -1)
                    errors.Add("Le document typé par le tag '" + st + "' n'est pas attendu par le profil");
            }

            // Test du profil
            foreach (String st in expectedTags) {
                if (tagsForKeysModified.IndexOf(st) == -1) {
                    errors.Add("Dans le profil, le tag '" + st + "' ne trouve pas de correspondance dans les données métier");
                }
            }
            foreach (String st in expectedTagsForDocs) {
                if (tagsForDocsModified.IndexOf(st) == -1) {
                    errors.Add("Dans le profil, le tag de document '" + st + "' ne trouve pas de correspondance dans les données métier");
                }
            }
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
