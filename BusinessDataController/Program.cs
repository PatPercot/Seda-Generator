using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SedaSummaryGenerator;
using CommonClassesLibrary;
using System.Collections.Specialized;

/* Premiers éléments à contrôler
 * 
 * 
Document/Description									Métier			Champ Nom (position 3)
Document/Creation										Métier			Champ Date (position 4)
Document/Attachment 				filename			Métier			Champ Nom (position 1)
/ArchiveTransfer/Archive/Name															Métier			#TransferName								1.0
/ArchiveTransfer/Contains/Name															Métier			#TransferName								0.2
/ArchiveTransfer/Comment																Métier			#Comment
/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/BusinessType				Métier			#OriginatingAgency.BusinessType				1.0	
/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/Identification			Métier			#OriginatingAgency.Identification			1.0	
/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/Description				Métier			#OriginatingAgency.Description				1.0	
/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/LegalClassification		Métier			#OriginatingAgency.LegalClassification		1.0	
/ArchiveTransfer/Archive/ContentDescription/OriginatingAgency/Name						Métier			#OriginatingAgency.Name						1.0	
/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/BusinessType				Métier			#OriginatingAgency.BusinessType				0.2	
/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/Identification			Métier			#OriginatingAgency.Identification			0.2	
/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/Description				Métier			#OriginatingAgency.Description				0.2
/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/LegalClassification		Métier			#OriginatingAgency.LegalClassification		0.2
/ArchiveTransfer/Contains/ContentDescription/OriginatingAgency/Name						Métier			#OriginatingAgency.Name						0.2
/ArchiveTransfer/Archive/ContentDescription/CustodialHistory							Métier			#CustodialHistory							1.0
/ArchiveTransfer/Contains/ContentDescription/CustodialHistory							Métier			#CustodialHistory							0.2
/ArchiveTransfer/Archive/ContentDescription/Keyword/KeywordContent						Métier			#KeywordContent[#x]							1.0
/ArchiveTransfer/Contains/ContentDescription/ContentDescriptive/KeywordContent			Métier			#KeywordContent[#x]							0.2
/ArchiveTransfer/Archive/ArchiveObject/TransferringAgencyObjectIdentifier				Métier			TODO										1.0
/ArchiveTransfer/Contains/Contains/TransferringAgencyObjectIdentifier					Métier			TODO										0.2
ArchiveObject/Name																		Métier			#ContainsName[TagPath]						1.0
Contains/Contains/Name																	Métier			#ContainsName[TagPath]						0.2
ArchiveObject/ContentDescription/Keyword/KeywordContent									Métier			#KeywordContent_TagPath[#x]					1.0
Contains/ContentDescription/ContentDescriptive/KeywordContent							Métier			#KeywordContent_TagPath[#x]					0.2
/ArchiveTransfer/Contains/Appraisal/Code                            					Métier			#Appraisal.Code								0.2
/ArchiveTransfer/Contains/Appraisal/Duration                           					Métier			#Appraisal.Duration							0.2
/ArchiveTransfer/Contains/AccessRestriction/Code                       					Métier			#AccessRestriction.Code						0.2
/ArchiveTransfer/Archive/AppraisalRule/Code                            					Métier			#Appraisal.Code								1.0
/ArchiveTransfer/Archive/AppraisalRule/Duration                        					Métier			#Appraisal.Duration							1.0
/ArchiveTransfer/Archive/AccessRestrictionRue/Code                     					Métier			#AccessRestriction.Code						1.0
/ArchiveTransfer/Contains/Contains/Appraisal/Code                      					Métier			#Appraisal.Code[TagPath]					0.2
/ArchiveTransfer/Contains/Contains/Appraisal/Duration                 					Métier			#Appraisal.Duration[TagPath]				0.2
/ArchiveTransfer/Contains/Contains/AccessRestriction/Code             					Métier			#AccessRestriction.Code[TagPath]			0.2
/ArchiveTransfer/Archive/ArchiveObject/AppraisalRule/Code              					Métier			#Appraisal.Code[TagPath]					1.0
/ArchiveTransfer/Archive/ArchiveObject/AppraisalRule/Duration          					Métier			#Appraisal.Duration[TagPath]				1.0
/ArchiveTransfer/Archive/ArchiveObject/AccessRestrictionRue/Code       					Métier			#AccessRestriction.Code[TagPath]			1.0

 * 
 * */

