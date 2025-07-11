namespace WSPR_Sked
{
    partial class LiveForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            testDBbutton = new System.Windows.Forms.Button();
            updatebutton = new System.Windows.Forms.Button();
            WXbutton = new System.Windows.Forms.Button();
            dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            bandlistBox = new System.Windows.Forms.ListBox();
            label5 = new System.Windows.Forms.Label();
            filterbutton = new System.Windows.Forms.Button();
            datecheckBox = new System.Windows.Forms.CheckBox();
            DFromtextBox = new System.Windows.Forms.TextBox();
            callFiltertextBox = new System.Windows.Forms.TextBox();
            DTotextBox = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            kmcheckBox = new System.Windows.Forms.CheckBox();
            Ulabel = new System.Windows.Forms.Label();
            Dlabel = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            textBox1 = new System.Windows.Forms.TextBox();
            Nowbutton = new System.Windows.Forms.Button();
            timer1 = new System.Windows.Forms.Timer(components);
            dataGridView1 = new System.Windows.Forms.DataGridView();
            label4 = new System.Windows.Forms.Label();
            PlistBox = new System.Windows.Forms.ListBox();
            label8 = new System.Windows.Forms.Label();
            Plabel = new System.Windows.Forms.Label();
            Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // testDBbutton
            // 
            testDBbutton.Location = new System.Drawing.Point(751, 1);
            testDBbutton.Name = "testDBbutton";
            testDBbutton.Size = new System.Drawing.Size(164, 21);
            testDBbutton.TabIndex = 0;
            testDBbutton.Text = "test connection to wspr.live";
            testDBbutton.UseVisualStyleBackColor = true;
            testDBbutton.Click += testDBbutton_Click;
            // 
            // updatebutton
            // 
            updatebutton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            updatebutton.Location = new System.Drawing.Point(619, 1);
            updatebutton.Margin = new System.Windows.Forms.Padding(0);
            updatebutton.Name = "updatebutton";
            updatebutton.Size = new System.Drawing.Size(126, 21);
            updatebutton.TabIndex = 2;
            updatebutton.Text = "Update from local db";
            updatebutton.UseVisualStyleBackColor = true;
            updatebutton.Click += updatebutton_Click;
            // 
            // WXbutton
            // 
            WXbutton.Location = new System.Drawing.Point(942, -1);
            WXbutton.Name = "WXbutton";
            WXbutton.Size = new System.Drawing.Size(75, 23);
            WXbutton.TabIndex = 4;
            WXbutton.Text = "button1";
            WXbutton.UseVisualStyleBackColor = true;
            WXbutton.Visible = false;
            WXbutton.Click += WXbutton_Click;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new System.Drawing.Point(608, 510);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new System.Drawing.Size(137, 23);
            dateTimePicker1.TabIndex = 5;
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Location = new System.Drawing.Point(778, 510);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new System.Drawing.Size(137, 23);
            dateTimePicker2.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(751, 514);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(21, 15);
            label1.TabIndex = 7;
            label1.Text = "to:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(564, 514);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(38, 15);
            label2.TabIndex = 8;
            label2.Text = "From:";
            // 
            // bandlistBox
            // 
            bandlistBox.FormattingEnabled = true;
            bandlistBox.Items.AddRange(new object[] { "All", "LF", "MF", "160m", "80m", "60m", "40m", "30m", "20m", "17m", "15m", "12m", "10m", "6m", "4m", "2m", "70cm", "23cm" });
            bandlistBox.Location = new System.Drawing.Point(325, 514);
            bandlistBox.Name = "bandlistBox";
            bandlistBox.Size = new System.Drawing.Size(70, 19);
            bandlistBox.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(282, 516);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(37, 15);
            label5.TabIndex = 12;
            label5.Text = "Band:";
            // 
            // filterbutton
            // 
            filterbutton.Location = new System.Drawing.Point(931, 510);
            filterbutton.Name = "filterbutton";
            filterbutton.Size = new System.Drawing.Size(66, 23);
            filterbutton.TabIndex = 13;
            filterbutton.Text = "Apply";
            filterbutton.UseVisualStyleBackColor = true;
            filterbutton.Click += filterbutton_Click;
            // 
            // datecheckBox
            // 
            datecheckBox.AutoSize = true;
            datecheckBox.Location = new System.Drawing.Point(434, 513);
            datecheckBox.Name = "datecheckBox";
            datecheckBox.Size = new System.Drawing.Size(114, 19);
            datecheckBox.TabIndex = 14;
            datecheckBox.Text = "Enable date filter";
            datecheckBox.UseVisualStyleBackColor = true;
            // 
            // DFromtextBox
            // 
            DFromtextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            DFromtextBox.Location = new System.Drawing.Point(642, 541);
            DFromtextBox.Name = "DFromtextBox";
            DFromtextBox.Size = new System.Drawing.Size(61, 22);
            DFromtextBox.TabIndex = 15;
            DFromtextBox.TextChanged += DFromtextBox_TextChanged;
            DFromtextBox.KeyPress += DFromtextBox_KeyPress;
            // 
            // callFiltertextBox
            // 
            callFiltertextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            callFiltertextBox.Location = new System.Drawing.Point(457, 543);
            callFiltertextBox.Margin = new System.Windows.Forms.Padding(0);
            callFiltertextBox.Name = "callFiltertextBox";
            callFiltertextBox.Size = new System.Drawing.Size(91, 22);
            callFiltertextBox.TabIndex = 16;
            callFiltertextBox.KeyPress += callFiltertextBox_KeyPress;
            // 
            // DTotextBox
            // 
            DTotextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            DTotextBox.Location = new System.Drawing.Point(768, 540);
            DTotextBox.Name = "DTotextBox";
            DTotextBox.Size = new System.Drawing.Size(59, 22);
            DTotextBox.TabIndex = 17;
            DTotextBox.KeyPress += DTotextBox_KeyPress;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(397, 547);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(57, 15);
            label3.TabIndex = 18;
            label3.Text = "Call filter:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(581, 542);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(55, 15);
            label6.TabIndex = 19;
            label6.Text = "Distance:";
            // 
            // kmcheckBox
            // 
            kmcheckBox.AutoSize = true;
            kmcheckBox.Location = new System.Drawing.Point(865, 541);
            kmcheckBox.Name = "kmcheckBox";
            kmcheckBox.Size = new System.Drawing.Size(65, 19);
            kmcheckBox.TabIndex = 21;
            kmcheckBox.Text = "Use km";
            kmcheckBox.UseVisualStyleBackColor = true;
            kmcheckBox.CheckedChanged += kmcheckBox_CheckedChanged;
            // 
            // Ulabel
            // 
            Ulabel.AutoSize = true;
            Ulabel.Location = new System.Drawing.Point(833, 541);
            Ulabel.Name = "Ulabel";
            Ulabel.Size = new System.Drawing.Size(26, 15);
            Ulabel.TabIndex = 22;
            Ulabel.Text = "mls";
            // 
            // Dlabel
            // 
            Dlabel.AutoSize = true;
            Dlabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Dlabel.Location = new System.Drawing.Point(709, 543);
            Dlabel.Name = "Dlabel";
            Dlabel.Size = new System.Drawing.Size(56, 13);
            Dlabel.TabIndex = 23;
            Dlabel.Text = "0 - 12,453";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label7.Location = new System.Drawing.Point(151, 1);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(390, 13);
            label7.TabIndex = 24;
            label7.Text = "To reduce traffic at wspr.live received reports may not appear for 6 minutes";
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(2, -1);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(100, 23);
            textBox1.TabIndex = 25;
            textBox1.Visible = false;
            // 
            // Nowbutton
            // 
            Nowbutton.Location = new System.Drawing.Point(27, 539);
            Nowbutton.Name = "Nowbutton";
            Nowbutton.Size = new System.Drawing.Size(88, 23);
            Nowbutton.TabIndex = 26;
            Nowbutton.Text = "Update now";
            Nowbutton.UseVisualStyleBackColor = true;
            Nowbutton.Click += Nowbutton_Click;
            // 
            // timer1
            // 
            timer1.Interval = 140000;
            timer1.Tick += timer1_Tick;
            // 
            // dataGridView1
            // 
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10, Column11, Column12, Column13 });
            dataGridView1.Location = new System.Drawing.Point(12, 28);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.Size = new System.Drawing.Size(1005, 476);
            dataGridView1.TabIndex = 3;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(116, 516);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(160, 15);
            label4.TabIndex = 10;
            label4.Text = "Filters (interrogates local db):";
            // 
            // PlistBox
            // 
            PlistBox.FormattingEnabled = true;
            PlistBox.Items.AddRange(new object[] { "10 min", "30 min", "1 hour", "3 hours", "6 hours", "12 hours", "24 hours" });
            PlistBox.Location = new System.Drawing.Point(137, 573);
            PlistBox.Name = "PlistBox";
            PlistBox.Size = new System.Drawing.Size(91, 19);
            PlistBox.TabIndex = 27;
            PlistBox.SelectedIndexChanged += PlistBox_SelectedIndexChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(46, 573);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(85, 15);
            label8.TabIndex = 28;
            label8.Text = "Update period:";
            // 
            // Plabel
            // 
            Plabel.AutoSize = true;
            Plabel.Location = new System.Drawing.Point(238, 577);
            Plabel.Name = "Plabel";
            Plabel.Size = new System.Drawing.Size(22, 15);
            Plabel.TabIndex = 29;
            Plabel.Text = "---";
            // 
            // Column1
            // 
            Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column1.HeaderText = "Date";
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            Column1.Width = 110;
            // 
            // Column2
            // 
            Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column2.HeaderText = "Call";
            Column2.Name = "Column2";
            Column2.ReadOnly = true;
            // 
            // Column3
            // 
            Column3.HeaderText = "Frequency";
            Column3.Name = "Column3";
            Column3.ReadOnly = true;
            Column3.Width = 87;
            // 
            // Column4
            // 
            Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column4.HeaderText = "SNR";
            Column4.Name = "Column4";
            Column4.ReadOnly = true;
            Column4.Width = 44;
            // 
            // Column5
            // 
            Column5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column5.HeaderText = "Drift";
            Column5.Name = "Column5";
            Column5.ReadOnly = true;
            Column5.Width = 45;
            // 
            // Column6
            // 
            Column6.HeaderText = "TX loc";
            Column6.Name = "Column6";
            Column6.ReadOnly = true;
            Column6.Width = 64;
            // 
            // Column7
            // 
            Column7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column7.HeaderText = "dBm";
            Column7.Name = "Column7";
            Column7.ReadOnly = true;
            Column7.Width = 44;
            // 
            // Column8
            // 
            Column8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column8.HeaderText = "Reporter";
            Column8.Name = "Column8";
            Column8.ReadOnly = true;
            Column8.Width = 110;
            // 
            // Column9
            // 
            Column9.HeaderText = "RX Loc";
            Column9.Name = "Column9";
            Column9.ReadOnly = true;
            Column9.Width = 68;
            // 
            // Column10
            // 
            Column10.HeaderText = "km";
            Column10.Name = "Column10";
            Column10.ReadOnly = true;
            Column10.Width = 49;
            // 
            // Column11
            // 
            Column11.HeaderText = "miles";
            Column11.Name = "Column11";
            Column11.ReadOnly = true;
            Column11.Width = 60;
            // 
            // Column12
            // 
            Column12.HeaderText = "Az";
            Column12.Name = "Column12";
            Column12.ReadOnly = true;
            Column12.Width = 45;
            // 
            // Column13
            // 
            Column13.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column13.HeaderText = "Version";
            Column13.Name = "Column13";
            Column13.ReadOnly = true;
            Column13.Width = 78;
            // 
            // LiveForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            ClientSize = new System.Drawing.Size(1028, 604);
            ControlBox = false;
            Controls.Add(Plabel);
            Controls.Add(label8);
            Controls.Add(PlistBox);
            Controls.Add(Nowbutton);
            Controls.Add(textBox1);
            Controls.Add(label7);
            Controls.Add(Dlabel);
            Controls.Add(Ulabel);
            Controls.Add(kmcheckBox);
            Controls.Add(label6);
            Controls.Add(label3);
            Controls.Add(DTotextBox);
            Controls.Add(callFiltertextBox);
            Controls.Add(DFromtextBox);
            Controls.Add(datecheckBox);
            Controls.Add(filterbutton);
            Controls.Add(label5);
            Controls.Add(bandlistBox);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dateTimePicker2);
            Controls.Add(dateTimePicker1);
            Controls.Add(WXbutton);
            Controls.Add(dataGridView1);
            Controls.Add(updatebutton);
            Controls.Add(testDBbutton);
            Name = "LiveForm";
            Text = "Received transmissions for this call";
            Load += LiveForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button testDBbutton;
        private System.Windows.Forms.Button updatebutton;
        private System.Windows.Forms.Button WXbutton;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox bandlistBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button filterbutton;
        private System.Windows.Forms.CheckBox datecheckBox;
        private System.Windows.Forms.TextBox DFromtextBox;
        private System.Windows.Forms.TextBox callFiltertextBox;
        private System.Windows.Forms.TextBox DTotextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox kmcheckBox;
        private System.Windows.Forms.Label Ulabel;
        private System.Windows.Forms.Label Dlabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button Nowbutton;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox PlistBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label Plabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column11;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column12;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column13;
    }
}