using System;
using System.Collections;
using System.Linq;

namespace DataOnion.Layers.VM
{
    public class TomtelStorage
    {
        // 8-bit registers
        readonly byte[] r8 = new byte[6];
        enum R8
        {
            a,
            b,
            c,
            d,
            e,
            f
        }

        public ref byte A { get { return ref r8[(int)R8.a]; } }
        public ref byte B { get { return ref r8[(int)R8.b]; } }
        public ref byte F { get { return ref r8[(int)R8.f]; } }

        // 32-bit registers
        readonly uint[] r32 = new uint[6];
        enum R32
        {
            la,
            lb,
            lc,
            ld,
            ptr,
            pc
        }

        public ref uint Ptr { get { return ref r32[(int)R32.ptr]; } }
        public ref uint Pc { get { return ref r32[(int)R32.pc]; } }

        byte[] Memory { get; set; }

        public TomtelStorage(byte[] bytes)
        {
            Memory = bytes.ToArray();
        }

        public byte ReadImm8()
        {
            return Memory[r32[(int)R32.pc]++];
        }

        public uint ReadImm32()
        {
            return Memory[r32[(int)R32.pc]++] +
                   Memory[r32[(int)R32.pc]++] * (uint)256 +
                   Memory[r32[(int)R32.pc]++] * (uint)65536 +
                   Memory[r32[(int)R32.pc]++] * (uint)16777216;
        }

        public uint ReadDstToUInt(BitArray insb)
        {
            return Convert.ToUInt32(insb[5]) * 4 + Convert.ToUInt32(insb[4]) * 2 + Convert.ToUInt32(insb[3]);
        }

        public uint ReadSrcToUInt(BitArray insb)
        {
            return Convert.ToUInt32(insb[2]) * 4 + Convert.ToUInt32(insb[1]) * 2 + Convert.ToUInt32(insb[0]);
        }

        public ref byte Reg(uint v)
        {
            if (v == 7)
            {
                return ref Memory[(byte)r32[(byte)R32.ptr] + r8[(byte)R8.c]];
            }

            return ref r8[v - 1];
        }

        public ref uint Reg32(uint v)
        {
            return ref r32[v - 1];
        }

    }
}
