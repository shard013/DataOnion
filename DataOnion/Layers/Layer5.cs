using System.Text;

namespace DataOnion.Layers
{
    public static class Layer5
    {
        public static byte[] Decrypt(byte[] bytes)
        {
            bytes[0] = 0;
            // todo need to decrypt payload with AES here
            return Encoding.ASCII.GetBytes("todo need to decrypt payload with AES in Layers/Layer5.cs");
        }
    }
}
