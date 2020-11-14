using DataOnion.Layers;
using Logos.Utility;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DataOnion
{
    class Program
    {
        const string DataDirectory = "..\\..\\..\\Data\\";

        const string Layer0Data = "Start.txt";
        const string Layer1Data = "Layer-1.txt";
        const string Layer2Data = "Layer-2.txt";
        const string Layer3Data = "Layer-3.txt";
        const string Layer4Data = "Layer-4.txt";
        const string Layer5Data = "Layer-5.txt";
        const string Layer6Data = "Layer-6.txt";
        const string Layer7Data = "TheCore.txt";

        static void Main()
        {
            DecodeLayer0();
            DecodeLayer1();
            DecodeLayer2();
            DecodeLayer3();
            DecodeLayer4();
            DecodeLayer5();
            DecodeLayer6();
        }

        static void DecodeLayer0()
        {
            var payload = GetPayload(Layer0Data);
            var decoded = Ascii85.Decode(payload);
            var output = Encoding.ASCII.GetString(decoded, 0, decoded.Length);
            File.WriteAllText($"{DataDirectory}{Layer1Data}", output);
        }

        static void DecodeLayer1()
        {
            var payload = GetPayload(Layer1Data);
            var decoded = Ascii85.Decode(payload);
            decoded = Layer1.ShiftRotate(decoded);
            var output = Encoding.ASCII.GetString(decoded, 0, decoded.Length);
            File.WriteAllText($"{DataDirectory}{Layer2Data}", output);
        }

        static void DecodeLayer2()
        {
            var payload = GetPayload(Layer2Data);
            var decoded = Ascii85.Decode(payload);
            decoded = Layer2.ParityFilter(decoded);
            var output = Encoding.ASCII.GetString(decoded, 0, decoded.Length);
            File.WriteAllText($"{DataDirectory}{Layer3Data}", output);
        }

        static void DecodeLayer3()
        {
            var payload = GetPayload(Layer3Data);
            var decoded = Ascii85.Decode(payload);
            decoded = Layer3.DecryptXor(decoded);
            var output = Encoding.ASCII.GetString(decoded, 0, decoded.Length);
            File.WriteAllText($"{DataDirectory}{Layer4Data}", output);
        }

        static void DecodeLayer4()
        {
            var payload = GetPayload(Layer4Data);
            var decoded = Ascii85.Decode(payload);
            decoded = Layer4.NetworkStreamToPayload(decoded);
            var output = Encoding.ASCII.GetString(decoded, 0, decoded.Length);
            File.WriteAllText($"{DataDirectory}{Layer5Data}", output);
        }

        static void DecodeLayer5()
        {
            var payload = GetPayload(Layer5Data);
            var decoded = Ascii85.Decode(payload);
            decoded = Layer5.Decrypt(decoded);
            var output = Encoding.ASCII.GetString(decoded, 0, decoded.Length);
            File.WriteAllText($"{DataDirectory}{Layer6Data}", output);
        }

        static void DecodeLayer6()
        {
            byte[] decoded;
            var runSampleProgram = false;
            if (runSampleProgram)
            {
                var lines = File.ReadLines($"{DataDirectory}{Layer6Data}");
                decoded = Layer6.GetSampleProgramTestBytes(lines);
            }
            else
            {
                var payload = GetPayload(Layer6Data);
                decoded = Ascii85.Decode(payload);
            }

            File.WriteAllText($"{DataDirectory}{Layer7Data}", $"Begin decoding {nameof(Layer6Data)} at {System.DateTime.Now}");

            var decrypted = Layer6.Decrypt(decoded);
            var output = Encoding.ASCII.GetString(decrypted, 0, decrypted.Length);
            File.WriteAllText($"{DataDirectory}{Layer7Data}", output);
        }

        static string GetPayload(string filename)
        {
            var raw = File.ReadAllText($"{DataDirectory}{filename}");
            var payload = Regex.Match(raw, @"<~(.*)~>", RegexOptions.Singleline).Groups[1].Value;
            payload = payload.Replace("\n", "").Replace("\r", "");
            return payload;
        }
    }
}
