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
	public partial class FormProfile : Form
	{
		FormCreateTasks fctasks;
		public FormProfile()
		{
			InitializeComponent();
		}

		private void btnCreerProfil_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("@/travail/profiles/agape-v115.jar");
		}

		private void btnProfilExistant_Click(object sender, EventArgs e)
		{
			fctasks = new FormCreateTasks();
			fctasks.Show();
		}
	}
}
