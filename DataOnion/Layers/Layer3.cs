using System.Collections;
using System.Linq;
using System.Text;

namespace DataOnion.Layers
{
    public static class Layer3
    {
        const int KeyLength = 32;
        const string KnownSearch = "----------------------------------------------------";
        const string KnownExpected = "==[ Layer 4/6: ";

        public static byte[] DecryptXor(byte[] bytes)
        {
            var key = SearchKey(bytes, KnownSearch, KnownExpected);
            var outBa = Xor(key, bytes);

            var output = new byte[outBa.Length / 8];
            outBa.CopyTo(output, 0);
            return output;
        }

        static byte[] SearchKey(byte[] bytes, string knownSearch, string expected)
        {
            var searchKey = GetXorKeyFromKnownPlaintext(bytes, knownSearch);
            var outBa = Xor(searchKey, bytes);

            var output = new byte[outBa.Length / 8];
            outBa.CopyTo(output, 0);
            var searchText = Encoding.ASCII.GetString(output, 0, output.Length);

            var index = searchText.IndexOf(expected);
            if (index < 0)
            {
                throw new System.Exception("Expected string not found");
            }

            for (int i = 0; i < KeyLength; i++)
            {
                //It's likely that we matched mid key
                //Test each key and replace end of key with bits from before index 1 by 1 until match
                var testText = searchText.Substring(index, KeyLength - i) + searchText.Substring(index - i, i);
                if (TestKey(bytes, testText, expected, knownSearch))
                {
                    return GetXorKeyFromKnownPlaintext(bytes, testText);
                }
            }

            throw new System.Exception("Key not found");
        }

        static bool TestKey(byte[] bytes, string key, string startsWith, string contains)
        {
            var keyXor = GetXorKeyFromKnownPlaintext(bytes, key);
            var outBa = Xor(keyXor, bytes);
            var output = new byte[outBa.Length / 8];
            outBa.CopyTo(output, 0);
            var searchText = Encoding.ASCII.GetString(output, 0, output.Length);
            if (searchText.StartsWith(startsWith) && searchText.Contains(contains))
            {
                return true;
            }
            return false;
        }

        static BitArray Xor(byte[] key, byte[] bytes)
        {
            var ka = new BitArray(key);
            var ba = new BitArray(bytes);
            var outBa = new BitArray(ba.Length);
            for (var i = 0; i < ba.Length; i++)
            {
                outBa[i] = ba[i] ^ ka[i % ka.Length];
            }
            return outBa;
        }

        static byte[] GetXorKeyFromKnownPlaintext(byte[] bytes, string known)
        {
            var knownBytes = Encoding.ASCII.GetBytes(known.Substring(0, KeyLength));

            var b1 = new BitArray(knownBytes);
            var b2 = new BitArray(bytes.Take(KeyLength).ToArray());

            var b3 = b1.Xor(b2);

            var key = new byte[b3.Length / 8];
            b3.CopyTo(key, 0);

            return key;
        }

    }
}
