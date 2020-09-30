namespace Cartesian
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.carBuPanel1 = new Cartesian.CarBuPanel(this.components);
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Silver;
            this.button1.Font = new System.Drawing.Font("宋体", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(36, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(147, 62);
            this.button1.TabIndex = 3;
            this.button1.Text = "NextBtn\r\n";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(308, 28);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(80, 72);
            this.button2.TabIndex = 7;
            this.button2.Text = "598*1057";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // carBuPanel1
            // 
            this.carBuPanel1.Location = new System.Drawing.Point(104, 131);
            this.carBuPanel1.Name = "carBuPanel1";
            this.carBuPanel1.Size = new System.Drawing.Size(797, 481);
            this.carBuPanel1.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1071, 687);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.carBuPanel1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private CarBuPanel carBuPanel1;
        private System.Windows.Forms.Button button2;
    }
}

