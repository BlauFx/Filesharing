using System;
using System.IO;
using System.Net.Sockets;
using static System.Console;

namespace BFs
{
    public class NoPortReq
    {
        public NoPortReq(bool send)
        {
            if (send)
                Send(null, null);
            else
                Receive(null);

            ReadLine();
        }

        private async void Send(string IP, FileInfo fi)
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
                Send(IP, fi);
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
        }

        private async void Receive(string IP)
        {
            if (IP == null)
            {
                WriteLine("Enter the IP");
                IP = ReadLine();
            }
            
            try
            {
                WriteLine("Trying to connect...");

                using TcpClient client = new TcpClient(IP, 1604) { ReceiveTimeout = int.MaxValue, ReceiveBufferSize = InternetProtocol.buffersize.Length };
                using NetworkStream nwStream = client.GetStream();

                if (client.Connected)
                {
                    await InternetProtocol.ReceiveLogic(client, nwStream, false);
                    WriteLine("Done!");
                }
            }
            catch (TimeoutException)
            {
                WriteLine("Couldn't connect \nretrying...");
                Receive(IP);
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
        }
    }
}