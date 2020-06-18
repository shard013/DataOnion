using System.Collections;
using System.Linq;
using System.Text;

namespace DataOnion.Layers
{
    public static class Layer3
    {
        const string KnownSearch = "--------------------------------";
        const string KnownExpected = "==[ Layer ";

        public static byte[] DecryptXor(byte[] bytes)
        {
            var key = SearchKey(bytes, KnownSearch, KnownExpected);

            var ka = new BitArray(key);
            var ba = new BitArray(bytes);
            var outBa = new BitArray(ba.Length);

            for (var i = 0; i < ba.Length; i++)
            {
                outBa[i] = ba[i] ^ ka[i % ka.Length];
            }

            var output = new byte[outBa.Length / 8];
            outBa.CopyTo(output, 0);
            return output;
        }

        static byte[] SearchKey(byte[] bytes, string knownSearch, string expected)
        {
            var searchKey = GetXorKeyFromKnownPlaintext(bytes, knownSearch);
            var ka = new BitArray(searchKey);
            var ba = new BitArray(bytes);
            var outBa = new BitArray(ba.Length);

            for (var i = 0; i < ba.Length; i++)
            {
                outBa[i] = ba[i] ^ ka[i % ka.Length];
            }

            var output = new byte[outBa.Length / 8];
            outBa.CopyTo(output, 0);
            var searchText = Encoding.ASCII.GetString(output, 0, output.Length);

            var index = searchText.IndexOf(expected);

            var known = searchText.Substring(index, 32);
            var actualKey = GetXorKeyFromKnownPlaintext(bytes, known);

            return actualKey;
        }

        static byte[] GetXorKeyFromKnownPlaintext(byte[] bytes, string known)
        {
            var knownBytes = Encoding.ASCII.GetBytes(known);

            var b1 = new BitArray(knownBytes);
            var b2 = new BitArray(bytes.Take(known.Length).ToArray());

            var b3 = b1.Xor(b2);

            var key = new byte[b3.Length / 8];
            b3.CopyTo(key, 0);

            return key;
        }

    }
}
