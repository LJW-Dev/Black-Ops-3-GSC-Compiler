using System;
using System.Windows.Forms;
using Irony.Parsing;
using System.IO;

namespace BO3_GSC_Compiler_XBOX
{
    public partial class XBOXUI : Form
    {
        public XBOXUI()
        {
            InitializeComponent();
        }

        string DirName = Path.GetDirectoryName(Application.ExecutablePath);

        private void CompileScript_Click(object sender, EventArgs e)
        {
            OutputText.Text = "Compiling Script..." + System.Environment.NewLine;
            string GSCPath;

            if (FolderCompile.Checked)
            {
                FolderBrowserDialog folderDlg = new FolderBrowserDialog
                {
                    ShowNewFolderButton = true
                };

                DialogResult result = folderDlg.ShowDialog();
                if (result != DialogResult.OK)
                {
                    OutputText.Text = "No Folder Chosen!" + System.Environment.NewLine;
                    return;
                }

                string[] files = Directory.GetFiles(folderDlg.SelectedPath, "*.gsc*", SearchOption.AllDirectories);

                string newgscname = DirName + "\\CompiledScripts\\" + Path.GetFileName(folderDlg.SelectedPath) + ".txt";

                bool foundFile = false;
                foreach (string file in files)
                {
                    if (file.ToString().Contains(@"\main.gsc"))
                    {
                        File.WriteAllText(newgscname, File.ReadAllText(file));
                        foundFile = true;
                        break;
                    }
                }
                if (!foundFile)
                {
                    OutputText.Text = "Cannot find main.gsc!" + System.Environment.NewLine;
                    return;
                }

                foreach (string file in files)
                {
                    if (!file.ToString().Contains(@"\main.gsc"))
                    {
                        File.AppendAllText(newgscname, File.ReadAllText(file));
                    }
                }

                GSCPath = newgscname;
            }
            else
            {
                var file = new OpenFileDialog { Filter = "Source code|*.txt;*.gsc" };
                if (file.ShowDialog() != DialogResult.OK)
                {
                    OutputText.Text = "No File Chosen!" + System.Environment.NewLine;
                    return;
                }

                GSCPath = file.FileName;
            }

            byte[] script = File.ReadAllBytes(GSCPath);

            if (script[4] == 0x0D && script[5] == 0x0A)
            {
                OutputText.Text += "This file is already compiled! Place the file in the CompiledScripts folder to be able to inject it.";
                return;
            }

            var gameScript = new GSCCompilerXBOX.GSCGrammar();
            var parser = new Parser(gameScript);

            var Compiler = new GSCCompilerXBOX.ScriptCompiler();
            var Tree = parser.Parse(File.ReadAllText(GSCPath).Replace("]]", "] ]").Replace("] ](", "]](")); //fixes array[array2[var]] syntax bug and fixes pointers that broke from the syntax fix

            bool DidScriptCompile = Compiler.Init(Tree, OutputFunctionHashes.Checked); //compile script

            OutputText.AppendText(Compiler.OutputString);
            if (DidScriptCompile)
            {
                string GSCName = Path.GetFileNameWithoutExtension(GSCPath) + ".gsc";

                File.WriteAllBytes(DirName + "\\CompiledScripts\\" + GSCName, GSCCompilerXBOX.ScriptCompiler.CompiledPub.ToArray());

                if (OutputFunctionHashes.Checked)
                {
                    File.WriteAllText(DirName + "\\CompiledScripts\\FunctionHashes.txt", Compiler.OutputFuncts.ToString());
                }
            }
            else
            {
                if(FolderCompile.Checked)
                    OutputText.Text += "Script compile failed! To find the line with the error, open the .txt in the CompiledScripts directory." + System.Environment.NewLine;
                else
                    OutputText.Text += "Script compile failed!" + System.Environment.NewLine;
            }
                
        }

        private void CreditsButton_Click(object sender, EventArgs e)
        {
            XBOXCredits Credits = new XBOXCredits();
            Credits.Show();
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/LJW-Dev/Black-Ops-3-GSC-Injector-for-Xbox-360");
        }
    }
}
