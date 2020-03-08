using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BFs
{
    public static class InternetProtocol
    {
        // ReSharper disable once InconsistentNaming
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

            Console.WriteLine("Code has been pasted into your clipboard");

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

        // ReSharper disable once InconsistentNaming
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
