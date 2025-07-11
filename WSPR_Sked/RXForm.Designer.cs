namespace WSPR_Sked
{
    partial class RXForm
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
            button1 = new System.Windows.Forms.Button();
            dataGridView1 = new System.Windows.Forms.DataGridView();
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
            label1 = new System.Windows.Forms.Label();
            RXFlabel = new System.Windows.Forms.Label();
            OSDlistBox = new System.Windows.Forms.ListBox();
            label3 = new System.Windows.Forms.Label();
            DeepcheckBox = new System.Windows.Forms.CheckBox();
            QuickcheckBox = new System.Windows.Forms.CheckBox();
            OSDlabel = new System.Windows.Forms.Label();
            uploadcheckBox = new System.Windows.Forms.CheckBox();
            Dlabel = new System.Windows.Forms.Label();
            Ulabel = new System.Windows.Forms.Label();
            kmcheckBox = new System.Windows.Forms.CheckBox();
            label6 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            DTotextBox = new System.Windows.Forms.TextBox();
            callFiltertextBox = new System.Windows.Forms.TextBox();
            DFromtextBox = new System.Windows.Forms.TextBox();
            datecheckBox = new System.Windows.Forms.CheckBox();
            filterbutton = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            bandlistBox = new System.Windows.Forms.ListBox();
            label4 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            stopRXcheckBox = new System.Windows.Forms.CheckBox();
            allowDeletecheckBox = new System.Windows.Forms.CheckBox();
            label9 = new System.Windows.Forms.Label();
            statuslabel = new System.Windows.Forms.Label();
            RXofflabel = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            myloclabel = new System.Windows.Forms.Label();
            Timelabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            button1.Location = new System.Drawing.Point(770, 9);
            button1.Margin = new System.Windows.Forms.Padding(0);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(140, 20);
            button1.TabIndex = 0;
            button1.Text = "Show previous decodes";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10 });
            dataGridView1.Location = new System.Drawing.Point(23, 65);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new System.Drawing.Size(866, 443);
            dataGridView1.TabIndex = 2;
            dataGridView1.RowHeaderMouseClick += dataGridView1_RowHeaderMouseClick;
            // 
            // Column1
            // 
            Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column1.HeaderText = "Date/time";
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            Column1.Width = 110;
            // 
            // Column2
            // 
            Column2.HeaderText = "TX call";
            Column2.Name = "Column2";
            Column2.ReadOnly = true;
            // 
            // Column3
            // 
            Column3.HeaderText = "Frequency";
            Column3.Name = "Column3";
            Column3.ReadOnly = true;
            // 
            // Column4
            // 
            Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column4.HeaderText = "SNR";
            Column4.Name = "Column4";
            Column4.ReadOnly = true;
            Column4.Width = 50;
            // 
            // Column5
            // 
            Column5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column5.HeaderText = "drift";
            Column5.Name = "Column5";
            Column5.ReadOnly = true;
            Column5.Width = 50;
            // 
            // Column6
            // 
            Column6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column6.HeaderText = "dBm";
            Column6.Name = "Column6";
            Column6.ReadOnly = true;
            Column6.Width = 50;
            // 
            // Column7
            // 
            Column7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column7.HeaderText = "Locator";
            Column7.Name = "Column7";
            Column7.ReadOnly = true;
            Column7.Width = 70;
            // 
            // Column8
            // 
            Column8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column8.HeaderText = "km";
            Column8.Name = "Column8";
            Column8.ReadOnly = true;
            Column8.Width = 75;
            // 
            // Column9
            // 
            Column9.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column9.HeaderText = "mls";
            Column9.Name = "Column9";
            Column9.ReadOnly = true;
            Column9.Width = 75;
            // 
            // Column10
            // 
            Column10.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Column10.HeaderText = "Az";
            Column10.Name = "Column10";
            Column10.ReadOnly = true;
            Column10.Width = 60;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(23, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(95, 15);
            label1.TabIndex = 3;
            label1.Text = "Current RX MHz:";
            // 
            // RXFlabel
            // 
            RXFlabel.AutoSize = true;
            RXFlabel.Location = new System.Drawing.Point(120, 9);
            RXFlabel.Name = "RXFlabel";
            RXFlabel.Size = new System.Drawing.Size(12, 15);
            RXFlabel.TabIndex = 4;
            RXFlabel.Text = "-";
            // 
            // OSDlistBox
            // 
            OSDlistBox.FormattingEnabled = true;
            OSDlistBox.Items.AddRange(new object[] { "Off", "1", "2", "3", "4", "5" });
            OSDlistBox.Location = new System.Drawing.Point(711, 10);
            OSDlistBox.Name = "OSDlistBox";
            OSDlistBox.Size = new System.Drawing.Size(43, 19);
            OSDlistBox.TabIndex = 7;
            OSDlistBox.SelectedIndexChanged += OSDlistBox_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(638, 11);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(67, 15);
            label3.TabIndex = 8;
            label3.Text = "OSD depth:";
            // 
            // DeepcheckBox
            // 
            DeepcheckBox.AutoSize = true;
            DeepcheckBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            DeepcheckBox.Location = new System.Drawing.Point(527, 9);
            DeepcheckBox.Name = "DeepcheckBox";
            DeepcheckBox.Size = new System.Drawing.Size(89, 17);
            DeepcheckBox.TabIndex = 9;
            DeepcheckBox.Text = "Deep search";
            DeepcheckBox.UseVisualStyleBackColor = true;
            DeepcheckBox.CheckedChanged += DeepcheckBox_CheckedChanged;
            // 
            // QuickcheckBox
            // 
            QuickcheckBox.AutoSize = true;
            QuickcheckBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            QuickcheckBox.Location = new System.Drawing.Point(527, 32);
            QuickcheckBox.Name = "QuickcheckBox";
            QuickcheckBox.Size = new System.Drawing.Size(91, 17);
            QuickcheckBox.TabIndex = 10;
            QuickcheckBox.Text = "Quick search";
            QuickcheckBox.UseVisualStyleBackColor = true;
            QuickcheckBox.CheckedChanged += QuickcheckBox_CheckedChanged;
            // 
            // OSDlabel
            // 
            OSDlabel.AutoSize = true;
            OSDlabel.Location = new System.Drawing.Point(638, 32);
            OSDlabel.Name = "OSDlabel";
            OSDlabel.Size = new System.Drawing.Size(48, 15);
            OSDlabel.TabIndex = 11;
            OSDlabel.Text = "OSD off";
            // 
            // uploadcheckBox
            // 
            uploadcheckBox.AutoSize = true;
            uploadcheckBox.Location = new System.Drawing.Point(794, 40);
            uploadcheckBox.Name = "uploadcheckBox";
            uploadcheckBox.Size = new System.Drawing.Size(95, 19);
            uploadcheckBox.TabIndex = 12;
            uploadcheckBox.Text = "Upload spots";
            uploadcheckBox.UseVisualStyleBackColor = true;
            // 
            // Dlabel
            // 
            Dlabel.AutoSize = true;
            Dlabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Dlabel.Location = new System.Drawing.Point(628, 565);
            Dlabel.Name = "Dlabel";
            Dlabel.Size = new System.Drawing.Size(56, 13);
            Dlabel.TabIndex = 40;
            Dlabel.Text = "0 - 12,453";
            // 
            // Ulabel
            // 
            Ulabel.AutoSize = true;
            Ulabel.Location = new System.Drawing.Point(752, 563);
            Ulabel.Name = "Ulabel";
            Ulabel.Size = new System.Drawing.Size(26, 15);
            Ulabel.TabIndex = 39;
            Ulabel.Text = "mls";
            // 
            // kmcheckBox
            // 
            kmcheckBox.AutoSize = true;
            kmcheckBox.Location = new System.Drawing.Point(784, 563);
            kmcheckBox.Name = "kmcheckBox";
            kmcheckBox.Size = new System.Drawing.Size(65, 19);
            kmcheckBox.TabIndex = 38;
            kmcheckBox.Text = "Use km";
            kmcheckBox.UseVisualStyleBackColor = true;
            kmcheckBox.CheckedChanged += kmcheckBox_CheckedChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(500, 564);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(55, 15);
            label6.TabIndex = 37;
            label6.Text = "Distance:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(316, 569);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(57, 15);
            label2.TabIndex = 36;
            label2.Text = "Call filter:";
            // 
            // DTotextBox
            // 
            DTotextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            DTotextBox.Location = new System.Drawing.Point(687, 562);
            DTotextBox.Name = "DTotextBox";
            DTotextBox.Size = new System.Drawing.Size(59, 22);
            DTotextBox.TabIndex = 35;
            DTotextBox.KeyPress += DTotextBox_KeyPress;
            // 
            // callFiltertextBox
            // 
            callFiltertextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            callFiltertextBox.Location = new System.Drawing.Point(376, 565);
            callFiltertextBox.Margin = new System.Windows.Forms.Padding(0);
            callFiltertextBox.Name = "callFiltertextBox";
            callFiltertextBox.Size = new System.Drawing.Size(91, 22);
            callFiltertextBox.TabIndex = 34;
            callFiltertextBox.TextChanged += callFiltertextBox_TextChanged;
            callFiltertextBox.KeyPress += callFiltertextBox_KeyPress;
            // 
            // DFromtextBox
            // 
            DFromtextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            DFromtextBox.Location = new System.Drawing.Point(561, 563);
            DFromtextBox.Name = "DFromtextBox";
            DFromtextBox.Size = new System.Drawing.Size(61, 22);
            DFromtextBox.TabIndex = 33;
            DFromtextBox.KeyPress += DFromtextBox_KeyPress;
            // 
            // datecheckBox
            // 
            datecheckBox.AutoSize = true;
            datecheckBox.Location = new System.Drawing.Point(353, 535);
            datecheckBox.Name = "datecheckBox";
            datecheckBox.Size = new System.Drawing.Size(114, 19);
            datecheckBox.TabIndex = 32;
            datecheckBox.Text = "Enable date filter";
            datecheckBox.UseVisualStyleBackColor = true;
            datecheckBox.CheckedChanged += datecheckBox_CheckedChanged;
            // 
            // filterbutton
            // 
            filterbutton.Location = new System.Drawing.Point(850, 532);
            filterbutton.Name = "filterbutton";
            filterbutton.Size = new System.Drawing.Size(66, 23);
            filterbutton.TabIndex = 31;
            filterbutton.Text = "Apply";
            filterbutton.UseVisualStyleBackColor = true;
            filterbutton.Click += filterbutton_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(201, 538);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(37, 15);
            label5.TabIndex = 30;
            label5.Text = "Band:";
            // 
            // bandlistBox
            // 
            bandlistBox.FormattingEnabled = true;
            bandlistBox.Items.AddRange(new object[] { "All", "LF\t", "MF", "160m", "80m", "60m", "40m", "30m", "20m", "17m", "15m", "12m", "10m", "6m", "4m", "2m", "70cm", "23cm" });
            bandlistBox.Location = new System.Drawing.Point(244, 536);
            bandlistBox.Name = "bandlistBox";
            bandlistBox.Size = new System.Drawing.Size(70, 19);
            bandlistBox.TabIndex = 29;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(35, 538);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(160, 15);
            label4.TabIndex = 28;
            label4.Text = "Filters (interrogates local db):";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(483, 536);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(38, 15);
            label7.TabIndex = 27;
            label7.Text = "From:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(670, 536);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(21, 15);
            label8.TabIndex = 26;
            label8.Text = "to:";
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Location = new System.Drawing.Point(697, 532);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new System.Drawing.Size(137, 23);
            dateTimePicker2.TabIndex = 25;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new System.Drawing.Point(527, 532);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new System.Drawing.Size(137, 23);
            dateTimePicker1.TabIndex = 24;
            dateTimePicker1.KeyPress += dateTimePicker1_KeyPress;
            // 
            // stopRXcheckBox
            // 
            stopRXcheckBox.AutoSize = true;
            stopRXcheckBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            stopRXcheckBox.Location = new System.Drawing.Point(12, 570);
            stopRXcheckBox.Name = "stopRXcheckBox";
            stopRXcheckBox.Size = new System.Drawing.Size(217, 17);
            stopRXcheckBox.TabIndex = 41;
            stopRXcheckBox.Text = "Stop receiving (don't interupt search)";
            stopRXcheckBox.UseVisualStyleBackColor = true;
            stopRXcheckBox.CheckedChanged += stopRXcheckBox_CheckedChanged;
            // 
            // allowDeletecheckBox
            // 
            allowDeletecheckBox.AutoSize = true;
            allowDeletecheckBox.Location = new System.Drawing.Point(12, 593);
            allowDeletecheckBox.Name = "allowDeletecheckBox";
            allowDeletecheckBox.Size = new System.Drawing.Size(125, 19);
            allowDeletecheckBox.TabIndex = 42;
            allowDeletecheckBox.Text = "Allow row deletion";
            allowDeletecheckBox.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(23, 34);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(42, 15);
            label9.TabIndex = 43;
            label9.Text = "Status:";
            // 
            // statuslabel
            // 
            statuslabel.AutoSize = true;
            statuslabel.Location = new System.Drawing.Point(71, 34);
            statuslabel.Name = "statuslabel";
            statuslabel.Size = new System.Drawing.Size(17, 15);
            statuslabel.TabIndex = 44;
            statuslabel.Text = "--";
            // 
            // RXofflabel
            // 
            RXofflabel.AutoSize = true;
            RXofflabel.Location = new System.Drawing.Point(244, 41);
            RXofflabel.Name = "RXofflabel";
            RXofflabel.Size = new System.Drawing.Size(69, 15);
            RXofflabel.TabIndex = 45;
            RXofflabel.Text = "RX listening";
            RXofflabel.Click += RXofflabel_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(201, 9);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(67, 15);
            label10.TabIndex = 46;
            label10.Text = "My locator:";
            // 
            // myloclabel
            // 
            myloclabel.AutoSize = true;
            myloclabel.Location = new System.Drawing.Point(274, 9);
            myloclabel.Name = "myloclabel";
            myloclabel.Size = new System.Drawing.Size(17, 15);
            myloclabel.TabIndex = 47;
            myloclabel.Text = "--";
            // 
            // Timelabel
            // 
            Timelabel.AutoSize = true;
            Timelabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            Timelabel.Location = new System.Drawing.Point(358, 11);
            Timelabel.Name = "Timelabel";
            Timelabel.Size = new System.Drawing.Size(35, 15);
            Timelabel.TabIndex = 48;
            Timelabel.Text = "Time";
            // 
            // RXForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            ClientSize = new System.Drawing.Size(919, 620);
            ControlBox = false;
            Controls.Add(Timelabel);
            Controls.Add(myloclabel);
            Controls.Add(label10);
            Controls.Add(RXofflabel);
            Controls.Add(statuslabel);
            Controls.Add(label9);
            Controls.Add(allowDeletecheckBox);
            Controls.Add(stopRXcheckBox);
            Controls.Add(Dlabel);
            Controls.Add(Ulabel);
            Controls.Add(kmcheckBox);
            Controls.Add(label6);
            Controls.Add(label2);
            Controls.Add(DTotextBox);
            Controls.Add(callFiltertextBox);
            Controls.Add(DFromtextBox);
            Controls.Add(datecheckBox);
            Controls.Add(filterbutton);
            Controls.Add(label5);
            Controls.Add(bandlistBox);
            Controls.Add(label4);
            Controls.Add(label7);
            Controls.Add(label8);
            Controls.Add(dateTimePicker2);
            Controls.Add(dateTimePicker1);
            Controls.Add(uploadcheckBox);
            Controls.Add(OSDlabel);
            Controls.Add(QuickcheckBox);
            Controls.Add(DeepcheckBox);
            Controls.Add(label3);
            Controls.Add(OSDlistBox);
            Controls.Add(RXFlabel);
            Controls.Add(label1);
            Controls.Add(dataGridView1);
            Controls.Add(button1);
            Name = "RXForm";
            Text = "Calls received by this station";
            Load += RXForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label RXFlabel;
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
        private System.Windows.Forms.ListBox OSDlistBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox DeepcheckBox;
        private System.Windows.Forms.CheckBox QuickcheckBox;
        private System.Windows.Forms.Label OSDlabel;
        private System.Windows.Forms.CheckBox uploadcheckBox;
        private System.Windows.Forms.Label Dlabel;
        private System.Windows.Forms.Label Ulabel;
        private System.Windows.Forms.CheckBox kmcheckBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DTotextBox;
        private System.Windows.Forms.TextBox callFiltertextBox;
        private System.Windows.Forms.TextBox DFromtextBox;
        private System.Windows.Forms.CheckBox datecheckBox;
        private System.Windows.Forms.Button filterbutton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox bandlistBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.CheckBox stopRXcheckBox;
        private System.Windows.Forms.CheckBox allowDeletecheckBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label statuslabel;
        private System.Windows.Forms.Label RXofflabel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label myloclabel;
        private System.Windows.Forms.Label Timelabel;
    }
}