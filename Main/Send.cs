using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class Send
    {
        private readonly byte[] buffersize = new byte[8192];

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
            string x = null;

            if (file.StartsWith("\""))
            {
                x = file.Substring(1, file.Length - 1);
                file = x;
            }

            if (file.EndsWith("\""))
            {
                if (!(x == null))
                {
                    string x2 = x.Substring(0, file.Length - 1);
                    file = x2;
                }
                else
                {
                    string x2 = file.Substring(0, file.Length - 1);
                    file = x2;
                }
            }

            FileInfo fi = new FileInfo(file);

            if (fi.Exists)
            {
                try
                {
                    WriteLine("Trying to connect...");

                    TcpClient client = new TcpClient(IP, 1604);
                    NetworkStream nwStream = client.GetStream();

                    if (client.Connected)
                    {
                        WriteLine("Connected!");

                        InternetProtocol.SendFileSize(nwStream, fi.Length);

                        await Task.Delay(700);

                        WriteLine("Sending the filename...");

                        byte[] name2 = Encoding.ASCII.GetBytes(fi.Name);
                        await nwStream.WriteAsync(name2, 0, name2.Length);
                        nwStream.Flush();
                        await Task.Delay(700);

                        WriteLine("Sending the file...");

                        FileStream strm = fi.OpenRead();

                        while (true)
                        {
                            int num = await strm.ReadAsync(buffersize, 0, buffersize.Length);

                            if (!(num > 0))
                                break;

                            await nwStream.WriteAsync(buffersize, 0, num);

                            InternetProtocol.UpdateProgressbar(num, fi.Length);
                        }

                        WriteLine("Done");

                        strm.Close();
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
