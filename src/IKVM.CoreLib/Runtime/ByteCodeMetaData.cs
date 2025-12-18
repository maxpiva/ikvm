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

namespace IKVM.CoreLib.Runtime
{

    internal struct ByteCodeMetaData
    {

        private static ByteCodeMetaData[] data = new ByteCodeMetaData[256];
        private OpCodeArgumentKind reg;
        private OpCodeArgumentKind? wide;
        private NormalizedByteCode normbc;
        private ByteCodeFlags flags;
        private int arg;

        private ByteCodeMetaData(OpCode bc, OpCodeArgumentKind reg, OpCodeArgumentKind? wide, bool cannotThrow)
        {
            this.reg = reg;
            this.wide = wide;
            this.normbc = (NormalizedByteCode)bc;
            this.arg = 0;
            this.flags = ByteCodeFlags.None;
            if (cannotThrow)
            {
                this.flags |= ByteCodeFlags.CannotThrow;
            }
            data[(int)bc] = this;
        }

        private ByteCodeMetaData(OpCode bc, NormalizedByteCode normbc, OpCodeArgumentKind reg, OpCodeArgumentKind? wide, bool cannotThrow)
        {
            this.reg = reg;
            this.wide = wide;
            this.normbc = normbc;
            this.arg = 0;
            this.flags = ByteCodeFlags.None;
            if (cannotThrow)
            {
                this.flags |= ByteCodeFlags.CannotThrow;
            }
            data[(int)bc] = this;
        }

        private ByteCodeMetaData(OpCode bc, NormalizedByteCode normbc, int arg, OpCodeArgumentKind reg, OpCodeArgumentKind? wide, bool cannotThrow)
        {
            this.reg = reg;
            this.wide = wide;
            this.normbc = normbc;
            this.arg = arg;
            this.flags = ByteCodeFlags.FixedArg;
            if (cannotThrow)
            {
                this.flags |= ByteCodeFlags.CannotThrow;
            }
            data[(int)bc] = this;
        }

        internal static NormalizedByteCode GetNormalizedByteCode(OpCode bc)
        {
            return data[(int)bc].normbc;
        }

        internal static int GetArg(OpCode bc, int arg)
        {
            if ((data[(int)bc].flags & ByteCodeFlags.FixedArg) != 0)
            {
                return data[(int)bc].arg;
            }
            return arg;
        }

        internal static OpCodeArgumentKind GetMode(OpCode bc)
        {
            return data[(int)bc].reg;
        }

        internal static OpCodeArgumentKind? GetWideMode(OpCode bc)
        {
            return data[(int)bc].wide;
        }

        internal static OpCodeFlowKind GetFlowControl(NormalizedByteCode bc)
        {
            switch (bc)
            {
                case NormalizedByteCode.__tableswitch:
                case NormalizedByteCode.__lookupswitch:
                    return OpCodeFlowKind.Switch;

                case NormalizedByteCode.__goto:
                case NormalizedByteCode.__goto_finally:
                    return OpCodeFlowKind.Branch;

                case NormalizedByteCode.__ifeq:
                case NormalizedByteCode.__ifne:
                case NormalizedByteCode.__iflt:
                case NormalizedByteCode.__ifge:
                case NormalizedByteCode.__ifgt:
                case NormalizedByteCode.__ifle:
                case NormalizedByteCode.__if_icmpeq:
                case NormalizedByteCode.__if_icmpne:
                case NormalizedByteCode.__if_icmplt:
                case NormalizedByteCode.__if_icmpge:
                case NormalizedByteCode.__if_icmpgt:
                case NormalizedByteCode.__if_icmple:
                case NormalizedByteCode.__if_acmpeq:
                case NormalizedByteCode.__if_acmpne:
                case NormalizedByteCode.__ifnull:
                case NormalizedByteCode.__ifnonnull:
                    return OpCodeFlowKind.ConditionalBranch;

                case NormalizedByteCode.__ireturn:
                case NormalizedByteCode.__lreturn:
                case NormalizedByteCode.__freturn:
                case NormalizedByteCode.__dreturn:
                case NormalizedByteCode.__areturn:
                case NormalizedByteCode.__return:
                    return OpCodeFlowKind.Return;

                case NormalizedByteCode.__athrow:
                case NormalizedByteCode.__athrow_no_unmap:
                case NormalizedByteCode.__static_error:
                    return OpCodeFlowKind.Throw;

                default:
                    return OpCodeFlowKind.Next;
            }
        }

