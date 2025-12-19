/*
  Copyright (C) 2002, 2004, 2005, 2006 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/

using IKVM.ByteCode;

namespace IKVM.CoreLib.Linking
{

    internal readonly struct OpCodeMetaData
    {

        readonly static OpCodeMetaData[] _data = new OpCodeMetaData[256];

        readonly NormalizedOpCode _normalizedOpCode;
        readonly OpCodeArgumentKind _argKind;
        readonly OpCodeFlags _flags;
        readonly int _arg0Value;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="argKind"></param>
        /// <param name="cannotThrow"></param>
        OpCodeMetaData(OpCode opcode, OpCodeArgumentKind argKind, bool cannotThrow)
        {
            _normalizedOpCode = (NormalizedOpCode)opcode;
            _argKind = argKind;
            _arg0Value = 0;
            _flags = OpCodeFlags.None;
            if (cannotThrow)
                _flags |= OpCodeFlags.CannotThrow;

            _data[(int)opcode] = this;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="normbc"></param>
        /// <param name="argKind"></param>
        /// <param name="cannotThrow"></param>
        OpCodeMetaData(OpCode opcode, NormalizedOpCode normbc, OpCodeArgumentKind argKind, bool cannotThrow)
        {
            _normalizedOpCode = normbc;
            _argKind = argKind;
            _arg0Value = 0;
            _flags = OpCodeFlags.None;
            if (cannotThrow)
                _flags |= OpCodeFlags.CannotThrow;

            _data[(int)opcode] = this;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="normbc"></param>
        /// <param name="arg0Value"></param>
        /// <param name="argKind"></param>
        /// <param name="cannotThrow"></param>
        OpCodeMetaData(OpCode opcode, NormalizedOpCode normbc, int arg0Value, OpCodeArgumentKind argKind, bool cannotThrow)
        {
            _normalizedOpCode = normbc;
            _argKind = argKind;
            _arg0Value = arg0Value;
            _flags = OpCodeFlags.FixedArg;
            if (cannotThrow)
                _flags |= OpCodeFlags.CannotThrow;

            _data[(int)opcode] = this;
        }

        internal static NormalizedOpCode GetNormalizedByteCode(OpCode opcode)
        {
            return _data[(int)opcode]._normalizedOpCode;
        }

        internal static int GetArg0Value(OpCode bc, int arg)
        {
            if ((_data[(int)bc]._flags & OpCodeFlags.FixedArg) != 0)
                return _data[(int)bc]._arg0Value;

            return arg;
        }

        internal static OpCodeFlowKind GetFlowKind(NormalizedOpCode opcode)
        {
            switch (opcode)
            {
                case NormalizedOpCode.TableSwitch:
                case NormalizedOpCode.LookupSwitch:
                    return OpCodeFlowKind.Switch;

                case NormalizedOpCode.Goto:
                case NormalizedOpCode.GotoFinally:
                    return OpCodeFlowKind.Branch;

                case NormalizedOpCode.Ifeq:
                case NormalizedOpCode.Ifne:
                case NormalizedOpCode.Iflt:
                case NormalizedOpCode.Ifge:
                case NormalizedOpCode.Ifgt:
                case NormalizedOpCode.Ifle:
                case NormalizedOpCode.IfIcmpeq:
                case NormalizedOpCode.IfIcmpne:
                case NormalizedOpCode.IfIcmplt:
                case NormalizedOpCode.IfIcmpge:
                case NormalizedOpCode.IfIcmpgt:
                case NormalizedOpCode.IfIcmple:
                case NormalizedOpCode.IfAcmpeq:
                case NormalizedOpCode.IfAcmpne:
                case NormalizedOpCode.IfNull:
                case NormalizedOpCode.IfNonNull:
                    return OpCodeFlowKind.ConditionalBranch;

                case NormalizedOpCode.Ireturn:
                case NormalizedOpCode.Lreturn:
                case NormalizedOpCode.Freturn:
                case NormalizedOpCode.Dreturn:
                case NormalizedOpCode.Areturn:
                case NormalizedOpCode.Return:
                    return OpCodeFlowKind.Return;

                case NormalizedOpCode.Athrow:
                case NormalizedOpCode.AthrowNoUnmap:
                case NormalizedOpCode.StaticError:
                    return OpCodeFlowKind.Throw;

                default:
                    return OpCodeFlowKind.Next;
            }
        }

        internal static bool CanThrowException(NormalizedOpCode opcode)
        {
            switch (opcode)
            {
                case NormalizedOpCode.DynamicInvokeInterface:
                case NormalizedOpCode.DynamicInvokeStatic:
                case NormalizedOpCode.DynamicInvokeVirtual:
                case NormalizedOpCode.DynamicGetStatic:
                case NormalizedOpCode.DynamicPutStatic:
                case NormalizedOpCode.DynamicGetField:
                case NormalizedOpCode.DynamicPutField:
                case NormalizedOpCode.CloneArray:
                case NormalizedOpCode.StaticError:
                case NormalizedOpCode.MethodHandleInvoke:
                case NormalizedOpCode.MethodHandleLink:
                    return true;
                case NormalizedOpCode.Iconst:
                case NormalizedOpCode.LdcNothrow:
                    return false;
                default:
                    return (_data[(int)opcode]._flags & OpCodeFlags.CannotThrow) == 0;
            }
        }

        internal static bool IsBranch(NormalizedOpCode opcode)
        {
            switch (_data[(int)opcode]._argKind)
            {
                case OpCodeArgumentKind.Branch2:
                case OpCodeArgumentKind.Branch4:
                case OpCodeArgumentKind.LookupSwitch:
                case OpCodeArgumentKind.TableSwitch:
                    return true;
                default:
                    return false;
            }
        }

        static OpCodeMetaData()
        {
            new OpCodeMetaData(OpCode.Nop, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.AconstNull, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.IconstM1, NormalizedOpCode.Iconst, -1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iconst0, NormalizedOpCode.Iconst, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iconst1, NormalizedOpCode.Iconst, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iconst2, NormalizedOpCode.Iconst, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iconst3, NormalizedOpCode.Iconst, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iconst4, NormalizedOpCode.Iconst, 4, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iconst5, NormalizedOpCode.Iconst, 5, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lconst0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lconst1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fconst0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fconst1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fconst2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dconst0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dconst1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Bipush, NormalizedOpCode.Iconst, OpCodeArgumentKind.ImmediateS1, true);
            new OpCodeMetaData(OpCode.Sipush, NormalizedOpCode.Iconst, OpCodeArgumentKind.ImmediateS2, true);
            new OpCodeMetaData(OpCode.Ldc, OpCodeArgumentKind.Constant1, false);
            new OpCodeMetaData(OpCode.LdcW, NormalizedOpCode.Ldc, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.Ldc2W, NormalizedOpCode.Ldc, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.Iload, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Lload, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Fload, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Dload, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Aload, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Iload0, NormalizedOpCode.Iload, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iload1, NormalizedOpCode.Iload, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iload2, NormalizedOpCode.Iload, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iload3, NormalizedOpCode.Iload, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lload0, NormalizedOpCode.Lload, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lload1, NormalizedOpCode.Lload, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lload2, NormalizedOpCode.Lload, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lload3, NormalizedOpCode.Lload, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fload0, NormalizedOpCode.Fload, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fload1, NormalizedOpCode.Fload, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fload2, NormalizedOpCode.Fload, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fload3, NormalizedOpCode.Fload, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dload0, NormalizedOpCode.Dload, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dload1, NormalizedOpCode.Dload, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dload2, NormalizedOpCode.Dload, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dload3, NormalizedOpCode.Dload, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Aload0, NormalizedOpCode.Aload, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Aload1, NormalizedOpCode.Aload, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Aload2, NormalizedOpCode.Aload, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Aload3, NormalizedOpCode.Aload, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iaload, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Laload, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Faload, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Daload, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Aaload, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Baload, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Caload, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Saload, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Istore, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Lstore, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Fstore, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Dstore, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Astore, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.Istore0, NormalizedOpCode.Istore, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Istore1, NormalizedOpCode.Istore, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Istore2, NormalizedOpCode.Istore, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Istore3, NormalizedOpCode.Istore, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lstore0, NormalizedOpCode.Lstore, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lstore1, NormalizedOpCode.Lstore, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lstore2, NormalizedOpCode.Lstore, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lstore3, NormalizedOpCode.Lstore, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fstore0, NormalizedOpCode.Fstore, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fstore1, NormalizedOpCode.Fstore, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fstore2, NormalizedOpCode.Fstore, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fstore3, NormalizedOpCode.Fstore, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dstore0, NormalizedOpCode.Dstore, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dstore1, NormalizedOpCode.Dstore, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dstore2, NormalizedOpCode.Dstore, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dstore3, NormalizedOpCode.Dstore, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Astore0, NormalizedOpCode.Astore, 0, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Astore1, NormalizedOpCode.Astore, 1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Astore2, NormalizedOpCode.Astore, 2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Astore3, NormalizedOpCode.Astore, 3, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iastore, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Lastore, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Fastore, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Dastore, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Aastore, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Bastore, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Castore, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Sastore, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Pop, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Pop2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dup, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.DupX1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.DupX2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dup2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dup2X1, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dup2X2, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Swap, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iadd, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Ladd, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fadd, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dadd, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Isub, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lsub, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fsub, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dsub, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Imul, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lmul, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fmul, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dmul, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Idiv, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Ldiv, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Fdiv, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Ddiv, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Irem, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Lrem, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Frem, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Drem, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Ineg, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lneg, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fneg, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dneg, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Ishl, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lshl, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Ishr, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lshr, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iushr, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lushr, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iand, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Land, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Ior, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lor, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Ixor, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lxor, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Iinc, OpCodeArgumentKind.Local1_ImmediateS1, true);
            new OpCodeMetaData(OpCode.I2l, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.I2f, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.I2d, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.L2i, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.L2f, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.L2d, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.F2i, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.F2l, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.F2d, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.D2i, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.D2l, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.D2f, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.I2b, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.I2c, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.I2s, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lcmp, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fcmpl, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Fcmpg, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dcmpl, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dcmpg, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Ifeq, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.Ifne, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.Iflt, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.Ifge, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.Ifgt, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.Ifle, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfIcmpeq, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfIcmpne, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfIcmplt, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfIcmpge, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfIcmpgt, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfIcmple, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfAcmpeq, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfAcmpne, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.Goto, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.Jsr, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.Ret, OpCodeArgumentKind.Local1, true);
            new OpCodeMetaData(OpCode.TableSwitch, OpCodeArgumentKind.TableSwitch, true);
            new OpCodeMetaData(OpCode.LookupSwitch, OpCodeArgumentKind.LookupSwitch, true);
            new OpCodeMetaData(OpCode.Ireturn, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Lreturn, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Freturn, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Dreturn, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Areturn, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.Return, OpCodeArgumentKind.Simple, true);
            new OpCodeMetaData(OpCode.GetStatic, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.PutStatic, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.GetField, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.PutField, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.InvokeVirtual, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.InvokeSpecial, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.InvokeStatic, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.InvokeInterface, OpCodeArgumentKind.Constant2_Count1_Zero1, false);
            new OpCodeMetaData(OpCode.InvokeDynamic, OpCodeArgumentKind.Constant2_Count1_Zero1, false);
            new OpCodeMetaData(OpCode.New, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.Newarray, OpCodeArgumentKind.ImmediateU1, false);
            new OpCodeMetaData(OpCode.Anewarray, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.Arraylength, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Athrow, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Checkcast, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.InstanceOf, OpCodeArgumentKind.Constant2, false);
            new OpCodeMetaData(OpCode.MonitorEnter, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.MonitorExit, OpCodeArgumentKind.Simple, false);
            new OpCodeMetaData(OpCode.Wide, NormalizedOpCode.Nop, OpCodeArgumentKind.WidePrefix, true);
            new OpCodeMetaData(OpCode.Multianewarray, OpCodeArgumentKind.Constant2_ImmediateU1, false);
            new OpCodeMetaData(OpCode.IfNull, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.IfNonNull, OpCodeArgumentKind.Branch2, true);
            new OpCodeMetaData(OpCode.GotoW, NormalizedOpCode.Goto, OpCodeArgumentKind.Branch4, true);
            new OpCodeMetaData(OpCode.JsrW, NormalizedOpCode.Jsr, OpCodeArgumentKind.Branch4, true);

        }

    }

}
