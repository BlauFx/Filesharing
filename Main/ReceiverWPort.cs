using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using static System.Console;

namespace BFs
{
    public class ReceiverWPort
    {
        private readonly byte[] buffersize = new byte[8192];

        public ReceiverWPort()
        {
            Receiver();
            ReadLine();
        }

        public void Receiver()
        {
            try
            {
                WriteLine("Enter the IP");
                string IP = ReadLine();

                WriteLine("Trying to connect...");

                TcpClient client = new TcpClient(IP, 1604) { ReceiveTimeout = int.MaxValue };
                NetworkStream nwStream = client.GetStream();

                if (client.Client.Connected)
                {
                    WriteLine("Connected to " + client.Client.RemoteEndPoint);

                    InternetProtocol.GetFileSize(nwStream, client);
                    Thread.Sleep(1000);

                    InternetProtocol.GetFileName(nwStream, client);
                    Thread.Sleep(1000);

                    using (FileStream strm = new FileStream((@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{InternetProtocol.Filename}"), FileMode.OpenOrCreate))
                    {
                        InternetProtocol.Transport(InternetProtocol.TransportWay.Receive, nwStream, strm, buffersize, InternetProtocol.Filesize);
                    }

                    nwStream.Close();
                    client.Close();

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
