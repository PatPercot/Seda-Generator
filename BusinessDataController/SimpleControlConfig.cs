using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace BusinessDataController {
    class SimpleControlConfig {
        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        protected StringCollection errorsList;

        public SimpleControlConfig() {
            errorsList = new StringCollection();
        }

        public void loadFile(String configFile) {
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
            } catch (ArgumentException e) { eh(e); } catch (DirectoryNotFoundException e) { eh(e); } catch (FileNotFoundException e) { eh(e); } catch (OutOfMemoryException e) { eh(e); } catch (IOException e) { eh(e); }

        }
    }
}
