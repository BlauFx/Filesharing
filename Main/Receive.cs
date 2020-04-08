using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class Receive
    {
        public Receive()
        {
            Download();
            ReadLine();
        }

        private async void Download()
        {
            InternetProtocol.WriteToClipboard(InternetProtocol.DownloadIP(InternetProtocol.IPVersion.IPV4).Result);

            try
            {
                WriteLine("Waiting for connection...");

                TcpListener listener = TcpListener.Create(1604);
                listener.Start();
                TcpClient client = await listener.AcceptTcpClientAsync();
                
                client.ReceiveTimeout = int.MaxValue;
                NetworkStream nwStream = client.GetStream();

                if (client.Connected)
                {
                    WriteLine("Connected!");
                    InternetProtocol.GetFileSize(nwStream, client);
                    await Task.Delay(1000);

                    InternetProtocol.GetFileName(nwStream, client);
                    await Task.Delay(1000);

                    using (FileStream strm = new FileStream((@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{InternetProtocol.Filename}"), FileMode.OpenOrCreate))
                    {
                        await InternetProtocol.TransportAsync(InternetProtocol.TransportWay.Receive, nwStream, strm, InternetProtocol.Filesize);
                    }

                    WriteLine("Done");

                    nwStream.Close();
                    client.Close();
                    listener.Server.Close();
                    listener.Stop();
                }
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
        }
    }
}
