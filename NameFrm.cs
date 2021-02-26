using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BakkesModInjectorCs
{
    public partial class NameFrm : Form
    {
        public NameFrm()
        {
            InitializeComponent();
        }

        private void NameFrm_Load(object sender, EventArgs e)
        {
            NameBox.Clear();
        }

        private void NameFrm_FormClosing(object sender, EventArgs e)
        {
            MainFrm mf = new MainFrm();
            mf.Show();
        }

        private void ConfirmBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.WINDOW_TTILE = NameBox.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.WINDOW_TTILE = "BakkesModInjectorCs - Community Edition";
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.WINDOW_TTILE = "BakkesModInjectorCs - Community Edition";
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
