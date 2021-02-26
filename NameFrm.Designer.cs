namespace BakkesModInjectorCs
{
    partial class NameFrm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NameFrm));
            this.NameLbl = new System.Windows.Forms.Label();
            this.ConfirmBtn = new System.Windows.Forms.Label();
            this.CancelBtn = new System.Windows.Forms.Label();
            this.NameBox = new System.Windows.Forms.TextBox();
            this.ResetBtn = new System.Windows.Forms.Label();
            this.ConfirmBackground = new System.Windows.Forms.Panel();
            this.ResetBackground = new System.Windows.Forms.Panel();
            this.CancelBackground = new System.Windows.Forms.Panel();
            this.ConfirmBackground.SuspendLayout();
            this.ResetBackground.SuspendLayout();
            this.CancelBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // NameLbl
            // 
            this.NameLbl.BackColor = System.Drawing.Color.Transparent;
            this.NameLbl.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NameLbl.Location = new System.Drawing.Point(12, 12);
            this.NameLbl.Name = "NameLbl";
            this.NameLbl.Size = new System.Drawing.Size(356, 25);
            this.NameLbl.TabIndex = 60;
            this.NameLbl.Text = "New window name:";
            this.NameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ConfirmBtn
            // 
            this.ConfirmBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ConfirmBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ConfirmBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConfirmBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ConfirmBtn.Location = new System.Drawing.Point(1, 1);
            this.ConfirmBtn.Name = "ConfirmBtn";
            this.ConfirmBtn.Size = new System.Drawing.Size(113, 30);
            this.ConfirmBtn.TabIndex = 62;
            this.ConfirmBtn.Text = "Confirm";
            this.ConfirmBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ConfirmBtn.Click += new System.EventHandler(this.ConfirmBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.CancelBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CancelBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CancelBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CancelBtn.Location = new System.Drawing.Point(1, 1);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(113, 30);
            this.CancelBtn.TabIndex = 63;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // NameBox
            // 
            this.NameBox.BackColor = System.Drawing.Color.White;
            this.NameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NameBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NameBox.ForeColor = System.Drawing.Color.Black;
            this.NameBox.Location = new System.Drawing.Point(12, 42);
            this.NameBox.Name = "NameBox";
            this.NameBox.Size = new System.Drawing.Size(357, 25);
            this.NameBox.TabIndex = 70;
            // 
            // ResetBtn
            // 
            this.ResetBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ResetBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ResetBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResetBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ResetBtn.Location = new System.Drawing.Point(1, 1);
            this.ResetBtn.Name = "ResetBtn";
            this.ResetBtn.Size = new System.Drawing.Size(113, 30);
            this.ResetBtn.TabIndex = 71;
            this.ResetBtn.Text = "Reset to Default";
            this.ResetBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ResetBtn.Click += new System.EventHandler(this.ResetBtn_Click);
            // 
            // ConfirmBackground
            // 
            this.ConfirmBackground.BackColor = System.Drawing.Color.Gainsboro;
            this.ConfirmBackground.Controls.Add(this.ConfirmBtn);
            this.ConfirmBackground.Location = new System.Drawing.Point(12, 72);
            this.ConfirmBackground.Name = "ConfirmBackground";
            this.ConfirmBackground.Size = new System.Drawing.Size(115, 32);
            this.ConfirmBackground.TabIndex = 72;
            // 
            // ResetBackground
            // 
            this.ResetBackground.BackColor = System.Drawing.Color.Gainsboro;
            this.ResetBackground.Controls.Add(this.ResetBtn);
            this.ResetBackground.Location = new System.Drawing.Point(133, 72);
            this.ResetBackground.Name = "ResetBackground";
            this.ResetBackground.Size = new System.Drawing.Size(115, 32);
            this.ResetBackground.TabIndex = 73;
            // 
            // CancelBackground
            // 
            this.CancelBackground.BackColor = System.Drawing.Color.Gainsboro;
            this.CancelBackground.Controls.Add(this.CancelBtn);
            this.CancelBackground.Location = new System.Drawing.Point(254, 72);
            this.CancelBackground.Name = "CancelBackground";
            this.CancelBackground.Size = new System.Drawing.Size(115, 32);
            this.CancelBackground.TabIndex = 74;
            // 
            // NameFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(379, 116);
            this.Controls.Add(this.CancelBackground);
            this.Controls.Add(this.ResetBackground);
            this.Controls.Add(this.ConfirmBackground);
            this.Controls.Add(this.NameBox);
            this.Controls.Add(this.NameLbl);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NameFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rename Window";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NameFrm_FormClosing);
            this.Load += new System.EventHandler(this.NameFrm_Load);
            this.ConfirmBackground.ResumeLayout(false);
            this.ResetBackground.ResumeLayout(false);
            this.CancelBackground.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NameLbl;
        private System.Windows.Forms.Label ConfirmBtn;
        private System.Windows.Forms.Label CancelBtn;
        private System.Windows.Forms.TextBox NameBox;
        private System.Windows.Forms.Label ResetBtn;
        private System.Windows.Forms.Panel ConfirmBackground;
        private System.Windows.Forms.Panel ResetBackground;
        private System.Windows.Forms.Panel CancelBackground;
    }
}