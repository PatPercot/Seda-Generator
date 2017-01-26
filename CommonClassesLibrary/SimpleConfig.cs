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
     * La classe AccordVersementConfig sert à contenir les paramètres d'un
     * accord de versement dans le cas où la base de données n'est pas utilisée
     * */
    public class AccordVersementConfig {
        public String accordVersement = String.Empty;
        public String SAE_Serveur = String.Empty;
        public String TransferIdPrefix = String.Empty;
        public String SAE_ProfilArchivage = String.Empty;
        public String TransferringAgencyId = String.Empty;
        public String TransferringAgencyName = String.Empty;
        public String TransferringAgencyDesc = String.Empty;
        public String ArchivalAgencyId = String.Empty;
        public String ArchivalAgencyName = String.Empty;
        public String ArchivalAgencyDesc = String.Empty;
    }

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
        public String repDocuments = String.Empty;
        public String baseURI = String.Empty;
    }

    public class SimpleConfig {
        protected TextWriter tracesWriter = null;
        protected bool traceActions = false;

        protected StringCollection errorsList;

        protected List<ProfileControlConfig> profilecontrolList;
        protected List<DataControlConfig> datacontrolList;
        protected List<GeneratorConfig> generatorList;
        protected List<AccordVersementConfig> accordVersementConfigList;

        protected String section, sectionName, traceFile, profileFile, dataFile, 
            repDocuments, baseURI, bordereauFile, accordVersement;
        protected String SAE_Serveur, TransferIdPrefix, SAE_ProfilArchivage, 
            TransferringAgencyId, TransferringAgencyName, TransferringAgencyDesc, 
            ArchivalAgencyId, ArchivalAgencyName, ArchivalAgencyDesc;

        protected bool inSection = false;

        public SimpleConfig() {
            errorsList = new StringCollection();
            generatorList = new List<GeneratorConfig>();
            datacontrolList = new List<DataControlConfig>();
            profilecontrolList = new List<ProfileControlConfig>();
            accordVersementConfigList = new List<AccordVersementConfig>();
        }

        // Vérifie s'il y a des accords de versement
        public bool hasAccordVersementConfig() {
            return accordVersementConfigList.Count > 0;
        }

        // Retourne l'accord de versement demandé pour un SAE donné
        public AccordVersementConfig getAccordVersementConfig(String accordName, String SAE_Serveur) {
            foreach (AccordVersementConfig a in accordVersementConfigList) {
                if (a.accordVersement.Equals(accordName) && a.SAE_Serveur.Equals(SAE_Serveur))
                    return a;
            }
            return null;
        }

        // Retourne la configuration demandée (ou la configuration default sinon la première si le nom de config est vide)
        public ProfileControlConfig getProfileConfig(String configName) {
            if (configName == String.Empty)
                configName = "default";
            foreach (ProfileControlConfig c in profilecontrolList) {
                if (c.nomJob.Equals(configName))
                    return c;
            }
            if (configName.Equals("default") && profilecontrolList.Count >= 1)
                return profilecontrolList.ElementAt(0);
            return null;
        }

        // Retourne la configuration demandée ou la première si le nom de config est vide
        public DataControlConfig getDatacontrolConfig(String configName) {
            if (configName == String.Empty)
                configName = "default";
            foreach (DataControlConfig c in datacontrolList) {
                if (c.nomJob.Equals(configName))
                    return c;
            }
            if (configName.Equals("default") && datacontrolList.Count >= 1)
                return datacontrolList.ElementAt(0);
            return null;
        }

        // Retourne la configuration demandée ou la première si le nom de config est vide
        public GeneratorConfig getGeneratorConfig(String configName) {
            if (configName == String.Empty)
                configName = "default";
            foreach (GeneratorConfig c in generatorList) {
                if (c.nomJob.Equals(configName))
                    return c;
            }
            if (configName.Equals("default") && generatorList.Count >= 1)
                return generatorList.ElementAt(0);
            return null;
        }

        protected void doSection() {
            if (inSection) {
                switch (section) {
                    case "accord-versement":
                        AccordVersementConfig paccord = new AccordVersementConfig();
                        paccord.accordVersement = sectionName;
                        paccord.SAE_Serveur = SAE_Serveur;
                        paccord.TransferIdPrefix = TransferIdPrefix;
                        paccord.SAE_ProfilArchivage = SAE_ProfilArchivage;
                        paccord.TransferringAgencyId = TransferringAgencyId;
                        paccord.TransferringAgencyName = TransferringAgencyName;
                        paccord.TransferringAgencyDesc = TransferringAgencyDesc;
                        paccord.ArchivalAgencyId = ArchivalAgencyId;
                        paccord.ArchivalAgencyName = ArchivalAgencyName;
                        paccord.ArchivalAgencyDesc = ArchivalAgencyDesc;
                        accordVersementConfigList.Add(paccord);
                        break;
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
                        generator.repDocuments = repDocuments;
                        generator.baseURI = baseURI;
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
                                String authorizedKeys = String.Empty;
                                inSection = true;
                                if (section.Equals("profile-control"))    
                                    authorizedKeys = "trace|profil";
                                else if (section.Equals("data-control"))  
                                    authorizedKeys = "trace|profil|data";
                                else if (section.Equals("generator"))
                                    authorizedKeys = "trace|accord|data|rep_documents|baseURI|bordereau";
                                else if (section.Equals("accord-versement")) 
                                    authorizedKeys = "SAE_Serveur|TransferIdPrefix|SAE_ProfilArchivage|TransferringAgencyId|TransferringAgencyName|TransferringAgencyDesc|ArchivalAgencyId|ArchivalAgencyName|ArchivalAgencyDesc";
                                else {
                                    String errMsg = "Le nom de section '" + section + "' n'existe pas (choix entre profile-control, data-control ou generator).";
                                    if (traceActions) tracesWriter.WriteLine(errMsg);
                                    errorsList.Add(errMsg);
                                    inSection = false;
                                }
                                if (inSection) {
                                    sectionName = m.Groups[2].ToString();
                                    traceFile = profileFile = dataFile = bordereauFile = repDocuments = baseURI = accordVersement = String.Empty;
                                    SAE_Serveur = TransferIdPrefix = SAE_ProfilArchivage = TransferringAgencyId = TransferringAgencyName =
                                        TransferringAgencyDesc = ArchivalAgencyId = ArchivalAgencyName = ArchivalAgencyDesc = String.Empty;
                                    fileRegex = new Regex(@"^\s*(" + authorizedKeys + @")\s*=\s*([-a-zA-Z0-9_./:]+)\s*$");
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
                                        if (g.ToString().Equals("rep_documents"))
                                            repDocuments = m.Groups[2].ToString();
                                        if (g.ToString().Equals("baseURI"))
                                            baseURI = m.Groups[2].ToString();
                                        if (g.ToString().Equals("data"))
                                            dataFile = m.Groups[2].ToString();
                                        if (g.ToString().Equals("bordereau"))
                                            bordereauFile = m.Groups[2].ToString();

                                        if (g.ToString().Equals("SAE_Serveur"))
                                            SAE_Serveur = m.Groups[2].ToString();
                                        if (g.ToString().Equals("TransferIdPrefix"))
                                            TransferIdPrefix = m.Groups[2].ToString();
                                        if (g.ToString().Equals("SAE_ProfilArchivage"))
                                            SAE_ProfilArchivage = m.Groups[2].ToString();
                                        if (g.ToString().Equals("TransferringAgencyId"))
                                            TransferringAgencyId = m.Groups[2].ToString();
                                        if (g.ToString().Equals("TransferringAgencyName"))
                                            TransferringAgencyName = m.Groups[2].ToString();
                                        if (g.ToString().Equals("TransferringAgencyDesc"))
                                            TransferringAgencyDesc = m.Groups[2].ToString();
                                        if (g.ToString().Equals("ArchivalAgencyId"))
                                            ArchivalAgencyId = m.Groups[2].ToString();
                                        if (g.ToString().Equals("ArchivalAgencyName"))
                                            ArchivalAgencyName = m.Groups[2].ToString();
                                        if (g.ToString().Equals("ArchivalAgencyDesc"))
                                            ArchivalAgencyDesc = m.Groups[2].ToString();                                   
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
