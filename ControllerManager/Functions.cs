using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ControllerManager {
	public class Functions {

		#region //Code de lancement de Agape (sous environnement de développement)
		/*private void btnCreateProfile_Click(object sender, EventArgs e) {
			String pathExecution = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\..\\..\\"; ;
			Process myProcess = new Process();
			try {
				myProcess.StartInfo.UseShellExecute = false;
				myProcess.StartInfo.WorkingDirectory = pathExecution;
				myProcess.StartInfo.FileName = @"C:/Program Files (x86)/Java/jre1.8.0_121/bin/java.exe";
				myProcess.StartInfo.Arguments = "-jar " + Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
					+ @"/travail/profils/agape-v115.jar";
				myProcess.StartInfo.CreateNoWindow = true;
				myProcess.StartInfo.RedirectStandardError = true;
				myProcess.StartInfo.StandardErrorEncoding = System.Text.ASCIIEncoding.Default;
				// ATTENTION : la présence des deux instructions ci-dessous empêche le processus de se terminer
				// myProcess.StartInfo.RedirectStandardOutput = true;
				// myProcess.StartInfo.StandardOutputEncoding = System.Text.ASCIIEncoding.Default;
				myProcess.Start();
				int nbBcl = 0;
				while (nbBcl < 120 && myProcess.WaitForExit(250) == false) {
					++nbBcl;
				}
				if (nbBcl == 120)  // 30 secondes max
					myProcess.Kill();
				// Parce que l'on a redirigé les sorties standards, il faut appeler WaitForExit
				myProcess.WaitForExit();
			} catch (Exception a) {
				MessageBox.Show(a.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			//System.Diagnostics.Process.Start("java.exe -jar agape-v115.jar");
		}*/
		#endregion


		#region Méthodes privées Cutter()
		/// <summary>
		/// Permet de récupérer une chaîne de caractères, coupé à partir d'une position.
		/// </summary>
		/// <example>
		/// toCut = "Voici un exemple !!!"
		/// index = 9
		/// return -> "exemple !!!"
		/// </example>
		/// <param name="toCut"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static string Cutter(string toCut, int index) {
			return toCut.Substring(index);
		}

		/// <summary>
		/// Permet de récupérer une chaîne de caractères, coupé à partir d'une position et 
		/// de couper la fin du texte récupéré pour retirer un certain nombre de caractères.
		/// </summary>
		/// <example>
		/// toCut = "Voici un exemple !!!"
		/// index = 9
		/// cutTheEnd = 4
		/// return -> "exemple"
		/// </example>
		/// <param name="toCut"></param>
		/// <param name="index"></param>
		/// <param name="cutTheEnd"></param>
		/// <returns></returns>
		private static string Cutter(string toCut, int index, int cutTheEnd) {
			return toCut.Substring(index, toCut.Length - index - cutTheEnd);
		}

		/// <summary>
		/// Permet de récupérer une chaîne de caractères, coupé à partir d'une autre.
		/// </summary>
		/// <example>
		/// toCut = "Voici un exemple !!!"
		/// toFind = "exe"
		/// return -> "exemple !!!"
		/// </example>
		/// <param name="toCut"></param>
		/// <param name="toFind"></param>
		/// <returns></returns>
		private static string Cutter(string toCut, string toFind) {
			return toCut.Substring(toCut.IndexOf(toFind));
		}

		/// <summary>
		/// Permet de récupérer une chaîne de caractères, coupé à partir d'une autre et de 
		/// couper la fin du texte récupéré pour retirer un certain nombre de caractères.
		/// </summary>
		/// <example>
		/// toCut = "Voici un exemple !!!"
		/// toFind = "exe"
		/// cutTheEnd = 4
		/// return -> "exemple"
		/// </example>
		/// <param name="toCut"></param>
		/// <param name="toFind"></param>
		/// <param name="cutTheEnd"></param>
		/// <returns></returns>
		private static string Cutter(string toCut, string toFind, int cutTheEnd) {
			return toCut.Substring(toCut.IndexOf(toFind), toCut.Length - toCut.IndexOf(toFind) - cutTheEnd);
		}
		#endregion

		public static List<BigTask> ReadingFile() {
			List<BigTask> list = new List<BigTask>();
			try {
				using (StreamReader sReader = new StreamReader(@"./job.config")) {
					string line = "", lineTask = "";
					while (true) {
						if (lineTask == "") {
							line = sReader.ReadLine();
							if (line == null) break;
						} else
							line = lineTask;
						if (line.Contains("[profile-control : ")) {
							ProfileController pc = new ProfileController();
							lineTask = pc.ReadConfig(line, sReader);
							list.Add(pc);
						} else if (line.Contains("[data-control : ")) {
							DataController dc = new DataController();
							lineTask = dc.ReadConfig(line, sReader);
							list.Add(dc);
						} else if (line.Contains("[accord-versement : ")) {
							AccordVersement gen = new AccordVersement();
							lineTask = gen.ReadConfig(line, sReader);
							list.Add(gen);
						} else if (line.Contains("[generator : ")) {
							Generator av = new Generator();
							lineTask = av.ReadConfig(line, sReader);
							list.Add(av);
						}
					}
				}
			} catch (Exception e) {
				Console.WriteLine("Le fichier ne peut pas être lu.");
				Console.WriteLine(e.Message);
			}
			return list;
		}
		
		private static int ReadingProfileControlTasks(string line, int countLineProfile) {
			bool verifTask = line.Contains("[profile-control : ");
			if (verifTask || countLineProfile < 4) {
				string aTaskName = "", aProfileName = "", aDataName = "", aTraceName = "";
				if (verifTask) {
					countLineProfile = 1;
					aTaskName = Cutter(line, line.IndexOf(" : " + 3), 1);
				} else if (countLineProfile < 4) {
					countLineProfile++;
					string lineSub = Cutter(line, line.LastIndexOf("/") + 1, 0);
					if (line.Contains("profil = ")) {
						aProfileName = lineSub;
					} else if (line.Contains("trace = ")) {
						aDataName = lineSub;
					} else if (line.Contains("data = ")) {
						aTraceName = lineSub;
					}
				}
				ProfileController oneProfile = new ProfileController("profile-control", aTaskName, aProfileName, aDataName, aTraceName);
			}
			return countLineProfile;
		}

		private static int ReadingDataControlTasks(string line, int countLineData) {
			bool verifTask = line.Contains("[data-control : ");
			if (verifTask || countLineData < 4) {
				string aTaskName = "", aProfileName = "", aDataName = "", aTraceName = "";
				if (verifTask) {
					countLineData = 1;
					aTaskName = Cutter(line, line.IndexOf(" : " + 3), 1);
				} else if (countLineData < 4) {
					countLineData++;
					string lineSub = Cutter(line, line.LastIndexOf("/") + 1, 0);
					if (line.Contains("profil = ")) {
						aProfileName = lineSub;
					} else if (line.Contains("trace = ")) {
						aDataName = lineSub;
					} else if (line.Contains("data = ")) {
						aTraceName = lineSub;
					}
				}
				DataController oneProfile = new DataController("data-control", aTaskName, aProfileName, aDataName, aTraceName);
			}
			return countLineData;
		}

		private static int ReadingGeneratorTasks(string line, int countLineGenerator) {
			bool verifTask = line.Contains("[generator : ");
			if (verifTask || countLineGenerator < 4) {
				string aTaskName = "", aProfileName = "", aDataName = "", aTraceName = "";
				if (verifTask) {
					countLineGenerator = 1;
					aTaskName = Cutter(line, line.IndexOf(" : " + 3), 1);
				} else if (countLineGenerator < 4) {
					countLineGenerator++;
					string lineSub = Cutter(line, line.LastIndexOf("/") + 1, 0);
					if (line.Contains("profil = ")) {
						aProfileName = lineSub;
					} else if (line.Contains("trace = ")) {
						aDataName = lineSub;
					} else if (line.Contains("data = ")) {
						aTraceName = lineSub;
					} /*else if () {
						
					} else if () {
						
					} else if () {
						
					} else if () {
						
					}*/
				}
				/*Generator oneProfile = new Generator("generator", aTaskName, aProfileName, aDataName,
					aTraceName, anAgreement, anUriBase, aRepDocument, unBordereau);*/
			}
			return countLineGenerator;
		}
	}
}
