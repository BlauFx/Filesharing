using System;
using System.Net.Sockets;
using static System.Console;

namespace BFs
{
    public class ReceivePortRequired
    {
        public ReceivePortRequired()
        {
            InternetProtocol.WriteToClipboard(InternetProtocol.DownloadIP(InternetProtocol.IPVersion.IPV4).Result);

            Download();
            ReadLine();
        }

        private async void Download()
        {
            try
            {
                WriteLine("Waiting for a connection...");

                TcpListener listener = TcpListener.Create(1604);
                listener.Start();

                using TcpClient client = await listener.AcceptTcpClientAsync();
                using NetworkStream nwStream = client.GetStream();

                client.ReceiveTimeout = int.MaxValue;
                client.ReceiveBufferSize = InternetProtocol.buffersize.Length;

                if (client.Connected)
                {
                    await InternetProtocol.ReceiveLogic(client, nwStream, true);
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
