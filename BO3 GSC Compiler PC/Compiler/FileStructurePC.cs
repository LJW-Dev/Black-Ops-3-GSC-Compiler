using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCCompilerPC
{
    public class GSCHeader
    {
        public byte[] Magic
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0, value.ToList());
            }
        }

        public uint SourceCRC
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x8, BitConverter.GetBytes(value).ToList());
            }
        }

        public int IncludesPtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0xC, BitConverter.GetBytes(value).ToList());
            }
        }

        public int AnimTreePtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x10, BitConverter.GetBytes(value).ToList());
            }
        }

        public int CodeSectionStart
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x14, BitConverter.GetBytes(value).ToList());
            }
        }

        public int StringTablePtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x18, BitConverter.GetBytes(value).ToList());
            }
        }

        public int DevStringTablePtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x1C, BitConverter.GetBytes(value).ToList());
            }
        }

        public int ExportsPtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x20, BitConverter.GetBytes(value).ToList());
            }
        }

        public int ImportsPtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x24, BitConverter.GetBytes(value).ToList());
            }
        }

        public int FixupPtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x28, BitConverter.GetBytes(value).ToList());
            }
        }

        public int ProfilePtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x2C, BitConverter.GetBytes(value).ToList());
            }
        }

        public int CodeSize
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x30, BitConverter.GetBytes(value).ToList());
            }
        }

        public int NamePtr
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x34, BitConverter.GetBytes(value).ToList());
            }
        }

        public ushort StringTableCount
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x38, BitConverter.GetBytes(value).ToList());
            }
        }

        public ushort ExportsCount
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x3A, BitConverter.GetBytes(value).ToList());
            }
        }

        public ushort ImportsCount
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x3C, BitConverter.GetBytes(value).ToList());
            }
        }

        public ushort FixupCount
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x3E, BitConverter.GetBytes(value).ToList());
            }
        }

        public ushort ProfileCount
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x40, BitConverter.GetBytes(value).ToList());
            }
        }

        public ushort DevStringTableCount
        {
            set
            {
                ScriptCompiler.CompiledPub.Replace(0x42, BitConverter.GetBytes(value).ToList());
            }
        }

        public byte IncludesCount
        {
            set
            {
                ScriptCompiler.CompiledPub[0x44] = value;
            }
        }

        public byte AnimTreeCount
        {
            set 
            { 
                ScriptCompiler.CompiledPub[0x45] = value; 
            }
        }

        public short Flags
        {
            set 
            {
                ScriptCompiler.CompiledPub.Replace(0x46, BitConverter.GetBytes(value).ToList());
            }
        }
    }

    public enum FunctionFlags : byte //FunctionFlags can be ORd together with CallFlags
    {
        GetFunction = 0x01,
        FunctionCall = 0x02,
        FunctionThreadCall = 0x03,
        MethodCall = 0x04,
        MethodThreadCall = 0x05,
    }

    public enum CallFlags : byte
    {
        ExternalCall = 0x00,        //calls to external GSC file functions
        ExternalDevCall = 0x10,     //calls to external GSC file dev functions
        LocalCall = 0x20,           //can be either builtins or local calls 
        LocalDevCall = 0x30         //can be either dev builtins or local dev calls
    }

    public enum ExportFlags : byte
    {
        None = 0x00,
        AutoExec = 0x02,
        Private = 0x04
    }

    public class Import
    {
        public uint FunctionNameHash { get; set; }
        public uint FileNameHash { get; set; }
        public short NumOfRefs { get; set; }
        public byte NumOfParams { get; set; }
        public byte Flags { get; set; }
        public List<int> Refs { get; set; }
    }

    public class Export
    {
        public uint CRC32 { get; set; }
        public int Start { get; set; }
        public uint FunctionNameHash { get; set; }
        public uint FileNameHash { get; set; }
        public byte NumOfParams { get; set; }
        public ExportFlags Flags { get; set; }
        public short Unknown { get; set; } //seems to always be 0, linker skips it
    }

    public class StringTable
    {
        public int StringPtr { get; set; }
        public int NumOfRefs { get; set; }
        public List<int> Refs { get; set; }
    }

    public class AnimTree
    {
        public int AnimTreeNamePtr { get; set; }
        public short NumOfAnimTreeRefs { get; set; }
        public short NumOfAnimRefs { get; set; }
        public List<int> AnimTreeRefs { get; set; } //used in "UseAnimTree(#animtree)"
        public List<AnimTreePtr> AnimRefs { get; set; } //used in the %anim of "AnimScripted("anim", origin, angles, %anim);"
    }

    public class AnimTreePtr
    {
        public long AnimTreeNamePtr { get; set; } //animation name
        public long AnimTreeRef { get; set; } //pointer to opcode value
    }
}