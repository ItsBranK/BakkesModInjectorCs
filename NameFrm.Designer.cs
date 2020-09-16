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
            this.nameLbl = new System.Windows.Forms.Label();
            this.confirmBtn = new System.Windows.Forms.Label();
            this.cancelBtn = new System.Windows.Forms.Label();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.defaultBtn = new System.Windows.Forms.Label();
            this.confirmBackground = new System.Windows.Forms.Panel();
            this.defaultBackground = new System.Windows.Forms.Panel();
            this.cancelBackground = new System.Windows.Forms.Panel();
            this.confirmBackground.SuspendLayout();
            this.defaultBackground.SuspendLayout();
            this.cancelBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // nameLbl
            // 
            this.nameLbl.BackColor = System.Drawing.Color.Transparent;
            this.nameLbl.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLbl.Location = new System.Drawing.Point(12, 12);
            this.nameLbl.Name = "nameLbl";
            this.nameLbl.Size = new System.Drawing.Size(356, 25);
            this.nameLbl.TabIndex = 60;
            this.nameLbl.Text = "New window name:";
            this.nameLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // confirmBtn
            // 
            this.confirmBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.confirmBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.confirmBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.confirmBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.confirmBtn.Location = new System.Drawing.Point(1, 1);
            this.confirmBtn.Name = "confirmBtn";
            this.confirmBtn.Size = new System.Drawing.Size(113, 30);
            this.confirmBtn.TabIndex = 62;
            this.confirmBtn.Text = "Confirm";
            this.confirmBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.confirmBtn.Click += new System.EventHandler(this.confirmBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.cancelBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cancelBtn.Location = new System.Drawing.Point(1, 1);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(113, 30);
            this.cancelBtn.TabIndex = 63;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // nameBox
            // 
            this.nameBox.BackColor = System.Drawing.Color.White;
            this.nameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nameBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameBox.ForeColor = System.Drawing.Color.Black;
            this.nameBox.Location = new System.Drawing.Point(12, 42);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(357, 25);
            this.nameBox.TabIndex = 70;
            // 
            // defaultBtn
            // 
            this.defaultBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.defaultBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.defaultBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.defaultBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.defaultBtn.Location = new System.Drawing.Point(1, 1);
            this.defaultBtn.Name = "defaultBtn";
            this.defaultBtn.Size = new System.Drawing.Size(113, 30);
            this.defaultBtn.TabIndex = 71;
            this.defaultBtn.Text = "Reset to Default";
            this.defaultBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.defaultBtn.Click += new System.EventHandler(this.defaultBtn_Click);
            // 
            // confirmBackground
            // 
            this.confirmBackground.BackColor = System.Drawing.Color.Gainsboro;
            this.confirmBackground.Controls.Add(this.confirmBtn);
            this.confirmBackground.Location = new System.Drawing.Point(12, 72);
            this.confirmBackground.Name = "confirmBackground";
            this.confirmBackground.Size = new System.Drawing.Size(115, 32);
            this.confirmBackground.TabIndex = 72;
            // 
            // defaultBackground
            // 
            this.defaultBackground.BackColor = System.Drawing.Color.Gainsboro;
            this.defaultBackground.Controls.Add(this.defaultBtn);
            this.defaultBackground.Location = new System.Drawing.Point(133, 72);
            this.defaultBackground.Name = "defaultBackground";
            this.defaultBackground.Size = new System.Drawing.Size(115, 32);
            this.defaultBackground.TabIndex = 73;
            // 
            // cancelBackground
            // 
            this.cancelBackground.BackColor = System.Drawing.Color.Gainsboro;
            this.cancelBackground.Controls.Add(this.cancelBtn);
            this.cancelBackground.Location = new System.Drawing.Point(254, 72);
            this.cancelBackground.Name = "cancelBackground";
            this.cancelBackground.Size = new System.Drawing.Size(115, 32);
            this.cancelBackground.TabIndex = 74;
            // 
            // NameFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(379, 116);
            this.Controls.Add(this.cancelBackground);
            this.Controls.Add(this.defaultBackground);
            this.Controls.Add(this.confirmBackground);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.nameLbl);
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
            this.confirmBackground.ResumeLayout(false);
            this.defaultBackground.ResumeLayout(false);
            this.cancelBackground.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nameLbl;
        private System.Windows.Forms.Label confirmBtn;
        private System.Windows.Forms.Label cancelBtn;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.Label defaultBtn;
        private System.Windows.Forms.Panel confirmBackground;
        private System.Windows.Forms.Panel defaultBackground;
        private System.Windows.Forms.Panel cancelBackground;
    }
}