using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerManager {
	public class Generator : BigTask {
		#region Propriétés privées
		private string m_agreement;
		private string m_uriBase;
		private string m_rep_document;
		private string m_bordereau;
		#endregion

		#region Méthodes d'accès
		public string Agreement {
			get { return m_agreement; }
			set { m_agreement = value; }
		}
		public string UriBase {
			get { return m_uriBase; }
			set { m_uriBase = value; }
		}
		public string RepDocument {
			get { return m_rep_document; }
			set { m_rep_document = value; }
		}
		public string Bordereau {
			get { return m_bordereau; }
			set { m_bordereau = value; }
		}
		#endregion

		public Generator(string aTaskRole, string aTaskName, string aProfileName, string aDataName, string aTraceName,
			string anAgreement, string anUriBase, string aRepDocument, string unBordereau)
			: base(aTaskRole, aTaskName, aProfileName, aDataName, aTraceName) {
			// Données communes
			base.TaskRole = aTaskRole;
			base.TaskName = aTaskName;
			base.ProfileName = aProfileName;
			base.DataName = aDataName;
			base.TraceName = aTraceName;
			//Données du générateur
			m_agreement = anAgreement;
			m_uriBase = anUriBase;
			m_rep_document = aRepDocument;
			m_bordereau = unBordereau;
		}

		public Generator() {}

		protected bool SetElement(string cle, string valeur) {
			bool bok = false;
			if (base.SetElement(cle, valeur) == false) {
				switch (cle) {
					case "accord":
						m_taskRole = cle;
						m_taskName = valeur;
						break;
					case "baseURI":
						break;
					case "rep_document":
						break;
					case "bordereau":
						break;
				}
			}
			return bok;
		}
	}
}
