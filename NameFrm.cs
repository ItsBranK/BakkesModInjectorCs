using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BakkesModInjectorCs {
    public partial class NameFrm : Form {
        public NameFrm() {
            InitializeComponent();
        }

        private void NameFrm_Load(object sender, EventArgs e)  {
            nameBox.Clear();
        }

        private void NameFrm_FormClosing(object sender, EventArgs e) {
            MainFrm mf = new MainFrm();
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
