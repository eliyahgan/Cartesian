using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cartesian
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //以下为要额外添加部分
        private void Form1_Load(object sender, EventArgs e)
        {
            this.carBuPanel1.DrawBtnsPanel((int[])this.carBuPanel1.panelTempSize.Clone());
            ////Graphics test
            //Image newImage = Image.FromFile("D:/windowsPjt/Cartesian/Cartesian/SampImag.jpg");
            //PointF ulCorner = new PointF(100.0F, 100.0F);
            //Graphics a = this.button2.CreateGraphics();
            //a.DrawImage(newImage, ulCorner);

        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            this.carBuPanel1.NextBtn();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.carBuPanel1.BtnsRotate = (CarBuPanel.RotateDegree)((int)(this.carBuPanel1.BtnsRotate + 1) % 4);
            this.carBuPanel1.DrawBtnsPanel((int[])this.carBuPanel1.panelTempSize.Clone());
        }

    }
}
