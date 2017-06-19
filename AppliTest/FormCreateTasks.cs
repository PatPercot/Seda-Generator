using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AppliTest
{
	public partial class FormCreateTasks : Form
	{
		public FormCreateTasks()
		{
			InitializeComponent();
			GroupBoxPosition();
		}
		
		#region Function ChecksVerif()
		/// <summary>
		/// Vérifie quels sont les cases cochés afin de bloquer certains
		/// paramètres pour éviter à l'utilisateur d'avoir à copier.
		/// </summary>
		private void ChecksVerif()
		{
			// Lorsque une tâche est à créer on affiche les informations globales
			this.tbxTaskName.Enabled = this.cbxProfileTask.Checked || this.cbxDataTask.Checked || this.cbxGeneratorTask.Checked;
			this.tbxTaskName.Visible = this.tbxTaskName.Enabled;
			this.lblTaskName.Visible = this.tbxTaskName.Enabled;
			this.lblExamples.Visible = this.tbxTaskName.Enabled;
			this.lblTaskExample1.Visible = this.tbxTaskName.Enabled;
			this.lblTaskExample2.Visible = this.tbxTaskName.Enabled;
			this.lblTaskExample3.Visible = this.tbxTaskName.Enabled;
			this.lblProfileExample1.Visible = this.tbxTaskName.Enabled;
			this.lblProfileExample2.Visible = this.tbxTaskName.Enabled;
			this.lblProfileExample3.Visible = this.tbxTaskName.Enabled;
			this.lblDataDonneesMetier.Visible = this.tbxTaskName.Enabled;
			this.rdbtnFollowTaskName.Enabled = this.tbxTaskName.Enabled;
			this.rdbtnFollowTaskName.Visible = this.tbxTaskName.Enabled;
			this.rdbtnFollowProfileName.Enabled = this.tbxTaskName.Enabled;
			this.rdbtnFollowProfileName.Visible = this.tbxTaskName.Enabled;
			this.rdbtnFollowNone.Enabled = this.tbxTaskName.Enabled;
			this.rdbtnFollowNone.Visible = this.tbxTaskName.Enabled;
			this.gbxFollowName.Visible = this.tbxTaskName.Enabled;
			// Pour la complétion automatique en lien avec le nom de la tâche, on désactive les liées si elle est cochée
			// Idem pour celle du profil
			this.tbxProfileProfile.Enabled = !this.rdbtnFollowTaskName.Checked || this.rdbtnFollowNone.Checked;
			this.tbxProfileData.Enabled = !(this.rdbtnFollowTaskName.Checked || this.rdbtnFollowProfileName.Checked) || this.rdbtnFollowNone.Checked;
			this.tbxProfileTrace.Enabled = !(this.rdbtnFollowTaskName.Checked || this.rdbtnFollowProfileName.Checked) || this.rdbtnFollowNone.Checked;
			this.tbxDataTrace.Enabled = !(this.rdbtnFollowTaskName.Checked || this.rdbtnFollowProfileName.Checked) || this.rdbtnFollowNone.Checked;
			this.tbxGeneratorTrace.Enabled = !(this.rdbtnFollowTaskName.Checked || this.rdbtnFollowProfileName.Checked) || this.rdbtnFollowNone.Checked;
			this.tbxBordereau.Enabled = !(this.rdbtnFollowTaskName.Checked || this.rdbtnFollowProfileName.Checked) || this.rdbtnFollowNone.Checked;
			// Si celle du profil est coché on bloque ce qui est en lien
			// ex : Nom du profil de Data, Nom de la data de Data et Nom de la data de Générateur
			this.tbxDataProfile.Enabled = !(this.cbxProfileTask.Checked || this.rdbtnFollowTaskName.Checked);
			this.tbxDataData.Enabled = !(this.cbxProfileTask.Checked || this.rdbtnFollowTaskName.Checked || this.rdbtnFollowProfileName.Checked);
			this.tbxGeneratorData.Enabled = !(this.cbxProfileTask.Checked || this.cbxDataTask.Checked || this.rdbtnFollowTaskName.Checked);
			this.tbxAccordVersementProfile.Enabled = !(this.cbxProfileTask.Checked || this.cbxDataTask.Checked || this.rdbtnFollowTaskName.Checked );
			//Positionnement des boîtes de groupement
			GroupBoxPosition();
			// Puis on complète les zones de texte si possible et demandé
			TextCompletion();
		}
		#endregion

		#region Function GroupBoxPosition()
		private void GroupBoxPosition() {
			if (!this.cbxProfileTask.Checked && !this.cbxDataTask.Checked && !this.cbxGeneratorTask.Checked) {
				// Aucune tâche coché
				this.ClientSize = new System.Drawing.Size(445, 65);
			} else if (this.cbxProfileTask.Checked && this.cbxDataTask.Checked && this.cbxGeneratorTask.Checked) {
				// Les trois cochées
				this.ClientSize = new System.Drawing.Size(920, 490);
				this.gbxProfile.Location = new System.Drawing.Point(13, 273);
				this.gbxGenerator.Location = new System.Drawing.Point(437, 12);
				this.gbxData.Location = new System.Drawing.Point(13, 379);
			} else if (this.cbxProfileTask.Checked && !this.cbxDataTask.Checked && !this.cbxGeneratorTask.Checked) {
				// Profil
				this.ClientSize = new System.Drawing.Size(445, 385);
			} else if (this.cbxDataTask.Checked && !this.cbxProfileTask.Checked && !this.cbxGeneratorTask.Checked) {
				// Données métier
				this.ClientSize = new System.Drawing.Size(445, 385);
				this.gbxData.Location = new System.Drawing.Point(13, 273);
			} else if (this.cbxGeneratorTask.Checked && !this.cbxProfileTask.Checked && !this.cbxDataTask.Checked) {
				// Générateur
				this.ClientSize = new System.Drawing.Size(500, 750);
				this.gbxGenerator.Location = new System.Drawing.Point(13, 273);
			} else if (this.cbxProfileTask.Checked && this.cbxDataTask.Checked && !this.cbxGeneratorTask.Checked) {
				// Profil et données métier
				this.ClientSize = new System.Drawing.Size(455, 490);
				this.gbxProfile.Location = new System.Drawing.Point(13, 273);
				this.gbxData.Location = new System.Drawing.Point(13, 379);
			} else if (this.cbxProfileTask.Checked && this.cbxGeneratorTask.Checked && !this.cbxDataTask.Checked) {
				// Profil et générateur
				this.ClientSize = new System.Drawing.Size(920, 490);
				this.gbxProfile.Location = new System.Drawing.Point(13, 273);
				this.gbxGenerator.Location = new System.Drawing.Point(437, 12);
			} else if (this.cbxDataTask.Checked && this.cbxGeneratorTask.Checked && !this.cbxProfileTask.Checked) {
				// Données métier et générateur
				this.ClientSize = new System.Drawing.Size(920, 490);
				this.gbxData.Location = new System.Drawing.Point(13, 273);
				this.gbxGenerator.Location = new System.Drawing.Point(437, 12);
			}
		}
		#endregion

		#region Function EnabledCreateButton() et CompletionVerif(String value)
		/// <summary>
		/// Pour le bouton Créer on le débloque que si toutes les zones de texte sont remplies en fonction de ce qui est coché
		/// </summary>
		private void EnabledCreateButton() {
			// Pour une tâche de contrôle du profil
			bool profileCreate = CompletionVerif(this.tbxProfileProfile.Text) && CompletionVerif(this.tbxProfileData.Text) && CompletionVerif(this.tbxProfileTrace.Text);
			// Pour une tâche de contrôle des données métier
			bool dataCreate = CompletionVerif(this.tbxDataProfile.Text) && CompletionVerif(this.tbxDataData.Text) && CompletionVerif(this.tbxDataTrace.Text);
			// Pour une tâche de contrôle du générateur de bordereau
			bool generatorCreate = CompletionVerif(this.tbxBordereau.Text) && CompletionVerif(this.tbxGeneratorTrace.Text)
				&& CompletionVerif(this.tbxGeneratorData.Text) && CompletionVerif(this.tbxDocumentsRepertory.Text) && CompletionVerif(this.tbxUriBase.Text)
				&& CompletionVerif(this.tbxAccordVersementProfile.Text) && CompletionVerif(this.tbxSaeServer.Text) && CompletionVerif(this.tbxAccordVersementProfile.Text)
				&& CompletionVerif(this.tbxTransferringAgencyId.Text) && CompletionVerif(this.tbxTransferringAgencyName.Text) && CompletionVerif(this.tbxTransferringAgencyDesc.Text)
				&& CompletionVerif(this.tbxArchivalAgencyId.Text) && CompletionVerif(this.tbxArchivalAgencyName.Text) && CompletionVerif(this.tbxArchivalAgencyDesc.Text);
			// Utilisation des valeurs
			if (this.cbxProfileTask.Checked) {
				this.btnCreate.Enabled = profileCreate;
				if (this.cbxDataTask.Checked) {
					this.btnCreate.Enabled = profileCreate && dataCreate;
					if (this.cbxGeneratorTask.Checked) {
						this.btnCreate.Enabled = profileCreate && dataCreate && generatorCreate;
					}
				} else if (this.cbxGeneratorTask.Checked) {
						this.btnCreate.Enabled = profileCreate && generatorCreate;
				}
			} else if (this.cbxDataTask.Checked) {
				if (this.cbxGeneratorTask.Checked) {
						this.btnCreate.Enabled = dataCreate && generatorCreate;
				}
			} else if (this.cbxGeneratorTask.Checked) {
				this.btnCreate.Enabled = generatorCreate;
			} else {
				this.btnCreate.Enabled = false;
			}
		}

		private bool CompletionVerif(String value) {
			return value != "" && value != " ";
		}
		#endregion

		#region Function TextCompletion()
		/// <summary>
		/// Affiche le même texte dans les cases reliés au profil et data lorsque l'on
		/// remplie les premiers du formulaire. Pour les boîtes de texte concernées par
		/// la complétion automatique en lien avec soit le profil, soit la tâche même objectif.
		/// </summary>
		private void TextCompletion() {
			if (this.rdbtnFollowTaskName.Checked) {
				this.tbxProfileProfile.Text = this.tbxTaskName.Text + "_profil";
				this.tbxProfileData.Text = this.tbxTaskName.Text + "_data";
				this.tbxProfileTrace.Text = this.tbxTaskName.Text + "_profil_trace";
				this.tbxDataTrace.Text = this.tbxTaskName.Text + "_data_trace";
				this.tbxGeneratorTrace.Text = this.tbxTaskName.Text + "_bordereau_trace";
				this.tbxBordereau.Text = this.tbxTaskName.Text + "_bordereau";
			} else if (this.rdbtnFollowProfileName.Checked) {
				this.tbxProfileData.Text = this.tbxProfileProfile.Text + "_data";
				this.tbxProfileTrace.Text = this.tbxProfileProfile.Text + "_profil_trace";
				this.tbxDataTrace.Text = this.tbxProfileProfile.Text + "_data_trace";
				this.tbxGeneratorTrace.Text = this.tbxProfileProfile.Text + "_bordereau_trace";
				this.tbxBordereau.Text = this.tbxProfileProfile.Text + "_bordereau";
			}
			if (this.cbxProfileTask.Checked) {
				if (this.cbxDataTask.Checked) {
					this.tbxDataProfile.Text = this.tbxProfileProfile.Text;
					this.tbxDataData.Text = this.tbxProfileData.Text;
				}
				if (this.cbxGeneratorTask.Checked) {
					this.tbxGeneratorData.Text = this.tbxProfileData.Text;
					this.tbxAccordVersementProfile.Text = this.tbxProfileProfile.Text;
					this.tbxSaeServer.Text = this.tbxUriBase.Text;
				} 
			} else if (this.cbxGeneratorTask.Checked) {
				this.tbxSaeServer.Text = this.tbxUriBase.Text ;
				if (this.cbxDataTask.Checked) {
					this.tbxAccordVersementProfile.Text = this.tbxProfileProfile.Text;
					this.tbxGeneratorData.Text = this.tbxProfileData.Text;
				}
			}
			EnabledCreateButton();
		}
		#endregion

		#region Events function Click
		private void btnRetour_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void btnCreate_Click(object sender, EventArgs e) {

		}
		#endregion

		#region Events function CheckedChanged
		// Active le formulaire pour créer une tâche de type profil
		private void cbxProfilTask_CheckedChanged(object sender, EventArgs e) {
			tbxProfileData.Enabled = cbxProfileTask.Checked;
			tbxProfileProfile.Enabled = cbxProfileTask.Checked;
			tbxProfileTrace.Enabled = cbxProfileTask.Checked;
			gbxProfile.Enabled = cbxProfileTask.Checked;
			gbxProfile.Visible = cbxProfileTask.Checked;
			ChecksVerif();
		}

		// Active le formulaire pour créer une tâche de type data
		private void cbxDataTask_CheckedChanged(object sender, EventArgs e) {
			tbxDataData.Enabled = cbxDataTask.Checked || !this.rdbtnFollowTaskName.Checked;
			tbxDataProfile.Enabled = cbxDataTask.Checked || !this.rdbtnFollowTaskName.Checked;
			tbxDataTrace.Enabled = cbxDataTask.Checked || !this.rdbtnFollowTaskName.Checked;
			gbxData.Enabled = cbxDataTask.Checked;
			gbxData.Visible = cbxDataTask.Checked;
			ChecksVerif();
		}

		// Active le formulaire pour créer une tâche de type générateur
		private void cbxGeneratorTask_CheckedChanged(object sender, EventArgs e) {
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
		}

		private void rdbtnFollowTaskName_CheckedChanged(object sender, EventArgs e) {
			ChecksVerif();
		}

		private void rdbtnFollowProfileName_CheckedChanged(object sender, EventArgs e) {
			ChecksVerif();
		}

		private void rdbtnFollowNone_CheckedChanged(object sender, EventArgs e) {
			ChecksVerif();
		}
		#endregion

		#region Events function KeyUp

		#region With TextCompletion()
		private void tbxTaskName_KeyUp(object sender, KeyEventArgs e) {
			TextCompletion();
		}

		private void tbxProfileProfile_KeyUp(object sender, KeyEventArgs e) {
			TextCompletion();
		}
		
		private void tbxProfileData_KeyUp(object sender, KeyEventArgs e) {
			TextCompletion();
		}

		private void tbxDataProfile_KeyUp(object sender, KeyEventArgs e) {
			TextCompletion();
		}

		private void tbxDataData_KeyUp(object sender, KeyEventArgs e) {
			TextCompletion();
		}

		private void tbxUriBase_KeyUp(object sender, KeyEventArgs e) {
			TextCompletion();
		}
		#endregion

		#region With EnabledCreateButton()
		private void tbxProfileTrace_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxDataTrace_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxBordereau_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxGeneratorTrace_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxDocumentsRepertory_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxGeneratorData_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxAccordVersementProfile_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxTransferIdPrefix_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxTransferringAgencyId_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxTransferringAgencyName_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxTransferringAgencyDesc_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxArchivalAgencyId_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxArchivalAgencyName_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}

		private void tbxArchivalAgencyDesc_KeyUp(object sender, KeyEventArgs e) {
			EnabledCreateButton();
		}
		#endregion

		#endregion
	}
}
