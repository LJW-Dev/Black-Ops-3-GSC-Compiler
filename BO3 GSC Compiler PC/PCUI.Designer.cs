namespace BO3_GSC_Compiler_PC
{
    partial class PCUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PCUI));
            this.CompileButton = new System.Windows.Forms.Button();
            this.OutputText = new System.Windows.Forms.TextBox();
            this.FolderCompile = new System.Windows.Forms.CheckBox();
            this.OutputFunctionHashes = new System.Windows.Forms.CheckBox();
            this.AutoInject = new System.Windows.Forms.CheckBox();
            this.CreditsButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CustomInject = new System.Windows.Forms.CheckBox();
            this.CustomInjectText = new System.Windows.Forms.TextBox();
            this.DebugCheckBox = new System.Windows.Forms.CheckBox();
            this.InjectScript = new System.Windows.Forms.Button();
            this.ScriptList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // CompileButton
            // 
            this.CompileButton.Location = new System.Drawing.Point(12, 12);
            this.CompileButton.Name = "CompileButton";
            this.CompileButton.Size = new System.Drawing.Size(130, 46);
            this.CompileButton.TabIndex = 0;
            this.CompileButton.Text = "Compile Script";
            this.CompileButton.UseVisualStyleBackColor = true;
            this.CompileButton.Click += new System.EventHandler(this.CompileButton_Click);
            // 
            // OutputText
            // 
            this.OutputText.Location = new System.Drawing.Point(12, 151);
            this.OutputText.Multiline = true;
            this.OutputText.Name = "OutputText";
            this.OutputText.Size = new System.Drawing.Size(630, 106);
            this.OutputText.TabIndex = 1;
            // 
            // FolderCompile
            // 
            this.FolderCompile.AutoSize = true;
            this.FolderCompile.Location = new System.Drawing.Point(13, 65);
            this.FolderCompile.Name = "FolderCompile";
            this.FolderCompile.Size = new System.Drawing.Size(95, 17);
            this.FolderCompile.TabIndex = 2;
            this.FolderCompile.Text = "Folder Compile";
            this.FolderCompile.UseVisualStyleBackColor = true;
            // 
            // OutputFunctionHashes
            // 
            this.OutputFunctionHashes.AutoSize = true;
            this.OutputFunctionHashes.Location = new System.Drawing.Point(13, 89);
            this.OutputFunctionHashes.Name = "OutputFunctionHashes";
            this.OutputFunctionHashes.Size = new System.Drawing.Size(141, 17);
            this.OutputFunctionHashes.TabIndex = 3;
            this.OutputFunctionHashes.Text = "Output Function Hashes";
            this.OutputFunctionHashes.UseVisualStyleBackColor = true;
            // 
            // AutoInject
            // 
            this.AutoInject.AutoSize = true;
            this.AutoInject.Checked = true;
            this.AutoInject.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoInject.Location = new System.Drawing.Point(12, 113);
            this.AutoInject.Name = "AutoInject";
            this.AutoInject.Size = new System.Drawing.Size(77, 17);
            this.AutoInject.TabIndex = 4;
            this.AutoInject.Text = "Auto Inject";
            this.AutoInject.UseVisualStyleBackColor = true;
            // 
            // CreditsButton
            // 
            this.CreditsButton.Location = new System.Drawing.Point(595, 263);
            this.CreditsButton.Name = "CreditsButton";
            this.CreditsButton.Size = new System.Drawing.Size(47, 23);
            this.CreditsButton.TabIndex = 5;
            this.CreditsButton.Text = "Credits";
            this.CreditsButton.UseVisualStyleBackColor = true;
            this.CreditsButton.Click += new System.EventHandler(this.CreditsButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 137);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Output";
            // 
            // CustomInject
            // 
            this.CustomInject.AutoSize = true;
            this.CustomInject.Location = new System.Drawing.Point(419, 12);
            this.CustomInject.Name = "CustomInject";
            this.CustomInject.Size = new System.Drawing.Size(151, 17);
            this.CustomInject.TabIndex = 8;
            this.CustomInject.Text = "Use Different GSC Header";
            this.CustomInject.UseVisualStyleBackColor = true;
            this.CustomInject.CheckedChanged += new System.EventHandler(this.CustomInject_CheckedChanged);
            // 
            // CustomInjectText
            // 
            this.CustomInjectText.Enabled = false;
            this.CustomInjectText.Location = new System.Drawing.Point(419, 35);
            this.CustomInjectText.Name = "CustomInjectText";
            this.CustomInjectText.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.CustomInjectText.Size = new System.Drawing.Size(223, 20);
            this.CustomInjectText.TabIndex = 9;
            // 
            // DebugCheckBox
            // 
            this.DebugCheckBox.AutoSize = true;
            this.DebugCheckBox.Location = new System.Drawing.Point(289, 43);
            this.DebugCheckBox.Name = "DebugCheckBox";
            this.DebugCheckBox.Size = new System.Drawing.Size(93, 17);
            this.DebugCheckBox.TabIndex = 10;
            this.DebugCheckBox.Text = "Debug Output";
            this.DebugCheckBox.UseVisualStyleBackColor = true;
            // 
            // InjectScript
            // 
            this.InjectScript.Location = new System.Drawing.Point(171, 39);
            this.InjectScript.Name = "InjectScript";
            this.InjectScript.Size = new System.Drawing.Size(112, 23);
            this.InjectScript.TabIndex = 11;
            this.InjectScript.Text = "Inject Script";
            this.InjectScript.UseVisualStyleBackColor = true;
            this.InjectScript.Click += new System.EventHandler(this.InjectScript_Click);
            // 
            // ScriptList
            // 
            this.ScriptList.FormattingEnabled = true;
            this.ScriptList.Location = new System.Drawing.Point(171, 12);
            this.ScriptList.Name = "ScriptList";
            this.ScriptList.Size = new System.Drawing.Size(211, 21);
            this.ScriptList.TabIndex = 12;
            // 
            // PCUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 298);
            this.Controls.Add(this.ScriptList);
            this.Controls.Add(this.InjectScript);
            this.Controls.Add(this.DebugCheckBox);
            this.Controls.Add(this.CustomInjectText);
            this.Controls.Add(this.CustomInject);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CreditsButton);
            this.Controls.Add(this.AutoInject);
            this.Controls.Add(this.OutputFunctionHashes);
            this.Controls.Add(this.FolderCompile);
            this.Controls.Add(this.OutputText);
            this.Controls.Add(this.CompileButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PCUI";
            this.Text = "BO3 GSC Compiler / Loader PC";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CompileButton;
        private System.Windows.Forms.TextBox OutputText;
        private System.Windows.Forms.CheckBox FolderCompile;
        private System.Windows.Forms.CheckBox OutputFunctionHashes;
        private System.Windows.Forms.CheckBox AutoInject;
        private System.Windows.Forms.Button CreditsButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox CustomInject;
        private System.Windows.Forms.TextBox CustomInjectText;
        private System.Windows.Forms.CheckBox DebugCheckBox;
        private System.Windows.Forms.Button InjectScript;
        private System.Windows.Forms.ComboBox ScriptList;
    }
}

