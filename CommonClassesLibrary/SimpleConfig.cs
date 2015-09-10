using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace CommonClassesLibrary {
    /*
     * Les classes Job, ProfileControlConfig, DataControlConfig et GeneratorConfig
     * contiennent les éléments nécessaires à la réalistion d'un contrôle 
     * ou d'une génération, à savoir les noms de fichiers qui sont concernés 
     * par le job.
     * */
    public class Job {
        public String nomJob = String.Empty;
        public String traceFile = String.Empty;
    }

    public class ProfileControlConfig : Job {
        public String profileFile = String.Empty;
    }

    public class DataControlConfig : ProfileControlConfig {
        public String dataFile = String.Empty;
    }

    public class GeneratorConfig : Job {
        public String accordVersement = String.Empty;
        public String bordereauFile = String.Empty;
        public String dataFile = String.Empty;
    }

    public class SimpleConfig {
        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        protected StringCollection errorsList;

        protected List<ProfileControlConfig> profilecontrolList;
        protected List<DataControlConfig> datacontrolList;
        protected List<GeneratorConfig> generatorList;

        protected String section, sectionName, traceFile, profileFile, dataFile, bordereauFile, accordVersement;
        protected bool inSection = false;

        public SimpleConfig() {
            errorsList = new StringCollection();
            generatorList = new List<GeneratorConfig>();
            datacontrolList = new List<DataControlConfig>();
            profilecontrolList = new List<ProfileControlConfig>();
        }

        // Retourne la configuration demandée ou la première si le nom de config est vide
        public ProfileControlConfig getProfileConfig(String configName) {
            ProfileControlConfig config = null;
            foreach (ProfileControlConfig c in profilecontrolList) {
                if (configName == String.Empty || c.nomJob.Equals(configName))
                    return c;
            }
            return config;
        }

        // Retourne la configuration demandée ou la première si le nom de config est vide
        public DataControlConfig getDatacontrolConfig(String configName) {
            DataControlConfig config = null;
            foreach (DataControlConfig c in datacontrolList) {
                if (configName == String.Empty || c.nomJob.Equals(configName))
                    return c;
            }
            return config;
        }

        // Retourne la configuration demandée ou la première si le nom de config est vide
        public GeneratorConfig getGeneratorConfig(String configName) {
            GeneratorConfig config = null;
            foreach (GeneratorConfig c in generatorList) {
                if (configName == String.Empty || c.nomJob.Equals(configName))
                    return c;
            }
            return config;
        }

        protected void doSection() {
            if (inSection) {
                switch (section) {
                    case "profile-control":
                        ProfileControlConfig pcontrol = new ProfileControlConfig();
                        pcontrol.nomJob = sectionName;
                        pcontrol.traceFile = traceFile;
                        pcontrol.profileFile = profileFile;
                        profilecontrolList.Add(pcontrol);
                        break;
                    case "data-control":
                        DataControlConfig dcontrol = new DataControlConfig();
                        dcontrol.nomJob = sectionName;
                        dcontrol.traceFile = traceFile;
                        dcontrol.profileFile = profileFile;
                        dcontrol.dataFile = dataFile;
                        datacontrolList.Add(dcontrol);
                        break;
                    case "generator":
                        GeneratorConfig generator = new GeneratorConfig();
                        generator.nomJob = sectionName;
                        generator.traceFile = traceFile;
                        generator.accordVersement = accordVersement;
                        generator.dataFile = dataFile;
                        generator.bordereauFile = bordereauFile;
                        generatorList.Add(generator);
                        break;
                }
            }
        }


        public String loadFile(String configFile) {
            String retourMsg = String.Empty;
            Action<Exception> eh = (ex) => {
                if (traceActions) tracesWriter.WriteLine(ex.GetType().Name + " while reading: " + configFile);
                if (traceActions) tracesWriter.WriteLine(ex.Message);
                retourMsg = ex.Message;
            };

            if (traceActions) tracesWriter.WriteLine("SimpleControlConfig.LoadFile");
            Regex sectionRegex = new Regex(@"^\s*\[([-a-zA-Z0-9_]+)\s*:\s*([-a-zA-Z0-9_./:]+)\]\s*$");
            Regex fileRegex = null;
            Match m;
            String line;
            try {
                using (StreamReader reader = new StreamReader(configFile)) {
                    while (reader.Peek() > -1) {
                        line = reader.ReadLine();
                        if (traceActions) tracesWriter.WriteLine(line);
                        line = line.Trim();
                        // on ne traite que les lignes non vides ou ne débutant pas par #
                        if (line.Length > 0 && ! line.StartsWith("#")) { 
                            m = sectionRegex.Match(line);
                            if (m.Success) {
                                if (inSection)  // Potentiellement on change de section
                                    doSection();
                                section = m.Groups[1].ToString();
                                String authorizedFiles = String.Empty;
                                inSection = true;
                                if (section.Equals("profile-control"))    authorizedFiles = "trace|profil";
                                else if (section.Equals("data-control"))  authorizedFiles = "trace|profil|data";
                                else if (section.Equals("generator"))     authorizedFiles = "trace|accord|data|bordereau";
                                else {
                                    String errMsg = "Le nom de section '" + section + "' n'existe pas (choix entre profile-control, data-control ou generator).";
                                    if (traceActions) tracesWriter.WriteLine(errMsg);
                                    errorsList.Add(errMsg);
                                    inSection = false;
                                }
                                if (inSection) {
                                    sectionName = m.Groups[2].ToString();
                                    traceFile = profileFile = dataFile = bordereauFile = accordVersement = String.Empty;
                                    fileRegex = new Regex(@"^\s*(" + authorizedFiles + @")\s*=\s*([-a-zA-Z0-9_./:]+)\s*$");
                                }
                            } else { // if (m.Success) 
                                if (inSection == true) {
                                    m = fileRegex.Match(line);
                                    if (m.Success) {
                                        Group g = m.Groups[1];
                                        if (g.ToString().Equals("trace"))
                                            traceFile = m.Groups[2].ToString();
                                        if (g.ToString().Equals("profil"))
                                            profileFile = m.Groups[2].ToString();
                                        if (g.ToString().Equals("accord"))
                                            accordVersement = m.Groups[2].ToString();
                                        if (g.ToString().Equals("data"))
                                            dataFile = m.Groups[2].ToString();
                                        if (g.ToString().Equals("bordereau"))
                                            bordereauFile = m.Groups[2].ToString();
                                    }
                                }
                            } // if (line.Length > 0 || ! line.StartsWith("#")) 
                        } // while (reader.Peek() > -1) 
                    } // using (StreamReader reader = new StreamReader(configFile)) 
                    if (inSection)
                        doSection();
                    reader.Close();
                }
            } 
            catch (ArgumentException e) { eh(e); } 
            catch (DirectoryNotFoundException e) { eh(e); } 
            catch (FileNotFoundException e) { eh(e); } 
            catch (OutOfMemoryException e) { eh(e); } 
            catch (IOException e) { eh(e); }
            return retourMsg;
        }
    }
}
