using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class SenderWPort
    {
        public SenderWPort()
        {
            Sender();
            ReadLine();
        }

        private async void Sender()
        {
            InternetProtocol.WriteToClipboard(InternetProtocol.DownloadIP(InternetProtocol.IPVersion.IPV4).Result);

            try
            {
                WriteLine("Waiting for a connection... ");

                TcpListener listener = TcpListener.Create(1604);
                listener.Start();
                TcpClient client = listener.AcceptTcpClient();

                client.ReceiveTimeout = int.MaxValue;
                client.SendTimeout = int.MaxValue;
                client.SendBufferSize = ushort.MaxValue * 3;

                NetworkStream nwStream = client.GetStream();

                if (client.Connected)
                {
                    WriteLine("Connected!");
                    //WriteLine("Connection accepted from " + client.Client.RemoteEndPoint);
                    WriteLine("File: ");

                    var FileInput = ReadLine();

                    if (FileInput.StartsWith("\""))
                        FileInput = FileInput[1..];

                    if (FileInput.EndsWith("\""))
                        FileInput = FileInput[0..^1];

                    FileInfo File = new FileInfo(FileInput);

                    InternetProtocol.SendFileSize(nwStream, File.Length);
                    await Task.Delay(1000);

                    InternetProtocol.SendFileName(nwStream, File.Name);
                    await Task.Delay(1000);

                    using (FileStream strm = File.OpenRead())
                    {
                        await InternetProtocol.Transport(InternetProtocol.TransportWay.Send, nwStream, strm, File.Length);
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
