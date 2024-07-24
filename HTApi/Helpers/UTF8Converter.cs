using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Text;

namespace HTAPI.Helpers
{
    public class UTF8Converter
    {
        public string Original { get; }
        public string Converted { get; }

        public UTF8Converter(string text) {
            this.Original = text;

            byte[] bytes = new byte[text.Length * sizeof(char)];
            System.Buffer.BlockCopy(text.ToCharArray(), 0, bytes, 0, bytes.Length);
            Encoding w1252 = Encoding.GetEncoding(1252);
            byte[] output = Encoding.Convert(Encoding.UTF8, w1252, bytes);
            string result = w1252.GetString(output);

            this.Converted = result;
        }
    }
}
