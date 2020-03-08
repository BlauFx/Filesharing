using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using static System.Console;

namespace BFs
{
    public class SenderWPort
    {
        private readonly byte[] buffersize = new byte[8192];

        public SenderWPort()
        {
            Sender();
            ReadLine();
        }

        public void Sender()
        {
            InternetProtocol.WriteToClipboard(InternetProtocol.DownloadIP(InternetProtocol.IPVersion.IPV4).Result);

            try
            {
                TcpListener listener = TcpListener.Create(1604);
                listener.Start();

                WriteLine("Waiting for a connection... ");

                TcpClient client = listener.AcceptTcpClient();
                client.ReceiveTimeout = int.MaxValue;
                NetworkStream nwStream = client.GetStream();

                if (client.Client.Connected)
                {
                    WriteLine("Connection accepted from " + client.Client.RemoteEndPoint);
                    WriteLine("File: ");

                    var FileInput = ReadLine();

                    FileInfo File = new FileInfo(FileInput);

                    InternetProtocol.SendFileSize(nwStream, File.Length);
                    Thread.Sleep(1000);

                    InternetProtocol.SendFileName(nwStream, File.Name);
                    Thread.Sleep(1000);

                    using (FileStream strm = File.OpenRead())
                    {
                        InternetProtocol.Transport(InternetProtocol.TransportWay.Send, InternetProtocol.IPVersion.IPV4, nwStream, strm, buffersize, File.Length);
                    }

                    nwStream.Close();
                    client.Close();
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
