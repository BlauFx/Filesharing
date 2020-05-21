using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class Send
    {
        private string IP = string.Empty;

        public Send()
        {
            SendFile();
            ReadLine();
        }

        private void SendFile()
        {
            if (IP == string.Empty)
            {
                WriteLine("Enter the IP");
                IP = ReadLine();
            }

            WriteLine("Drag the file to send");
            string file = ReadLine();

            if (file.StartsWith("\""))
                file = file[1..];

            if (file.EndsWith("\""))
                file = file[0..^1];

            FileInfo fi = new FileInfo(file);

            if (fi.Exists)
            {
                Connect(fi);
            }
            else
            {
                WriteLine("File does not exists");
                SendFile();
            }
        }

        private async void Connect(FileInfo fi)
        {
            try
            {
                WriteLine("Trying to connect...");

                using TcpClient client = new TcpClient(IP, 1604) { ReceiveTimeout = int.MaxValue, ReceiveBufferSize = int.MaxValue };
                using NetworkStream nwStream = client.GetStream();

                if (client.Connected)
                {
                    WriteLine("Connected!");

                    InternetProtocol.SendFileSize(nwStream, fi.Length);
                    await Task.Delay(1000);

                    InternetProtocol.SendFileName(nwStream, fi.Name);
                    await Task.Delay(1000);

                    using (FileStream strm = fi.OpenRead())
                    {
                        await InternetProtocol.Transport(InternetProtocol.TransportWay.Send, nwStream, strm, fi.Length);
                    }

                    WriteLine("Done!");
                }
            }
            catch (TimeoutException)
            {
                WriteLine("Couldn't connect \nretrying...");
                Connect(fi);
            }
        }
    }
}
