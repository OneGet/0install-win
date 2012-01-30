﻿namespace ZeroInstall.Central.WinForms.SyncConfig
{
    partial class ResetCryptoKeyPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResetCryptoKeyPage));
            this.buttonReset = new System.Windows.Forms.Button();
            this.textBoxCryptoKey = new System.Windows.Forms.TextBox();
            this.labelCryptoKey = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.resetWorker = new System.ComponentModel.BackgroundWorker();
            this.labelInfo2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonReset
            // 
            resources.ApplyResources(this.buttonReset, "buttonReset");
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // textBoxCryptoKey
            // 
            resources.ApplyResources(this.textBoxCryptoKey, "textBoxCryptoKey");
            this.textBoxCryptoKey.Name = "textBoxCryptoKey";
            this.textBoxCryptoKey.TextChanged += new System.EventHandler(this.textBoxCryptoKey_TextChanged);
            // 
            // labelCryptoKey
            // 
            resources.ApplyResources(this.labelCryptoKey, "labelCryptoKey");
            this.labelCryptoKey.Name = "labelCryptoKey";
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.Name = "labelTitle";
            // 
            // resetWorker
            // 
            this.resetWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.resetWorker_DoWork);
            this.resetWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.resetWorker_RunWorkerCompleted);
            // 
            // labelInfo2
            // 
            resources.ApplyResources(this.labelInfo2, "labelInfo2");
            this.labelInfo2.Name = "labelInfo2";
            // 
            // ResetCryptoKeyPage
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.labelInfo2);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.textBoxCryptoKey);
            this.Controls.Add(this.labelCryptoKey);
            this.Controls.Add(this.labelTitle);
            this.Name = "ResetCryptoKeyPage";
            this.Controls.SetChildIndex(this.labelTitle, 0);
            this.Controls.SetChildIndex(this.labelCryptoKey, 0);
            this.Controls.SetChildIndex(this.textBoxCryptoKey, 0);
            this.Controls.SetChildIndex(this.buttonReset, 0);
            this.Controls.SetChildIndex(this.labelInfo2, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.TextBox textBoxCryptoKey;
        private System.Windows.Forms.Label labelCryptoKey;
        private System.Windows.Forms.Label labelTitle;
        private System.ComponentModel.BackgroundWorker resetWorker;
        private System.Windows.Forms.Label labelInfo2;
    }
}
