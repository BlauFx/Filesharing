using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public static class InternetProtocol
    {
        public static byte[] buffersize = new byte[8192];

        public static long Current { get; set; } = 0;

        public static long Filesize { get; set; } = 0;

        public static string Filename { get; set; } = string.Empty;

        public static int Percentage { get; private set; } = 0;

        public static bool DoAsync { get; set; } = false;

        public static async Task<string> DownloadIP(IPVersion IPversion)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", ".");

            HttpResponseMessage response = null;

            switch (IPversion)
            {
                case IPVersion.IPV4:
                    response = await client.GetAsync("http://checkip.dyndns.org/");
                    break;
                case IPVersion.IPV6:
                    break;
            }

            response?.EnsureSuccessStatusCode();

            string responseStr = response?.Content.ReadAsStringAsync().Result;

            int num1 = responseStr.IndexOf("Address: ", StringComparison.Ordinal) + 9;

            WriteLine("IP has been pasted into your clipboard");

            return responseStr.Substring(num1, responseStr.Length - (num1 + 16));
        }

        public static void WriteToClipboard(string str)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OpenClipboard(IntPtr.Zero);
                var ptr = Marshal.StringToHGlobalUni(str);
                SetClipboardData(13, ptr);
                CloseClipboard();
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static void UpdateProgressbar(int num, float filesize)
        {
            if (Current < filesize)
                Current += num;

            Percentage = (int)Math.Round((double)(100f * Current) / filesize);

            if (Math.Floor(Math.Log10(Filesize) + 1) > 10)
            {
                Percentage = (int)Math.Round((double)(100f * Current) / (filesize / 10000000));

                if (Percentage >= 999999)
                    Percentage = 100;
            }

            Title = $"BFs {Percentage}%";
        }

        public static async Task Transport(TransportWay transportWay, NetworkStream nwStream, Stream strm, float filesize)
        {
            async Task AsyncDoWork(Stream a, Stream b)
            {
                while (true)
                {
                    int num = await a.ReadAsync(buffersize, 0, buffersize.Length);

                    if (num <= 0)
                        break;

                    await b.WriteAsync(buffersize, 0, num);
                    UpdateProgressbar(num, filesize);
                }
            }

            void DoWork(Stream a, Stream b)
            {
                while (true)
                {
                    int num = a.Read(buffersize, 0, buffersize.Length);

                    if (num <= 0)
                        break;

                    b.Write(buffersize, 0, num);
                    UpdateProgressbar(num, filesize);
                }
            }

            switch (transportWay)
            {
                case TransportWay.Receive:
                    WriteLine("Receiving the file...");

                    if (DoAsync)
                        await AsyncDoWork(nwStream, strm);
                    else
                        DoWork(nwStream, strm);
                    break;
                case TransportWay.Send:
                    WriteLine("Sending the file...");

                    if (DoAsync)
                        await AsyncDoWork(strm, nwStream);
                    else
                        DoWork(strm, nwStream);
                    break;
            }
        }

        public static long GetFileSize(NetworkStream nwStream, TcpClient client)
        {
            byte[] ReceiveBuffer = new byte[client.ReceiveBufferSize];
            int nwRead = nwStream.Read(ReceiveBuffer, 0, ReceiveBuffer.Length);
            WriteLine("Receiving the filesize...");

            string tmp = Encoding.ASCII.GetString(ReceiveBuffer, 0, nwRead);
            nwStream.Flush();

            Filesize = long.Parse(tmp);
            return Filesize;
        }

        public static string GetFileName(NetworkStream nwStream, TcpClient client)
        {
            byte[] ReceiveBuffer = new byte[client.ReceiveBufferSize];
            int nwRead = nwStream.Read(ReceiveBuffer, 0, ReceiveBuffer.Length);
            WriteLine("Receiving the filename...");

            Filename = Encoding.ASCII.GetString(ReceiveBuffer, 0, nwRead);
            nwStream.Flush();

            return "";
        }

        public static void SendFileSize(NetworkStream nwStream, long FileLength)
        {
            WriteLine("Sending the filesize...");
            byte[] name = Encoding.ASCII.GetBytes(FileLength.ToString());
            nwStream.Write(name, 0, name.Length);
            nwStream.Flush();
        }

        public static void SendFileName(NetworkStream nwStream, string FileName)
        {
            WriteLine("Sending the filename...");
            byte[] name = Encoding.ASCII.GetBytes(FileName);
            nwStream.Write(name, 0, name.Length);
            nwStream.Flush();
        }

        public enum IPVersion
        {
            IPV4,
            IPV6
        }

        public enum TransportWay
        {
            Receive = 0,
            Send = 1,
        }

        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);
    }
}
