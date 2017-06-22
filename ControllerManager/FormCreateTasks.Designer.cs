namespace ControllerManager
{
	partial class FormCreateTasks
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
			this.gbxTasks = new System.Windows.Forms.GroupBox();
			this.cbxProfileTask = new System.Windows.Forms.CheckBox();
			this.cbxGeneratorTask = new System.Windows.Forms.CheckBox();
			this.cbxDataTask = new System.Windows.Forms.CheckBox();
			this.btnRetour = new System.Windows.Forms.Button();
			this.tbxTaskName = new System.Windows.Forms.TextBox();
			this.lblTaskName = new System.Windows.Forms.Label();
			this.gbxProfile = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tbxProfileData = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.lblTrace = new System.Windows.Forms.Label();
			this.tbxProfileTrace = new System.Windows.Forms.TextBox();
			this.tbxProfileProfile = new System.Windows.Forms.TextBox();
			this.gbxGenerator = new System.Windows.Forms.GroupBox();
			this.label19 = new System.Windows.Forms.Label();
			this.tbxGeneratorData = new System.Windows.Forms.TextBox();
			this.gbxAccordVersement = new System.Windows.Forms.GroupBox();
			this.label25 = new System.Windows.Forms.Label();
			this.tbxAccordVersementProfile = new System.Windows.Forms.TextBox();
			this.gbxArchivalAgency = new System.Windows.Forms.GroupBox();
			this.label16 = new System.Windows.Forms.Label();
			this.tbxArchivalAgencyDesc = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.tbxArchivalAgencyName = new System.Windows.Forms.TextBox();
			this.tbxArchivalAgencyId = new System.Windows.Forms.TextBox();
			this.gbxTansferringAgency = new System.Windows.Forms.GroupBox();
			this.label11 = new System.Windows.Forms.Label();
			this.tbxTransferringAgencyDesc = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.tbxTransferringAgencyName = new System.Windows.Forms.TextBox();
			this.tbxTransferringAgencyId = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.tbxSaeServer = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.tbxTransferIdPrefix = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tbxUriBase = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbxDocumentsRepertory = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.tbxBordereau = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.tbxGeneratorTrace = new System.Windows.Forms.TextBox();
			this.gbxData = new System.Windows.Forms.GroupBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tbxDataData = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.tbxDataTrace = new System.Windows.Forms.TextBox();
			this.tbxDataProfile = new System.Windows.Forms.TextBox();
			this.btnCreate = new System.Windows.Forms.Button();
			this.lblExamples = new System.Windows.Forms.Label();
			this.lblProfileExample1 = new System.Windows.Forms.Label();
			this.lblProfileExample2 = new System.Windows.Forms.Label();
			this.lblProfileExample3 = new System.Windows.Forms.Label();
			this.lblTaskExample3 = new System.Windows.Forms.Label();
			this.lblTaskExample2 = new System.Windows.Forms.Label();
			this.lblTaskExample1 = new System.Windows.Forms.Label();
			this.lblDataDonneesMetier = new System.Windows.Forms.Label();
			this.rdbtnFollowTaskName = new System.Windows.Forms.RadioButton();
			this.rdbtnFollowProfileName = new System.Windows.Forms.RadioButton();
			this.gbxFollowName = new System.Windows.Forms.GroupBox();
			this.rdbtnFollowNone = new System.Windows.Forms.RadioButton();
			this.gbxTasks.SuspendLayout();
			this.gbxProfile.SuspendLayout();
			this.gbxGenerator.SuspendLayout();
			this.gbxAccordVersement.SuspendLayout();
			this.gbxArchivalAgency.SuspendLayout();
			this.gbxTansferringAgency.SuspendLayout();
			this.gbxData.SuspendLayout();
			this.gbxFollowName.SuspendLayout();
			this.SuspendLayout();
			// 
			// gbxTasks
			// 
			this.gbxTasks.Controls.Add(this.cbxProfileTask);
			this.gbxTasks.Controls.Add(this.cbxGeneratorTask);
			this.gbxTasks.Controls.Add(this.cbxDataTask);
			this.gbxTasks.Location = new System.Drawing.Point(22, 12);
			this.gbxTasks.Name = "gbxTasks";
			this.gbxTasks.Size = new System.Drawing.Size(197, 42);
			this.gbxTasks.TabIndex = 0;
			this.gbxTasks.TabStop = false;
			this.gbxTasks.Text = "Tâche(s) à créer";
			// 
			// cbxProfileTask
			// 
			this.cbxProfileTask.AutoSize = true;
			this.cbxProfileTask.Location = new System.Drawing.Point(6, 19);
			this.cbxProfileTask.Name = "cbxProfileTask";
			this.cbxProfileTask.Size = new System.Drawing.Size(49, 17);
			this.cbxProfileTask.TabIndex = 0;
			this.cbxProfileTask.Text = "&Profil";
			this.cbxProfileTask.UseVisualStyleBackColor = true;
			this.cbxProfileTask.CheckedChanged += new System.EventHandler(this.cbxProfilTask_CheckedChanged);
			// 
			// cbxGeneratorTask
			// 
			this.cbxGeneratorTask.AutoSize = true;
			this.cbxGeneratorTask.Location = new System.Drawing.Point(116, 19);
			this.cbxGeneratorTask.Name = "cbxGeneratorTask";
			this.cbxGeneratorTask.Size = new System.Drawing.Size(79, 17);
			this.cbxGeneratorTask.TabIndex = 2;
			this.cbxGeneratorTask.Text = "&Générateur";
			this.cbxGeneratorTask.UseVisualStyleBackColor = true;
			this.cbxGeneratorTask.CheckedChanged += new System.EventHandler(this.cbxGeneratorTask_CheckedChanged);
			// 
			// cbxDataTask
			// 
			this.cbxDataTask.AutoSize = true;
			this.cbxDataTask.Location = new System.Drawing.Point(61, 19);
			this.cbxDataTask.Name = "cbxDataTask";
			this.cbxDataTask.Size = new System.Drawing.Size(49, 17);
			this.cbxDataTask.TabIndex = 1;
			this.cbxDataTask.Text = "&Data";
			this.cbxDataTask.UseVisualStyleBackColor = true;
			this.cbxDataTask.CheckedChanged += new System.EventHandler(this.cbxDataTask_CheckedChanged);
			// 
			// btnRetour
			// 
			this.btnRetour.Location = new System.Drawing.Point(331, 24);
			this.btnRetour.Name = "btnRetour";
			this.btnRetour.Size = new System.Drawing.Size(75, 23);
			this.btnRetour.TabIndex = 28;
			this.btnRetour.Text = "&Retour";
			this.btnRetour.UseVisualStyleBackColor = true;
			this.btnRetour.Click += new System.EventHandler(this.btnRetour_Click);
			// 
			// tbxTaskName
			// 
			this.tbxTaskName.Enabled = false;
			this.tbxTaskName.Location = new System.Drawing.Point(112, 60);
			this.tbxTaskName.Name = "tbxTaskName";
			this.tbxTaskName.Size = new System.Drawing.Size(177, 20);
			this.tbxTaskName.TabIndex = 3;
			this.tbxTaskName.Visible = false;
			this.tbxTaskName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxTaskName_KeyUp);
			// 
			// lblTaskName
			// 
			this.lblTaskName.AutoSize = true;
			this.lblTaskName.Location = new System.Drawing.Point(15, 63);
			this.lblTaskName.Name = "lblTaskName";
			this.lblTaskName.Size = new System.Drawing.Size(91, 13);
			this.lblTaskName.TabIndex = 3;
			this.lblTaskName.Text = "Nom de la tâche :";
			this.lblTaskName.Visible = false;
			// 
			// gbxProfile
			// 
			this.gbxProfile.Controls.Add(this.label3);
			this.gbxProfile.Controls.Add(this.tbxProfileData);
			this.gbxProfile.Controls.Add(this.label2);
			this.gbxProfile.Controls.Add(this.lblTrace);
			this.gbxProfile.Controls.Add(this.tbxProfileTrace);
			this.gbxProfile.Controls.Add(this.tbxProfileProfile);
			this.gbxProfile.Enabled = false;
			this.gbxProfile.Location = new System.Drawing.Point(13, 273);
			this.gbxProfile.Name = "gbxProfile";
			this.gbxProfile.Size = new System.Drawing.Size(418, 103);
			this.gbxProfile.TabIndex = 7;
			this.gbxProfile.TabStop = false;
			this.gbxProfile.Text = "Profil";
			this.gbxProfile.Visible = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 74);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(85, 13);
			this.label3.TabIndex = 9;
			this.label3.Text = "Nom de la data :";
			// 
			// tbxProfileData
			// 
			this.tbxProfileData.Enabled = false;
			this.tbxProfileData.Location = new System.Drawing.Point(99, 71);
			this.tbxProfileData.Name = "tbxProfileData";
			this.tbxProfileData.Size = new System.Drawing.Size(313, 20);
			this.tbxProfileData.TabIndex = 9;
			this.tbxProfileData.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxProfileData_KeyUp);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 22);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(75, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Nom du profil :";
			// 
			// lblTrace
			// 
			this.lblTrace.AutoSize = true;
			this.lblTrace.Location = new System.Drawing.Point(6, 48);
			this.lblTrace.Name = "lblTrace";
			this.lblTrace.Size = new System.Drawing.Size(88, 13);
			this.lblTrace.TabIndex = 8;
			this.lblTrace.Text = "Nom de la trâce :";
			// 
			// tbxProfileTrace
			// 
			this.tbxProfileTrace.Enabled = false;
			this.tbxProfileTrace.Location = new System.Drawing.Point(99, 45);
			this.tbxProfileTrace.Name = "tbxProfileTrace";
			this.tbxProfileTrace.Size = new System.Drawing.Size(313, 20);
			this.tbxProfileTrace.TabIndex = 8;
			this.tbxProfileTrace.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxProfileTrace_KeyUp);
			// 
			// tbxProfileProfile
			// 
			this.tbxProfileProfile.Enabled = false;
			this.tbxProfileProfile.Location = new System.Drawing.Point(99, 19);
			this.tbxProfileProfile.Name = "tbxProfileProfile";
			this.tbxProfileProfile.Size = new System.Drawing.Size(313, 20);
			this.tbxProfileProfile.TabIndex = 7;
			this.tbxProfileProfile.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxProfileProfile_KeyUp);
			// 
			// gbxGenerator
			// 
			this.gbxGenerator.Controls.Add(this.label19);
			this.gbxGenerator.Controls.Add(this.tbxGeneratorData);
			this.gbxGenerator.Controls.Add(this.gbxAccordVersement);
			this.gbxGenerator.Controls.Add(this.label8);
			this.gbxGenerator.Controls.Add(this.tbxUriBase);
			this.gbxGenerator.Controls.Add(this.label4);
			this.gbxGenerator.Controls.Add(this.tbxDocumentsRepertory);
			this.gbxGenerator.Controls.Add(this.label5);
			this.gbxGenerator.Controls.Add(this.tbxBordereau);
			this.gbxGenerator.Controls.Add(this.label7);
			this.gbxGenerator.Controls.Add(this.tbxGeneratorTrace);
			this.gbxGenerator.Enabled = false;
			this.gbxGenerator.Location = new System.Drawing.Point(437, 12);
			this.gbxGenerator.Name = "gbxGenerator";
			this.gbxGenerator.Size = new System.Drawing.Size(476, 470);
			this.gbxGenerator.TabIndex = 13;
			this.gbxGenerator.TabStop = false;
			this.gbxGenerator.Text = "Générateur";
			this.gbxGenerator.Visible = false;
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(6, 126);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(85, 13);
			this.label19.TabIndex = 17;
			this.label19.Text = "Nom de la data :";
			// 
			// tbxGeneratorData
			// 
			this.tbxGeneratorData.Enabled = false;
			this.tbxGeneratorData.Location = new System.Drawing.Point(143, 123);
			this.tbxGeneratorData.Name = "tbxGeneratorData";
			this.tbxGeneratorData.Size = new System.Drawing.Size(313, 20);
			this.tbxGeneratorData.TabIndex = 17;
			this.tbxGeneratorData.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxGeneratorData_KeyUp);
			// 
			// gbxAccordVersement
			// 
			this.gbxAccordVersement.Controls.Add(this.label25);
			this.gbxAccordVersement.Controls.Add(this.tbxAccordVersementProfile);
			this.gbxAccordVersement.Controls.Add(this.gbxArchivalAgency);
			this.gbxAccordVersement.Controls.Add(this.gbxTansferringAgency);
			this.gbxAccordVersement.Controls.Add(this.label13);
			this.gbxAccordVersement.Controls.Add(this.tbxSaeServer);
			this.gbxAccordVersement.Controls.Add(this.label14);
			this.gbxAccordVersement.Controls.Add(this.tbxTransferIdPrefix);
			this.gbxAccordVersement.Location = new System.Drawing.Point(6, 149);
			this.gbxAccordVersement.Name = "gbxAccordVersement";
			this.gbxAccordVersement.Size = new System.Drawing.Size(463, 311);
			this.gbxAccordVersement.TabIndex = 18;
			this.gbxAccordVersement.TabStop = false;
			this.gbxAccordVersement.Text = "Accord de versement";
			// 
			// label25
			// 
			this.label25.AutoSize = true;
			this.label25.Location = new System.Drawing.Point(6, 42);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(75, 13);
			this.label25.TabIndex = 19;
			this.label25.Text = "Nom du profil :";
			// 
			// tbxAccordVersementProfile
			// 
			this.tbxAccordVersementProfile.Enabled = false;
			this.tbxAccordVersementProfile.Location = new System.Drawing.Point(137, 39);
			this.tbxAccordVersementProfile.Name = "tbxAccordVersementProfile";
			this.tbxAccordVersementProfile.Size = new System.Drawing.Size(313, 20);
			this.tbxAccordVersementProfile.TabIndex = 19;
			this.tbxAccordVersementProfile.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxAccordVersementProfile_KeyUp);
			// 
			// gbxArchivalAgency
			// 
			this.gbxArchivalAgency.Controls.Add(this.label16);
			this.gbxArchivalAgency.Controls.Add(this.tbxArchivalAgencyDesc);
			this.gbxArchivalAgency.Controls.Add(this.label17);
			this.gbxArchivalAgency.Controls.Add(this.label18);
			this.gbxArchivalAgency.Controls.Add(this.tbxArchivalAgencyName);
			this.gbxArchivalAgency.Controls.Add(this.tbxArchivalAgencyId);
			this.gbxArchivalAgency.Location = new System.Drawing.Point(7, 200);
			this.gbxArchivalAgency.Name = "gbxArchivalAgency";
			this.gbxArchivalAgency.Size = new System.Drawing.Size(450, 103);
			this.gbxArchivalAgency.TabIndex = 24;
			this.gbxArchivalAgency.TabStop = false;
			this.gbxArchivalAgency.Text = "&ArchivalAgency";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(6, 74);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(38, 13);
			this.label16.TabIndex = 26;
			this.label16.Text = "Desc :";
			// 
			// tbxArchivalAgencyDesc
			// 
			this.tbxArchivalAgencyDesc.Enabled = false;
			this.tbxArchivalAgencyDesc.Location = new System.Drawing.Point(131, 71);
			this.tbxArchivalAgencyDesc.Name = "tbxArchivalAgencyDesc";
			this.tbxArchivalAgencyDesc.Size = new System.Drawing.Size(313, 20);
			this.tbxArchivalAgencyDesc.TabIndex = 26;
			this.tbxArchivalAgencyDesc.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxArchivalAgencyDesc_KeyUp);
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(6, 22);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(22, 13);
			this.label17.TabIndex = 24;
			this.label17.Text = "Id :";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(6, 48);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(41, 13);
			this.label18.TabIndex = 25;
			this.label18.Text = "Name :";
			// 
			// tbxArchivalAgencyName
			// 
			this.tbxArchivalAgencyName.Enabled = false;
			this.tbxArchivalAgencyName.Location = new System.Drawing.Point(131, 45);
			this.tbxArchivalAgencyName.Name = "tbxArchivalAgencyName";
			this.tbxArchivalAgencyName.Size = new System.Drawing.Size(313, 20);
			this.tbxArchivalAgencyName.TabIndex = 25;
			this.tbxArchivalAgencyName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxArchivalAgencyName_KeyUp);
			// 
			// tbxArchivalAgencyId
			// 
			this.tbxArchivalAgencyId.Enabled = false;
			this.tbxArchivalAgencyId.Location = new System.Drawing.Point(131, 19);
			this.tbxArchivalAgencyId.Name = "tbxArchivalAgencyId";
			this.tbxArchivalAgencyId.Size = new System.Drawing.Size(313, 20);
			this.tbxArchivalAgencyId.TabIndex = 24;
			this.tbxArchivalAgencyId.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxArchivalAgencyId_KeyUp);
			// 
			// gbxTansferringAgency
			// 
			this.gbxTansferringAgency.Controls.Add(this.label11);
			this.gbxTansferringAgency.Controls.Add(this.tbxTransferringAgencyDesc);
			this.gbxTansferringAgency.Controls.Add(this.label12);
			this.gbxTansferringAgency.Controls.Add(this.label15);
			this.gbxTansferringAgency.Controls.Add(this.tbxTransferringAgencyName);
			this.gbxTansferringAgency.Controls.Add(this.tbxTransferringAgencyId);
			this.gbxTansferringAgency.Location = new System.Drawing.Point(7, 91);
			this.gbxTansferringAgency.Name = "gbxTansferringAgency";
			this.gbxTansferringAgency.Size = new System.Drawing.Size(450, 103);
			this.gbxTansferringAgency.TabIndex = 21;
			this.gbxTansferringAgency.TabStop = false;
			this.gbxTansferringAgency.Text = "&TansferringAgency";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(6, 74);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(38, 13);
			this.label11.TabIndex = 23;
			this.label11.Text = "Desc :";
			// 
			// tbxTransferringAgencyDesc
			// 
			this.tbxTransferringAgencyDesc.Enabled = false;
			this.tbxTransferringAgencyDesc.Location = new System.Drawing.Point(131, 71);
			this.tbxTransferringAgencyDesc.Name = "tbxTransferringAgencyDesc";
			this.tbxTransferringAgencyDesc.Size = new System.Drawing.Size(313, 20);
			this.tbxTransferringAgencyDesc.TabIndex = 23;
			this.tbxTransferringAgencyDesc.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxTransferringAgencyDesc_KeyUp);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(6, 22);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(22, 13);
			this.label12.TabIndex = 21;
			this.label12.Text = "Id :";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(6, 48);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(41, 13);
			this.label15.TabIndex = 22;
			this.label15.Text = "Name :";
			// 
			// tbxTransferringAgencyName
			// 
			this.tbxTransferringAgencyName.Enabled = false;
			this.tbxTransferringAgencyName.Location = new System.Drawing.Point(131, 45);
			this.tbxTransferringAgencyName.Name = "tbxTransferringAgencyName";
			this.tbxTransferringAgencyName.Size = new System.Drawing.Size(313, 20);
			this.tbxTransferringAgencyName.TabIndex = 22;
			this.tbxTransferringAgencyName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxTransferringAgencyName_KeyUp);
			// 
			// tbxTransferringAgencyId
			// 
			this.tbxTransferringAgencyId.Enabled = false;
			this.tbxTransferringAgencyId.Location = new System.Drawing.Point(131, 19);
			this.tbxTransferringAgencyId.Name = "tbxTransferringAgencyId";
			this.tbxTransferringAgencyId.Size = new System.Drawing.Size(313, 20);
			this.tbxTransferringAgencyId.TabIndex = 21;
			this.tbxTransferringAgencyId.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxTransferringAgencyId_KeyUp);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(6, 16);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(74, 13);
			this.label13.TabIndex = 18;
			this.label13.Text = "&Serveur SAE :";
			// 
			// tbxSaeServer
			// 
			this.tbxSaeServer.Enabled = false;
			this.tbxSaeServer.Location = new System.Drawing.Point(137, 13);
			this.tbxSaeServer.Name = "tbxSaeServer";
			this.tbxSaeServer.Size = new System.Drawing.Size(313, 20);
			this.tbxSaeServer.TabIndex = 18;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(6, 68);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(87, 13);
			this.label14.TabIndex = 20;
			this.label14.Text = "TransferIdPrefix :";
			// 
			// tbxTransferIdPrefix
			// 
			this.tbxTransferIdPrefix.Enabled = false;
			this.tbxTransferIdPrefix.Location = new System.Drawing.Point(137, 65);
			this.tbxTransferIdPrefix.Name = "tbxTransferIdPrefix";
			this.tbxTransferIdPrefix.Size = new System.Drawing.Size(313, 20);
			this.tbxTransferIdPrefix.TabIndex = 20;
			this.tbxTransferIdPrefix.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxTransferIdPrefix_KeyUp);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 100);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(84, 13);
			this.label8.TabIndex = 16;
			this.label8.Text = "URI de la base :";
			// 
			// tbxUriBase
			// 
			this.tbxUriBase.Enabled = false;
			this.tbxUriBase.Location = new System.Drawing.Point(143, 97);
			this.tbxUriBase.Name = "tbxUriBase";
			this.tbxUriBase.Size = new System.Drawing.Size(313, 20);
			this.tbxUriBase.TabIndex = 16;
			this.tbxUriBase.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxUriBase_KeyUp);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 74);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(137, 13);
			this.label4.TabIndex = 15;
			this.label4.Text = "Répertoire des documents :";
			// 
			// tbxDocumentsRepertory
			// 
			this.tbxDocumentsRepertory.Enabled = false;
			this.tbxDocumentsRepertory.Location = new System.Drawing.Point(143, 71);
			this.tbxDocumentsRepertory.Name = "tbxDocumentsRepertory";
			this.tbxDocumentsRepertory.Size = new System.Drawing.Size(313, 20);
			this.tbxDocumentsRepertory.TabIndex = 15;
			this.tbxDocumentsRepertory.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxDocumentsRepertory_KeyUp);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 22);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(101, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "Nom du &bordereau :";
			// 
			// tbxBordereau
			// 
			this.tbxBordereau.Enabled = false;
			this.tbxBordereau.Location = new System.Drawing.Point(143, 19);
			this.tbxBordereau.Name = "tbxBordereau";
			this.tbxBordereau.Size = new System.Drawing.Size(313, 20);
			this.tbxBordereau.TabIndex = 13;
			this.tbxBordereau.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxBordereau_KeyUp);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 48);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(88, 13);
			this.label7.TabIndex = 14;
			this.label7.Text = "Nom de la trâce :";
			// 
			// tbxGeneratorTrace
			// 
			this.tbxGeneratorTrace.Enabled = false;
			this.tbxGeneratorTrace.Location = new System.Drawing.Point(143, 44);
			this.tbxGeneratorTrace.Name = "tbxGeneratorTrace";
			this.tbxGeneratorTrace.Size = new System.Drawing.Size(313, 20);
			this.tbxGeneratorTrace.TabIndex = 14;
			this.tbxGeneratorTrace.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxGeneratorTrace_KeyUp);
			// 
			// gbxData
			// 
			this.gbxData.Controls.Add(this.label6);
			this.gbxData.Controls.Add(this.tbxDataData);
			this.gbxData.Controls.Add(this.label9);
			this.gbxData.Controls.Add(this.label10);
			this.gbxData.Controls.Add(this.tbxDataTrace);
			this.gbxData.Controls.Add(this.tbxDataProfile);
			this.gbxData.Enabled = false;
			this.gbxData.Location = new System.Drawing.Point(13, 379);
			this.gbxData.Name = "gbxData";
			this.gbxData.Size = new System.Drawing.Size(418, 103);
			this.gbxData.TabIndex = 10;
			this.gbxData.TabStop = false;
			this.gbxData.Text = "Data";
			this.gbxData.Visible = false;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 74);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(85, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "Nom de la data :";
			// 
			// tbxDataData
			// 
			this.tbxDataData.Enabled = false;
			this.tbxDataData.Location = new System.Drawing.Point(99, 71);
			this.tbxDataData.Name = "tbxDataData";
			this.tbxDataData.Size = new System.Drawing.Size(313, 20);
			this.tbxDataData.TabIndex = 12;
			this.tbxDataData.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxDataData_KeyUp);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(6, 22);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(75, 13);
			this.label9.TabIndex = 10;
			this.label9.Text = "Nom du profil :";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(6, 48);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(88, 13);
			this.label10.TabIndex = 11;
			this.label10.Text = "Nom de la trâce :";
			// 
			// tbxDataTrace
			// 
			this.tbxDataTrace.Enabled = false;
			this.tbxDataTrace.Location = new System.Drawing.Point(99, 45);
			this.tbxDataTrace.Name = "tbxDataTrace";
			this.tbxDataTrace.Size = new System.Drawing.Size(313, 20);
			this.tbxDataTrace.TabIndex = 11;
			this.tbxDataTrace.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxDataTrace_KeyUp);
			// 
			// tbxDataProfile
			// 
			this.tbxDataProfile.Enabled = false;
			this.tbxDataProfile.Location = new System.Drawing.Point(99, 19);
			this.tbxDataProfile.Name = "tbxDataProfile";
			this.tbxDataProfile.Size = new System.Drawing.Size(313, 20);
			this.tbxDataProfile.TabIndex = 10;
			this.tbxDataProfile.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxDataProfile_KeyUp);
			// 
			// btnCreate
			// 
			this.btnCreate.Enabled = false;
			this.btnCreate.Location = new System.Drawing.Point(250, 24);
			this.btnCreate.Name = "btnCreate";
			this.btnCreate.Size = new System.Drawing.Size(75, 23);
			this.btnCreate.TabIndex = 27;
			this.btnCreate.Text = "&Créer";
			this.btnCreate.UseVisualStyleBackColor = true;
			this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
			// 
			// lblExamples
			// 
			this.lblExamples.AutoSize = true;
			this.lblExamples.Location = new System.Drawing.Point(180, 142);
			this.lblExamples.Name = "lblExamples";
			this.lblExamples.Size = new System.Drawing.Size(58, 13);
			this.lblExamples.TabIndex = 5;
			this.lblExamples.Text = "Exemples :";
			this.lblExamples.Visible = false;
			// 
			// lblProfileExample1
			// 
			this.lblProfileExample1.AutoSize = true;
			this.lblProfileExample1.Location = new System.Drawing.Point(243, 168);
			this.lblProfileExample1.Name = "lblProfileExample1";
			this.lblProfileExample1.Size = new System.Drawing.Size(129, 13);
			this.lblProfileExample1.TabIndex = 5;
			this.lblProfileExample1.Text = "Nom profil -> testDeProfil1";
			this.lblProfileExample1.Visible = false;
			// 
			// lblProfileExample2
			// 
			this.lblProfileExample2.AutoSize = true;
			this.lblProfileExample2.Location = new System.Drawing.Point(243, 195);
			this.lblProfileExample2.Name = "lblProfileExample2";
			this.lblProfileExample2.Size = new System.Drawing.Size(189, 13);
			this.lblProfileExample2.TabIndex = 5;
			this.lblProfileExample2.Text = "Nom trâce -> testDeProfil1_profil_trace";
			this.lblProfileExample2.Visible = false;
			// 
			// lblProfileExample3
			// 
			this.lblProfileExample3.AutoSize = true;
			this.lblProfileExample3.Location = new System.Drawing.Point(243, 223);
			this.lblProfileExample3.Name = "lblProfileExample3";
			this.lblProfileExample3.Size = new System.Drawing.Size(155, 13);
			this.lblProfileExample3.TabIndex = 5;
			this.lblProfileExample3.Text = "Nom data -> testDeProfil1_data";
			this.lblProfileExample3.Visible = false;
			// 
			// lblTaskExample3
			// 
			this.lblTaskExample3.AutoSize = true;
			this.lblTaskExample3.Location = new System.Drawing.Point(16, 223);
			this.lblTaskExample3.Name = "lblTaskExample3";
			this.lblTaskExample3.Size = new System.Drawing.Size(211, 13);
			this.lblTaskExample3.TabIndex = 5;
			this.lblTaskExample3.Text = "Nom bordereau -> PrimTask911_bordereau";
			this.lblTaskExample3.Visible = false;
			// 
			// lblTaskExample2
			// 
			this.lblTaskExample2.AutoSize = true;
			this.lblTaskExample2.Location = new System.Drawing.Point(16, 195);
			this.lblTaskExample2.Name = "lblTaskExample2";
			this.lblTaskExample2.Size = new System.Drawing.Size(159, 13);
			this.lblTaskExample2.TabIndex = 5;
			this.lblTaskExample2.Text = "Nom profil -> PrimTask911_profil";
			this.lblTaskExample2.Visible = false;
			// 
			// lblTaskExample1
			// 
			this.lblTaskExample1.AutoSize = true;
			this.lblTaskExample1.Location = new System.Drawing.Point(16, 168);
			this.lblTaskExample1.Name = "lblTaskExample1";
			this.lblTaskExample1.Size = new System.Drawing.Size(136, 13);
			this.lblTaskExample1.TabIndex = 5;
			this.lblTaskExample1.Text = "Nom tâche -> PrimTask911";
			this.lblTaskExample1.Visible = false;
			// 
			// lblDataDonneesMetier
			// 
			this.lblDataDonneesMetier.AutoSize = true;
			this.lblDataDonneesMetier.Location = new System.Drawing.Point(154, 257);
			this.lblDataDonneesMetier.Name = "lblDataDonneesMetier";
			this.lblDataDonneesMetier.Size = new System.Drawing.Size(112, 13);
			this.lblDataDonneesMetier.TabIndex = 28;
			this.lblDataDonneesMetier.Text = "data = données métier";
			this.lblDataDonneesMetier.Visible = false;
			// 
			// rdbtnFollowTaskName
			// 
			this.rdbtnFollowTaskName.AutoSize = true;
			this.rdbtnFollowTaskName.Location = new System.Drawing.Point(6, 19);
			this.rdbtnFollowTaskName.Name = "rdbtnFollowTaskName";
			this.rdbtnFollowTaskName.Size = new System.Drawing.Size(67, 17);
			this.rdbtnFollowTaskName.TabIndex = 4;
			this.rdbtnFollowTaskName.TabStop = true;
			this.rdbtnFollowTaskName.Text = "La tâche";
			this.rdbtnFollowTaskName.UseVisualStyleBackColor = true;
			this.rdbtnFollowTaskName.CheckedChanged += new System.EventHandler(this.rdbtnFollowProfileName_CheckedChanged);
			// 
			// rdbtnFollowProfileName
			// 
			this.rdbtnFollowProfileName.AutoSize = true;
			this.rdbtnFollowProfileName.Location = new System.Drawing.Point(179, 19);
			this.rdbtnFollowProfileName.Name = "rdbtnFollowProfileName";
			this.rdbtnFollowProfileName.Size = new System.Drawing.Size(62, 17);
			this.rdbtnFollowProfileName.TabIndex = 5;
			this.rdbtnFollowProfileName.TabStop = true;
			this.rdbtnFollowProfileName.Text = "Le profil";
			this.rdbtnFollowProfileName.UseVisualStyleBackColor = true;
			// 
			// gbxFollowName
			// 
			this.gbxFollowName.Controls.Add(this.rdbtnFollowNone);
			this.gbxFollowName.Controls.Add(this.rdbtnFollowTaskName);
			this.gbxFollowName.Controls.Add(this.rdbtnFollowProfileName);
			this.gbxFollowName.Location = new System.Drawing.Point(13, 86);
			this.gbxFollowName.Name = "gbxFollowName";
			this.gbxFollowName.Size = new System.Drawing.Size(418, 53);
			this.gbxFollowName.TabIndex = 4;
			this.gbxFollowName.TabStop = false;
			this.gbxFollowName.Text = "Attribuer des noms en rapport avec";
			this.gbxFollowName.Visible = false;
			// 
			// rdbtnFollowNone
			// 
			this.rdbtnFollowNone.AutoSize = true;
			this.rdbtnFollowNone.Checked = true;
			this.rdbtnFollowNone.Enabled = false;
			this.rdbtnFollowNone.Location = new System.Drawing.Point(350, 19);
			this.rdbtnFollowNone.Name = "rdbtnFollowNone";
			this.rdbtnFollowNone.Size = new System.Drawing.Size(56, 17);
			this.rdbtnFollowNone.TabIndex = 6;
			this.rdbtnFollowNone.TabStop = true;
			this.rdbtnFollowNone.Text = "Aucun";
			this.rdbtnFollowNone.UseVisualStyleBackColor = true;
			this.rdbtnFollowNone.CheckedChanged += new System.EventHandler(this.rdbtnFollowNone_CheckedChanged);
			// 
			// FormCreateTasks
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(920, 484);
			this.Controls.Add(this.gbxFollowName);
			this.Controls.Add(this.lblDataDonneesMetier);
			this.Controls.Add(this.lblTaskExample3);
			this.Controls.Add(this.lblTaskExample2);
			this.Controls.Add(this.lblTaskExample1);
			this.Controls.Add(this.lblProfileExample3);
			this.Controls.Add(this.lblProfileExample2);
			this.Controls.Add(this.lblProfileExample1);
			this.Controls.Add(this.lblExamples);
			this.Controls.Add(this.btnCreate);
			this.Controls.Add(this.gbxData);
			this.Controls.Add(this.gbxGenerator);
			this.Controls.Add(this.gbxProfile);
			this.Controls.Add(this.lblTaskName);
			this.Controls.Add(this.tbxTaskName);
			this.Controls.Add(this.btnRetour);
			this.Controls.Add(this.gbxTasks);
			this.Location = new System.Drawing.Point(100, 100);
			this.Name = "FormCreateTasks";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Controller Task Manager";
			this.gbxTasks.ResumeLayout(false);
			this.gbxTasks.PerformLayout();
			this.gbxProfile.ResumeLayout(false);
			this.gbxProfile.PerformLayout();
			this.gbxGenerator.ResumeLayout(false);
			this.gbxGenerator.PerformLayout();
			this.gbxAccordVersement.ResumeLayout(false);
			this.gbxAccordVersement.PerformLayout();
			this.gbxArchivalAgency.ResumeLayout(false);
			this.gbxArchivalAgency.PerformLayout();
			this.gbxTansferringAgency.ResumeLayout(false);
			this.gbxTansferringAgency.PerformLayout();
			this.gbxData.ResumeLayout(false);
			this.gbxData.PerformLayout();
			this.gbxFollowName.ResumeLayout(false);
			this.gbxFollowName.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox gbxTasks;
		private System.Windows.Forms.CheckBox cbxProfileTask;
		private System.Windows.Forms.CheckBox cbxGeneratorTask;
		private System.Windows.Forms.CheckBox cbxDataTask;
		private System.Windows.Forms.Button btnRetour;
		private System.Windows.Forms.TextBox tbxTaskName;
		private System.Windows.Forms.Label lblTaskName;
		private System.Windows.Forms.GroupBox gbxProfile;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbxProfileData;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbxProfileProfile;
		private System.Windows.Forms.Label lblTrace;
		private System.Windows.Forms.TextBox tbxProfileTrace;
		private System.Windows.Forms.GroupBox gbxGenerator;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox tbxUriBase;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbxDocumentsRepertory;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox tbxBordereau;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox tbxGeneratorTrace;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.TextBox tbxGeneratorData;
		private System.Windows.Forms.GroupBox gbxAccordVersement;
		private System.Windows.Forms.GroupBox gbxArchivalAgency;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox tbxArchivalAgencyDesc;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.TextBox tbxArchivalAgencyName;
		private System.Windows.Forms.TextBox tbxArchivalAgencyId;
		private System.Windows.Forms.GroupBox gbxTansferringAgency;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox tbxTransferringAgencyDesc;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TextBox tbxTransferringAgencyName;
		private System.Windows.Forms.TextBox tbxTransferringAgencyId;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.TextBox tbxSaeServer;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox tbxTransferIdPrefix;
		private System.Windows.Forms.GroupBox gbxData;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox tbxDataData;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox tbxDataTrace;
		private System.Windows.Forms.TextBox tbxDataProfile;
		private System.Windows.Forms.Button btnCreate;
		private System.Windows.Forms.Label lblExamples;
		private System.Windows.Forms.Label lblProfileExample2;
		private System.Windows.Forms.Label lblProfileExample3;
		private System.Windows.Forms.Label lblTaskExample3;
		private System.Windows.Forms.Label lblTaskExample2;
		private System.Windows.Forms.Label lblTaskExample1;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.TextBox tbxAccordVersementProfile;
		private System.Windows.Forms.Label lblProfileExample1;
		private System.Windows.Forms.Label lblDataDonneesMetier;
		private System.Windows.Forms.RadioButton rdbtnFollowTaskName;
		private System.Windows.Forms.RadioButton rdbtnFollowProfileName;
		private System.Windows.Forms.GroupBox gbxFollowName;
		private System.Windows.Forms.RadioButton rdbtnFollowNone;
	}
}