        internal static bool CanThrowException(NormalizedByteCode bc)
        {
            switch (bc)
            {
                case NormalizedByteCode.__dynamic_invokeinterface:
                case NormalizedByteCode.__dynamic_invokestatic:
                case NormalizedByteCode.__dynamic_invokevirtual:
                case NormalizedByteCode.__dynamic_getstatic:
                case NormalizedByteCode.__dynamic_putstatic:
                case NormalizedByteCode.__dynamic_getfield:
                case NormalizedByteCode.__dynamic_putfield:
                case NormalizedByteCode.__clone_array:
                case NormalizedByteCode.__static_error:
                case NormalizedByteCode.__methodhandle_invoke:
                case NormalizedByteCode.__methodhandle_link:
                    return true;
                case NormalizedByteCode.__iconst:
                case NormalizedByteCode.__ldc_nothrow:
                    return false;
                default:
                    return (data[(int)bc].flags & ByteCodeFlags.CannotThrow) == 0;
            }
        }

        internal static bool IsBranch(NormalizedByteCode bc)
        {
            switch (data[(int)bc].reg)
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

        static ByteCodeMetaData()
        {
            new ByteCodeMetaData(OpCode.Nop, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.AconstNull, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.IconstM1, NormalizedByteCode.__iconst, -1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iconst0, NormalizedByteCode.__iconst, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iconst1, NormalizedByteCode.__iconst, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iconst2, NormalizedByteCode.__iconst, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iconst3, NormalizedByteCode.__iconst, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iconst4, NormalizedByteCode.__iconst, 4, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iconst5, NormalizedByteCode.__iconst, 5, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lconst0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lconst1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fconst0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fconst1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fconst2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dconst0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dconst1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Bipush, NormalizedByteCode.__iconst, OpCodeArgumentKind.ImmediateS1, null, true);
            new ByteCodeMetaData(OpCode.Sipush, NormalizedByteCode.__iconst, OpCodeArgumentKind.ImmediateS2, null, true);
            new ByteCodeMetaData(OpCode.Ldc, OpCodeArgumentKind.Constant1, null, false);
            new ByteCodeMetaData(OpCode.LdcW, NormalizedByteCode.__ldc, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.Ldc2W, NormalizedByteCode.__ldc, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.Iload, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Lload, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Fload, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Dload, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Aload, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Iload0, NormalizedByteCode.__iload, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iload1, NormalizedByteCode.__iload, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iload2, NormalizedByteCode.__iload, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iload3, NormalizedByteCode.__iload, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lload0, NormalizedByteCode.__lload, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lload1, NormalizedByteCode.__lload, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lload2, NormalizedByteCode.__lload, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lload3, NormalizedByteCode.__lload, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fload0, NormalizedByteCode.__fload, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fload1, NormalizedByteCode.__fload, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fload2, NormalizedByteCode.__fload, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fload3, NormalizedByteCode.__fload, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dload0, NormalizedByteCode.__dload, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dload1, NormalizedByteCode.__dload, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dload2, NormalizedByteCode.__dload, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dload3, NormalizedByteCode.__dload, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Aload0, NormalizedByteCode.__aload, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Aload1, NormalizedByteCode.__aload, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Aload2, NormalizedByteCode.__aload, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Aload3, NormalizedByteCode.__aload, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iaload, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Laload, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Faload, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Daload, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Aaload, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Baload, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Caload, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Saload, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Istore, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Lstore, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Fstore, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Dstore, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Astore, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.Istore0, NormalizedByteCode.__istore, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Istore1, NormalizedByteCode.__istore, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Istore2, NormalizedByteCode.__istore, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Istore3, NormalizedByteCode.__istore, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lstore0, NormalizedByteCode.__lstore, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lstore1, NormalizedByteCode.__lstore, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lstore2, NormalizedByteCode.__lstore, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lstore3, NormalizedByteCode.__lstore, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fstore0, NormalizedByteCode.__fstore, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fstore1, NormalizedByteCode.__fstore, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fstore2, NormalizedByteCode.__fstore, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fstore3, NormalizedByteCode.__fstore, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dstore0, NormalizedByteCode.__dstore, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dstore1, NormalizedByteCode.__dstore, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dstore2, NormalizedByteCode.__dstore, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dstore3, NormalizedByteCode.__dstore, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Astore0, NormalizedByteCode.__astore, 0, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Astore1, NormalizedByteCode.__astore, 1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Astore2, NormalizedByteCode.__astore, 2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Astore3, NormalizedByteCode.__astore, 3, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iastore, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Lastore, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Fastore, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Dastore, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Aastore, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Bastore, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Castore, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Sastore, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Pop, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Pop2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dup, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.DupX1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.DupX2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dup2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dup2X1, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dup2X2, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Swap, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iadd, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Ladd, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fadd, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dadd, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Isub, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lsub, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fsub, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dsub, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Imul, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lmul, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fmul, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dmul, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Idiv, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Ldiv, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Fdiv, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Ddiv, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Irem, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Lrem, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Frem, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Drem, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Ineg, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lneg, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fneg, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dneg, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Ishl, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lshl, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Ishr, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lshr, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iushr, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lushr, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iand, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Land, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Ior, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lor, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Ixor, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lxor, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Iinc, OpCodeArgumentKind.Local1_ImmediateS1, OpCodeArgumentKind.Local2_ImmediateS2, true);
            new ByteCodeMetaData(OpCode.I2l, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.I2f, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.I2d, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.L2i, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.L2f, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.L2d, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.F2i, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.F2l, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.F2d, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.D2i, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.D2l, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.D2f, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.I2b, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.I2c, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.I2s, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lcmp, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fcmpl, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Fcmpg, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dcmpl, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dcmpg, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Ifeq, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.Ifne, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.Iflt, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.Ifge, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.Ifgt, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.Ifle, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfIcmpeq, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfIcmpne, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfIcmplt, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfIcmpge, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfIcmpgt, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfIcmple, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfAcmpeq, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfAcmpne, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.Goto, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.Jsr, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.Ret, OpCodeArgumentKind.Local1, OpCodeArgumentKind.Local2, true);
            new ByteCodeMetaData(OpCode.TableSwitch, OpCodeArgumentKind.TableSwitch, null, true);
            new ByteCodeMetaData(OpCode.LookupSwitch, OpCodeArgumentKind.LookupSwitch, null, true);
            new ByteCodeMetaData(OpCode.Ireturn, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Lreturn, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Freturn, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Dreturn, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Areturn, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.Return, OpCodeArgumentKind.Simple, null, true);
            new ByteCodeMetaData(OpCode.GetStatic, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.PutStatic, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.GetField, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.PutField, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.InvokeVirtual, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.InvokeSpecial, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.InvokeStatic, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.InvokeInterface, OpCodeArgumentKind.Constant2_Count1_Zero1, null, false);
            new ByteCodeMetaData(OpCode.InvokeDynamic, OpCodeArgumentKind.Constant2_Count1_Zero1, null, false);
            new ByteCodeMetaData(OpCode.New, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.Newarray, OpCodeArgumentKind.ImmediateU1, null, false);
            new ByteCodeMetaData(OpCode.Anewarray, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.Arraylength, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Athrow, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Checkcast, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.InstanceOf, OpCodeArgumentKind.Constant2, null, false);
            new ByteCodeMetaData(OpCode.MonitorEnter, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.MonitorExit, OpCodeArgumentKind.Simple, null, false);
            new ByteCodeMetaData(OpCode.Wide, NormalizedByteCode.__nop, OpCodeArgumentKind.WidePrefix, null, true);
            new ByteCodeMetaData(OpCode.Multianewarray, OpCodeArgumentKind.Constant2_ImmediateU1, null, false);
            new ByteCodeMetaData(OpCode.IfNull, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.IfNonNull, OpCodeArgumentKind.Branch2, null, true);
            new ByteCodeMetaData(OpCode.GotoW, NormalizedByteCode.__goto, OpCodeArgumentKind.Branch4, null, true);
            new ByteCodeMetaData(OpCode.JsrW, NormalizedByteCode.__jsr, OpCodeArgumentKind.Branch4, null, true);

        }

    }

}
