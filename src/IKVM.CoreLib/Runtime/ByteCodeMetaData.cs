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
        private ByteCodeMode reg;
        private ByteCodeModeWide wide;
        private NormalizedByteCode normbc;
        private ByteCodeFlags flags;
        private int arg;

        private ByteCodeMetaData(OpCode bc, ByteCodeMode reg, ByteCodeModeWide wide, bool cannotThrow)
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

        private ByteCodeMetaData(OpCode bc, NormalizedByteCode normbc, ByteCodeMode reg, ByteCodeModeWide wide, bool cannotThrow)
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

        private ByteCodeMetaData(OpCode bc, NormalizedByteCode normbc, int arg, ByteCodeMode reg, ByteCodeModeWide wide, bool cannotThrow)
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

        internal static ByteCodeMode GetMode(OpCode bc)
        {
            return data[(int)bc].reg;
        }

        internal static ByteCodeModeWide GetWideMode(OpCode bc)
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
                case ByteCodeMode.Branch_2:
                case ByteCodeMode.Branch_4:
                case ByteCodeMode.Lookupswitch:
                case ByteCodeMode.Tableswitch:
                    return true;
                default:
                    return false;
            }
        }

        static ByteCodeMetaData()
        {
            new ByteCodeMetaData(OpCode.Nop, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.AconstNull, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IconstM1, NormalizedByteCode.__iconst, -1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iconst0, NormalizedByteCode.__iconst, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iconst1, NormalizedByteCode.__iconst, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iconst2, NormalizedByteCode.__iconst, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iconst3, NormalizedByteCode.__iconst, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iconst4, NormalizedByteCode.__iconst, 4, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iconst5, NormalizedByteCode.__iconst, 5, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lconst0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lconst1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fconst0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fconst1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fconst2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dconst0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dconst1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Bipush, NormalizedByteCode.__iconst, ByteCodeMode.Immediate_1, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Sipush, NormalizedByteCode.__iconst, ByteCodeMode.Immediate_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ldc, ByteCodeMode.Constant_1, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.LdcW, NormalizedByteCode.__ldc, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Ldc2W, NormalizedByteCode.__ldc, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Iload, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Lload, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Fload, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Dload, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Aload, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Iload0, NormalizedByteCode.__iload, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iload1, NormalizedByteCode.__iload, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iload2, NormalizedByteCode.__iload, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iload3, NormalizedByteCode.__iload, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lload0, NormalizedByteCode.__lload, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lload1, NormalizedByteCode.__lload, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lload2, NormalizedByteCode.__lload, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lload3, NormalizedByteCode.__lload, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fload0, NormalizedByteCode.__fload, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fload1, NormalizedByteCode.__fload, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fload2, NormalizedByteCode.__fload, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fload3, NormalizedByteCode.__fload, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dload0, NormalizedByteCode.__dload, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dload1, NormalizedByteCode.__dload, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dload2, NormalizedByteCode.__dload, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dload3, NormalizedByteCode.__dload, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Aload0, NormalizedByteCode.__aload, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Aload1, NormalizedByteCode.__aload, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Aload2, NormalizedByteCode.__aload, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Aload3, NormalizedByteCode.__aload, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iaload, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Laload, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Faload, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Daload, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Aaload, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Baload, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Caload, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Saload, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Istore, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Lstore, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Fstore, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Dstore, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Astore, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.Istore0, NormalizedByteCode.__istore, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Istore1, NormalizedByteCode.__istore, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Istore2, NormalizedByteCode.__istore, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Istore3, NormalizedByteCode.__istore, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lstore0, NormalizedByteCode.__lstore, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lstore1, NormalizedByteCode.__lstore, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lstore2, NormalizedByteCode.__lstore, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lstore3, NormalizedByteCode.__lstore, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fstore0, NormalizedByteCode.__fstore, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fstore1, NormalizedByteCode.__fstore, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fstore2, NormalizedByteCode.__fstore, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fstore3, NormalizedByteCode.__fstore, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dstore0, NormalizedByteCode.__dstore, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dstore1, NormalizedByteCode.__dstore, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dstore2, NormalizedByteCode.__dstore, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dstore3, NormalizedByteCode.__dstore, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Astore0, NormalizedByteCode.__astore, 0, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Astore1, NormalizedByteCode.__astore, 1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Astore2, NormalizedByteCode.__astore, 2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Astore3, NormalizedByteCode.__astore, 3, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iastore, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Lastore, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Fastore, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Dastore, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Aastore, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Bastore, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Castore, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Sastore, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Pop, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Pop2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dup, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.DupX1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.DupX2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dup2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dup2X1, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dup2X2, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Swap, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iadd, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ladd, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fadd, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dadd, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Isub, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lsub, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fsub, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dsub, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Imul, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lmul, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fmul, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dmul, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Idiv, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Ldiv, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Fdiv, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ddiv, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Irem, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Lrem, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Frem, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Drem, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ineg, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lneg, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fneg, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dneg, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ishl, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lshl, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ishr, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lshr, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iushr, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lushr, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iand, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Land, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ior, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lor, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ixor, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lxor, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iinc, ByteCodeMode.Local_1_Immediate_1, ByteCodeModeWide.Local_2_Immediate_2, true);
            new ByteCodeMetaData(OpCode.I2l, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.I2f, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.I2d, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.L2i, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.L2f, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.L2d, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.F2i, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.F2l, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.F2d, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.D2i, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.D2l, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.D2f, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.I2b, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.I2c, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.I2s, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lcmp, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fcmpl, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Fcmpg, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dcmpl, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dcmpg, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ifeq, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ifne, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Iflt, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ifge, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ifgt, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ifle, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfIcmpeq, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfIcmpne, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfIcmplt, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfIcmpge, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfIcmpgt, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfIcmple, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfAcmpeq, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfAcmpne, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Goto, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Jsr, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ret, ByteCodeMode.Local_1, ByteCodeModeWide.Local_2, true);
            new ByteCodeMetaData(OpCode.TableSwitch, ByteCodeMode.Tableswitch, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.LookupSwitch, ByteCodeMode.Lookupswitch, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Ireturn, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Lreturn, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Freturn, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Dreturn, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Areturn, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Return, ByteCodeMode.Simple, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.GetStatic, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.PutStatic, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.GetField, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.PutField, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.InvokeVirtual, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.InvokeSpecial, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.InvokeStatic, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.InvokeInterface, ByteCodeMode.Constant_2_1_1, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.InvokeDynamic, ByteCodeMode.Constant_2_1_1, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.New, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Newarray, ByteCodeMode.Immediate_1, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Anewarray, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Arraylength, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Athrow, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Checkcast, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.InstanceOf, ByteCodeMode.Constant_2, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.MonitorEnter, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.MonitorExit, ByteCodeMode.Simple, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.Wide, NormalizedByteCode.__nop, ByteCodeMode.WidePrefix, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.Multianewarray, ByteCodeMode.Constant_2_Immediate_1, ByteCodeModeWide.Unused, false);
            new ByteCodeMetaData(OpCode.IfNull, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.IfNonNull, ByteCodeMode.Branch_2, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.GotoW, NormalizedByteCode.__goto, ByteCodeMode.Branch_4, ByteCodeModeWide.Unused, true);
            new ByteCodeMetaData(OpCode.JsrW, NormalizedByteCode.__jsr, ByteCodeMode.Branch_4, ByteCodeModeWide.Unused, true);

        }

    }

}
