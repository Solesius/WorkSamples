using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Net.Security;
using System.Threading.Tasks;

namespace microhttp
{
    class SocketHelper
    {
        public static void Write(List<byte[]> payload, SslStream stream)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (byte[] byteChunk in payload)
                    stream.Write(byteChunk);
                stream.Flush();
                stream.Close();
                stream.Dispose();
            });
        }
        public static void Write(byte[] payload, SslStream stream)
        {
            Task.Factory.StartNew(() =>
            {
                stream.Write(payload);
                stream.Flush();
                stream.Close();
                stream.Dispose();
            });
        }

        public static byte[] Bytes(string s) => Encoding.UTF8.GetBytes(s);
    }
}