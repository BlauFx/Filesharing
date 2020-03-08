using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class Receive
    {
        private readonly byte[] buffersize = new byte[8192];

        public Receive()
        {
            Download();
            ReadLine();
        }

        private async void Download()
        {
            InternetProtocol.WriteToClipboard(await InternetProtocol.DownloadIP(InternetProtocol.IPVersion.IPV4));

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
                    WriteLine("Receiving the filesize...");

                    byte[] ReceiveBuffer1 = new byte[client.ReceiveBufferSize];
                    int nwRead1 = await nwStream.ReadAsync(ReceiveBuffer1, 0, ReceiveBuffer1.Length);
                    string tmp1 = Encoding.ASCII.GetString(ReceiveBuffer1, 0, nwRead1);
                    float filesize = float.Parse(tmp1);
                    nwStream.Flush();
                    await Task.Delay(1000);

                    WriteLine("Receiving the filename...");

                    byte[] ReceiveBuffer2 = new byte[client.ReceiveBufferSize];
                    int nwRead2 = await nwStream.ReadAsync(ReceiveBuffer2, 0, ReceiveBuffer2.Length);
                    string filename = Encoding.ASCII.GetString(ReceiveBuffer2, 0, nwRead2);
                    nwStream.Flush();
                    await Task.Delay(1000);

                    FileStream strm = new FileStream((@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{filename}"), FileMode.OpenOrCreate);

                    while (true)
                    {
                        int num = await nwStream.ReadAsync(buffersize, 0, buffersize.Length);

                        if (!(num > 0))
                            break;

                        await strm.WriteAsync(buffersize, 0, num);

                        InternetProtocol.UpdateProgressbar(num, filesize);
                    }

                    WriteLine("Done");

                    strm.Close();
                    nwStream.Close();
                    client.Close();
                    listener.Server.Close();
                    listener.Stop();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
