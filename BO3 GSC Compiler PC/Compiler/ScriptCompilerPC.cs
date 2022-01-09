using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace GSCCompilerPC
{
    public partial class ScriptCompiler
    {
        public uint GSCNameSpaceHash;
        public static List<byte> CompiledPub; //compiledpub is a list of bytes that make up the compiled gsc

        public static List<Import> Imports;
        public static List<Export> Exports;
        private static List<AnimTree> AnimTrees;
        public static List<string> Includes;
        public static Dictionary<string, int> StringPositionDict; //dictionary of all strings and their offsets
        private static Dictionary<string, StringTable> StringTableDict; //dictionary of all the StringTable structs

        public static Dictionary<string, ParseTreeNode> DefinedVariables;
        public static List<string> LocalVariables;

        public static List<Key> ArrayKeys;
        public static List<string> SwitchKeys;
        public static byte numofForeachStatements;
        public static byte numofSwitchStatements;
        public static byte NumOfParams;

        private static List<List<int>> BreakList; //first list is a loop count and second list is the break pos
        private static List<List<int>> ContinueList;

        private static List<List<Operators>> OperatorList;
        private static List<List<string>> PrevOperatorList;

        private static List<List<int>> BoolExprList;

        public string OutputString;
        public StringBuilder OutputFuncts;
        bool DoOutputFuncts;
        bool ErrorInScript;

        Random RandomNum = new Random();

        private void SetAlignedWord(byte offset = 0)
        {
            var alignedPos = (int) (CompiledPub.Count + 1 + offset & 0xFFFFFFFE);
            while (CompiledPub.Count < alignedPos)
            {
                CompiledPub.Add(0);
            }
        }

        private void SetAlignedDword(byte offset = 0)
        {
            var alignedPos = (int) (CompiledPub.Count + 3 + offset & 0xFFFFFFFC);
            while (CompiledPub.Count < alignedPos)
            {
                CompiledPub.Add(0);
            }
        }

        private void SetAlignedQWord(byte offset = 0)
        {
            var alignedPos = (int)(CompiledPub.Count + 8 + offset & 0xFFFFFFF8);
            while (CompiledPub.Count < alignedPos)
            {
                CompiledPub.Add(0);
            }
        }

        private void GetFunctionAllign()
        {
            var alignedPos = (int)(CompiledPub.Count + 7 & 0xFFFFFFF8);
            while (CompiledPub.Count < alignedPos)
            {
                CompiledPub.Add(0);
            }
        }

        static uint CanonicalHash(string str) //reversed sl_getcanonicalstring from exe
        {
            str = (str + "\0").ToLower();
            Int64 hash = (str[0] ^ 0x4B9ACE2F) * 0x1000193;
            for (int z = 1; z < str.Length; z++)
                hash = (hash ^ char.ToLower(str[z])) * 0x1000193;
            return (uint)(hash & 0x00000000FFFFFFFF); //remove bytes past the uint32 limit and convert to uint
        }

        private void AddString(string str)
        {
            StringPositionDict.Add(str, CompiledPub.Count);
            CompiledPub.AddRange(Encoding.ASCII.GetBytes(str + '\0')); //add string to GSC with a null terminator
            
        }

        private void AddRefToCall(uint FunctionNameHash, byte NumOfParams, byte Flags, uint FileNameHash, string namestring) //add to imports
        {
            foreach (Import Import in Imports.Where(call => call.FunctionNameHash == FunctionNameHash && call.FileNameHash == FileNameHash && call.NumOfParams == NumOfParams && call.Flags == Flags)) //check if the function is already referenced
            {
                Import.Refs.Add((CompiledPub.Count - 2)); //-2 as imports reference the opcode start

                Import.NumOfRefs++;
                return;
            }

            Imports.Add(new Import
            {
                FunctionNameHash = FunctionNameHash,
                FileNameHash = FileNameHash, 
                NumOfRefs = 1, 
                NumOfParams = NumOfParams,
                Flags = Flags, 
                Refs = new List<int> { CompiledPub.Count - 2 }
            });

            if (DoOutputFuncts)
            {
                OutputFuncts.Append("0x" + FunctionNameHash.ToString("x"));
                OutputFuncts.Append("," + namestring);
                OutputFuncts.AppendLine();
            }
        }


        private void AddRefToString(string str)
        {
            if(StringTableDict.ContainsKey(str))
            {
                StringTableDict[str].Refs.Add(CompiledPub.Count);
                StringTableDict[str].NumOfRefs++;
                return;
            }
            StringTableDict.Add(str, new StringTable
            {
                StringPtr = GetStringPosByName(str),
                NumOfRefs = 1,
                Refs = new List<int> { CompiledPub.Count },

            });
        }

        private void AddLong(long i)
        {
            CompiledPub.AddRange(BitConverter.GetBytes(i).ToArray());
        }

        private void AddUInt(uint i)
        {
            CompiledPub.AddRange(BitConverter.GetBytes(i).ToArray());
        }

        private void AddInt(int i)
        {
            CompiledPub.AddRange(BitConverter.GetBytes(i).ToArray());
        }

        private void AddFloat(float f)
        {
            CompiledPub.AddRange(BitConverter.GetBytes(f).ToArray());
        }

        private void AddUshort(ushort u)
        {
            CompiledPub.AddRange(BitConverter.GetBytes(u).ToArray());
        }

        private void AddShort(short s)
        {
            CompiledPub.AddRange(BitConverter.GetBytes(s).ToArray());
        }

        private void WriteIncludesToFile()
        {
            Includes.Reverse();
            foreach (string Include in Includes)
            {
                AddInt(GetStringPosByName(Include));
            }
        }

        private void WriteExportsToFile()
        {
            foreach (Export Export in Exports)
            {
                AddUInt(Export.CRC32);
                AddInt(Export.Start);
                AddUInt(Export.FunctionNameHash);
                AddUInt(Export.FileNameHash);
                CompiledPub.Add(Export.NumOfParams);
                CompiledPub.Add((byte)Export.Flags);
                AddShort(Export.Unknown);
            }
        }

        private void WriteImportsToFile()
        {
            foreach (Import Import in Imports)
            {
                AddUInt(Import.FunctionNameHash);
                AddUInt(Import.FileNameHash);
                AddShort(Import.NumOfRefs);
                CompiledPub.Add(Import.NumOfParams);
                CompiledPub.Add(Import.Flags);
                foreach (int _ref in Import.Refs)
                {
                    AddInt(_ref);
                }
            }
        }

        private void WriteStringTableToFile()
        {
            foreach (StringTable refString in StringTableDict.Values)
            {
                AddInt(refString.StringPtr);
                AddInt(refString.NumOfRefs);
                foreach (int _ref in refString.Refs)
                {
                    AddInt(_ref);
                }
            }
        }

        private void WriteAnimTreesToFile()
        {
            foreach (AnimTree animtree in AnimTrees)
            {
                AddInt(animtree.AnimTreeNamePtr);
                AddShort(animtree.NumOfAnimTreeRefs);
                AddShort(animtree.NumOfAnimRefs);

                foreach (int _ref in animtree.AnimTreeRefs)
                {
                    AddInt(_ref);
                }

                foreach (AnimTreePtr animref in animtree.AnimRefs)
                {
                    AddLong(animref.AnimTreeNamePtr);
                    AddLong(animref.AnimTreeRef);
                }
            }
        }

        private void ResetAllVars()
        {
            GSCNameSpaceHash = 0;
            CompiledPub = new List<byte>();

            Imports = new List<Import>();
            Exports = new List<Export>();
            AnimTrees = new List<AnimTree>();
            Includes = new List<string>();
            StringPositionDict = new Dictionary<string, int>();
            StringTableDict = new Dictionary<string, StringTable>();

            DefinedVariables = new Dictionary<string, ParseTreeNode>();
            LocalVariables = new List<string>();

            ArrayKeys = new List<Key>();
            SwitchKeys = new List<string>();
            numofForeachStatements = 0;
            numofSwitchStatements = 0;
            NumOfParams = 0;

            BreakList = new List<List<int>>();
            ContinueList = new List<List<int>>();

            OperatorList = new List<List<Operators>>();
            PrevOperatorList = new List<List<string>>();

            BoolExprList = new List<List<int>>();

            OutputString = "";
            OutputFuncts = new StringBuilder();
            DoOutputFuncts = false;
            ErrorInScript = false;
        }

        public bool Init(ParseTree Tree, bool OutputFunctions)
        {
            ResetAllVars();
            DoOutputFuncts = OutputFunctions;

            if (Tree.ParserMessages.Count > 0)
            {
                int errorLoc = Tree.ParserMessages[0].Location.Line + 1;
                OutputString += "ERROR: Bad syntax in line " + errorLoc + ". " + Tree.ParserMessages[0].Message + System.Environment.NewLine; //when it finds an error the parsing stops, so multiple syntax errors can't be outputted
                return false;
            }

            var file = new GSCHeader();
            CompiledPub.AddRange(new byte[0x48]); //add bytes to be replaced by the header

            List<ParseTreeNode> IncludeNodes = new List<ParseTreeNode>();
            List<ParseTreeNode> DefineNodes = new List<ParseTreeNode>();
            List<ParseTreeNode> GSCOptionsNodes = new List<ParseTreeNode>();
            List<ParseTreeNode> AnimTreeNodes = new List<ParseTreeNode>();
            List<ParseTreeNode> FunctionNodes = new List<ParseTreeNode>();

            ParseTreeNode currnode;
            bool FirstLoop = true;
            foreach (ParseTreeNode Node in Tree.Root.ChildNodes)
            {
                if (FirstLoop) //makeplusrule adds the second and onwards node to an individual parent node called "unnamed0" and this corrects this
                {
                    FirstLoop = false;
                    currnode = Node;
                }
                else
                {
                    currnode = Node.ChildNodes[0];
                }

                switch (currnode.Term.Name)
                {
                    case "include":
                        IncludeNodes.Add(currnode);
                        break;
                    case "define":
                        DefineNodes.Add(currnode);
                        break;
                    case "GSCPath":
                    case "GSCNamespace":
                    case "GSCChecksum":
                        GSCOptionsNodes.Add(currnode);
                        break;
                    case "usingAnimTree":
                        AnimTreeNodes.Add(currnode);
                        break;
                    case "function":
                        FunctionNodes.Add(currnode);
                        break;
                }
            }

            if (IncludeNodes.Count > 0)
            {
                foreach (ParseTreeNode childNode in IncludeNodes)
                {
                    string IncludeStr = childNode.ChildNodes[0].Token.ValueString.Replace(@"\", "/").ToLower();
                    if(!StringPositionDict.ContainsKey(IncludeStr))
                    {
                        AddString(IncludeStr);
                        Includes.Add(IncludeStr);
                    }
                    else
                    {
                        OutputString += "ERROR: Duplicate include: " + IncludeStr + System.Environment.NewLine;
                        return false;
                    }
                }
            }

            if (DefineNodes.Count > 0)
            {
                foreach (ParseTreeNode childNode in DefineNodes)
                {
                    string DefineVar = childNode.ChildNodes[0].ChildNodes[0].Token.ValueString.ToLower();
                    if (!DefinedVariables.ContainsKey(DefineVar))
                    {
                        DefinedVariables.Add(DefineVar, childNode.ChildNodes[2]);
                    }
                    else
                    {
                        OutputString += "ERROR: Duplicate define variable: " + DefineVar + System.Environment.NewLine;
                        ErrorInScript = true;
                    }
                }
            }

            string GSCPathName = "scripts/shared/duplicaterender_mgr.gsc"; //by default duplicaterender_mgr.gsc is overwritten when injecting, so we use it's values unless a 
            GSCNameSpaceHash = CanonicalHash("duplicate_render");          //script defines them differently. Only really for extra customisation or multi script injection
            uint CRC = 0x2337484f;

            if (GSCOptionsNodes.Count > 0)
            {
                foreach (ParseTreeNode childNode in GSCOptionsNodes)
                {
                    switch (childNode.Term.Name)
                    {
                        case "GSCPath":
                            GSCPathName = childNode.ChildNodes[0].Token.ValueString.Replace(@"\", "/");
                            if (GSCPathName == "")
                            {
                                OutputString += "ERROR: Path cannot have a size of 0." + System.Environment.NewLine;
                                return false;
                            }
                            break;

                        case "GSCNamespace":
                            string gscnamespace = childNode.ChildNodes[0].Token.ValueString;
                            if (!gscnamespace.StartsWith("hash_"))
                                GSCNameSpaceHash = CanonicalHash(gscnamespace);
                            else
                                GSCNameSpaceHash = ParseStringToUint(gscnamespace, 5);
                            break;

                        case "GSCChecksum":
                            CRC = ParseStringToUint(childNode.ChildNodes[0].Token.ValueString, 0);
                            break;
                    }
                }
            }

            file.NamePtr = CompiledPub.Count;
            AddString(GSCPathName); //add the gsc file path, as it is always the first string

            if (AnimTreeNodes.Count > 0)
            {
                if (AnimTreeNodes.Count > 1)
                {
                    OutputString += "ERROR: Only 1 #using_animtree allowed!" + System.Environment.NewLine;
                    return false;
                }
                foreach (ParseTreeNode childNode in AnimTreeNodes)
                {
                    AddString(childNode.ChildNodes[0].Token.ValueString);
                    AnimTrees.Add(new AnimTree
                    {
                        AnimTreeNamePtr = GetStringPosByName(childNode.ChildNodes[0].Token.ValueString),
                        NumOfAnimRefs = 0,
                        NumOfAnimTreeRefs = 0,
                        AnimRefs = new List<AnimTreePtr>(),
                        AnimTreeRefs = new List<int>(),
                    });
                }
            }

            PrepareStringsAndVariables(Tree.Root); //add all the strings to the file and prepare switch/foreach statement variables

            if(IncludeNodes.Count > byte.MaxValue)
            {
                OutputString += "ERROR: Max of 255 Includes reached!" + System.Environment.NewLine;
                return false;
            }
            file.IncludesPtr = CompiledPub.Count;
            file.IncludesCount = (byte)IncludeNodes.Count;
            WriteIncludesToFile();

            int CodeSectionStart = CompiledPub.Count;

            if (FunctionNodes.Count > 0) //write the actual functions
            {
                foreach (ParseTreeNode childNode in FunctionNodes)
                {
                    EmitFunction(childNode);
                }
            }

            if (ErrorInScript) //if there is a script error during compilation the GSC file is not outputted
            {
                return false;
            }


            file.CodeSectionStart = CodeSectionStart;
            file.CodeSize = CompiledPub.Count - CodeSectionStart;

            file.ExportsPtr = CompiledPub.Count;
            file.ExportsCount = (ushort)Exports.Count;
            WriteExportsToFile();

            file.ImportsPtr = CompiledPub.Count;
            file.ImportsCount = (ushort)Imports.Count;
            WriteImportsToFile();

            file.AnimTreePtr = CompiledPub.Count;
            file.AnimTreeCount = (byte)AnimTrees.Count;
            WriteAnimTreesToFile();

            if (StringTableDict.Count > ushort.MaxValue)
            {
                OutputString += "ERROR: Max of 65535 unique strings reached!" + System.Environment.NewLine;
                return false;
            }
            file.StringTablePtr = CompiledPub.Count;
            file.StringTableCount = (ushort)StringTableDict.Count;
            WriteStringTableToFile();

            file.DevStringTablePtr = CompiledPub.Count;
            file.DevStringTableCount = 0;

            file.FixupPtr = CompiledPub.Count;
            file.FixupCount = 0;

            file.ProfilePtr = CompiledPub.Count;
            file.ProfileCount = 0;

            file.Magic = new byte[] { 0x80, 0x47, 0x53, 0x43, 0x0D, 0x0A, 0x00, 0x1C };
            file.SourceCRC = CRC;

            file.Flags = 0;

            OutputString += "Script compiled successfully!" + System.Environment.NewLine;
            return true;
        }

        private void ResetFunctionVars()
        {
            LocalVariables.Clear();
            numofForeachStatements = 0;
            numofSwitchStatements = 0;
            NumOfParams = 0;

            BreakList.Clear();
            ContinueList.Clear();

            OperatorList.Clear();
            PrevOperatorList.Clear();

            BoolExprList.Clear();
        }

        private void EmitFunction(ParseTreeNode functionNode)
        {
            ResetFunctionVars();

            SetAlignedQWord();
            SetAlignedQWord();

            var function = new Export();
            List<ParseTreeNode> DefaultParams = new List<ParseTreeNode>();
            int FuncNameNodeOffset = 0;

            if (functionNode.ChildNodes[0].Term.Name == "FunctionFlag")
            {
                function.Flags = ExportFlags.AutoExec;
                FuncNameNodeOffset = 1;
            }
            else
            {
                function.Flags = ExportFlags.None;
            }
                                                                 //FuncParenParameters              FuncParameters  
            foreach (ParseTreeNode parameterNode in functionNode.ChildNodes[FuncNameNodeOffset + 1].ChildNodes[0].ChildNodes) //parse parameters
            {
                if (parameterNode.Term.Name == "DefaultParameters") //only if there is 1 parameter
                {
                    DefaultParams.Add(parameterNode);
                    ParseParameter(parameterNode.ChildNodes[0].ChildNodes[0]);
                }
                else if(parameterNode.ChildNodes[0].Term.Name == "DefaultParameters")
                {
                    DefaultParams.Add(parameterNode.ChildNodes[0]);
                    ParseParameter(parameterNode.ChildNodes[0].ChildNodes[0]);
                }
                else
                    ParseParameter(parameterNode);
            }

            PrepareLocalVariables(functionNode.ChildNodes[FuncNameNodeOffset + 2]); //define all the local variables

            function.Start = CompiledPub.Count;

            if (LocalVariables.Count > 0)
            {
                EmitLocalVariables();
            }
            else
            {
                EmitOpcode(OP_CheckClearParams);
            }

            foreach (ParseTreeNode defaultnode in DefaultParams)
            {
                ScriptCompile(defaultnode.ChildNodes[0]);
                EmitOpcode(OP_IsDefined);
                EmitOpcode(OP_JumpOnTrue);
                SetAlignedWord();
                int jmpRangePos = CompiledPub.Count;
                CompiledPub.AddRange(new byte[2]);
                int statementStart = CompiledPub.Count;
                ScriptCompile(defaultnode.ChildNodes[2]);
                ScriptCompile(defaultnode.ChildNodes[0], true);
                EmitOpcode(OP_SetVariableField);

                CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - statementStart)).ToList());
            }

            ScriptCompile(functionNode.ChildNodes[FuncNameNodeOffset + 2]); //compile function code
            EmitOpcode(OP_End);

            Exports.Add(function);
            EmitCrc32();

            string name = functionNode.ChildNodes[FuncNameNodeOffset].Token.ValueString;

            function.Unknown = 0;
            function.FunctionNameHash = CanonicalHash(name);
            function.NumOfParams = NumOfParams;
            function.FileNameHash = GSCNameSpaceHash;

            if (DoOutputFuncts)
            {
                OutputFuncts.Append("0x" + function.FunctionNameHash.ToString("x"));
                OutputFuncts.Append("," + name);
                OutputFuncts.AppendLine();
            }
        }

        private void ParseParameter(ParseTreeNode Node)
        {
            if (Node.Term.Name == "identifier")
            {
                LocalVariables.Add(Node.FindTokenAndGetValue().ToLower());
                NumOfParams++;
            }
            else
            {
                foreach (ParseTreeNode child in Node.ChildNodes)
                {
                    ParseParameter(child);
                }
            }
        }

        private void EmitCrc32()
        {
            var crc32 = new Crc32();
            int start = Exports.Last().Start;
            crc32.AddData(start, CompiledPub.Count - start);
            Exports.Last().CRC32 = crc32.GetCrc32();
        }

        private void EmitGetAnimation(ParseTreeNode node)
        {
            AnimTrees.Last().NumOfAnimRefs++;
            string str = node.ChildNodes[1].Token.ValueString;

            EmitOpcode(OP_GetAnimation);
            SetAlignedQWord();

            AnimTrees.Last().AnimRefs.Add(new AnimTreePtr
            {
                AnimTreeNamePtr = GetStringPosByName(str),
                AnimTreeRef = CompiledPub.Count,
            });

            AddInt(GetStringPosByName(str));
            CompiledPub.AddRange(new byte[4]);
        }

        void EmitJumpStatement(ParseTreeNode Node)
        {
            EmitOpcode(OP_Jump);
            SetAlignedWord();
            if (Node.ChildNodes[0].Term.Name.ToLower() == "continue")
            {
                if (Node.ChildNodes.Count == 2) //used for "continue X;" where X is a number. Allows to continue out of multiple nested loops
                    ContinueList[ContinueList.Count - int.Parse(Node.ChildNodes[1].Token.ValueString)].Add(CompiledPub.Count);
                else
                    ContinueList.Last().Add(CompiledPub.Count);
                AddShort(0);
            }
            else //if "break;"
            {
                if (Node.ChildNodes.Count == 2) //used for "break X;" where X is a number. Allows to break out of multiple nested loops
                    BreakList[BreakList.Count - int.Parse(Node.ChildNodes[1].Token.ValueString)].Add(CompiledPub.Count);
                else
                    BreakList.Last().Add(CompiledPub.Count);
                AddShort(0);
            }
        }

        private void ScriptCompile(ParseTreeNode Node, bool _ref = false, bool waitTillVar = false)
        {
            switch (Node.Term.Name)
            {
                case "jumpStatement":
                    EmitJumpStatement(Node);
                    break;

                case "getAnimation":
                    EmitGetAnimation(Node);
                    break;

                case "animTree":
                    AnimTrees.Last().NumOfAnimTreeRefs++;

                    EmitOpcode(OP_GetInteger);
                    SetAlignedDword();

                    AnimTrees.Last().AnimTreeRefs.Add(CompiledPub.Count);

                    AddInt(0); //this is overwritten during linking
                    break;

                case "waittillframeend":
                        EmitOpcode(OP_WaitTillFrameEnd);
                    break;

                case "expression":
                    EmitExpression(Node, false);
                    break;

                case "boolNot":
                    if (Node.ChildNodes[1].ChildNodes[0].Term.Name == "expression")
                        EmitExpression(Node.ChildNodes[1].ChildNodes[0], true);
                    else if(Node.ChildNodes[1].ChildNodes[0].Term.Name == "conditionalStatement")
                        EmitconditionalStatement(Node.ChildNodes[1].ChildNodes[0], true);
                    else
                    {
                        ScriptCompile(Node.ChildNodes[1]);
                        EmitOpcode(OP_BoolNot);
                    }
                        
                    break;

                case "block":
                    if (Node.ChildNodes.Count > 0)
                    {
                        ScriptCompile(Node.ChildNodes[0]);
                    }
                    break;

                case "blockContent":
                    foreach (ParseTreeNode childNode in Node.ChildNodes[0].ChildNodes)
                    {
                        ScriptCompile(childNode.ChildNodes[0]);
                    }
                    break;

                case "simpleCall":
                    EmitCall(Node.ChildNodes[0].ChildNodes[0], true);
                    break;

                case "call":
                    EmitCall(Node.ChildNodes[0], false);
                    break;

                case "conditionalStatement":
                    EmitconditionalStatement(Node, false);
                    break;

                case "wait":
                    ScriptCompile(Node.ChildNodes[1]);
                    EmitOpcode(OP_Wait);
                    break;

                case "return":
                    if (Node.ChildNodes.Count > 1)
                    {
                        ScriptCompile(Node.ChildNodes[1]);
                        EmitOpcode(OP_Return);
                    }
                    else
                    {
                        EmitOpcode(OP_End);
                    }
                    break;

                case "stringLiteral":
                    EmitGetString(Node.Token.ValueString, false);
                    break;

                case "size":
                    ScriptCompile(Node.ChildNodes[0]);
                    EmitOpcode(OP_SizeOf);
                    break;

                case "isString": // &(string)
                    EmitGetString(Node.ChildNodes[1].Token.ValueString, true);
                    break;

                case "hashedString":
                    EmitGetHash(Node.ChildNodes[1]);
                    break;

                case "identifier":
                    string iden = Node.Token.ValueString;
                    if (IsObjectOwner(iden))
                    {
                        EmitOwner(Node, _ref);
                        break;
                    }
                    EvalLocalVariable(iden, Node, _ref, waitTillVar);
                    break;

                case "setVariableField":
                    EmitSetVariableField(Node);
                    break;

                case "directAccess":
                    EmitEvalFieldVariable(Node, _ref);
                    break;

                case "numberLiteral":
                    if (Node.Token.Value is int)
                    {
                        EmitGetInt(int.Parse(Node.Token.ValueString));
                    }
                    else if (Node.Token.Value is double)
                    {
                        EmitGetFloat(float.Parse(Node.Token.ValueString));
                    }
                    break;

                case "array":
                    EmitEvalArray(Node, _ref);
                    break;

                case "NewArray":
                    EmitEvalNewArray(Node);
                    break;

                case "ifStatement":
                    EmitIfStatement(Node);
                    break;

                case "whileStatement":
                    EmitWhileStatement(Node);
                    break;

                case "forStatement":
                    EmitForStatement(Node);
                    break;

                case "switchStatement":
                    EmitSwitchStatement(Node);
                    break;

                case "foreachStatement":
                    EmitForEachStatement(Node);
                    break;

                case "expr":
                case "expr+":
                    Node.ChildNodes.Reverse();
                    foreach (ParseTreeNode childNode in Node.ChildNodes)
                    {
                        ScriptCompile(childNode, _ref, false);
                    }
                    break;

                case "statement":
                case "statementBlock":
                case "declaration":
                case "parenExpr":
                    ScriptCompile(Node.ChildNodes[0], _ref);
                    break;

                case "getFunction":
                    EmitGetFunction(Node);
                    break;

                case "vector":
                    EmitVector(Node);
                    break;

                default:
                    OutputString += "ERROR: Unknown expr node " + Node.Term.Name + "! Please report this." + System.Environment.NewLine;
                    ErrorInScript = true;
                    break;
            }
        }

        uint ParseStringToUint(string str, int RemoveAt)
        {
            try
            {
                uint buffer = 0;
                if (RemoveAt != 0)
                    buffer = UInt32.Parse(str.Remove(0, RemoveAt).ToLower(), System.Globalization.NumberStyles.HexNumber);
                else
                    buffer = UInt32.Parse(str.ToLower(), System.Globalization.NumberStyles.HexNumber);
                return buffer;
            }
            catch (System.FormatException)
            {
                OutputString += "ERROR: uint number" + str + " is not formatted correctly."  + System.Environment.NewLine;
                ErrorInScript = true;
                return 0xffffffff;
            }
            
        }

        private void EmitGetHash(ParseTreeNode node)
        {
            EmitOpcode(OP_GetHash);
            SetAlignedDword();
            if (node.Token.ValueString.StartsWith("hash_"))
                AddUInt(ParseStringToUint(node.Token.ValueString, 5));
            else
                AddUInt(CanonicalHash(node.Token.ValueString));
        }

        private void EmitExpressionOpcode(string name)
        {
            switch (name)
            {
                case "+":
                    EmitOpcode(OP_Plus);
                    break;

                case "-":
                    EmitOpcode(OP_Minus);
                    break;

                case "*":
                    EmitOpcode(OP_Multiply);
                    break;

                case "/":
                    EmitOpcode(OP_Divide);
                    break;

                case "%":
                    EmitOpcode(OP_Modulus);
                    break;

                case "<<":
                    EmitOpcode(OP_ShiftLeft);
                    break;

                case ">>":
                    EmitOpcode(OP_ShiftRight);
                    break;

                case ">":
                    EmitOpcode(OP_GreaterThan);
                    break;

                case ">=":
                    EmitOpcode(OP_GreaterThanOrEqualTo);
                    break;

                case "<":
                    EmitOpcode(OP_LessThan);
                    break;

                case "<=":
                    EmitOpcode(OP_LessThanOrEqualTo);
                    break;

                case "==":
                    EmitOpcode(OP_Equal);
                    break;

                case "!=":
                    EmitOpcode(OP_NotEqual);
                    break;

                case "===":
                    EmitOpcode(OP_SuperEqual);
                    break;

                case "!==":
                    EmitOpcode(OP_SuperNotEqual);
                    break;

                case "&":
                    EmitOpcode(OP_Bit_And);
                    break;

                case "^":
                    EmitOpcode(OP_Bit_Xor);
                    break;

                case "|":
                    EmitOpcode(OP_Bit_Or);
                    break;
                default:
                    OutputString += "ERROR: Unknown operator used! please report this." + System.Environment.NewLine;
                    ErrorInScript = true;
                    break;
            }
        }

        void ParseAllOperators(ParseTreeNode node, bool firstRun, bool IsStartBoolNot)
        {
            if (firstRun)
            {
                if (IsStartBoolNot)
                    OperatorList[OperatorList.Count - 1].Add(new Operators { EndExpr = node.ChildNodes[0], HasBoolNot = true });
                else
                    OperatorList[OperatorList.Count - 1].Add(new Operators { EndExpr = node.ChildNodes[0] });
            }

            if (node.ChildNodes[2].ChildNodes[0].Term.Name == "boolNot")
            {
                if (node.ChildNodes[2].ChildNodes[0].ChildNodes[1].ChildNodes[0].Term.Name != "expression")
                {
                    OperatorList[OperatorList.Count - 1].Add(new Operators { Operator = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Term.Name, EndExpr = node.ChildNodes[2].ChildNodes[0].ChildNodes[1], HasBoolNot = true });
                    return;
                }
                else
                    OperatorList[OperatorList.Count - 1].Add(new Operators { Operator = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Term.Name, EndExpr = node.ChildNodes[2].ChildNodes[0].ChildNodes[1].ChildNodes[0].ChildNodes[0], HasBoolNot = true });
                ParseAllOperators(node.ChildNodes[2].ChildNodes[0].ChildNodes[1].ChildNodes[0], false, false);
            }
            else
            {
                if (node.ChildNodes[2].ChildNodes[0].Term.Name != "expression")
                {
                    OperatorList[OperatorList.Count - 1].Add(new Operators { Operator = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Term.Name, EndExpr = node.ChildNodes[2].ChildNodes[0] });
                    return;
                }
                else
                    OperatorList[OperatorList.Count - 1].Add(new Operators { Operator = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Term.Name, EndExpr = node.ChildNodes[2].ChildNodes[0].ChildNodes[0] });
                ParseAllOperators(node.ChildNodes[2].ChildNodes[0], false, false);
            }
        }

        int GetOperatorPrecedence(string NewOperator) //higher is higher precedence, same as C# operator precedence apart from && and ||
        {
            if (NewOperator == "*" || NewOperator == "/" || NewOperator == "%")
                return 8;
            else if (NewOperator == "+" || NewOperator == "-")
                return 7;
            else if (NewOperator == "<<" || NewOperator == ">>")
                return 6;
            else if (NewOperator == "<" || NewOperator == ">" || NewOperator == "<=" || NewOperator == ">=")
                return 5;
            else if (NewOperator == "==" || NewOperator == "!=" || NewOperator == "===" || NewOperator == "!==")
                return 4;
            else if (NewOperator == "&")
                return 3;
            else if (NewOperator == "^")
                return 2;
            else if (NewOperator == "|")
                return 1;
            else
            {
                OutputString += "ERROR: Found unknown operator in GetOperatorPrecedence! please report this." + System.Environment.NewLine;
                ErrorInScript = true;
                return 0;
            }
        }

        void EmitExtraExpressions()
        {
            if (PrevOperatorList.Last().Count != 0)
            {
                if (PrevOperatorList.Last().Count > 1)
                {
                    OutputString += "ERROR: over 1 operators still in PrevOperatorList! your code will not work correctly, please report this." + System.Environment.NewLine;
                    ErrorInScript = true;
                    EmitExpressionOpcode(PrevOperatorList.Last().Last());
                }
                else
                    EmitExpressionOpcode(PrevOperatorList.Last().Last());
                PrevOperatorList.Last().RemoveAt(PrevOperatorList.Last().Count - 1);
            }
        }

        void EmitBooleanExpression(Operators operators)
        {
            EmitExtraExpressions();
            if (BoolExprList.Last().Count != 0)
            {
                if (operators.Operator == "&&")
                {
                    EmitOpcode(OP_JumpOnFalseExpr);
                }
                else
                {
                    foreach (int i in BoolExprList.Last())
                    {
                        CompiledPub.Replace(i, BitConverter.GetBytes((short)(CompiledPub.Count - i - 2)).ToList());
                    }
                    BoolExprList.Last().RemoveAt(BoolExprList.Last().Count - 1);
                    EmitOpcode(OP_JumpOnTrueExpr);
                }
            }
            else
                EmitOpcode(operators.Operator == "&&" ? OP_JumpOnFalseExpr : OP_JumpOnTrueExpr);
            SetAlignedWord();
            BoolExprList.Last().Add(CompiledPub.Count);
            CompiledPub.AddRange(new byte[2]);
            ExpressionScriptCompile(operators);
        }

        void ExpressionScriptCompile(Operators Operators) //boolnots suck, as their expression overwrites the rest of the math. e.g. 3 + !5 + 7 is  3 + !(5 + 7) in node terms
        {
            ScriptCompile(Operators.EndExpr);
            if (Operators.HasBoolNot == true)
                EmitOpcode(OP_BoolNot);
        }

        private void EmitExpression(ParseTreeNode node, bool IsStartBoolNot)
        {
            OperatorList.Add(new List<Operators>());
            PrevOperatorList.Add(new List<string>());
            BoolExprList.Add(new List<int>());

            ParseAllOperators(node, true, IsStartBoolNot);
            foreach (Operators operators in OperatorList.Last())
            {
                if (operators.Operator == null) //if it is the first operator compile just first expr
                {
                    ExpressionScriptCompile(operators);
                    continue;
                }

                if (operators.Operator == "&&" || operators.Operator == "||")
                    EmitBooleanExpression(operators);
                else if (PrevOperatorList.Last().Count == 0)
                {
                    ExpressionScriptCompile(operators);
                    PrevOperatorList.Last().Add(operators.Operator);
                }
                else
                {
                    if (GetOperatorPrecedence(operators.Operator) > GetOperatorPrecedence(PrevOperatorList.Last().Last()))
                    {
                        ExpressionScriptCompile(operators);
                        EmitExpressionOpcode(operators.Operator);
                    }
                    else
                    {
                        EmitExpressionOpcode(PrevOperatorList.Last().Last());
                        PrevOperatorList.Last().RemoveAt(PrevOperatorList.Last().Count - 1);
                        ExpressionScriptCompile(operators);
                        PrevOperatorList.Last().Add(operators.Operator);
                    }
                }
            }

            EmitExtraExpressions();

            foreach (int i in BoolExprList.Last())
            {
                CompiledPub.Replace(i, BitConverter.GetBytes((short)(CompiledPub.Count - i - 2)).ToList());
            }
            OperatorList.RemoveAt(OperatorList.Count - 1);
            PrevOperatorList.RemoveAt(PrevOperatorList.Count - 1);
            BoolExprList.RemoveAt(BoolExprList.Count - 1);
        }

        private void EmitconditionalStatement(ParseTreeNode node, bool FirstExprIsBoolNot)
        {
            ScriptCompile(node.ChildNodes[0]);
            if (FirstExprIsBoolNot)
                EmitOpcode(OP_BoolNot);
            EmitOpcode(OP_JumpOnFalse);
            SetAlignedWord();
            int jmpRangePos = CompiledPub.Count;
            CompiledPub.AddRange(new byte[2]);
            ScriptCompile(node.ChildNodes[2]);
            EmitOpcode(OP_Jump);
            SetAlignedWord();
            int opJumpJmpRangePos = CompiledPub.Count;
            CompiledPub.AddRange(new byte[2]);
            CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - jmpRangePos - 2)).ToList());
            ScriptCompile(node.ChildNodes[4]);
            CompiledPub.Replace(opJumpJmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - opJumpJmpRangePos - 2)).ToList());
        }

        private void EmitForEachStatement(ParseTreeNode node)
        {
            BreakList.Add(new List<int>());
            ContinueList.Add(new List<int>());

            var arrayKey = new Key { First = ArrayKeys[0].First, Second = ArrayKeys[0].Second }; //array key variables
            ArrayKeys.RemoveAt(0);                                      //remove array keys to make multiple foreach statments unique

            if (node.ChildNodes.Count == 5) //regular nodes: 0 = foreach, 1 = identifier, 2 = in , 3 = expr, 4 = statementBlock
            {
                
                ScriptCompile(node.ChildNodes[3]);                          //scriptcompile array
                EvalLocalVariable(arrayKey.First, node, true);
                EmitOpcode(OP_SetVariableField);                            //set the arraykey's variable to the array
                EvalLocalVariable(arrayKey.First, node, false);             
                EmitOpcode(OP_FirstArrayKey);                               //get the first key in the array
                EvalLocalVariable(arrayKey.Second, node, true);
                EmitOpcode(OP_SetVariableField);                            //set the second var to the first array key, the index
                                                                            //where logic starts
                int negJmpPos = CompiledPub.Count;
                EvalLocalVariable(arrayKey.Second, node, false);            //if the first array key is undefined, jump past loop
                EmitOpcode(OP_IsDefined);                                   //^
                EmitOpcode(OP_JumpOnFalse);                                 //^
                SetAlignedWord();                                           //^
                int jmpRangePos = CompiledPub.Count;                        //^
                CompiledPub.AddRange(new byte[2]);                          //^

                EvalLocalVariable(arrayKey.Second, node, false);            // array index
                EvalLocalVariable(arrayKey.First, node, false);             //array to use
                EmitOpcode(OP_EvalArray);                                   //evalarray the first array key
                ScriptCompile(node.ChildNodes[1], true);                    
                EmitOpcode(OP_SetVariableField);                            //set the identifier var to the array key
            
                ScriptCompile(node.ChildNodes[4]);                          //compile statement block

                EmitContinue();
                EvalLocalVariable(arrayKey.Second, node, false);            //current key
                EvalLocalVariable(arrayKey.First, node, false);             //array to use
                EmitOpcode(OP_NextArrayKey);                                //get next array key
                EvalLocalVariable(arrayKey.Second, node, true);
                EmitOpcode(OP_SetVariableField);                            //set the index to the next array key
                EmitOpcode(OP_Jump);                                        //jump to the loop top
                SetAlignedWord();
                AddShort((short)(negJmpPos - (CompiledPub.Count + 2)));
                CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - jmpRangePos - 2)).ToList());
            }
            else    //double foreach nodes: 0 = foreach, 1 = index, 2 = value, 3 = in, 4 = array, 5 = statementBlock
            {
                ScriptCompile(node.ChildNodes[4]);                          //scriptcompile array
                EvalLocalVariable(arrayKey.First, node, true);
                EmitOpcode(OP_SetVariableField);                            //set the arraykey's variable to the array
                EvalLocalVariable(arrayKey.First, node, false);
                EmitOpcode(OP_FirstArrayKey);                               //get the first key in the array
                EvalLocalVariable(arrayKey.Second, node, true);
                EmitOpcode(OP_SetVariableField);                            //set the second var to the first array key, the index

                //where logic starts
                int negJmpPos = CompiledPub.Count;
                EvalLocalVariable(arrayKey.Second, node, false);            //if there are no indexes in the array, skip
                EmitOpcode(OP_IsDefined);                                   //^
                EmitOpcode(OP_JumpOnFalse);                                 //^
                SetAlignedWord();                                           //^
                int jmpRangePos = CompiledPub.Count;                        //^
                CompiledPub.AddRange(new byte[2]);                          //^
                
                EvalLocalVariable(arrayKey.Second, node, false);            // array index
                EvalLocalVariable(arrayKey.First, node, false);             //array to use
                EmitOpcode(OP_EvalArray);                                   //get value of first index
                ScriptCompile(node.ChildNodes[2], true);
                EmitOpcode(OP_SetVariableField);                            //set the value var to the array index value
                
                EvalLocalVariable(arrayKey.Second, node, false);
                ScriptCompile(node.ChildNodes[1], true);
                EmitOpcode(OP_SetVariableField);                            //set the index var to the array index
                
                ScriptCompile(node.ChildNodes[5]);                          //compile statement block

                EmitContinue();
                EvalLocalVariable(arrayKey.Second, node, false);            // array index
                EvalLocalVariable(arrayKey.First, node, false);             //array to use
                EmitOpcode(OP_NextArrayKey);                                //get NextArrayKey
                EvalLocalVariable(arrayKey.Second, node, true);
                EmitOpcode(OP_SetVariableField);                          
                
                EmitOpcode(OP_Jump);                                        //jump to the loop top
                SetAlignedWord();
                AddShort((short)(negJmpPos - (CompiledPub.Count + 2)));
                CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - jmpRangePos - 2)).ToList());
                
            }
            EmitBreak();
        }

        private void EmitSwitchStatement(ParseTreeNode node) //real switch statements only work with hardcoded ints or strings and can't use any vars or functions, but
                                                             //many scripts use more than just ints or strings in switch statements
        {
            ContinueList.Add(new List<int>());
            BreakList.Add(new List<int>());
            string SwitchKey = SwitchKeys[0]; //Using a switchkey instead of compiling the expression for each case means that the value doesn't change while the switch statement is executing
            SwitchKeys.RemoveAt(0);

            List<int> RollOverLabels = new List<int>();

            ScriptCompile(node.ChildNodes[1]); //parenExpr
            EvalLocalVariable(SwitchKey, node, true);
            EmitOpcode(OP_SetVariableField);

            ParseTreeNode defaultNode = null;

            foreach (ParseTreeNode caseNode in node.ChildNodes[2].ChildNodes) //casenode is switchContent
            {
                if(caseNode.ChildNodes[0].ChildNodes.Count == 2)
                {
                    if(defaultNode != null)
                    {
                        OutputString += "ERROR: more than 1 default switch label at switch " + node.Token.Location.Line + System.Environment.NewLine;
                        ErrorInScript = true;
                    }
                    defaultNode = caseNode;
                    continue;
                }
                EvalLocalVariable(SwitchKey, node, false);
                if(caseNode.ChildNodes.Count == 2)
                {
                    ScriptCompile(caseNode.ChildNodes[0].ChildNodes[1]); //switchLabel expr
                    EmitOpcode(OP_Equal);
                    if (RollOverLabels.Count != 0)
                    {
                        foreach (int pos in RollOverLabels)
                            CompiledPub.Replace(pos, BitConverter.GetBytes((short)(CompiledPub.Count - pos - 2)).ToList());
                        RollOverLabels.Clear();
                    }
                    EmitOpcode(OP_JumpOnFalse);
                    SetAlignedWord();
                    int jmpRangePos = CompiledPub.Count;
                    CompiledPub.AddRange(new byte[2]);
                    ScriptCompile(caseNode.ChildNodes[1]); //block or blockContent
                    EmitOpcode(OP_Jump);
                    SetAlignedWord();
                    BreakList.Last().Add(CompiledPub.Count);
                    CompiledPub.AddRange(new byte[2]);
                    CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - jmpRangePos - 2)).ToList());
                }
                else
                {
                    ScriptCompile(caseNode.ChildNodes[0].ChildNodes[1]); //switchLabel expr
                    EmitOpcode(OP_Equal);
                    if (RollOverLabels.Count != 0)
                    {
                        foreach (int pos in RollOverLabels)
                            CompiledPub.Replace(pos, BitConverter.GetBytes((short)(CompiledPub.Count - pos - 2)).ToList());
                        RollOverLabels.Clear();
                    }
                    EmitOpcode(OP_JumpOnTrueExpr);
                    SetAlignedWord();
                    RollOverLabels.Add(CompiledPub.Count);
                    CompiledPub.AddRange(new byte[2]);
                }
            }

            if(defaultNode != null) //emit the default last
            {
                if(defaultNode.ChildNodes.Count == 1)
                {
                    OutputString += "ERROR: default switch label must have code at switch " + node.Token.Location.Line + System.Environment.NewLine;
                    ErrorInScript = true;
                }
                else
                    ScriptCompile(defaultNode.ChildNodes[1]); //block or blockContent
            }

            EmitBreak(); 

            EmitContinue();//continues aren't meant to be used in switch statements, but the grammar allows it so this just makes them do the same thing as a break
        }

        private void EmitForStatement(ParseTreeNode node)
        {
            int negJmpPos = 0;
            int jmpRangePos = 0;
            var forBodyNode = node.ChildNodes[1];
            int setVariableNodeIndex = forBodyNode.ChildNodes.FindIndex(e => e.Term.Name == "setVariableField");
            int booleanExprNodeIndex = forBodyNode.ChildNodes.FindIndex(e => e.Term.Name == "expr");
            int forIterateNodeIndex = forBodyNode.ChildNodes.FindIndex(e => e.Term.Name == "forIterate");
            int statementBlockNodeIndex = node.ChildNodes.FindIndex(e => e.Term.Name == "statementBlock");

            BreakList.Add(new List<int>());
            ContinueList.Add(new List<int>());

            if (setVariableNodeIndex != -1)
                ScriptCompile(forBodyNode.ChildNodes[setVariableNodeIndex]);

            negJmpPos = CompiledPub.Count;
            
            if (booleanExprNodeIndex != -1)
            {
                ScriptCompile(forBodyNode.ChildNodes[booleanExprNodeIndex]);
                EmitOpcode(OP_JumpOnFalse);
                SetAlignedWord();
                jmpRangePos = CompiledPub.Count;
                AddShort(0);
            }

            int Start = CompiledPub.Count;

            if (statementBlockNodeIndex != -1)
                ScriptCompile(node.ChildNodes[statementBlockNodeIndex]);

            EmitContinue();

            if (forIterateNodeIndex != -1)
                EmitSetVariableField(forBodyNode.ChildNodes[forIterateNodeIndex]);

            EmitOpcode(OP_Jump);
            SetAlignedWord();
            AddShort((short)(negJmpPos - (CompiledPub.Count + 2)));
            if (jmpRangePos != 0)
                CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - jmpRangePos - 2)).ToList());
            EmitBreak();
        }

        private void EmitBreak()
        {
            foreach (int BreakPos in BreakList.Last())
            {
            CompiledPub.Replace(BreakPos, BitConverter.GetBytes((short)(CompiledPub.Count - BreakPos - 2)).ToList());
            }    
              
            BreakList.RemoveAt(BreakList.Count - 1);
        }

        private void EmitContinue()
        {
            foreach (int ContinuePos in ContinueList.Last())
            {
                CompiledPub.Replace(ContinuePos, BitConverter.GetBytes((short)(CompiledPub.Count - ContinuePos - 2)).ToList());
            }
            ContinueList.RemoveAt(ContinueList.Count - 1);
        }

        private void EmitGetFunction(ParseTreeNode node)
        {
            uint includeHash = GSCNameSpaceHash;
            int nodeIndex = 0;
            if (node.ChildNodes[0].Term.Name == "gscForFunction")
            {
                includeHash = CanonicalHash(node.FindTokenAndGetValue());
                nodeIndex = 1;
            }
            EmitOpcode(OP_GetFunction);
            string namestring = node.ChildNodes[nodeIndex].FindTokenAndGetValue();
            uint name = CanonicalHash(namestring);
            AddRefToCall(name, 0, (byte)CallFlags.LocalCall | (byte)FunctionFlags.GetFunction, includeHash, namestring);
            GetFunctionAllign();
            AddUInt(name);
            AddInt(0);
        }

        private void EmitVector(ParseTreeNode node)
        {
            ScriptCompile(node.ChildNodes[2]);
            ScriptCompile(node.ChildNodes[1]);
            ScriptCompile(node.ChildNodes[0]);
            EmitOpcode(OP_Vector);
        }

        private void EmitWhileStatement(ParseTreeNode node)
        {
            BreakList.Add(new List<int>());
            ContinueList.Add(new List<int>());
            int negJmpPos = CompiledPub.Count;
            ScriptCompile(node.ChildNodes[1]);
            EmitOpcode(OP_JumpOnFalse);
            SetAlignedWord();
            int jmpRangePos = CompiledPub.Count; 
            CompiledPub.AddRange(new byte[2]);

            int statementStart = CompiledPub.Count; //start of statement block
            ScriptCompile(node.ChildNodes[2]);

            EmitContinue();

            EmitOpcode(OP_Jump);
            SetAlignedWord();
            AddShort((short) (negJmpPos - (CompiledPub.Count + 2)));
            CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - statementStart)).ToList());
            EmitBreak();
        }

        private void EmitIfStatement(ParseTreeNode node)
        {
            ScriptCompile(node.ChildNodes[1]);
            EmitOpcode(OP_JumpOnFalse);
            SetAlignedWord();
            int jmpRangePos = CompiledPub.Count;
            CompiledPub.AddRange(new byte[2]);
            int statementStart = CompiledPub.Count;
            ScriptCompile(node.ChildNodes[2]);
            if (node.ChildNodes.Count == 4)
            {
                EmitOpcode(OP_Jump);
                SetAlignedWord();
                int opJumpJmpRangePos = CompiledPub.Count;
                CompiledPub.AddRange(new byte[2]);
                CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - statementStart)).ToList());
                ScriptCompile(node.ChildNodes[3].ChildNodes[1]);
                CompiledPub.Replace(opJumpJmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - opJumpJmpRangePos - 2)).ToList());
            }
            else
            {
                CompiledPub.Replace(jmpRangePos, BitConverter.GetBytes((short)(CompiledPub.Count - statementStart)).ToList());
            }
        }

        private void EmitEvalArray(ParseTreeNode node, bool _ref)
        {
            if (node.ChildNodes[0].Term.Name == "[]")
            {
                EmitOpcode(OP_GetEmptyArray);
                return;
            }
            ScriptCompile(node.ChildNodes[1]); // array index
            ScriptCompile(node.ChildNodes[0], _ref); //array to use
            EmitOpcode(!_ref ? OP_EvalArray : OP_EvalArrayRef);
        }

        private void EmitEvalNewArray(ParseTreeNode node)
        {
            EmitOpcode(OP_PreScriptCall);
            node.ChildNodes[0].ChildNodes.Reverse();
            byte numofParams = 0;

            if (node.ChildNodes[0].ChildNodes.Count > byte.MaxValue)
            {
                OutputString += "ERROR: More than 255 elements declared in an array! Consider declaring an array using [] and use a for loop." + System.Environment.NewLine;
                ErrorInScript = true;
                return;
            }

            foreach (ParseTreeNode childNode in node.ChildNodes[0].ChildNodes)
            {
                ScriptCompile(childNode);
                numofParams++;
            }
            EmitOpcode(OP_ScriptFunctionCall);
            byte flag = (byte)CallFlags.LocalCall | (byte)FunctionFlags.FunctionCall;
            uint includeHash = GSCNameSpaceHash;
            uint name = CanonicalHash("array");
            AddRefToCall(name, numofParams, flag, includeHash, "array");
            CompiledPub.Add(numofParams);
            SetAlignedQWord();
            AddUInt(name);
            AddInt(0);
        }

        private void EmitGetInt(int i)
        {
            if (i == 0)
            {
                EmitOpcode(OP_GetZero);
                return;
            }

            bool negative = false;

            int n = i;

            if (i < 0)
            {
                n = i*-1;
                negative = true;
            }

            if (n <= byte.MaxValue)
            {
                EmitOpcode(negative ? OP_GetNegByte : OP_GetByte);
                AddUshort((ushort) n); //getbyte and negative are size 4 for some reason
                return;
            }

            if (n <= ushort.MaxValue)
            {
                EmitOpcode(negative ? OP_GetNegUnsignedShort : OP_GetUnsignedShort);
                SetAlignedWord();
                AddUshort((ushort) n);
                return;
            }

            EmitOpcode(OP_GetInteger);
            SetAlignedDword();
            AddInt(i);
        }

        private void EmitGetFloat(float f)
        {
            EmitOpcode(OP_GetFloat);
            SetAlignedDword();
            AddFloat(f);
        }


        private void EmitEvalFieldVariable(ParseTreeNode node, bool _ref)
        {
            EmitObject(node.ChildNodes[0].ChildNodes[0]);
            EmitOpcode(_ref ? OP_EvalFieldVariableRef : OP_EvalFieldVariable);
            SetAlignedDword();
            if (node.ChildNodes[1].Term.Name == "identifier")
            {
                string valname = node.ChildNodes[1].Token.ValueString.ToLower();
                if(valname.StartsWith("var_"))
                    AddUInt(ParseStringToUint(valname, 4));
                else
                    AddUInt(CanonicalHash(valname));
            }
            else
            {
                ScriptCompile(node.ChildNodes[1]);
            }
        }

        private void EmitSetVariableField(ParseTreeNode node)
        {
            if (node.ChildNodes[1].ChildNodes[0].Term.Name != "=" && node.ChildNodes.Count > 2) //if not "=", "--" or "++"
            {
                ScriptCompile(node.ChildNodes[0].ChildNodes[0]);
                ScriptCompile(node.ChildNodes[2].ChildNodes[0]);
            }

            switch (node.ChildNodes[1].ChildNodes[0].Term.Name) //switch shortExprOperator's term name
            {
                case "++":
                    ScriptCompile(node.ChildNodes[0], true);
                    EmitOpcode(OP_Inc);
                    return;

                case "--":
                    ScriptCompile(node.ChildNodes[0], true);
                    EmitOpcode(OP_Dec);
                    return;

                case "+=":
                    EmitOpcode(OP_Plus);
                    break;

                case "-=":
                    EmitOpcode(OP_Minus);
                    break;

                case "*=":
                    EmitOpcode(OP_Multiply);
                    break;

                case "/=":
                    EmitOpcode(OP_Divide);
                    break;

                case "%=":
                    EmitOpcode(OP_Modulus);
                    break;

                case "&=":
                    EmitOpcode(OP_Bit_And);
                    break;

                case "|=":
                    EmitOpcode(OP_Bit_Or);
                    break;

                case "^=":
                    EmitOpcode(OP_Bit_Xor);
                    break;

                case "<<=":
                    EmitOpcode(OP_ShiftLeft);
                    break;

                case ">>=":
                    EmitOpcode(OP_ShiftRight);
                    break;

                case "=":
                    ScriptCompile(node.ChildNodes[2].ChildNodes[0]); //compile expression after = sign
                    break;
            }
            ScriptCompile(node.ChildNodes[0].ChildNodes[0], true);//compile expression before sign
            EmitOpcode(OP_SetVariableField);
        }

        private void EvalLocalVariable(string variable, ParseTreeNode node, bool _ref, bool waitTillVar = false)
        {
            variable = variable.ToLower();

            if (DefinedVariables.ContainsKey(variable))
            {
                ScriptCompile(DefinedVariables[variable]);
                return;
            }

            if (!waitTillVar)
            {
                EmitOpcode(_ref ? OP_EvalLocalVariableRefCached : OP_EvalLocalVariableCached);
            }
            else
            {
                EmitOpcode(OP_SetWaittillVariableFieldCached);
            }
            for (byte i = 0; i < LocalVariables.Count; i++) //loop through all defined variables and add the var number if it matches
            {
                if (LocalVariables[i] == variable)
                {
                    AddUshort(i);
                    return;
                }
            }
            
            if (LocalVariables.Count > 0)
                OutputString += String.Format("ERROR: unknown variable \"{0}\" referenced at line {1}.", variable, node.Token.Location.Line + 1) + System.Environment.NewLine;
            else
                OutputString += String.Format("ERROR: unknown variable \"{0}\" referenced at line {1} when no variables have been declared.", variable, node.Token.Location.Line + 1) + System.Environment.NewLine;
            ErrorInScript = true;

            AddUshort(0);
        }

        private void EmitGetString(string str, bool isIString)
        {
            EmitOpcode(!isIString ? OP_GetString : OP_GetIString);
            SetAlignedDword();
            AddRefToString(str); //stringtable refs point to the string position int
            AddInt(GetStringPosByName(str));
        }

        private void EmitOwner(ParseTreeNode node, bool _ref = false)
        {
            if (node.Token == null)
            {
                ScriptCompile(node);
                return;
            }

            switch (node.Token.ValueString.ToLower())
            {
                case "undefined":
                    EmitOpcode(OP_GetUndefined);
                    break;

                case "true":
                    EmitGetInt(1);
                    break;

                case "false":
                    EmitOpcode(OP_GetZero);
                    break;

                case "self":
                    EmitOpcode(OP_GetSelf);
                    break;

                case "level":
                    EmitOpcode(OP_GetLevel);
                    break;

                case "world":
                    EmitOpcode(OP_GetWorld);
                    break;

                case "game":
                    EmitOpcode(_ref ? OP_GetGameRef : OP_GetGame);
                    break;

                case "anim":
                    EmitOpcode(OP_GetAnim);
                    break;

                default:
                    ScriptCompile(node);
                    break;
            }
        }

        private void EmitObject(ParseTreeNode node)
        {
            if (node.Token == null)
            {
                ScriptCompile(node);
                EmitOpcode(OP_CastFieldObject);
                return;
            }

            switch (node.Token.ValueString.ToLower())
            {
                case "self":
                    EmitOpcode(OP_GetSelfObject);
                    break;

                case "level":
                    EmitOpcode(OP_GetLevelObject);
                    break;

                case "world":
                    EmitOpcode(OP_GetWorldObject);
                    break;

                case "anim":
                    EmitOpcode(OP_GetAnimObject);
                    break;

                default:
                    ScriptCompile(node);
                    EmitOpcode(OP_CastFieldObject);
                    break;
            }
        }

        private void EmitCall(ParseTreeNode callNode, bool decTop) //dectop if it is a call not wrapped in another call
        {
            switch (callNode.Term.Name)
            {
                case "scriptFunctionCallPointer":
                case "scriptMethodCallPointer":
                case "scriptThreadCallPointer":
                case "scriptMethodThreadCallPointer":
                    EmitCallPointer(callNode, decTop);
                    return;
            }
            int baseCallNodeIndex = callNode.ChildNodes.FindIndex(e => e.Term.Name == "baseCall");
            int parenParamsNodeIndex = callNode.ChildNodes[baseCallNodeIndex].ChildNodes.FindIndex(e => e.Term.Name == "parenParameters");
            int functionNameNodeIndex = 0;
            functionNameNodeIndex = callNode.ChildNodes[baseCallNodeIndex].ChildNodes.FindIndex(e => e.Term.Name == "stringLiteral");

            uint namehash = 0;
            string namestring = "";
            if (callNode.ChildNodes[baseCallNodeIndex].ChildNodes[1].Term.Name == "identifier")
            {
                namestring = callNode.ChildNodes[baseCallNodeIndex].ChildNodes[1].Token.ValueString.ToLower();
                if (namestring.StartsWith("func_"))
                    namehash = ParseStringToUint(namestring, 5);
                else
                    namehash = CanonicalHash(namestring);
            }
            else
            {
                namestring = callNode.ChildNodes[baseCallNodeIndex].ChildNodes[0].Token.ValueString.ToLower();
                if (namestring.StartsWith("func_"))
                    namehash = ParseStringToUint(namestring, 5);
                else
                    namehash = CanonicalHash(namestring);
            }


            if (IsObjectBuiltIn(callNode.ChildNodes[baseCallNodeIndex].ChildNodes[0].FindTokenAndGetValue()))
            {
                EmitFunctionOpcode(callNode);
                return;
            }
            EmitOpcode(OP_PreScriptCall);
            ParseTreeNode parametersNode = null;
            if (callNode.ChildNodes[baseCallNodeIndex].ChildNodes[parenParamsNodeIndex].ChildNodes.Count > 0)
            {
                parametersNode = callNode.ChildNodes[baseCallNodeIndex].ChildNodes[parenParamsNodeIndex].ChildNodes[0];
                parametersNode.ChildNodes.Reverse();
                foreach (ParseTreeNode childNode in parametersNode.ChildNodes)
                {
                    ScriptCompile(childNode);
                }
            }

            string includeName = callNode.ChildNodes[baseCallNodeIndex].ChildNodes[0].FindTokenAndGetValue().ToLower();

            if (GetNumOfParams(parametersNode) > byte.MaxValue)
            {
                OutputString += String.Format("ERROR: Tried to call function {0} with more than 255 parameters!", includeName) + System.Environment.NewLine;
                ErrorInScript = true;
                return;
            }

            byte flag = 0;
            byte numofParams = (byte)GetNumOfParams(parametersNode);
            uint includeHash = GSCNameSpaceHash;

            if (callNode.ChildNodes[baseCallNodeIndex].ChildNodes[0].Term.Name == "gscForFunction")
            {
                flag = (byte)CallFlags.ExternalCall; //if it is a :: call change flag byte
                if (callNode.ChildNodes[baseCallNodeIndex].ChildNodes[0].ChildNodes[0].Term.Name == "identifier")
                {
                    if (includeName.StartsWith("hash_"))
                        includeHash = ParseStringToUint(includeName, 5);
                    else
                        includeHash = CanonicalHash(includeName);
                }
            }
            else
                flag = (byte)CallFlags.LocalCall;

            switch (callNode.Term.Name)
            {
                case "scriptFunctionCall":
                    EmitOpcode(OP_ScriptFunctionCall);
                    flag |= (byte)FunctionFlags.FunctionCall;
                    break;

                case "scriptThreadCall":
                    EmitOpcode(OP_ScriptThreadCall);
                    flag |= (byte)FunctionFlags.FunctionThreadCall;
                    break;

                case "scriptMethodCall":
                    EmitOwner(callNode.ChildNodes[0]);
                    EmitOpcode(OP_ScriptMethodCall);
                    flag |= (byte)FunctionFlags.MethodCall;
                    break;

                case "scriptMethodThreadCall":
                    EmitOwner(callNode.ChildNodes[0]);
                    EmitOpcode(OP_ScriptMethodThreadCall);
                    flag |= (byte)FunctionFlags.MethodThreadCall;
                    break;
            }
            
            AddRefToCall(namehash, numofParams, flag, includeHash, namestring);
            CompiledPub.Add(numofParams);
            SetAlignedQWord();
            AddUInt(namehash);
            AddInt(0);
            if (decTop)
            {
                EmitOpcode(OP_DecTop);
            }
        }

        private int GetNumOfParams(ParseTreeNode node)
        {
            int count = 0;
            foreach (ParseTreeNode parameterNode in node.ChildNodes)
            {
                if (parameterNode.Term.Name != "expr" && parameterNode.Term.Name != "expr+")
                    count++;
                else
                    count += GetNumOfParams(parameterNode);
            }
            return count;
        }

        private void EmitFunctionOpcode(ParseTreeNode function)
        {
            if (function.Term.Name == "scriptMethodCall")
            {
                ParseTreeNodeList parametersNode = function.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes;
                parametersNode.Reverse();
                switch (function.ChildNodes[1].ChildNodes[0].Token.ValueString.ToLower())
                {
                    case "waittillmatch": //im pretty sure waittillmatch is the same as waittill
                    case "waittill":
                        parametersNode.Reverse();
                        ScriptCompile(parametersNode[0].ChildNodes[0]);
                        EmitOwner(function.ChildNodes[0]);
                        EmitOpcode(OP_WaitTill);
                        if (parametersNode.Count > 1)
                        {
                            foreach (ParseTreeNode parameter in parametersNode)
                            {
                                ParseWaittillVars(parameter, true);
                            }
                        }
                        EmitOpcode(OP_ClearParams);
                        break;

                    case "notify":
                        EmitOpcode(OP_PreScriptCall);
                        foreach (ParseTreeNode parameter in parametersNode)
                        {
                            ScriptCompile(parameter);
                        }
                        EmitOwner(function.ChildNodes[0]);
                        EmitOpcode(OP_Notify);
                        break;

                    case "endon":
                        foreach (ParseTreeNode parameter in parametersNode)
                        {
                            ScriptCompile(parameter.ChildNodes[0]);
                        }
                        EmitOwner(function.ChildNodes[0]);
                        EmitOpcode(OP_EndOn);
                        break;
                }
            }
            else if (function.Term.Name == "scriptFunctionCall")
            {
                ParseTreeNodeList parametersNode = function.ChildNodes[0].ChildNodes[1].ChildNodes[0].ChildNodes;
                parametersNode.Reverse();
                foreach (ParseTreeNode parameter in parametersNode)
                {
                    ScriptCompile(parameter.ChildNodes[0]);
                }
                switch (function.ChildNodes[0].ChildNodes[0].Token.ValueString.ToLower())
                {
                    case "wait":
                        EmitOpcode(OP_Wait);
                        break;

                    case "isdefined":
                        EmitOpcode(OP_IsDefined);
                        break;

                    case "vectorscale":
                        EmitOpcode(OP_VectorScale);
                        break;

                    case "gettime":
                        EmitOpcode(OP_GetTime);
                        break;

                    case "waittillframeend":
                        EmitOpcode(OP_WaitTillFrameEnd);
                        break;

                    case "getfirstarraykey":
                        EmitOpcode(OP_FirstArrayKey);
                        break;

                    case "getnextarraykey":
                        EmitOpcode(OP_NextArrayKey);
                        break;
                }
            }
        }

        private void EmitCallPointer(ParseTreeNode callNode, bool decTop)
        {
            EmitOpcode(OP_PreScriptCall);
            int baseCallPointerNodeIndex = callNode.ChildNodes.FindIndex(e => e.Term.Name == "baseCallPointer");
            ParseTreeNode parametersNode = callNode.ChildNodes[baseCallPointerNodeIndex].ChildNodes[1].ChildNodes[0];
            byte numofParams = parametersNode != null ? (byte) GetNumOfParams(parametersNode) : (byte) 0;
            if (parametersNode != null)
                parametersNode.ChildNodes.Reverse();
            foreach (ParseTreeNode childNode in parametersNode.ChildNodes) //compile the paramters
            {
                ScriptCompile(childNode);
            }

            switch (callNode.Term.Name)
            {
                case "scriptFunctionCallPointer":
                    ScriptCompile(callNode.ChildNodes[baseCallPointerNodeIndex].ChildNodes[0]);
                    EmitOpcode(OP_ScriptFunctionCallPointer);
                    AddUshort((ushort)numofParams);
                    break;

                case "scriptMethodCallPointer":
                    EmitOwner(callNode.ChildNodes[0]);
                    ScriptCompile(callNode.ChildNodes[baseCallPointerNodeIndex].ChildNodes[0]);
                    EmitOpcode(OP_ScriptMethodCallPointer);
                    AddUshort((ushort)numofParams);
                    break;

                case "scriptThreadCallPointer":
                    ScriptCompile(callNode.ChildNodes[baseCallPointerNodeIndex].ChildNodes[0]);
                    EmitOpcode(OP_ScriptThreadCallPointer);
                    AddUshort((ushort)numofParams);
                    break;

                case "scriptMethodThreadCallPointer":
                    EmitOwner(callNode.ChildNodes[0]);
                    ScriptCompile(callNode.ChildNodes[baseCallPointerNodeIndex].ChildNodes[0]);
                    EmitOpcode(OP_ScriptMethodThreadCallPointer);
                    AddUshort((ushort)numofParams);
                    break;
            }
            if (decTop)
            {
                EmitOpcode(OP_DecTop);
            }
        }

        private void EmitOpcode(ushort opcode)
        {
            CompiledPub.AddRange(BitConverter.GetBytes(opcode).ToArray<byte>());
        }

        private void EmitLocalVariables()
        {
            EmitOpcode(OP_SafeCreateLocalVariables);
            CompiledPub.Add((byte) LocalVariables.Count); //variable count goes at the start
            foreach (string variable in LocalVariables)
            {
                SetAlignedDword();
                AddUInt(CanonicalHash(variable)); //add hash of variable
                CompiledPub.Add(0x00); //1 byte of padding
            }
            CompiledPub.Add(0x00); //1 byte of padding at the end

            LocalVariables.Reverse(); //reverse local variables as the game indexes variables backwards, so the last variable to be declared is 0, the second last is 1 and so on
        }

        private int GetStringPosByName(string str)
        {
            return StringPositionDict[str];
        }

        private bool StringShouldBeWritten(string str)
        {
            return !StringPositionDict.ContainsKey(str);
        }

        private bool IsObjectOwner(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case "true":
                case "false":
                case "undefined":
                case "anim":
                case "game":
                case "self":
                case "level":
                case "world":
                    return true;
                default:
                    return false;
            }
        }

        private bool IsObjectBuiltIn(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case "waittillframeend":
                case "waittillmatch":
                case "waittill":
                case "wait":
                case "endon":
                case "notify":
                case "vectorscale":
                case "getnextarraykey":
                case "getfirstarraykey":
                case "gettime":
                case "isdefined":
                    return true;
                default:
                    return false;
            }
        }

        private void PrepareLocalVariables(ParseTreeNode node)
        {
            foreach (ParseTreeNode childNode in node.ChildNodes)
            {
                switch (childNode.Term.Name)
                {
                    case "setVariableField":
                        if (childNode.ChildNodes[0].ChildNodes[0].Term.Name == "identifier" && !LocalVariables.Contains(childNode.FindTokenAndGetValue().ToLower()))
                            LocalVariables.Add(childNode.FindTokenAndGetValue().ToLower());
                        break;
                    case "foreachStatement":
                        LocalVariables.Add(ArrayKeys[numofForeachStatements].First);
                        LocalVariables.Add(ArrayKeys[numofForeachStatements].Second);
                        numofForeachStatements++;
                        if (!LocalVariables.Contains(childNode.ChildNodes[1].Token.ValueString.ToLower()))
                        {
                            LocalVariables.Add(childNode.ChildNodes[1].Token.ValueString.ToLower());
                        }
                        if ((!LocalVariables.Contains(childNode.ChildNodes[2].Token.ValueString.ToLower())) && childNode.ChildNodes.Count != 5) //for triple foreach
                        {
                            LocalVariables.Add(childNode.ChildNodes[2].Token.ValueString.ToLower());
                        }

                        PrepareLocalVariables(childNode);
                        break;

                    case "switchStatement":
                        LocalVariables.Add(SwitchKeys[numofSwitchStatements]);
                        numofSwitchStatements++;
                        PrepareLocalVariables(childNode);
                        break;

                    case "scriptMethodCall":
                        if (childNode.ChildNodes[1].FindTokenAndGetValue().ToLower() == "waittill")
                        {
                            ParseWaittillVars(childNode.ChildNodes[1].ChildNodes[1].ChildNodes[0]);
                        }
                        break;

                    default:
                        PrepareLocalVariables(childNode);
                        break;
                }
            }
        }

        private void ParseWaittillVars(ParseTreeNode Node, bool EvaluateVar = false)
        {
            foreach (ParseTreeNode parameter in Node.ChildNodes)
            {
                if (parameter.Term.Name == "identifier" && !LocalVariables.Contains(parameter.Token.ValueString.ToLower()) && !EvaluateVar)
                {
                    LocalVariables.Add(parameter.FindTokenAndGetValue().ToLower());
                }
                else if (parameter.Term.Name == "identifier" && EvaluateVar)
                {
                    EvalLocalVariable(parameter.FindTokenAndGetValue(), Node, false, true);
                }
                else
                {
                    ParseWaittillVars(parameter, EvaluateVar);
                }
            }
        }

        private void PrepareStringsAndVariables(ParseTreeNode node)
        {
            foreach (ParseTreeNode childNode in node.ChildNodes)
            {
                switch (childNode.Term.Name)
                {
                    case "stringLiteral":
                        if (!StringShouldBeWritten(childNode.Token.ValueString)) 
                            break; //check if string has been written before
                        AddString(childNode.Token.ValueString);
                        break;

                    case "foreachStatement":
                        int number = RandomNum.Next(1000);
                        string first = string.Format("_a{0}", number);
                        string second = string.Format("_k{0}", number);
                        ArrayKeys.Add(new Key { First = first, Second = second });
                        PrepareStringsAndVariables(childNode);
                        break;

                    case "switchStatement":
                        SwitchKeys.Add(string.Format("_s{0}", RandomNum.Next(1000)));
                        PrepareStringsAndVariables(childNode);
                        break;

                    case "getAnimation":
                        if (!StringShouldBeWritten(childNode.ChildNodes[1].Token.ValueString))
                            break;
                        AddString(childNode.ChildNodes[1].Token.ValueString);
                        break;

                    case "hashedString":
                        break; //stops the string to be hashed from being added

                    default:
                        PrepareStringsAndVariables(childNode);
                        break;
                }
            }
        }

        public class Key //for arrays
        {
            public string First { get; set; }
            public string Second { get; set; }
        }

        public class Operators //for math and boolean expressions
        {
            public string Operator { get; set; }
            public ParseTreeNode EndExpr { get; set; }

            public bool HasBoolNot { get; set; }
        }
    }
}

public static class Extensions
{
    public static void Replace<T>(this List<T> list, int index, List<T> input)
    {
        list.RemoveRange(index, input.Count);
        list.InsertRange(index, input);
    }
}