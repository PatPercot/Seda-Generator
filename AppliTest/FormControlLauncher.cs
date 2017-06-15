using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AppliTest;
using RngProfileControllerLauncher;


namespace AppliTest
{
    public partial class FormControlLauncher : Form
	{
		FormControlResult fres;
        public FormControlLauncher()
        {
            InitializeComponent();
        }

        private void btnControler_Click(object sender, EventArgs e)
        {
			fres = new FormControlResult();
			fres.Show();
        }

		private void cbx_MouseUp(object sender, MouseEventArgs e)
		{
			activationBoutonControler();
		}

		private void cbx_KeyUp(object sender, KeyEventArgs e)
		{
			activationBoutonControler();
		}

		private void activationBoutonControler()
		{
			btnControler.Enabled = this.cbxProfilControl.Checked || this.cbxDataControl.Checked || this.cbxGeneratorControl.Checked;
		}

		private void button1_Click(object sender, EventArgs e)
		{

		}
    }
}
