﻿using System;
using System.Net.Sockets;
using static System.Console;

namespace BFs
{
    public class PortReq
    {
        public PortReq(bool Receive)
        {
            InternetProtocol.WriteToClipboard(InternetProtocol.DownloadIP(InternetProtocol.IPVersion.IPV4).Result);

            Start(Receive);
            ReadLine();
        }

        private async void Start(bool Receive)
        {
            try
            {
                WriteLine("Waiting for a connection...");

                TcpListener listener = TcpListener.Create(1604);
                listener.Start();

                using TcpClient client = await listener.AcceptTcpClientAsync();
                using NetworkStream nwStream = client.GetStream();

                client.ReceiveTimeout = int.MaxValue;
                client.ReceiveBufferSize = InternetProtocol.buffersize.Length;

                if (client.Connected)
                {
                    if (Receive)
                        await InternetProtocol.ReceiveLogic(client, nwStream, true);
                    else
                        await InternetProtocol.SendLogic(nwStream, InternetProtocol.GetFile(), client.Client.RemoteEndPoint, true);

                    listener.Stop();
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