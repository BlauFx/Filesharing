using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class Send
    {
        public Send()
        {
            WriteLine("Enter the IP");
            var x = ReadLine();

            WriteLine("Drag the file to send");
            var y = ReadLine();

            SendFile(x, y);

            ReadLine();
        }

        private async void SendFile(string IP, string file)
        {
            if (file.StartsWith("\""))
                file = file[1..];

            if (file.EndsWith("\""))
                file = file[0..^1];

            FileInfo fi = new FileInfo(file);

            if (fi.Exists)
            {
                try
                {
                    WriteLine("Trying to connect...");

                    TcpClient client = new TcpClient(IP, 1604) { ReceiveTimeout = int.MaxValue, ReceiveBufferSize = int.MaxValue };
                    NetworkStream nwStream = client.GetStream();

                    if (client.Connected)
                    {
                        WriteLine("Connected!");

                        InternetProtocol.SendFileSize(nwStream, fi.Length);
                        await Task.Delay(1000);

                        InternetProtocol.SendFileName(nwStream, fi.Name);
                        await Task.Delay(1000);

                        using (FileStream strm = fi.OpenRead())
                        {
                            await InternetProtocol.TransportAsync(InternetProtocol.TransportWay.Send, nwStream, strm, fi.Length);
                        }

                        WriteLine("Done");

                        nwStream.Close();
                        client.Close();
                    }
                }
                catch
                {
                    WriteLine("Couldn't connect \nretrying...");
                    SendFile(IP, file);
                }
            }
            else
            {
                WriteLine("File does not exists");
                WriteLine("Drag the file to send");
                var x3 = ReadLine();
                SendFile(IP, x3);
            }
        }
    }
}
