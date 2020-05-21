using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class ReceiverWPort
    {
        public ReceiverWPort()
        {
            Receiver();
            ReadLine();
        }

        public async void Receiver()
        {
            try
            {
                WriteLine("Enter the IP");
                string IP = ReadLine();

                WriteLine("Trying to connect...");

                TcpClient client = new TcpClient(IP, 1604) { ReceiveTimeout = int.MaxValue, SendTimeout = int.MaxValue, ReceiveBufferSize = ushort.MaxValue * 3 };
                NetworkStream nwStream = client.GetStream();

                if (client.Connected)
                {
                    WriteLine("Connected!");

                    InternetProtocol.GetFileSize(nwStream, client);
                    await Task.Delay(1000);

                    InternetProtocol.GetFileName(nwStream, client);
                    await Task.Delay(1000);

                    using (FileStream strm = new FileStream(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{InternetProtocol.Filename}", FileMode.OpenOrCreate))
                    {
                        InternetProtocol.Transport(InternetProtocol.TransportWay.Receive, nwStream, strm, InternetProtocol.Filesize);
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
