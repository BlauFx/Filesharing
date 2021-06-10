using System;
using System.IO;
using System.Net.Sockets;
using static System.Console;

namespace Filesharing
{
    public class NoPortReq
    {
        public NoPortReq(bool send)
        {
            if (send)
                HandelConnection(null,true);
            else
                HandelConnection(null);

            ReadLine();
        }

        private async void HandelConnection(string ip, bool isSend = false, FileInfo fi = null)
        {
            if (ip == null)
            {
                WriteLine("Enter the IP");
                ip = ReadLine();
            }

            if (isSend)
                fi ??= InternetProtocol.GetFile();

            try
            {
                WriteLine("Trying to connect...");

                using TcpClient client = new TcpClient(ip ?? throw new ArgumentNullException(nameof(ip)), InternetProtocol.Port);

                client.ChangeTimeout();
                client.ChangeBuffer();

                await using NetworkStream nwStream = client.GetStream();

                if (!client.Connected) return;
                if (isSend)
                    await InternetProtocol.Send(nwStream, fi, client.Client.RemoteEndPoint, true);
                else
                    await InternetProtocol.Receive(client, nwStream, true);
                WriteLine("Done!");
            }
            catch (TimeoutException)
            {
                WriteLine(isSend ? "Timeout \nretrying..." : "Couldn't connect \nretrying...");
                HandelConnection(ip, isSend, fi);
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
        }
    }
}
