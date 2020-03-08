using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
                    WriteLine("Connection accepted from " + client.Client.LocalEndPoint);
                    WriteLine("File: ");

                    var FileInput = ReadLine();

                    FileInfo file = new FileInfo(FileInput);
                    FileStream strm = file.OpenRead();

                    WriteLine("Sending the filesize...");
                    //send filesize
                    byte[] name1 = Encoding.ASCII.GetBytes(file.Length.ToString());
                    nwStream.Write(name1, 0, name1.Length);
                    nwStream.Flush();

                    WriteLine("Sending the filename...");
                    //send filename
                    byte[] name2 = Encoding.ASCII.GetBytes(file.Name);
                    nwStream.Write(name2, 0, name2.Length);
                    nwStream.Flush();

                    WriteLine("Sending the file...");

                    float current = 0;

                    while (true)
                    {
                        int num = strm.Read(buffersize, 0, buffersize.Length);

                        if (!(num > 0))
                            break;

                        nwStream.Write(buffersize, 0, num);

                        if (current < file.Length)
                            current += num;

                        int percentComplete = (int)Math.Round((double)(100 * current) / file.Length);
                        Title = $"BFs {percentComplete.ToString()}%";
                    }

                    strm.Close();
                    nwStream.Close();
                    client.Close();
                    listener.Stop();

                    WriteLine("Done!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
