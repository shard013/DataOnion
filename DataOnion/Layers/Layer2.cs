using System.Collections;

namespace DataOnion.Layers
{
    public static class Layer2
    {
        public static byte[] ParityFilter(byte[] bytes)
        {
            var bits = new BitArray(bytes.Length * 7);

            var p = 7;
            var q = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                var ba = new BitArray(new byte[1] { bytes[i] });
                if (ParityCheck(ba))
                {
                    for (var j = 7; j > 0; j--)
                    {
                        bits.Set(p + q, ba[j]);
                        p--;
                        if (p < 0)
                        {
                            p = 7;
                            q += 8;
                        }
                    }
                }

            }

            var b2 = new BitArray(q);
            for (var i = 0; i < q; i++)
            {
                b2.Set(i, bits[i]);
            }

            var output = new byte[b2.Length / 8];
            b2.CopyTo(output, 0);

            return output;
        }

        static bool ParityCheck(BitArray ba)
        {
            var parity = false;

            for (var j = 7; j > 0; j--)
            {
                parity = ba[j] ^ parity;
            }

            return parity == ba[0];
        }

    }
}
