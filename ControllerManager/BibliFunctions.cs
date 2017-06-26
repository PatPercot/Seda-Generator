using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerManager {
	public class BibliFunctions {
	}

	// Les classes d'objets sont ici :
	// -> La maman
	//		BigTasks
	// -> Les enfants
	//		ProfileController
	//		DataController
	//		Generator
	public class BigTasks {
		#region Propriété privée
		private string m_taskRole;
		private string m_taskName;
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

		public BigTasks(string aTaskRole, string aTaskName, string aProfileName, string aDataName, string aTraceName) {
			m_taskRole = aTaskRole;
			m_taskName = aTaskName;
			m_profileName = aProfileName;
			m_dataName = aDataName;
			m_traceName = aTraceName;
		}
	}

	public class ProfileController : BigTasks {

		public ProfileController(string aTaskRole, string aTaskName, string aProfileName, string aDataName, string aTraceName, string aRole)
			: base(aTaskRole, aTaskName, aProfileName, aDataName, aTraceName) {
			base.TaskRole = aTaskRole;
			base.TaskName = aTaskName;
			base.ProfileName = aProfileName;
			base.DataName = aDataName;
			base.TraceName = aTraceName;
		}
	}

	public class DataController : BigTasks {

		public DataController(string aTaskRole, string aTaskName, string aProfileName, string aDataName, string aTraceName)
			: base(aTaskRole, aTaskName, aProfileName, aDataName, aTraceName) {
			base.TaskRole = aTaskRole;
			base.TaskName = aTaskName;
			base.ProfileName = aProfileName;
			base.DataName = aDataName;
			base.TraceName = aTraceName;
		}
	}

	public class Generator : BigTasks {
		#region Propriétés privées
		#region De l'accord de versement
		private string m_transferIdPrefix;
		private string m_transferringAgencyId;
		private string m_transferringAgencyName;
		private string m_transferringAgencyDesc;
		private string m_archivalAgencyId;
		private string m_archivalAgencyName;
		private string m_archivalAgencyDesc;
		#endregion
		#region pour le générateur
		private string m_accord;
		private string m_baseUri;
		private string m_rep_document;
		private string m_bordereau;
		#endregion
		#endregion

		#region Méthodes d'accès
		#region Données pour le générateur
		public string Accord {
			get { return m_accord; }
			set { m_accord = value; }
		}
		public string BaseUri {
			get { return m_baseUri; }
			set { m_baseUri = value; }
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
		#region Données pour l'accord de versement
		// SAE_Serveur = m_baseUri
		public string SaeServer {
			get { return m_baseUri; }
		}
		public string TransferIdPrefix {
			get { return m_transferIdPrefix; }
			set { m_transferIdPrefix = value; }
		}
		// SAE_ProfilArchivage = base.ProfileName
		public string TransferIdPrefix {
			get { return m_transferIdPrefix; }
			set { m_transferIdPrefix = value; }
		}
		public string TransferringAgencyId {
			get { return m_transferringAgencyId; }
			set { m_transferringAgencyId = value; }
		}
		public string TransferringAgencyName {
			get { return m_transferringAgencyName; }
			set { m_transferringAgencyName = value; }
		}
		public string TransferringAgencyDesc {
			get { return m_transferringAgencyDesc; }
			set { m_transferringAgencyDesc = value; }
		}
		public string ArchivalAgencyId {
			get { return m_archivalAgencyId; }
			set { m_archivalAgencyId = value; }
		}
		public string ArchivalAgencyName {
			get { return m_archivalAgencyName; }
			set { m_archivalAgencyName = value; }
		}
		public string ArchivalAgencyDesc {
			get { return m_archivalAgencyDesc; }
			set { m_archivalAgencyDesc = value; }
		}
		#endregion
		#endregion

		public Generator(string aTaskRole, string aTaskName, string aProfileName, string aDataName, string aTraceName,
			string unAccord, string uneBaseUri, string unRepDocument, string unBordereau, string aTransferIdPrefix,
			string aTransferringAgencyId, string aTransferringAgencyName, string aTransferringAgencyDesc,
			string aArchivalAgencyId, string aArchivalAgencyName, string aArchivalAgencyDesc)
			: base(aTaskRole, aTaskName, aProfileName, aDataName, aTraceName) {
			// Données communes
			base.TaskRole = aTaskRole;
			base.TaskName = aTaskName;
			base.ProfileName = aProfileName;
			base.DataName = aDataName;
			base.TraceName = aTraceName;
			//Données du générateur
			m_accord = unAccord;
			m_baseUri = uneBaseUri;
			m_rep_document = unRepDocument;
			m_bordereau = unBordereau;
			// Données de l'accord
			m_transferIdPrefix = aTransferIdPrefix;
			m_transferringAgencyId = aTransferringAgencyId;
			m_transferringAgencyName = aTransferringAgencyName;
			m_transferringAgencyDesc = aTransferringAgencyDesc;
			m_archivalAgencyId = aArchivalAgencyId;
			m_archivalAgencyName = aArchivalAgencyName;
			m_archivalAgencyDesc = aArchivalAgencyDesc;
		}
	}
}
