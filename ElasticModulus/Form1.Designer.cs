namespace ElasticModulus
{
    partial class FormMain
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBoxMain = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.buttonStart2 = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label_MaterialsNum = new System.Windows.Forms.Label();
            this.nUD_MaterialsNum = new System.Windows.Forms.NumericUpDown();
            this.label_SquareNum = new System.Windows.Forms.Label();
            this.nUD_SquareNum = new System.Windows.Forms.NumericUpDown();
            this.buttonStart = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_MaterialsNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_SquareNum)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1406, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(59, 24);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureBoxMain);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(205, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1201, 799);
            this.panel1.TabIndex = 1;
            // 
            // pictureBoxMain
            // 
            this.pictureBoxMain.Location = new System.Drawing.Point(1, 1);
            this.pictureBoxMain.Name = "pictureBoxMain";
            this.pictureBoxMain.Size = new System.Drawing.Size(894, 559);
            this.pictureBoxMain.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxMain.TabIndex = 0;
            this.pictureBoxMain.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.Controls.Add(this.buttonStart2);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.buttonStart);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(205, 827);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1201, 223);
            this.panel2.TabIndex = 2;
            // 
            // buttonStart2
            // 
            this.buttonStart2.Location = new System.Drawing.Point(1083, 98);
            this.buttonStart2.Name = "buttonStart2";
            this.buttonStart2.Size = new System.Drawing.Size(88, 36);
            this.buttonStart2.TabIndex = 2;
            this.buttonStart2.Text = "button2";
            this.buttonStart2.UseVisualStyleBackColor = true;
            this.buttonStart2.Click += new System.EventHandler(this.buttonStart2_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label_MaterialsNum);
            this.panel3.Controls.Add(this.nUD_MaterialsNum);
            this.panel3.Controls.Add(this.label_SquareNum);
            this.panel3.Controls.Add(this.nUD_SquareNum);
            this.panel3.Location = new System.Drawing.Point(43, 29);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(355, 164);
            this.panel3.TabIndex = 1;
            // 
            // label_MaterialsNum
            // 
            this.label_MaterialsNum.AutoSize = true;
            this.label_MaterialsNum.Location = new System.Drawing.Point(38, 84);
            this.label_MaterialsNum.Name = "label_MaterialsNum";
            this.label_MaterialsNum.Size = new System.Drawing.Size(108, 17);
            this.label_MaterialsNum.TabIndex = 3;
            this.label_MaterialsNum.Text = "Число веществ";
            // 
            // nUD_MaterialsNum
            // 
            this.nUD_MaterialsNum.Location = new System.Drawing.Point(38, 107);
            this.nUD_MaterialsNum.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nUD_MaterialsNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUD_MaterialsNum.Name = "nUD_MaterialsNum";
            this.nUD_MaterialsNum.Size = new System.Drawing.Size(120, 22);
            this.nUD_MaterialsNum.TabIndex = 2;
            this.nUD_MaterialsNum.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUD_MaterialsNum.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label_SquareNum
            // 
            this.label_SquareNum.AutoSize = true;
            this.label_SquareNum.Location = new System.Drawing.Point(38, 15);
            this.label_SquareNum.Name = "label_SquareNum";
            this.label_SquareNum.Size = new System.Drawing.Size(167, 17);
            this.label_SquareNum.TabIndex = 1;
            this.label_SquareNum.Text = "Число клеток в радиусе";
            // 
            // nUD_SquareNum
            // 
            this.nUD_SquareNum.Location = new System.Drawing.Point(38, 38);
            this.nUD_SquareNum.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nUD_SquareNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUD_SquareNum.Name = "nUD_SquareNum";
            this.nUD_SquareNum.Size = new System.Drawing.Size(120, 22);
            this.nUD_SquareNum.TabIndex = 0;
            this.nUD_SquareNum.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nUD_SquareNum.ValueChanged += new System.EventHandler(this.nUD_SquareNum_ValueChanged);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(1083, 38);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(88, 36);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControl1.Location = new System.Drawing.Point(0, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(200, 1022);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(192, 993);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 880);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(205, 822);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1201, 5);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.splitter2.Location = new System.Drawing.Point(200, 28);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(5, 1022);
            this.splitter2.TabIndex = 5;
            this.splitter2.TabStop = false;
            this.splitter2.Visible = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1406, 1050);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "Do Not Forget To Name Me";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_MaterialsNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_SquareNum)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.PictureBox pictureBoxMain;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label_SquareNum;
        private System.Windows.Forms.NumericUpDown nUD_SquareNum;
        private System.Windows.Forms.Button buttonStart2;
        private System.Windows.Forms.Label label_MaterialsNum;
        private System.Windows.Forms.NumericUpDown nUD_MaterialsNum;
    }
}

