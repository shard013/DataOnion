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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataOnion.Layers
{
    public static class Layer5
    {
        static readonly int[] DataSequence = new int[] { 256, 8, 40, 128, 0 };

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
                using MemoryStream msDecrypt = new MemoryStream(cipherText);
                var outStream = AesCtrTransform(key, iv, msDecrypt);
                output = new byte[outStream.Length];
                outStream.Position = 0;
                outStream.Read(output);
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

        public static Stream AesCtrTransform(byte[] key, byte[] salt, Stream inputStream)
        {
            var outputStream = new MemoryStream();
            SymmetricAlgorithm aes =
                new AesManaged { Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 };

            int blockSize = aes.BlockSize / 8;

            if (salt.Length != blockSize)
            {
                throw new ArgumentException(
                    string.Format(
                        "Salt size must be same as block size (actual: {0}, expected: {1})",
                        salt.Length, blockSize));
            }

            byte[] counter = (byte[])salt.Clone();

            Queue<byte> xorMask = new Queue<byte>();

            var zeroIv = new byte[blockSize];
            ICryptoTransform counterEncryptor = aes.CreateEncryptor(key, zeroIv);

            int b;
            while ((b = inputStream.ReadByte()) != -1)
            {
                if (xorMask.Count == 0)
                {
                    var counterModeBlock = new byte[blockSize];

                    counterEncryptor.TransformBlock(
                        counter, 0, counter.Length, counterModeBlock, 0);

                    for (var i2 = counter.Length - 1; i2 >= 0; i2--)
                    {
                        if (++counter[i2] != 0)
                        {
                            break;
                        }
                    }

                    foreach (var b2 in counterModeBlock)
                    {
                        xorMask.Enqueue(b2);
                    }
                }

                var mask = xorMask.Dequeue();
                var bMask = (byte)(((byte)b) ^ mask);
                outputStream.WriteByte(bMask);
            }

            return outputStream;
        }

    }
}
