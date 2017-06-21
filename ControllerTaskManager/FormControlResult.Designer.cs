namespace ControllerTaskManager
{
    partial class FormControlResult
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.tbxRapport = new System.Windows.Forms.TextBox();
			this.lbxErrors = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// tbxRapport
			// 
			this.tbxRapport.AcceptsReturn = true;
			this.tbxRapport.AllowDrop = true;
			this.tbxRapport.Location = new System.Drawing.Point(12, 12);
			this.tbxRapport.Multiline = true;
			this.tbxRapport.Name = "tbxRapport";
			this.tbxRapport.Size = new System.Drawing.Size(205, 367);
			this.tbxRapport.TabIndex = 0;
			// 
			// lbxErrors
			// 
			this.lbxErrors.FormattingEnabled = true;
			this.lbxErrors.Location = new System.Drawing.Point(223, 11);
			this.lbxErrors.Name = "lbxErrors";
			this.lbxErrors.Size = new System.Drawing.Size(434, 368);
			this.lbxErrors.TabIndex = 1;
			// 
			// FormControlResult
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(669, 391);
			this.Controls.Add(this.lbxErrors);
			this.Controls.Add(this.tbxRapport);
			this.Name = "FormControlResult";
			this.ShowIcon = false;
			this.Text = "Rapport de controle";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbxRapport;
		private System.Windows.Forms.ListBox lbxErrors;
    }
}