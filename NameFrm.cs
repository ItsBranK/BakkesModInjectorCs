using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BakkesModInjectorCs {
    public partial class nameFrm : Form {
        public nameFrm() {
            InitializeComponent();
        }

        private void NameFrm_Load(object sender, EventArgs e)  {
            nameBox.Clear();
        }

        private void NameFrm_FormClosing(object sender, EventArgs e) {
            mainFrm mf = new mainFrm();
            mf.Show();
        }

        private void confirmBtn_Click(object sender, EventArgs e) {
            Properties.Settings.Default.WINDOW_TTILE = nameBox.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void defaultBtn_Click(object sender, EventArgs e) {
            Properties.Settings.Default.WINDOW_TTILE = "BakkesModInjectorCs - Community Edition";
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e) {
            Properties.Settings.Default.WINDOW_TTILE = "BakkesModInjectorCs - Community Edition";
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
