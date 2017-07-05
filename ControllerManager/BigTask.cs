using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ControllerManager {
	public class BigTask {
		#region Propriétés privées
		public string m_taskRole;
		public string m_taskName;
		private string m_profileName;
		private string m_dataName;
		private string m_traceName;
		#endregion

		#region Méthodes d'accès
		public string TaskRole {
			get { return m_taskName; }
			set { m_taskName = value; }
		}
		public string TaskName {
			get { return m_taskName; }
			set { m_taskName = value; }
		}
		public string ProfileName {
			get { return m_profileName; }
			set { m_profileName = value; }
		}
		public string DataName {
			get { return m_dataName; }
			set { m_dataName = value; }
		}
		public string TraceName {
			get { return m_traceName; }
			set { m_traceName = value; }
		}
		#endregion

		public BigTask(string aTaskRole, string aTaskName, string aProfileName, string aDataName = null, string aTraceName = null) {
			m_taskRole = aTaskRole;
			m_taskName = aTaskName;
			m_profileName = aProfileName;
			m_dataName = aDataName;
			m_traceName = aTraceName;
			if (m_taskRole != "accord-versement" && (aDataName == null || aTraceName == null)) {
				throw new Exception("Les données métier et la trace ne peuvent être 'null' !");
			}
		}

		public BigTask() {}

		public string ReadConfig(string line, StreamReader sReader) {
			// On extrait les infos de 'line'
			while ((line = sReader.ReadLine()) != null) {
				//
				break;
				// Si 'line' est une ligne de section on arrête
				// Si 'line' est un commentaire, ou ligne vide on passe à la suivante
				// Si 'line' est une définition de variable, on l'interprête
			}
			return line;
		}

		protected bool SetElement(string cle, string valeur) {
			bool bok = false;
			switch (cle) {
				case "profile-control":
					m_taskRole = cle;
					m_taskName = valeur;
					bok = true;
					break;
				case "data":
					break;
				case "trace":
					break;
				case "profil":
					break;
			}
			return bok;
		}

		/*public Exception WriteTaskInJobConfig() {
			try {
				string task = "";
				if (profileTask) {
					// Ecrit la tâche du contrôle de profil
					tasks += "\n";
					tasks += "[profile-control : " + m_taskname + "]\n";
					tasks += "trace = travail/traces/" + m_trace + ".txt\n";
					tasks += "profil = travail/profils/" + this.tbxProfileProfile.Text + ".rng\n";
					tasks += "data = travail/metier/" + this.tbxProfileData.Text + ".txt\n";
				}
				if (dataTask) {
					// Ecrit la tâche du contrôle des données métier
					tasks += "\n";
					tasks += "[data-control : " + this.tbxTaskName.Text + "]\n";
					tasks += "trace = travail/traces/" + this.tbxDataTrace.Text + ".txt\n";
					tasks += "profil = travail/profils/" + this.tbxDataProfile.Text + ".rng\n";
					tasks += "data = travail/metier/" + this.tbxDataData.Text + ".txt\n";
				}
				if (generatorTask) {
					// Ecrit la tâche du contrôle du générateur (accord de versement en +)
					tasks += "\n";
					tasks += "[accord-versement : " + this.tbxTaskName.Text + "]\n";
					tasks += "SAE_Serveur = " + this.tbxSaeServer.Text + "\n";
					tasks += "TransferIdPrefix = " + this.tbxTransferIdPrefix.Text + "\n";
					tasks += "SAE_ProfilArchivage = travail/profils/" + this.tbxAccordVersementProfile.Text + ".rng\n";
					tasks += "TransferringAgencyId = " + this.tbxTransferringAgencyId.Text + "\n";
					tasks += "TransferringAgencyName = " + this.tbxTransferringAgencyName.Text + "\n";
					tasks += "TransferringAgencyDesc = " + this.tbxTransferringAgencyDesc.Text + "\n";
					tasks += "ArchivalAgencyId = " + this.tbxArchivalAgencyId.Text + "\n";
					tasks += "ArchivalAgencyName = " + this.tbxArchivalAgencyName.Text + "\n";
					tasks += "ArchivalAgencyDesc = " + this.tbxArchivalAgencyDesc.Text + "\n";
					tasks += "\n";
					tasks += "[generator : " + this.tbxTaskName.Text + "]\n";
					tasks += "accord = " + this.tbxTaskName.Text + "\n";
					tasks += "baseURI = " + this.tbxUriBase.Text + "\n";
					tasks += "trace = travail/traces/" + this.tbxGeneratorTrace.Text + ".txt\n";
					tasks += "data = travail/metier/" + this.tbxGeneratorData.Text + ".txt\n";
					tasks += "rep_documents = travail/documents/" + this.tbxDocumentsRepertory.Text + "\n";
					tasks += "bordereau = travail/bordereaux/" + this.tbxBordereau.Text + ".xml\n";
				}
				using (StreamWriter outputFile = new StreamWriter(@"./job.config", true)) {
					outputFile.WriteLine(tasks);
			} catch (Exception e) {
				Console.WriteLine("Le fichier ne peut pas être lu.");
				Console.WriteLine(e.Message);
			}
			return e;
		}*/

	}
}
