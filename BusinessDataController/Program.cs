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

 * 
 * */

namespace BusinessDataControllerLauncher {
    class Program {
        static void Main(string[] args) {
            String jobName;
            if (args.Length < 1) {
                System.Console.WriteLine("Syntaxe attendue : BusinessDataControllerLauncher nom-job-controle");
                System.Console.WriteLine("nom-job-controle est une section dans le fichier job.config");
                System.Console.WriteLine("Une section a la forme :");
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
                System.Console.WriteLine("Aucun job 'data-control' trouvé dans le fichier job.config. Vérifiez la syntaxe ou créez une tâche.");
                System.Environment.Exit(-1);
            }

            System.Console.WriteLine("Contrôle métier du job '" + datacontrol.nomJob + "' : '" + datacontrol.dataFile + "' avec le profil '" + datacontrol.profileFile + "'");

            BusinessDataController bdc = new BusinessDataController();
            //bdc.setTracesWriter(tracesWriter)
            StringCollection errors = bdc.controlDataFormat(datacontrol.dataFile);

            if (errors.Count > 0) {
                System.Console.WriteLine("\nDes erreurs ont été rencontrées\n\n");
                foreach (String str in errors) {
                    System.Console.WriteLine(str);
                }
                System.Console.WriteLine("\n\n");
            }

            System.Console.WriteLine("hitakey");
            System.Console.ReadKey();

        }
    }
}
