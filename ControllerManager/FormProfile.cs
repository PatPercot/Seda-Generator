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
using System.Diagnostics;

namespace ControllerManager
{
	public partial class FormProfile : Form
	{
		FormCreateTasks fctasks;
		public FormProfile()
		{
			InitializeComponent();
		}

		private void btnCreateProfile_Click(object sender, EventArgs e) {
			String pathExecution = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\..\\..\\"; ;
			Process myProcess = new Process();
			try {
				myProcess.StartInfo.UseShellExecute = false;
				myProcess.StartInfo.WorkingDirectory = pathExecution;
				myProcess.StartInfo.FileName = @"C:/Program Files (x86)/Java/jre1.8.0_121/bin/java.exe";
				myProcess.StartInfo.Arguments = "-jar " + Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
					+ @"/travail/profils/agape-v115.jar";
				myProcess.StartInfo.CreateNoWindow = true;
				myProcess.StartInfo.RedirectStandardError = true;
				myProcess.StartInfo.StandardErrorEncoding = System.Text.ASCIIEncoding.Default;
				// ATTENTION : la présence des deux instructions ci-dessous empêche le processus de se terminer
				// myProcess.StartInfo.RedirectStandardOutput = true;
				// myProcess.StartInfo.StandardOutputEncoding = System.Text.ASCIIEncoding.Default;
				myProcess.Start();
				int nbBcl = 0;
				while (nbBcl < 120 && myProcess.WaitForExit(250) == false) {
					++nbBcl;
				}
				if (nbBcl == 120)  // 30 secondes max
					myProcess.Kill();
				// Parce que l'on a redirigé les sorties standards, il faut appeler WaitForExit
				myProcess.WaitForExit();
			} catch (Exception a) {
				MessageBox.Show(a.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			//System.Diagnostics.Process.Start("java.exe -jar agape-v115.jar");
		}

		private void btnManageTasks_Click(object sender, EventArgs e) {
			
		}

		private void btnNewTask_Click(object sender, EventArgs e) {
			fctasks = new FormCreateTasks();
			fctasks.Show();
		}
	}
}
