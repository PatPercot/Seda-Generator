using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppliTest
{
	public partial class FormCreateTasks : Form
	{
		public FormCreateTasks()
		{
			InitializeComponent();
		}

		private void btnRetour_Click(object sender, EventArgs e)
		{
			FormProfile fprofil = new FormProfile();
			fprofil.Show();
		}

		/// <summary>
		/// Vérifie quels sont les cases cochés afin de bloquer certains paramètres pour éviter
		/// à l'utilisateur d'avoir à copier. De plus cela affiche le même texte lorsque l'on remplie.
		/// </summary>
		private void ChecksVerif()
		{
			// Lorsque une tâche est à créer on affiche les informations globales
			tbxTaskName.Enabled = this.cbxProfileTask.Checked || this.cbxDataTask.Checked || this.cbxGeneratorTask.Checked;
			tbxTaskName.Visible = tbxTaskName.Enabled;
			lblTaskName.Visible = tbxTaskName.Enabled;
			lblExamples.Visible = tbxTaskName.Enabled;
			lblTaskExample1.Visible = tbxTaskName.Enabled;
			lblTaskExample2.Visible = tbxTaskName.Enabled;
			lblTaskExample3.Visible = tbxTaskName.Enabled;
			lblProfileExample1.Visible = tbxTaskName.Enabled;
			lblProfileExample2.Visible = tbxTaskName.Enabled;
			lblProfileExample3.Visible = tbxTaskName.Enabled;
			cbxFollowTaskName.Enabled = tbxTaskName.Enabled;
			cbxFollowTaskName.Visible = tbxTaskName.Enabled;
			cbxFollowProfileName.Enabled = tbxTaskName.Enabled;
			cbxFollowProfileName.Visible = tbxTaskName.Enabled;
			// Si le profil est coché on bloque ce qui est en lien
			// ex : Nom du profil de Data, Nom de la data de Data et Nom de la data de Générateur
			tbxDataProfile.Enabled = !this.cbxProfileTask.Checked;
			tbxDataData.Enabled = !this.cbxProfileTask.Checked;
			tbxGeneratorData.Enabled = !(this.cbxProfileTask.Checked || this.cbxDataTask.Checked);
			tbxAccordVersementProfile.Enabled = !(this.cbxProfileTask.Checked || this.cbxDataTask.Checked);
		}

		private void TextCompletion()
		{
			if (this.cbxProfileTask.Checked)
			{
				if (this.cbxGeneratorTask.Checked)
				{
					this.tbxGeneratorData.Text = this.tbxDataData.Text;
					this.tbxAccordVersementProfile.Text = this.tbxProfileProfile.Text;
					this.tbxSaeServer.Text = this.tbxUriBase.Text;
				}
			}
			else if (this.cbxDataTask.Checked)
			{
				if (this.cbxDataTask.Checked)
				{
					this.tbxDataProfile.Text = this.tbxProfileProfile.Text;
					this.tbxDataData.Text = this.tbxProfileData.Text;
				}
			}
		}

		private void cbxProfilTask_CheckedChanged(object sender, EventArgs e)
		{
			gbxProfil.Enabled = cbxProfileTask.Checked;
			gbxProfil.Visible = cbxProfileTask.Checked;
			tbxProfileData.Enabled = cbxProfileTask.Checked;
			tbxProfileProfile.Enabled = cbxProfileTask.Checked;
			tbxProfileTrace.Enabled = cbxProfileTask.Checked;
			ChecksVerif();
			TextCompletion();
		}

		private void cbxDataTask_CheckedChanged(object sender, EventArgs e)
		{
			tbxDataData.Enabled = cbxDataTask.Checked;
			tbxDataProfile.Enabled = cbxDataTask.Checked;
			tbxDataTrace.Enabled = cbxDataTask.Checked;
			gbxData.Enabled = cbxDataTask.Checked;
			gbxData.Visible = cbxDataTask.Checked;
			ChecksVerif();
			TextCompletion();
		}

		private void cbxGeneratorTask_CheckedChanged(object sender, EventArgs e)
		{
			tbxUriBase.Enabled = cbxGeneratorTask.Checked;
			tbxGeneratorTrace.Enabled = cbxGeneratorTask.Checked;
			tbxGeneratorData.Enabled = cbxGeneratorTask.Checked;
			tbxDocumentsRepertory.Enabled = cbxGeneratorTask.Checked;
			tbxBordereau.Enabled = cbxGeneratorTask.Checked;
			gbxGenerator.Enabled = cbxGeneratorTask.Checked;
			gbxGenerator.Visible = cbxGeneratorTask.Checked;
			tbxAccordVersementProfile.Enabled = cbxGeneratorTask.Checked;
			tbxTransferIdPrefix.Enabled = cbxGeneratorTask.Checked;
			tbxTransferringAgencyId.Enabled = cbxGeneratorTask.Checked;
			tbxTransferringAgencyName.Enabled = cbxGeneratorTask.Checked;
			tbxTransferringAgencyDesc.Enabled = cbxGeneratorTask.Checked;
			tbxArchivalAgencyId.Enabled = cbxGeneratorTask.Checked;
			tbxArchivalAgencyName.Enabled = cbxGeneratorTask.Checked;
			tbxArchivalAgencyDesc.Enabled = cbxGeneratorTask.Checked;
			gbxAccordVersement.Enabled = cbxGeneratorTask.Checked;
			gbxAccordVersement.Visible = cbxGeneratorTask.Checked;
			ChecksVerif();
			TextCompletion();
		}

		private void tbxProfileProfile_KeyUp(object sender, KeyEventArgs e)
		{
			TextCompletion();
		}
	}
}
