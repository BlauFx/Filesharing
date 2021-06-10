using System;
using System.Net.Sockets;
using static System.Console;

namespace Filesharing
{
    public class PortReq
    {
        public PortReq(bool receive)
        {
            InternetProtocol.WriteToClipboard(InternetProtocol.GetIP(InternetProtocol.Ipv6 ? InternetProtocol.IPVersion.IPV6 : InternetProtocol.IPVersion.IPV4).Result);

            Start(receive);
            ReadLine();
        }

        private async void Start(bool receive)
        {
            try
            {
                WriteLine("Waiting for a connection...");

                TcpListener listener = TcpListener.Create(InternetProtocol.Port);
                listener.Start();

                using TcpClient client = listener.AcceptTcpClient();
                await using NetworkStream nwStream = client.GetStream();

                client.ChangeTimeout();
                client.ChangeBuffer();

                if (!client.Connected) return;
                if (receive)
                    await InternetProtocol.Receive(client, nwStream, true);
                else
                    await InternetProtocol.Send(nwStream, InternetProtocol.GetFile(), client.Client.RemoteEndPoint,true);

                listener.Stop();
                WriteLine("Done!");
            }
            catch (TimeoutException)
            {
                WriteLine("Timeout");
                Start(receive);
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
        }
    }
}
