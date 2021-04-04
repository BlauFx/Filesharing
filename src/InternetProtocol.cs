using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Filesharing
{
    public static class InternetProtocol
    {
        private static byte[] buffersize = new byte[8192];

        private static long Current { get; set; }

        private static long Filesize { get; set; }

        private static string Filename { get; set; } = string.Empty;

        private static int Percentage { get; set; }

        public static bool DoAsync { get; set; }

        public static bool Ipv6 { get; set; }

        public static int Port { get; } = 1604;

        public static async Task<string> GetIP(IPVersion version)
        {
            using HttpClient client = new HttpClient();

            var IP = version switch
            {
                IPVersion.IPV6 => await client.GetStringAsync("https://api64.ipify.org/"),
                IPVersion.IPV4 => await client.GetStringAsync("https://api.ipify.org"),
                _ => throw new Exception($"{version} must be either IPV6 or IPV4")
            };

            WriteLine("Your IP has been pasted into your clipboard");

            return IP;
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
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var tempStr = $"echo {str} | xclip";
                var arguments = $"-c \"{tempStr}\"";

                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "bash",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                    }
                })
                {
                    process.Start();
                }

            }
        }

        private static void UpdateProgressbar(int num, float filesize, double ElapsedSeconds)
        {
            if (Current < filesize)
                Current += num;

            Percentage = (int)Math.Round((double)(100f * Current) / filesize);

            double transferspeed = CalcTransferSpeed(num, ElapsedSeconds);
            Title = $"{Percentage}% | {transferspeed:0.0} MB/s | Estimated transfer time: {CalcEstimatedTime(filesize - Current, transferspeed)}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CalcTransferSpeed(double filesize, double time) => (filesize / 1000 / 1000) / time;

        private static string CalcEstimatedTime(float filesize, double speed)
        {
            float KB = filesize / 1000;
            float Kbit = KB * 8;

            double SpeedInKbit = speed * 8000;
            float calcTime = Kbit / (float)SpeedInKbit;

            if (calcTime > 60 * 60)
                return (calcTime / (60 * 60)).ToString("0.0") + "h";
            else if (calcTime > 60)
                return (calcTime / 60).ToString("0.0") + "m";

            return calcTime.ToString("0.0") + "s";
        }

        public static FileInfo GetFile()
        {
            WriteLine("File: ");
            string file = ReadLine();

            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(file);

            if (file.StartsWith("\"") || file.StartsWith("\'"))
                file = file[1..];

            if (file.EndsWith("\"") || file.EndsWith("\'"))
                file = file[..^1];

            var fi = new FileInfo(file);

            if (fi.Exists)
                return fi;

            WriteLine("File does not exist");
            return GetFile();
        }

        public static void ChangeTimeout(this TcpClient client) => client.SendTimeout = client.ReceiveTimeout = int.MaxValue;

        public static void ChangeBuffer(this TcpClient client) => client.SendBufferSize = client.ReceiveBufferSize = buffersize.Length;

        private static async Task Transport(Stream a, Stream b, float filesize)
        {
            var currentTime = DateTime.Now;
            int bytesSent = 0;
            double Milliseconds = .5d;

            while (true)
            {
                int num = DoAsync ? await a.ReadAsync(buffersize, 0, buffersize.Length) : a.Read(buffersize, 0, buffersize.Length);
                bytesSent += num;

                if (num <= 0)
                {
                    UpdateProgressbar(bytesSent, filesize, Milliseconds);
                    break;
                }

                if (DoAsync)
                    await b.WriteAsync(buffersize, 0, num);
                else
                    b.Write(buffersize, 0, num);

                if (DateTime.Now - currentTime < TimeSpan.FromSeconds(Milliseconds))
                    continue;

                currentTime = DateTime.Now;
                UpdateProgressbar(bytesSent, filesize, Milliseconds);
                bytesSent = 0;
            }

            Title = $"{Percentage}% | Done!";
        }

        public static async Task Send(NetworkStream nwStream, FileInfo fi, EndPoint remoteIp, bool showRemoteEndPoint)
        {
            WriteLine("Connected!");

            if (showRemoteEndPoint)
                WriteLine($"Connection established with {remoteIp}");

            SendText(nwStream, fi.Length.ToString(), "filesize");
            await Task.Delay(1000);

            SendText(nwStream, fi.Name, "filename");
            await Task.Delay(1000);

            await using FileStream strm = fi.OpenRead();
            WriteLine("Sending the file...");
            await Transport(strm, nwStream, fi.Length);
        }

        public static async Task Receive(TcpClient client, NetworkStream nwStream, bool showRemoteEndPoint)
        {
            WriteLine("Connected!");

            if (showRemoteEndPoint)
                WriteLine($"Connection accepted from {client.Client.RemoteEndPoint}");

            GetFileSize(nwStream, client);
            await Task.Delay(1000);

            GetFileName(nwStream, client);
            await Task.Delay(1000);

            await using FileStream strm = new FileStream(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}{Path.DirectorySeparatorChar}{Filename}", FileMode.OpenOrCreate);
            WriteLine("Receiving the file...");
            await Transport(nwStream, strm, Filesize);
        }

        private static void GetFileSize(NetworkStream nwStream, TcpClient client) => Filesize = long.Parse(ReceiveFileBytes(nwStream, client, "filesize"));

        private static void GetFileName(NetworkStream nwStream, TcpClient client) => Filename = ReceiveFileBytes(nwStream, client, "filename");

        private static string ReceiveFileBytes(NetworkStream nwStream, TcpClient client, string text = null)
        {
            byte[] receiveBuffer = new byte[client.ReceiveBufferSize];
            int nwRead = nwStream.Read(receiveBuffer, 0, receiveBuffer.Length);

            if (text is not null)
                WriteLine($"Receiving {text}...");

            var str = Encoding.UTF8.GetString(receiveBuffer, 0, nwRead);
            nwStream.Flush();

            return str;
        }

        private static void SendText(NetworkStream nwStream, string fileName, string text = null) => SendFileBytes(nwStream, Encoding.UTF8.GetBytes(fileName), text);

        private static void SendFileBytes(NetworkStream nwStream, byte[] bytes, string text = null)
        {
            if (text is not null)
                WriteLine($"Sending {text}...");

            nwStream.Write(bytes, 0, bytes.Length);
            nwStream.Flush();
        }

        public enum IPVersion
        {
            IPV6,
            IPV4
        }

#region WINDOWS
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);
#endregion
    }
}
