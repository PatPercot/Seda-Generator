using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerManager {
	public class AccordVersement : BigTask {
		#region Propriétés privées
		private string m_saeServer;
		private string m_transferIdPrefix;
		private string m_transferringAgencyId;
		private string m_transferringAgencyName;
		private string m_transferringAgencyDesc;
		private string m_archivalAgencyId;
		private string m_archivalAgencyName;
		private string m_archivalAgencyDesc;
		#endregion

		#region Méthodes d'accès
		// SAE_Serveur = m_baseUri
		public string SaeServer {
			get { return m_saeServer; }
			set { m_saeServer = value; }
		}
		public string TransferIdPrefix {
			get { return m_transferIdPrefix; }
			set { m_transferIdPrefix = value; }
		}
		// SAE_ProfilArchivage = base.ProfileName
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

		public AccordVersement(string aTaskRole, string aTaskName, string aProfileName,
			string aSaeServer, string aTransferIdPrefix,
			string aTransferringAgencyId, string aTransferringAgencyName, string aTransferringAgencyDesc,
			string aArchivalAgencyId, string aArchivalAgencyName, string aArchivalAgencyDesc)
			: base(aTaskRole, aTaskName, aProfileName) {
			// Données de l'accord
			base.TaskRole = aTaskRole;
			base.TaskName = aTaskName;
			base.ProfileName = aProfileName;
			m_saeServer = aSaeServer;
			m_transferIdPrefix = aTransferIdPrefix;
			m_transferringAgencyId = aTransferringAgencyId;
			m_transferringAgencyName = aTransferringAgencyName;
			m_transferringAgencyDesc = aTransferringAgencyDesc;
			m_archivalAgencyId = aArchivalAgencyId;
			m_archivalAgencyName = aArchivalAgencyName;
			m_archivalAgencyDesc = aArchivalAgencyDesc;
		}

		public AccordVersement() {}


	}
}
