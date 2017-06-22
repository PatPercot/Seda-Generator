namespace ControllerManager
{
	partial class FormProfile
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
			this.btnManageTasks = new System.Windows.Forms.Button();
			this.btnCreateProfil = new System.Windows.Forms.Button();
			this.btnNewTask = new System.Windows.Forms.Button();
			this.btnEdit = new System.Windows.Forms.Button();
			this.btnRemove = new System.Windows.Forms.Button();
			this.btnReturn = new System.Windows.Forms.Button();
			this.lbxTasks = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// btnManageTasks
			// 
			this.btnManageTasks.Location = new System.Drawing.Point(12, 12);
			this.btnManageTasks.Name = "btnManageTasks";
			this.btnManageTasks.Size = new System.Drawing.Size(81, 35);
			this.btnManageTasks.TabIndex = 0;
			this.btnManageTasks.Text = "&Management des tâches";
			this.btnManageTasks.UseVisualStyleBackColor = true;
			this.btnManageTasks.Click += new System.EventHandler(this.btnManageTasks_Click);
			// 
			// btnCreateProfil
			// 
			this.btnCreateProfil.Location = new System.Drawing.Point(142, 12);
			this.btnCreateProfil.Name = "btnCreateProfil";
			this.btnCreateProfil.Size = new System.Drawing.Size(81, 35);
			this.btnCreateProfil.TabIndex = 1;
			this.btnCreateProfil.Text = "&Créer un profil";
			this.btnCreateProfil.UseVisualStyleBackColor = true;
			this.btnCreateProfil.Click += new System.EventHandler(this.btnCreateProfile_Click);
			// 
			// btnNewTask
			// 
			this.btnNewTask.Location = new System.Drawing.Point(306, 12);
			this.btnNewTask.Name = "btnNewTask";
			this.btnNewTask.Size = new System.Drawing.Size(81, 35);
			this.btnNewTask.TabIndex = 2;
			this.btnNewTask.Text = "&Nouvelle tâche";
			this.btnNewTask.UseVisualStyleBackColor = true;
			this.btnNewTask.Click += new System.EventHandler(this.btnNewTask_Click);
			// 
			// btnEdit
			// 
			this.btnEdit.Location = new System.Drawing.Point(306, 53);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.Size = new System.Drawing.Size(81, 35);
			this.btnEdit.TabIndex = 3;
			this.btnEdit.Text = "&Modifier";
			this.btnEdit.UseVisualStyleBackColor = true;
			// 
			// btnRemove
			// 
			this.btnRemove.Location = new System.Drawing.Point(306, 94);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(81, 35);
			this.btnRemove.TabIndex = 4;
			this.btnRemove.Text = "&Supprimer";
			this.btnRemove.UseVisualStyleBackColor = true;
			// 
			// btnReturn
			// 
			this.btnReturn.Location = new System.Drawing.Point(306, 135);
			this.btnReturn.Name = "btnReturn";
			this.btnReturn.Size = new System.Drawing.Size(81, 35);
			this.btnReturn.TabIndex = 5;
			this.btnReturn.Text = "&Retour";
			this.btnReturn.UseVisualStyleBackColor = true;
			// 
			// lbxTasks
			// 
			this.lbxTasks.FormattingEnabled = true;
			this.lbxTasks.Location = new System.Drawing.Point(12, 12);
			this.lbxTasks.Name = "lbxTasks";
			this.lbxTasks.Size = new System.Drawing.Size(288, 160);
			this.lbxTasks.TabIndex = 6;
			// 
			// FormProfile
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(399, 182);
			this.Controls.Add(this.lbxTasks);
			this.Controls.Add(this.btnReturn);
			this.Controls.Add(this.btnRemove);
			this.Controls.Add(this.btnEdit);
			this.Controls.Add(this.btnNewTask);
			this.Controls.Add(this.btnCreateProfil);
			this.Controls.Add(this.btnManageTasks);
			this.Location = new System.Drawing.Point(100, 100);
			this.Name = "FormProfile";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Controller Task Manager";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnManageTasks;
		private System.Windows.Forms.Button btnCreateProfil;
		private System.Windows.Forms.Button btnNewTask;
		private System.Windows.Forms.Button btnEdit;
		private System.Windows.Forms.Button btnRemove;
		private System.Windows.Forms.Button btnReturn;
		private System.Windows.Forms.ListBox lbxTasks;
	}
}