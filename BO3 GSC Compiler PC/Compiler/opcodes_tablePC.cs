﻿namespace GSCCompilerPC
{
    public partial class ScriptCompiler
    {
        public const ushort OP_Invalid = 0x00;
        public const ushort OP_End = 0x10;
        public const ushort OP_Return = 0xA3;
        public const ushort OP_GetUndefined = 0x179;
        public const ushort OP_GetZero = 0x2A;
        public const ushort OP_GetByte = 0xB4;
        public const ushort OP_GetNegByte = 0x13;
        public const ushort OP_GetUnsignedShort = 0x198;
        public const ushort OP_GetNegUnsignedShort = 0x1FA;
        public const ushort OP_GetInteger = 0x100;
        public const ushort OP_GetFloat = 0x150;
        public const ushort OP_GetString = 0x1C;
        public const ushort OP_GetIString = 0xC9;
        //public const short OP_GetVector = 0x
        public const ushort OP_GetLevelObject = 0x20;
        public const ushort OP_GetAnimObject = 0x23;
        public const ushort OP_GetSelf = 0x146;
        public const ushort OP_GetLevel = 0x8F;
        public const ushort OP_GetGame = 0x1D4;
        public const ushort OP_GetAnim = 0x21C;
        public const ushort OP_GetAnimation = 0x420;
        public const ushort OP_GetGameRef = 0x510;
        public const ushort OP_GetFunction = 0x514;
        //public const short OP_CreateLocalVariable = 0x
        public const ushort OP_SafeCreateLocalVariables = 0x1D2;
        //public const short OP_RemoveLocalVariables = 0x
        public const ushort OP_EvalLocalVariableCached = 0x57;
        public const ushort OP_EvalArray = 0xA7;
        //public const short OP_EvalLocalArrayRefCached = 0x
        public const ushort OP_EvalArrayRef = 0x79;
        public const ushort OP_ClearArray = 0x19;
        public const ushort OP_GetEmptyArray = 0xFE;
        public const ushort OP_GetSelfObject = 0x31;
        public const ushort OP_EvalFieldVariable = 0x84;
        public const ushort OP_EvalFieldVariableRef = 0x11;
        public const ushort OP_ClearFieldVariable = 0x8C;
        //public const short OP_SafeSetVariableFieldCached = 0x
        public const ushort OP_SetWaittillVariableFieldCached = 0x126;
        public const ushort OP_ClearParams = 0x0C;
        public const ushort OP_CheckClearParams = 0x0D;
        public const ushort OP_EvalLocalVariableRefCached = 0x194;
        public const ushort OP_SetVariableField = 0x110;
        //public const short OP_CallBuiltin = 0x
        //public const short OP_CallBuiltinMethod = 0x
        public const ushort OP_Wait = 0xD5;
        public const ushort OP_WaitTillFrameEnd = 0xF0;
        public const ushort OP_PreScriptCall = 0x0E;
        public const ushort OP_ScriptFunctionCall = 0x76;
        public const ushort OP_ScriptFunctionCallPointer = 0x3A;
        public const ushort OP_ScriptMethodCall = 0x1B;
        public const ushort OP_ScriptMethodCallPointer = 0x77;
        public const ushort OP_ScriptThreadCall = 0x1E5;
        public const ushort OP_ScriptThreadCallPointer = 0xA0;
        public const ushort OP_ScriptMethodThreadCall = 0x26E;
        public const ushort OP_ScriptMethodThreadCallPointer = 0x377;
        public const ushort OP_DecTop = 0x112;
        public const ushort OP_CastFieldObject = 0x3F;
        //public const short OP_CastBool = 0x
        public const ushort OP_BoolNot = 0x105;
        //public const short OP_BoolComplement = 0x
        public const ushort OP_JumpOnFalse = 0x96;
        public const ushort OP_JumpOnTrue = 0x5F;
        public const ushort OP_JumpOnFalseExpr = 0xC0;
        public const ushort OP_JumpOnTrueExpr = 0xF2;
        public const ushort OP_Jump = 0x22;
        //public const short OP_JumpBack = 0x
        public const ushort OP_Inc = 0x61;
        public const ushort OP_Dec = 0x95;
        public const ushort OP_Bit_Or = 0xAB;
        public const ushort OP_Bit_Xor = 0xAD;
        public const ushort OP_Bit_And = 0x20A;
        public const ushort OP_Equal = 0x149;
        public const ushort OP_NotEqual = 0x2DA;
        public const ushort OP_LessThan = 0x47;
        public const ushort OP_GreaterThan = 0x49;
        public const ushort OP_LessThanOrEqualTo = 0xF6;
        public const ushort OP_GreaterThanOrEqualTo = 0x1B6;
        public const ushort OP_ShiftLeft = 0x18;
        public const ushort OP_ShiftRight = 0x4BD;
        public const ushort OP_Plus = 0x191;
        public const ushort OP_Minus = 0x1B7;
        public const ushort OP_Multiply = 0xD9;
        public const ushort OP_Divide = 0x1BA;
        public const ushort OP_Modulus = 0x4DB;
        public const ushort OP_SizeOf = 0x24;
        public const ushort OP_WaitTillMatch = 0x4FE;
        public const ushort OP_WaitTill = 0x2B2;
        public const ushort OP_Notify = 0x46;
        public const ushort OP_EndOn = 0x8B;
        //public const short OP_VoidCodePos = 0x
        public const ushort OP_Switch = 0x6B;
        public const ushort OP_EndSwitch = 0x15;
        public const ushort OP_Vector = 0xB7;
        public const ushort OP_GetHash = 0x108;
        //public const short OP_RealWait = 0x
        public const ushort OP_VectorConstant = 0x10E;
        public const ushort OP_IsDefined = 0x70;
        public const ushort OP_VectorScale = 0x103;
        //public const short OP_AnglesToUp = 0x
        //public const short OP_AnglesToRight = 0x
        //public const short OP_AnglesToForward = 0x
        //public const short OP_AngleClamp180 = 0x
        //public const short OP_VectorToAngles = 0x
        //public const short OP_Abs = 0x
        public const ushort OP_GetTime = 0x117;
        //public const short OP_GetDvar = 0x
        //public const short OP_GetDvarInt = 0x
        //public const short OP_GetDvarFloat = 0x
        //public const short OP_GetDvarVector = 0x
        //public const short OP_GetDvarColorRed = 0x
        //public const short OP_GetDvarColorGreen = 0x
        //public const short OP_GetDvarColorBlue = 0x
        //public const short OP_GetDvarColorAlpha = 0x
        public const ushort OP_FirstArrayKey = 0xB2;
        public const ushort OP_NextArrayKey = 0x25;
        //public const short OP_ProfileStart = 0x
        //public const short OP_ProfileStop = 0x
        //public const short OP_SafeDecTop = 0x
        //public const short OP_Nop = 0x
        //public const short OP_Abort = 0x
        //public const short OP_Obj = 0x
        //public const short OP_ThreadObject = 0x
        //public const short OP_EvalLocalVariable = 0x
        //public const short OP_EvalLocalVariableRef = 0x
        public const ushort OP_DevblockBegin = 0x64;
        //public const short OP_DevblockEnd = 0x
        //public const short OP_Breakpoint = 0x
        //public const short OP_AutoBreakpoint = 0x
        //public const short OP_ErrorBreakpoint = 0x
        //public const short OP_WatchBreakpoint = 0x
        //public const short OP_NotifyBreakpoint = 0x
        public const ushort OP_GetObjectType = 0x30;
        public const ushort OP_WaitRealTime = 0x104;
        public const ushort OP_GetWorldObject = 0xA2;
        public const ushort OP_GetClassesObject = 0xA6;
        public const ushort OP_ClassFunctionCall = 0x3E;
        public const ushort OP_Bit_Not = 0x7A;
        public const ushort OP_GetWorld = 0x42;
        public const ushort OP_EvalLevelFieldVariable = 0x27;
        public const ushort OP_EvalLevelFieldVariableRef = 0x242;
        public const ushort OP_EvalSelfFieldVariable = 0xCC;
        public const ushort OP_EvalSelfFieldVariableRef = 0x109;
        public const ushort OP_SuperEqual = 0x6C;
        public const ushort OP_SuperNotEqual = 0xDC;
        //public const short OP_Count = 0x
    }
}