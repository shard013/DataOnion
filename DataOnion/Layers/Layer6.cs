using System.Text;

namespace DataOnion.Layers
{
    public static class Layer6
    {
        public static byte[] Decrypt(byte[] bytes)
        {
            return Encoding.GetEncoding("UTF-8").GetBytes("In progress".ToCharArray());
        }
    }
}
