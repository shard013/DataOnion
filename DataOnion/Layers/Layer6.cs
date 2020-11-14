/*      
 *      *** Note from author ***
 * 
 *      The Tomtel VM has been intentionally crippled, it can be fixed with a minor alteration to a single line.
 *      This is your final warning to turn back.
 * 
 */

using DataOnion.Layers.VM;
using System.Collections.Generic;

namespace DataOnion.Layers
{
    public static class Layer6
    {
        public static byte[] Decrypt(byte[] bytes)
        {
            var vm = new Tomtel(bytes);
            vm.Execute();
            return vm.GetOutputBytes();
        }

        public static byte[] GetSampleProgramTestBytes(IEnumerable<string> lines)
        {
            var programBytes = new List<byte>();

            foreach (var line in lines)
            {
                if (!line.StartsWith("    ") || !line.Contains('#'))
                {
                    continue;
                }

                var s = line.Replace(" ", "");
                var ins = "";
                foreach (var c in s)
                {
                    if (c == '#')
                    {
                        break;
                    }
                    ins += c;
                    if (ins.Length == 2)
                    {
                        programBytes.Add((byte)int.Parse(ins, System.Globalization.NumberStyles.HexNumber));
                        ins = "";
                    }
                }
            }

            var decoded = programBytes.ToArray();
            return decoded;
        }

    }
}
