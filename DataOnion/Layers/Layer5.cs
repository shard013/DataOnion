/*      
 *      *** Note from author ***
 * 
 *      Layer 5 has been intentionally crippled, it can be fixed with a minor alteration to a single line.
 *      This is your final warning to turn back.
 * 
 */

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataOnion.Layers
{
    public static class Layer5
    {
        static readonly int[] DataSequence = new int[] { 256, 8, 40, 128, 5 };

        public static byte[] Decrypt(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var i = 0;

            //Unwrap key
            var kek = ReadData(stream, DataSequence[i++] / DataSequence[i]);
            var kek_iv = ReadData(stream, DataSequence[i++]);
            var wrapped_key = ReadData(stream, DataSequence[i]);
            var unwrapped_key = UnwrapKey(kek, kek_iv, wrapped_key);

            //Payload
            var payload_iv = ReadData(stream, DataSequence[i] / DataSequence[1]);
            var encrypted_payload = ReadData(stream, (int)(stream.Length - stream.Position));
            var payload = DecryptStringFromBytes_Aes(encrypted_payload, unwrapped_key, payload_iv);

            return payload.Take(payload.Length - DataSequence[++i]).ToArray();
        }

        public static byte[] UnwrapKey(byte[] kek, byte[] iv, byte[] unwrapped_key)
        {
            IWrapper wrapper = new AesWrapEngine();
            ICipherParameters kp = new KeyParameter(kek);
            ICipherParameters kpwiv = new ParametersWithIV(kp, iv);

            wrapper.Init(false, kpwiv);
            byte[] pText = wrapper.Unwrap(unwrapped_key, 0, unwrapped_key.Length);

            return pText;
        }

        static byte[] DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            byte[] output = null;

            try
            {
                using Aes aesAlg = Aes.Create();
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using MemoryStream msDecrypt = new MemoryStream(cipherText);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                output = new byte[msDecrypt.Length];
                csDecrypt.Read(output, 0, output.Length);
            }
            catch (CryptographicException e)
            {
                return Encoding.ASCII.GetBytes(e.Message);
            }

            return output;
        }

        static byte[] ReadData(Stream stream, int length)
        {
            var data = new byte[length];
            stream.Read(data, 0, data.Length);
            return data;
        }

    }
}
