namespace BO3_GSC_Compiler_XBOX
{
    partial class XBOXUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XBOXUI));
            this.CompileButton = new System.Windows.Forms.Button();
            this.OutputFunctionHashes = new System.Windows.Forms.CheckBox();
            this.FolderCompile = new System.Windows.Forms.CheckBox();
            this.HelpButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CreditsButton = new System.Windows.Forms.Button();
            this.OutputText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // CompileButton
            // 
            this.CompileButton.Location = new System.Drawing.Point(12, 12);
            this.CompileButton.Name = "CompileButton";
            this.CompileButton.Size = new System.Drawing.Size(130, 40);
            this.CompileButton.TabIndex = 0;
            this.CompileButton.Text = "Compile Script";
            this.CompileButton.UseVisualStyleBackColor = true;
            this.CompileButton.Click += new System.EventHandler(this.CompileScript_Click);
            // 
            // OutputFunctionHashes
            // 
            this.OutputFunctionHashes.AutoSize = true;
            this.OutputFunctionHashes.Location = new System.Drawing.Point(148, 35);
            this.OutputFunctionHashes.Name = "OutputFunctionHashes";
            this.OutputFunctionHashes.Size = new System.Drawing.Size(141, 17);
            this.OutputFunctionHashes.TabIndex = 5;
            this.OutputFunctionHashes.Text = "Output Function Hashes";
            this.OutputFunctionHashes.UseVisualStyleBackColor = true;
            // 
            // FolderCompile
            // 
            this.FolderCompile.AutoSize = true;
            this.FolderCompile.Location = new System.Drawing.Point(148, 12);
            this.FolderCompile.Name = "FolderCompile";
            this.FolderCompile.Size = new System.Drawing.Size(95, 17);
            this.FolderCompile.TabIndex = 4;
            this.FolderCompile.Text = "Folder Compile";
            this.FolderCompile.UseVisualStyleBackColor = true;
            // 
            // HelpButton
            // 
            this.HelpButton.Location = new System.Drawing.Point(509, 182);
            this.HelpButton.Name = "HelpButton";
            this.HelpButton.Size = new System.Drawing.Size(79, 23);
            this.HelpButton.TabIndex = 17;
            this.HelpButton.Text = "How to Inject";
            this.HelpButton.UseVisualStyleBackColor = true;
            this.HelpButton.Click += new System.EventHandler(this.HelpButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Output";
            // 
            // CreditsButton
            // 
            this.CreditsButton.Location = new System.Drawing.Point(594, 182);
            this.CreditsButton.Name = "CreditsButton";
            this.CreditsButton.Size = new System.Drawing.Size(47, 23);
            this.CreditsButton.TabIndex = 15;
            this.CreditsButton.Text = "Credits";
            this.CreditsButton.UseVisualStyleBackColor = true;
            this.CreditsButton.Click += new System.EventHandler(this.CreditsButton_Click);
            // 
            // OutputText
            // 
            this.OutputText.Location = new System.Drawing.Point(11, 70);
            this.OutputText.Multiline = true;
            this.OutputText.Name = "OutputText";
            this.OutputText.Size = new System.Drawing.Size(630, 106);
            this.OutputText.TabIndex = 14;
            // 
            // XBOXUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 215);
            this.Controls.Add(this.HelpButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CreditsButton);
            this.Controls.Add(this.OutputText);
            this.Controls.Add(this.OutputFunctionHashes);
            this.Controls.Add(this.FolderCompile);
            this.Controls.Add(this.CompileButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "XBOXUI";
            this.Text = "BO3 GSC Compiler XBOX";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CompileButton;
        private System.Windows.Forms.CheckBox OutputFunctionHashes;
        private System.Windows.Forms.CheckBox FolderCompile;
        private System.Windows.Forms.Button HelpButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CreditsButton;
        private System.Windows.Forms.TextBox OutputText;
    }
}

