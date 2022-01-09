using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Irony.Parsing;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace BO3_GSC_Compiler_PC
{
    public partial class PCUI : Form
    {
        public PCUI()
        {
            InitializeComponent();
            InitScriptList();
        }

        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;

        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")] //for byte pointers not arrays
        unsafe public static extern bool ReadProcessMemory(int hProcess, long lpBaseAddress, byte* lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static unsafe extern bool WriteProcessMemory(int hProcess, long lpBaseAddress, byte** lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        unsafe public struct ScriptParseTree
        {
            public char* name;
            public int len;
            public int unk;
            public char* buffer;
        };
        unsafe public struct XAssetEntry
        {
            public int type;
            public int unk1;
            public long* HeaderPointer;
            public byte zoneindex;
            public byte inuse;
            public short unk2;
            public int nextHash;
            public int nextOverride;
            public int unk3;
        };

        IntPtr processBAddr = (IntPtr)0;
        IntPtr processHandle = (IntPtr)0;

        string DirName = Path.GetDirectoryName(Application.ExecutablePath);

        IntPtr memIntPtr;

        void InitScriptList(string setValue = "")
        {
            string[] ScriptFiles = Directory.GetFiles(DirName + "\\CompiledScripts\\");
            foreach (string Script in ScriptFiles)
            {
                string name = Path.GetFileName(Script);
                if (!ScriptList.Items.Contains(name) && name.EndsWith(".gsc"))
                    ScriptList.Items.Add(name);
            }
            if (setValue != "")
                ScriptList.SelectedItem = setValue;
            else if (ScriptList.Items.Count != 0)
                ScriptList.SelectedItem = ScriptList.Items[0];
        }

        //ReadBytes, ReadArrayUnsafe and ReadNullTerminatedString were all taken from HydraX
        byte[] ReadBytes(long address, int numBytes)
        {
            // Resulting buffer
            byte[] buffer = new byte[numBytes];
            // Request ReadProcessMemory
            ReadProcessMemory((int)processHandle, address, buffer, buffer.Length, out int bytesRead);
            // Return result
            return buffer;
        }

        unsafe T[] ReadArrayUnsafe<T>(long address, int count) where T : unmanaged
        {
            var buffer = ReadBytes(address, count * sizeof(T));
            var result = new T[count];

            fixed (byte* a = buffer)
            fixed (T* b = result)
                Buffer.MemoryCopy(a, b, buffer.Length, buffer.Length);

            return result;
        }

        unsafe string ReadNullTerminatedString(long address, int bufferSize = 512)
        {
            var result = stackalloc byte[bufferSize];
            ReadProcessMemory((int)processHandle, address, result, bufferSize, out int bytesRead);
            int sizeOf;
            for (sizeOf = 0; sizeOf < bufferSize; sizeOf++)
            {
                if (result[sizeOf] == 0x0)
                    break;
            }
            return Encoding.ASCII.GetString(result, sizeOf);
        }

        bool UpdateProcessInfo()
        {
            try
            {
                Process process;
                process = Process.GetProcessesByName("BlackOps3")[0];
                processBAddr = process.MainModule.BaseAddress;
                processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, true, process.Id);
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                // The process isn't currently running
                return false;
            }
        }

        private void CompileButton_Click(object sender, EventArgs e)
        {
            OutputText.Text = "Compiling Script..." + System.Environment.NewLine;
            string GSCPath;

            if (FolderCompile.Checked)
            {
                FolderBrowserDialog folderDlg = new FolderBrowserDialog();
                folderDlg.ShowNewFolderButton = true;

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

            var gameScript = new GSCCompilerPC.GSCGrammar();
            var parser = new Parser(gameScript);

            var Compiler = new GSCCompilerPC.ScriptCompiler();

            string Script = File.ReadAllText(GSCPath).Replace("]]", "] ]").Replace("] ](", "]]("); //fixes array[array2[var]] syntax bug and fixes pointers that broke from the syntax fix

            var Tree = parser.Parse(Script);

            bool DidScriptCompile = Compiler.Init(Tree, OutputFunctionHashes.Checked); //compile script

            OutputText.AppendText(Compiler.OutputString);
            if (DidScriptCompile)
            {
                string GSCName = Path.GetFileNameWithoutExtension(GSCPath) + ".gsc";

                File.WriteAllBytes(DirName + "\\CompiledScripts\\" + GSCName, GSCCompilerPC.ScriptCompiler.CompiledPub.ToArray());
                InitScriptList(GSCName);
                if (AutoInject.Checked)
                    InjectScript_Click(null, null);
            }
            else
                OutputText.Text += "Script compile failed!";
        }

        private unsafe void InjectScript_Click(object sender, EventArgs e)
        {
            if (!AutoInject.Checked)
                OutputText.Text = "";

            if (!UpdateProcessInfo())
            {
                OutputText.Text += "BO3 is not running!" + System.Environment.NewLine;
                return;
            }

            if ((string)ScriptList.SelectedItem == "")
            {
                OutputText.Text += "Select a script to inject first!" + System.Environment.NewLine;
                return;
            }

            bool DebugOutput = DebugCheckBox.Checked;

            long AssetPoolOffset = ((long)processBAddr + (0x7FF6BD4CaB80 - 0x7FF6B4000000)) - 0x1000;
            if (DebugOutput)
            {
                OutputText.Text += "BO3 base address offset: " + (long)processBAddr + System.Environment.NewLine;
                OutputText.Text += "Assetpool offset: " + AssetPoolOffset + System.Environment.NewLine;
            }

            var AssetPools = ReadArrayUnsafe<XAssetEntry>(AssetPoolOffset, 156672);
            long headerPointer = 0;
            foreach (var Asset in AssetPools)
            {
                if (Asset.type == 0x36 && (long)Asset.HeaderPointer != 0) //make sure asset is scriptparsetree type (0x36) and is not empty
                {
                    if (ReadNullTerminatedString(BitConverter.ToInt64(ReadBytes((long)Asset.HeaderPointer, 8), 0)).Equals(CustomInject.Checked ? CustomInjectText.Text : "scripts/shared/duplicaterender_mgr.gsc"))
                    {
                        headerPointer = (long)Asset.HeaderPointer; //pointer to the header
                        if (DebugOutput)
                            OutputText.Text += "Overwritten header name: " + ReadNullTerminatedString(BitConverter.ToInt64(ReadBytes((long)Asset.HeaderPointer, 8), 0)) + System.Environment.NewLine;
                        break;

                    }
                }
            }
            if (headerPointer == 0)
            {
                OutputText.Text += "Unable to find Header to Inject! Make sure you are in the main menu." + System.Environment.NewLine;
                return;
            }

            long assetbufferB = BitConverter.ToInt64(ReadBytes(headerPointer + 0x10, 8), 0);

            if (DebugOutput)
            {
                OutputText.Text += "Asset header offset: " + headerPointer + System.Environment.NewLine;
                OutputText.Text += "Asset buffer before writing: " + assetbufferB + System.Environment.NewLine;
            }

            if (!File.Exists(DirName + "\\CompiledScripts\\" + ScriptList.SelectedItem))
            {
                OutputText.Text += "Cannot find " + ScriptList.SelectedItem + "!" + System.Environment.NewLine;
                return;
            }

            byte[] InjectedScript = File.ReadAllBytes(DirName + "\\CompiledScripts\\" + ScriptList.SelectedItem);
            if (InjectedScript[4] != 0x0D && InjectedScript[5] != 0x0A)
            {
                OutputText.Text += "This is not a compiled GSC file! Double check that you aren't trying to inject source code.";
                return;
            }

            memIntPtr = Marshal.AllocHGlobal(InjectedScript.Length);
            Marshal.Copy(InjectedScript, 0, memIntPtr, InjectedScript.Length);



            IntPtr allocMemAddress = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)InjectedScript.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            WriteProcessMemory((int)processHandle, (long)allocMemAddress, (byte**)memIntPtr, InjectedScript.Length, out int bytesRead);

            byte* buffer = (byte*)allocMemAddress; //we are writing a pointer not the pointer's value so need to do this

            WriteProcessMemory((int)processHandle, headerPointer + 0x10, &buffer, 8, out bytesRead);

            long assetbufferA = BitConverter.ToInt64(ReadBytes(headerPointer + 0x10, 8), 0);

            if (DebugOutput)
                OutputText.Text += "Asset buffer after writing: " + assetbufferA + System.Environment.NewLine;

            if (assetbufferA != assetbufferB) //Check that the buffer pointer has been changed
                OutputText.Text += "Script injected successfully!" + System.Environment.NewLine;
            else
                OutputText.Text += "Unable to write to BO3's memory!" + System.Environment.NewLine;
        }

        private void CreditsButton_Click(object sender, EventArgs e)
        {
            PCCredits Credits = new PCCredits();
            Credits.Show();
        }

        private void CustomInject_CheckedChanged(object sender, EventArgs e)
        {
            if (CustomInject.Checked)
                CustomInjectText.Enabled = true;
            else
                CustomInjectText.Enabled = false;
        }
    }
}