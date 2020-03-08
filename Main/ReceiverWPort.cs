using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
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

                TcpClient client = new TcpClient(IP, 1604);
                NetworkStream nwStream = client.GetStream();

                if (client.Client.Connected)
                {
                    WriteLine("Connected to " + client.Client.LocalEndPoint);

                    //get filesize
                    byte[] ReceiveBuffer1 = new byte[client.ReceiveBufferSize];
                    int nwRead1 = nwStream.Read(ReceiveBuffer1, 0, ReceiveBuffer1.Length);
                    string tmp1 = Encoding.ASCII.GetString(ReceiveBuffer1, 0, nwRead1);
                    float filesize = float.Parse(tmp1);
                    nwStream.Flush();

                    WriteLine("Receiving the filesize...");
                    WriteLine("Receiving the filename...");
                    //get filename
                    byte[] ReceiveBuffer2 = new byte[client.ReceiveBufferSize];
                    int nwRead2 = nwStream.Read(ReceiveBuffer2, 0, ReceiveBuffer2.Length);
                    string filename = Encoding.ASCII.GetString(ReceiveBuffer2, 0, nwRead2);
                    nwStream.Flush();

                    WriteLine("Receiving the file...");
                    FileStream strm = new FileStream((@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{filename}"), FileMode.OpenOrCreate);

                    float current = 0f;

                    while (true)
                    {
                        int num = nwStream.Read(buffersize, 0, buffersize.Length);

                        if (!(num > 0))
                            break;

                        strm.Write(buffersize, 0, num);

                        if (current < filesize)
                            current += num;

                        int percentComplete = (int)Math.Round((double)(100 * current) / filesize);
                        Title = $"BFs {percentComplete.ToString()}%";
                    }

                    strm.Close();
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
