using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility.Tabs
{
    public partial class GPUTweaks : Form
    {
        public GPUTweaks()
        {
            InitializeComponent();

        }

        private void PremG_Click(object sender, EventArgs e)
        {
            Purchase purchase = new Purchase();
            purchase.Show();
        }

        private void PremG_MouseHover(object sender, EventArgs e)
        {

        }

        private void PremG2_Click(object sender, EventArgs e)
        {
            Purchase purchase = new Purchase();
            purchase.Show();
        }
    }
}
