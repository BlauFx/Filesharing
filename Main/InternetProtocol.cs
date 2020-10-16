using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public static class InternetProtocol
    {
        public static byte[] buffersize = new byte[8192];

        private static long Current { get; set; } = 0;

        private static long Filesize { get; set; } = 0;

        private static string Filename { get; set; } = string.Empty;

        private static int Percentage { get; set; } = 0;

        public static bool DoAsync { get; set; } = false;

        public static async Task<string> DownloadIP(IPVersion IPversion)
        {
            using HttpClient client = new HttpClient();
            
            var IP = IPversion switch
            {
                IPVersion.IPV4 => await client.GetStringAsync("https://api.ipify.org"),
                _ => null
            };

            if (IP != null)
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
        }

        private static void UpdateProgressbar(int num, float filesize, double ElapsedSeconds)
        {
            if (Current < filesize)
                Current += num;

            Percentage = (int)Math.Round((double)(100f * Current) / filesize);

            double Transferspeed = CalcTransferSpeed(num, ElapsedSeconds);
            Title = $"BFs {Percentage}% | {Transferspeed:0.0} MB/s | Estimated transfer time: {CalcEstimatedTime(filesize - Current, Transferspeed)}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CalcTransferSpeed(double filesize, double time)
            => (filesize / 1000 / 1000) / time;

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
            
            if (file.StartsWith("\""))
                file = file[1..];

            if (file.EndsWith("\""))
                file = file[..^1];

            var fi = new FileInfo(file);

            if (!fi.Exists)
            {
                WriteLine("File does not exist");
                return GetFile();
            }

            return fi;
        }

        private static async Task Transport(Stream a, Stream b, float filesize)
        {
            var CurrentTime = DateTime.Now;
            int BytesSent = 0;
            double Milliseconds = .5d;

            while (true)
            {
                int num = DoAsync ? await a.ReadAsync(buffersize, 0, buffersize.Length) : a.Read(buffersize, 0, buffersize.Length);
                BytesSent += num;

                if (num <= 0)
                {
                    UpdateProgressbar(BytesSent, filesize, Milliseconds);
                    break;
                }

                if (DoAsync)
                    await b.WriteAsync(buffersize, 0, num);
                else
                    b.Write(buffersize, 0, num);

                if (DateTime.Now - CurrentTime < TimeSpan.FromSeconds(Milliseconds))
                    continue;

                CurrentTime = DateTime.Now;
                UpdateProgressbar(BytesSent, filesize, Milliseconds);
                BytesSent = 0;
            }

            Title = $"BFs {Percentage}% | Done!";
        }

        public static async Task SendLogic(NetworkStream nwStream, FileInfo fi, EndPoint RemoteIP, bool ShowRemoteEndPoint)
        {
            WriteLine("Connected!");

            if (ShowRemoteEndPoint)
                WriteLine($"Connection established with {RemoteIP}");

            SendFileSize(nwStream, fi.Length);
            await Task.Delay(1000);

            SendFileName(nwStream, fi.Name);
            await Task.Delay(1000);

            await using FileStream strm = fi.OpenRead();
            WriteLine("Sending the file...");
            await Transport(strm, nwStream, fi.Length);
        }

        public static async Task ReceiveLogic(TcpClient client, NetworkStream nwStream, bool ShowRemoteEndPoint)
        {
            WriteLine("Connected!");

            if (ShowRemoteEndPoint)
                WriteLine("Connection accepted from " + client.Client.RemoteEndPoint);

            GetFileSize(nwStream, client);
            await Task.Delay(1000);

            GetFileName(nwStream, client);
            await Task.Delay(1000);

            await using FileStream strm = new FileStream(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{Filename}", FileMode.OpenOrCreate);
            WriteLine("Receiving the file...");
            await Transport(nwStream, strm, Filesize);
        }

        private static void GetFileSize(NetworkStream nwStream, TcpClient client)
        {
            byte[] ReceiveBuffer = new byte[client.ReceiveBufferSize];
            int nwRead = nwStream.Read(ReceiveBuffer, 0, ReceiveBuffer.Length);
            WriteLine("Receiving the filesize...");

            string tmp = Encoding.ASCII.GetString(ReceiveBuffer, 0, nwRead);
            nwStream.Flush();

            Filesize = long.Parse(tmp);
        }

        private static void GetFileName(NetworkStream nwStream, TcpClient client)
        {
            byte[] ReceiveBuffer = new byte[client.ReceiveBufferSize];
            int nwRead = nwStream.Read(ReceiveBuffer, 0, ReceiveBuffer.Length);
            WriteLine("Receiving the filename...");

            Filename = Encoding.ASCII.GetString(ReceiveBuffer, 0, nwRead);
            nwStream.Flush();
        }

        private static void SendFileSize(NetworkStream nwStream, long FileLength)
        {
            WriteLine("Sending the filesize...");
            byte[] name = Encoding.UTF8.GetBytes(FileLength.ToString());
            nwStream.Write(name, 0, name.Length);
            nwStream.Flush();
        }

        private static void SendFileName(NetworkStream nwStream, string FileName)
        {
            WriteLine("Sending the filename...");
            byte[] name = Encoding.UTF8.GetBytes(FileName);
            nwStream.Write(name, 0, name.Length);
            nwStream.Flush();
        }

        public enum IPVersion
        {
            IPV4,
            IPV6
        }

        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);
    }
}
