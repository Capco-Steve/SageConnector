namespace SageConnector
{
    partial class Main
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
			this.txtResults = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.btnLoadHistoricalInvoices = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.btnContinuousSync = new System.Windows.Forms.Button();
			this.btnQuickSync = new System.Windows.Forms.Button();
			this.btnFullSync = new System.Windows.Forms.Button();
			this.chkHttpLogging = new System.Windows.Forms.CheckBox();
			this.SyncTimer = new System.Windows.Forms.Timer(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtResults
			// 
			this.txtResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.txtResults, 6);
			this.txtResults.Location = new System.Drawing.Point(3, 3);
			this.txtResults.Multiline = true;
			this.txtResults.Name = "txtResults";
			this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtResults.Size = new System.Drawing.Size(636, 314);
			this.txtResults.TabIndex = 1;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 6;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.67052F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.27168F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0578F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 113F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 91F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
			this.tableLayoutPanel1.Controls.Add(this.txtResults, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.btnLoadHistoricalInvoices, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnStop, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnContinuousSync, 3, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnQuickSync, 4, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnFullSync, 5, 2);
			this.tableLayoutPanel1.Controls.Add(this.chkHttpLogging, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(642, 380);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// btnLoadHistoricalInvoices
			// 
			this.btnLoadHistoricalInvoices.Location = new System.Drawing.Point(3, 353);
			this.btnLoadHistoricalInvoices.Name = "btnLoadHistoricalInvoices";
			this.btnLoadHistoricalInvoices.Size = new System.Drawing.Size(155, 23);
			this.btnLoadHistoricalInvoices.TabIndex = 3;
			this.btnLoadHistoricalInvoices.Text = "Load Historical Invoices...";
			this.btnLoadHistoricalInvoices.UseVisualStyleBackColor = true;
			this.btnLoadHistoricalInvoices.Click += new System.EventHandler(this.btnLoadHistoricalInvoices_Click);
			// 
			// btnStop
			// 
			this.btnStop.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnStop.Location = new System.Drawing.Point(262, 353);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 7;
			this.btnStop.Text = "Stop";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnContinuousSync
			// 
			this.btnContinuousSync.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnContinuousSync.Location = new System.Drawing.Point(346, 353);
			this.btnContinuousSync.Name = "btnContinuousSync";
			this.btnContinuousSync.Size = new System.Drawing.Size(104, 23);
			this.btnContinuousSync.TabIndex = 6;
			this.btnContinuousSync.Text = "Continuous Sync";
			this.btnContinuousSync.UseVisualStyleBackColor = true;
			this.btnContinuousSync.Click += new System.EventHandler(this.btnContinuousSync_Click);
			// 
			// btnQuickSync
			// 
			this.btnQuickSync.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnQuickSync.Location = new System.Drawing.Point(466, 353);
			this.btnQuickSync.Name = "btnQuickSync";
			this.btnQuickSync.Size = new System.Drawing.Size(75, 23);
			this.btnQuickSync.TabIndex = 5;
			this.btnQuickSync.Text = "Quick Sync";
			this.btnQuickSync.UseVisualStyleBackColor = true;
			this.btnQuickSync.Click += new System.EventHandler(this.btnQuickSync_Click);
			// 
			// btnFullSync
			// 
			this.btnFullSync.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnFullSync.Location = new System.Drawing.Point(564, 353);
			this.btnFullSync.Name = "btnFullSync";
			this.btnFullSync.Size = new System.Drawing.Size(75, 23);
			this.btnFullSync.TabIndex = 4;
			this.btnFullSync.Text = "Full Sync";
			this.btnFullSync.UseVisualStyleBackColor = true;
			this.btnFullSync.Click += new System.EventHandler(this.btnFullSync_Click);
			// 
			// chkHttpLogging
			// 
			this.chkHttpLogging.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.chkHttpLogging.AutoSize = true;
			this.chkHttpLogging.Checked = global::SageConnector.Properties.Settings.Default.EnableHTTPLogging;
			this.chkHttpLogging.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::SageConnector.Properties.Settings.Default, "EnableHTTPLogging", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.chkHttpLogging.Location = new System.Drawing.Point(3, 326);
			this.chkHttpLogging.Name = "chkHttpLogging";
			this.chkHttpLogging.Size = new System.Drawing.Size(132, 17);
			this.chkHttpLogging.TabIndex = 8;
			this.chkHttpLogging.Text = "Enable HTTP Logging";
			this.chkHttpLogging.UseVisualStyleBackColor = true;
			// 
			// SyncTimer
			// 
			this.SyncTimer.Tick += new System.EventHandler(this.OnSyncTimer_Tick);
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(642, 380);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Main";
			this.Text = "Sage 200 Connector";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion
		private System.Windows.Forms.TextBox txtResults;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button btnLoadHistoricalInvoices;
		private System.Windows.Forms.Button btnFullSync;
		private System.Windows.Forms.Button btnQuickSync;
		private System.Windows.Forms.Button btnContinuousSync;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Timer SyncTimer;
		private System.Windows.Forms.CheckBox chkHttpLogging;
	}
}

