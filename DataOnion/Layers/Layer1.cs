using System.Collections;

namespace DataOnion.Layers
{
    public static class Layer1
    {
        public static byte[] ShiftRotate(byte[] bytes)
        {
            var flipMask = new BitArray(new bool[8] { true, false, true, false, true, false, true, false });

            var output = new byte[bytes.Length];
            for (var i = 0; i < bytes.Length; i++)
            {
                var ba = new BitArray(new byte[1] { bytes[i] });
                ba.Xor(flipMask);
                var ba2 = new BitArray(ba.Length);
                ba2[ba.Length - 1] = ba[0];
                ba = ba.RightShift(1);
                ba2 = ba2.Or(ba);
                ba2.CopyTo(output, i);
            }
            return output;
        }
    }
}
