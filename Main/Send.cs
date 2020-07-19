using System;
using System.IO;
using System.Net.Sockets;
using static System.Console;

namespace BFs
{
    public class SendNoPort
    {
        public SendNoPort()
        {
            SendFile(null, null);
            ReadLine();
        }

        private async void SendFile(string IP, FileInfo fi)
        {
            if (IP == null)
            {
                WriteLine("Enter the IP");
                IP = ReadLine();
            }

            fi ??= InternetProtocol.GetFile();

            try
            {
                WriteLine("Trying to connect...");

                using TcpClient client = new TcpClient(IP, 1604) { SendTimeout = int.MaxValue, SendBufferSize = InternetProtocol.buffersize.Length };
                using NetworkStream nwStream = client.GetStream();

                if (client.Connected)
                {
                    await InternetProtocol.SendLogic(nwStream, fi, client.Client.RemoteEndPoint, false);
                    WriteLine("Done!");
                }
            }
            catch (TimeoutException)
            {
                WriteLine("Couldn't connect \nretrying...");
                SendFile(IP, fi);
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                ReadKey();
            }
        }
    }
}
