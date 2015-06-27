using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace denvlib.Injections
{
    class StringEncInj
    {
        static int sigToken;
        static byte[] data;
        static string s;

        public static void StringInj()
        {
            Assembly e = Assembly.GetExecutingAssembly();
            Module m = e.ManifestModule;
            byte[] b = m.ResolveSignature(sigToken);
            byte[] buffer = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                buffer[i] = Convert.ToByte(data[i] ^ b[i & 15]);
            buffer.CopyTo(data, 0);
        }

        public static string StringDec(string @string)
        {
            byte[] b = new byte[@string.Length];
            for (int i = 0; i < @string.Length; i++)
                b[i] = (byte)(@string[i] ^ sigToken);
            return Encoding.UTF8.GetString(b);
        }

        public void InsertFields()
        {
            s = Encoding.UTF8.GetString(data, 0, 1);
        }
    }
}
