using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace BusinessDataController {
    /*
     * Les classes Job, ProfileControl, DataControl et Generator
     * contiennent les éléments nécessaires à la réalistion d'un contrôle 
     * ou d'une génération, à savoir les noms de fichiers qui sont concernés 
     * par le job.
     * */
    protected class Job {
        public String nomJob;
        public String traceFile;
    }

    class ProfileControl : Job {
        public String profileFile;
    }

    class DataControl : ProfileControl {
        public String dataFile;
    }

    class Generator : DataControl {
        public String accordVersment;
        public String bordereauFile;
    }

    class SimpleConfig {
        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        protected StringCollection errorsList;

        protected List<Generator> generatorList;
        protected List<DataControl> datacontrolList;
        protected List<ProfileControl> profilecontrolList;
        
        public SimpleConfig() {
            errorsList = new StringCollection();
            generatorList = new List<Generator>();
            datacontrolList = new List<DataControl>();
            profilecontrolList = new List<ProfileControl>();
        }

        public void loadFile(String configFile) {
            Action<Exception> eh = (ex) => {
                if (traceActions) tracesWriter.WriteLine(ex.GetType().Name + " while reading: " + configFile);
                if (traceActions) tracesWriter.WriteLine(ex.Message);
                throw (ex);
            };

            if (traceActions) tracesWriter.WriteLine("SimpleControlConfig.LoadFile");
            Regex sectionRegex = new Regex(@"^\s*\[(\w+):(\w+)\]\s*$");
            Regex fileRegex;
            Match m;
            String section = String.Empty;
            String line;
            bool inSection = false;
            string[] elements;
            try {
                using (StreamReader reader = new StreamReader(configFile)) {
                    while (reader.Peek() > -1) {
                        line = reader.ReadLine();
                        if (traceActions) tracesWriter.WriteLine(line);
                        line = line.Trim();
                        // on ne traite que les lignes non vides ou ne débutant pas par #
                        if (line.Length > 0 || ! line.StartsWith("#")) { 
                            m = sectionRegex.Match(line);
                            if (m.Success) {
                                Group g = m.Groups[0];
                                section = g.ToString();
                                String authorizedFiles;
                                inSection = true;
                                if (section.Equals("profile-control"))    authorizedFiles = "trace|profil";
                                else if (section.Equals("data-control"))  authorizedFiles = "trace|profil|data";
                                else if (section.Equals("generator"))     authorizedFiles = "trace|profil|data|bordereau";
                                else {
                                    String errMsg = "Le nom de section '" + section + "' n'existe pas (choix entre profile-control, data-control ou generator).";
                                    if (traceActions) tracesWriter.WriteLine(errMsg);
                                    errorsList.Add(errMsg);
                                    inSection = false;
                                }
                                if (inSection) {
                                    fileRegex = new Regex(@"^\s*(" + authorizedFiles + @")\s*=\s*(\w+)\s*$");
                                }
                            } else {
                                if (inSection == true) {
                                    m = fileRegex.Match(line);
                                    if (m.Success) {
                                        Group g = m.Groups[0];
                                        if (g.ToString().Equals("trace")) {

                                        }
                                    }
                                }
                            }
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
