using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
namespace VisualExplorer
{
    
    public partial class aboutForm : Form
    {
        
        string currentVersion = "1.0.1";
        public aboutForm()
        {
            InitializeComponent();
            aboutLabel.Text = "Visual Explorer ver " + currentVersion;
        }

        private void aboutLabel_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Uri resultUri = new Uri("https://raw.githubusercontent.com/SangMinhTruong/visual-explorer/master/Setup/_version_.txt");
                Uri setupUri = new Uri("https://github.com/SangMinhTruong/visual-explorer/raw/master/Setup/veSetup.msi");
                using (WebClient wc = new WebClient())
                {

                    wc.DownloadFile(
                        resultUri
                   // Param1 = Link of file
                   ,
                   // Param2 = Path to save

                   "update.txt"
                    );
                }
                string updateVersion = File.ReadAllText("update.txt");
                if (updateVersion.Trim() == currentVersion.Trim())
                {
                    MessageBox.Show("You are already using the latest version !");
                    return;
                }
                FileInfo setupFile = new FileInfo("veSetup.msi");
                if (setupFile.Exists)
                    setupFile.Delete();
                using (WebClient wc = new WebClient())
                {

                    wc.DownloadFile(
                        setupUri
                   // Param1 = Link of file
                   ,
                   // Param2 = Path to save

                   "veSetup.msi"
                    );
                }

                Process.Start(setupFile.ToString());
                Application.Exit();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }
    }
}
