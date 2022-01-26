using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsMediaPlayer
{
    public partial class SeekForm : Form
    {
        public double SeekTime { get; set; }
        public double MaxTime { get; set; } 

        public SeekForm()
        {
            InitializeComponent();
        }

        private void SeekForm_Load(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = Convert.ToDecimal(MaxTime);
            numericUpDown1.Value = Convert.ToDecimal(SeekTime);
        }

        private void btnSeek_Click(object sender, EventArgs e)
        {
            SeekTime = Convert.ToDouble(numericUpDown1.Value);
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
