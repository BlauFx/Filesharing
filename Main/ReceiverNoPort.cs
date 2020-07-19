using System;
using System.IO;
using System.Net.Sockets;
using static System.Console;

namespace BFs
{
    public class ReceiverNoPort
    {
        public ReceiverNoPort()
        {
            Receiver(null);
            ReadLine();
        }

        private async void Receiver(string IP)
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
                Receiver(IP);
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
        }
    }
}
