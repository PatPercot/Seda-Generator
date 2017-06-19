namespace AppliTest
{
	partial class FormControlLauncher
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
			this.gbControleurs = new System.Windows.Forms.GroupBox();
			this.cbxGeneratorControl = new System.Windows.Forms.CheckBox();
			this.cbxDataControl = new System.Windows.Forms.CheckBox();
			this.cbxProfilControl = new System.Windows.Forms.CheckBox();
			this.tbxTaskName = new System.Windows.Forms.TextBox();
			this.btnControler = new System.Windows.Forms.Button();
			this.gbControleurs.SuspendLayout();
			this.SuspendLayout();
			// 
			// gbControleurs
			// 
			this.gbControleurs.Controls.Add(this.cbxGeneratorControl);
			this.gbControleurs.Controls.Add(this.cbxDataControl);
			this.gbControleurs.Controls.Add(this.cbxProfilControl);
			this.gbControleurs.Location = new System.Drawing.Point(12, 38);
			this.gbControleurs.Name = "gbControleurs";
			this.gbControleurs.Size = new System.Drawing.Size(100, 87);
			this.gbControleurs.TabIndex = 0;
			this.gbControleurs.TabStop = false;
			this.gbControleurs.Text = "Controleurs";
			// 
			// cbxGeneratorControl
			// 
			this.cbxGeneratorControl.AutoSize = true;
			this.cbxGeneratorControl.Location = new System.Drawing.Point(6, 66);
			this.cbxGeneratorControl.Name = "cbxGeneratorControl";
			this.cbxGeneratorControl.Size = new System.Drawing.Size(79, 17);
			this.cbxGeneratorControl.TabIndex = 2;
			this.cbxGeneratorControl.Text = "Générateur";
			this.cbxGeneratorControl.UseVisualStyleBackColor = true;
			// 
			// cbxDataControl
			// 
			this.cbxDataControl.AutoSize = true;
			this.cbxDataControl.Location = new System.Drawing.Point(7, 43);
			this.cbxDataControl.Name = "cbxDataControl";
			this.cbxDataControl.Size = new System.Drawing.Size(49, 17);
			this.cbxDataControl.TabIndex = 1;
			this.cbxDataControl.Text = "Data";
			this.cbxDataControl.UseVisualStyleBackColor = true;
			// 
			// cbxProfilControl
			// 
			this.cbxProfilControl.AutoSize = true;
			this.cbxProfilControl.Location = new System.Drawing.Point(7, 20);
			this.cbxProfilControl.Name = "cbxProfilControl";
			this.cbxProfilControl.Size = new System.Drawing.Size(49, 17);
			this.cbxProfilControl.TabIndex = 0;
			this.cbxProfilControl.Text = "Profil";
			this.cbxProfilControl.UseVisualStyleBackColor = true;
			// 
			// tbxTaskName
			// 
			this.tbxTaskName.Location = new System.Drawing.Point(12, 12);
			this.tbxTaskName.Name = "tbxTaskName";
			this.tbxTaskName.Size = new System.Drawing.Size(100, 20);
			this.tbxTaskName.TabIndex = 5;
			// 
			// btnControler
			// 
			this.btnControler.Location = new System.Drawing.Point(23, 131);
			this.btnControler.Name = "btnControler";
			this.btnControler.Size = new System.Drawing.Size(75, 23);
			this.btnControler.TabIndex = 6;
			this.btnControler.Text = "Contrôler";
			this.btnControler.UseVisualStyleBackColor = true;
			this.btnControler.Click += new System.EventHandler(this.btnControler_Click);
			// 
			// FormControlLauncher
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(124, 162);
			this.Controls.Add(this.btnControler);
			this.Controls.Add(this.tbxTaskName);
			this.Controls.Add(this.gbControleurs);
			this.MaximizeBox = false;
			this.Name = "FormControlLauncher";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.gbControleurs.ResumeLayout(false);
			this.gbControleurs.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbControleurs;
		private System.Windows.Forms.TextBox tbxTaskName;
		private System.Windows.Forms.Button btnControler;
		private System.Windows.Forms.CheckBox cbxProfilControl;
		private System.Windows.Forms.CheckBox cbxGeneratorControl;
		private System.Windows.Forms.CheckBox cbxDataControl;
    }
}

