﻿using System;
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

        private static float current { get; set; } = 0;

        public static float Filesize { get; set; } = 0;

        public static string Filename { get; set; } = string.Empty;

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
            if (current < filesize)
                current += num;

            int percentComplete = (int)Math.Round((double)(100 * current) / filesize);
            Title = $"BFs {percentComplete.ToString()}%";
        }

        public static void Transport(TransportWay transportWay, NetworkStream nwStream, Stream strm, float filesize)
        {
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

                    DoWork(nwStream, strm);
                    break;
                case TransportWay.Send:
                    WriteLine("Sending the file...");

                    DoWork(strm, nwStream);
                    break;
            }
        }

        public static async Task TransportAsync(TransportWay transportWay, NetworkStream nwStream, Stream strm, float filesize)
        {
            async Task DoWork(Stream a, Stream b)
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

            switch (transportWay)
            {
                case TransportWay.Receive:
                    WriteLine("Receiving the file...");

                    await DoWork(nwStream, strm);
                    break;
                case TransportWay.Send:
                    WriteLine("Sending the file...");

                    await DoWork(strm, nwStream);
                    break;
            }
        }

        public static float GetFileSize(NetworkStream nwStream, TcpClient client)
        {
            byte[] ReceiveBuffer = new byte[client.ReceiveBufferSize];
            int nwRead = nwStream.Read(ReceiveBuffer, 0, ReceiveBuffer.Length);
            WriteLine("Receiving the filesize...");

            string tmp = Encoding.ASCII.GetString(ReceiveBuffer, 0, nwRead);
            nwStream.Flush();

            Filesize = float.Parse(tmp);
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
            //send filename
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