﻿namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class DownloadRetrievalMethodOnlinePage<T>
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelQuestion = new System.Windows.Forms.Label();
            this.textBoxUrl = new Common.Controls.UriTextBox();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 18);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(470, 37);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "{0} on the internet";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelQuestion
            // 
            this.labelQuestion.Location = new System.Drawing.Point(35, 82);
            this.labelQuestion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelQuestion.Name = "labelQuestion";
            this.labelQuestion.Size = new System.Drawing.Size(400, 22);
            this.labelQuestion.TabIndex = 1;
            this.labelQuestion.Text = "Where can the {0} be downloaded?";
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.AllowDrop = true;
            this.textBoxUrl.ForeColor = System.Drawing.Color.Red;
            this.textBoxUrl.HintText = "HTTP/FTP URL";
            this.textBoxUrl.Location = new System.Drawing.Point(39, 107);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(396, 26);
            this.textBoxUrl.TabIndex = 2;
            this.textBoxUrl.TextChanged += new System.EventHandler(this.textBoxUrl_TextChanged);
            // 
            // buttonDownload
            // 
            this.buttonDownload.Enabled = false;
            this.buttonDownload.Location = new System.Drawing.Point(39, 148);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(100, 33);
            this.buttonDownload.TabIndex = 3;
            this.buttonDownload.Text = "&Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // DownloadRetrievalMethodOnlinePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.textBoxUrl);
            this.Controls.Add(this.labelQuestion);
            this.Controls.Add(this.labelTitle);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DownloadRetrievalMethodOnlinePage";
            this.Size = new System.Drawing.Size(470, 300);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelQuestion;
        private Common.Controls.UriTextBox textBoxUrl;
        private System.Windows.Forms.Button buttonDownload;
    }
}