namespace BusinessDataControllerLauncher {
    class Program {
        static int Main(string[] args) {
            System.IO.StreamWriter streamWriter = null;

            String jobName;
            if (args.Length < 1) {
                System.Console.WriteLine("Syntaxe attendue : BusinessDataControllerLauncher nom-job-controle");
                System.Console.WriteLine("nom-job-controle est une section dans le fichier job.config");
                System.Console.WriteLine("Une section de contrôle de données métier a la forme :");
                System.Console.WriteLine("[data-control : nom-job-controle]");
                System.Console.WriteLine("  trace = chemin/vers/fichier-de-trace.txt");
                System.Console.WriteLine("  profil = chemin/vers/fichier-de-profil.rng");
                System.Console.WriteLine("  data = chemin/vers/fichier-de-donnees-metier.txt");
                System.Console.WriteLine("");
                System.Console.WriteLine("Aucun job demandé, le premier job sera exécuté");
                System.Console.WriteLine("");
                jobName = String.Empty;
            } else {
                jobName = args[0];
            }

            SimpleConfig config = new SimpleConfig();
            String erreur = config.loadFile("./job.config");
            if (erreur != String.Empty) // on tient compte du fait qu'en environnement de développement, l'exe est dans bin/Release
                erreur = config.loadFile("../../job.config");

            if (erreur != String.Empty) {
                System.Console.WriteLine(erreur);
                System.Environment.Exit(-1);
            }

            DataControlConfig datacontrol = config.getDatacontrolConfig(jobName);
            if (datacontrol == null) {
                System.Console.WriteLine("Aucun job 'data-control: " + jobName + "' trouvé dans le fichier job.config. Vérifiez la syntaxe ou créez une tâche.");
                System.Environment.Exit(-1);
            }

            System.Console.WriteLine("Contrôle métier du job '" + datacontrol.nomJob + "' : '" + datacontrol.dataFile + "' avec le profil '" + datacontrol.profileFile + "'");

            Action<Exception, String> eh = (ex, str) => {
                Console.WriteLine(ex.GetType().Name + " while trying to use trace file: " + datacontrol.traceFile + ". Complementary message: " + str);
                System.Environment.Exit(-1);
            };

            try {
                streamWriter = new System.IO.StreamWriter(datacontrol.traceFile);
            } catch (System.IO.IOException e) { eh(e, "Mauvaise syntaxe de nom de fichier"); } catch (UnauthorizedAccessException e) { eh(e, "Droits d'accès à corriger"); } catch (System.Security.SecurityException e) { eh(e, "Droits d'accès à corriger"); }

            BusinessDataController bdc = new BusinessDataController();
            bdc.setTracesWriter(streamWriter);
            StringCollection errors = bdc.controlDataFormat(datacontrol.dataFile);

            StringCollection erreursCorrespondance =
                bdc.controlMatchingBetweenDataAndProfile(datacontrol.dataFile, datacontrol.profileFile);

            int nbErreurs = 0;
            if (errors.Count > 0)
                nbErreurs += errors.Count;
            if (erreursCorrespondance != null && erreursCorrespondance.Count != 0)
                nbErreurs += erreursCorrespondance.Count;
            if (nbErreurs > 0)
                System.Console.WriteLine("\nDes erreurs ont été rencontrées\n");

            if (errors.Count > 0) {
                System.Console.WriteLine("\nErreurs dans les données :\n");
                foreach (String str in errors) {
                    System.Console.WriteLine(str);
                    streamWriter.WriteLine(str);
                }
                System.Console.WriteLine("\n");
                streamWriter.WriteLine("\n");
            }

            if (erreursCorrespondance != null && erreursCorrespondance.Count != 0) {
                System.Console.WriteLine("\nErreurs de correspondance entre les données et le profil :\n");
                foreach (String err in erreursCorrespondance) {
                    Console.WriteLine(err);
                    streamWriter.WriteLine(err);
                }
                System.Console.WriteLine("\n");
                streamWriter.WriteLine("\n");
            }
            streamWriter.Flush();

            return nbErreurs;
        }
    }
}
