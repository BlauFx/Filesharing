using System;
using System.IO;
using System.Net.Sockets;
using static System.Console;

namespace BFs
{
    public class SendPortRequired
    {
        public SendPortRequired()
        {
            InternetProtocol.WriteToClipboard(InternetProtocol.DownloadIP(InternetProtocol.IPVersion.IPV4).Result);

            Sender();
            ReadLine();
        }

        private async void Sender()
        {
            try
            {
                WriteLine("Waiting for a connection...");

                TcpListener listener = TcpListener.Create(1604);
                listener.Start();

                using TcpClient client = listener.AcceptTcpClient();
                using NetworkStream nwStream = client.GetStream();

                client.SendTimeout = int.MaxValue;
                client.SendBufferSize = InternetProtocol.buffersize.Length;

                if (client.Connected)
                {
                    await InternetProtocol.SendLogic(nwStream, InternetProtocol.GetFile(), client.Client.RemoteEndPoint, true);
                    listener.Stop();

                    WriteLine("Done!");
                }
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
        }
    }
}
