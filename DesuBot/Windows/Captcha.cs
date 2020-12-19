using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesuBot
{
    public partial class Captcha : Form
    {
        public Captcha()
        {
            InitializeComponent();
        }
    private void Captcha_Load(object sender, EventArgs e)
        {
            DesuBot main = this.Owner as DesuBot;
            //if (main != null)
            //{
            //    string s = richCaptcha.Text;
            //    main.captchaKey = "";
            //    pictureBox1.ImageLocation = main.captchaUrl;
            //}
        }
        public string TheValue
        {
            get { return richCaptcha.Text; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
