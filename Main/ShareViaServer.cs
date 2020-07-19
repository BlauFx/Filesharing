using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class ShareViaServer
    {
        private TcpListener listener;

        private readonly List<Tuple<TcpClient, NetworkStream>> clients = new List<Tuple<TcpClient, NetworkStream>>();
        private bool FinishedTransfer = false;

        public ShareViaServer(bool ConnectToServer)
        {
            Clear();

            if (ConnectToServer)
            {
                Client();
                ReadLine();
            }
            else
            {
                Server();
                ReadLine();
            }
        }

        private void Client()
        {
            Write("Do you want to send or receive a file? [1/2]: ");
            string input = ReadLine();

            if (input == "1")
            {
                new SendNoPort();
            }
            else if (input == "2")
            {
                new ReceiverNoPort();
            }
        }

        private async void Server()
        {
            Write("Number of participants (receiver + sender): ");
            int participants = int.Parse(ReadLine());

            WriteLine("Listening...");

            listener = TcpListener.Create(1604);
            listener.Start();

            try
            {
                int i = 0;

                while (!FinishedTransfer)
                {
                    if (i < participants)
                    {
                        WriteLine("Waiting for a connection...");

                        var client = await listener.AcceptTcpClientAsync();
                        clients.Add(new Tuple<TcpClient, NetworkStream>(client, client.GetStream()));

                        int clientPos = clients.Count() - 1;

                        WriteLine($"{clientPos} | {clients[clientPos].Item1.Client.RemoteEndPoint} has connected");
                    }
                    else if (i == participants)
                    {
                        HandleClient(clients);
                    }
                    else
                    {
                        i = participants + 1;
                        Thread.Sleep(10000);
                    }

                    i++;
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(ObjectDisposedException))
                    WriteLine("The server has been shutdown");
                else
                    WriteLine(e.Message);
            }

            clients.Clear();
            listener?.Stop();
        }

        public async void HandleClient(List<Tuple<TcpClient, NetworkStream>> clients)
        {
            int clientPos = clients.Count() - 1;
            using TcpClient client = clients[clientPos].Item1;

            InternetProtocol.GetFileSize(clients[clientPos].Item2, client);
            await Task.Delay(1000);

            InternetProtocol.GetFileName(clients[clientPos].Item2, client);
            await Task.Delay(1000);

            for (int i = 0; i < clients.Count(); i++)
            {
                if (i == clientPos)
                    continue;

                InternetProtocol.SendFileSize(clients[i].Item2, InternetProtocol.Filesize);
                await Task.Delay(1000);

                InternetProtocol.SendFileName(clients[i].Item2, InternetProtocol.Filename);
                await Task.Delay(1000);
            }

            WriteLine("Receiving and sending the file...");
            MemoryStream ms = new MemoryStream();

            bool SendMissingParts = true;
            DateTime CurrentTime;
            double Milliseconds = .5d;
            int BytesSent = 0;

            void DoStuffWithProgressbar()
            {
                if (DateTime.Now - CurrentTime >= TimeSpan.FromSeconds(Milliseconds))
                {
                    CurrentTime = DateTime.Now;
                    InternetProtocol.UpdateProgressbar(BytesSent, InternetProtocol.Filesize, Milliseconds);
                    BytesSent = 0;
                }
            }

            CurrentTime = DateTime.Now;

            while (true)
            {
                int num;

                if (InternetProtocol.DoAsync)
                    num = await clients[clientPos].Item2.ReadAsync(InternetProtocol.buffersize, 0, InternetProtocol.buffersize.Length);
                else
                    num = clients[clientPos].Item2.Read(InternetProtocol.buffersize, 0, InternetProtocol.buffersize.Length);

                BytesSent += num;

                if (num <= 0 && InternetProtocol.Current == InternetProtocol.Filesize)
                    break;

                long msPos = ms.Position;

                if (num > 0)
                    await ms.WriteAsync(InternetProtocol.buffersize, 0, num);

                if (InternetProtocol.Percentage >= 10)
                {
                    async void Send()
                    {
                        int num2 = await ms.ReadAsync(InternetProtocol.buffersize, 0, InternetProtocol.buffersize.Length);

                        for (int i = 0; i < clients.Count(); i++)
                        {
                            if (i == clientPos)
                                continue;

                            if (InternetProtocol.DoAsync)
                                await clients[i].Item2.WriteAsync(InternetProtocol.buffersize, 0, num2);
                            else
                                clients[i].Item2.Write(InternetProtocol.buffersize, 0, num2);
                        }
                    }

                    try
                    {
                        if (SendMissingParts)
                        {
                            SendMissingParts = false;
                            ms.Position = 0;

                            while (ms.Position < InternetProtocol.Current)
                                Send();

                            DoStuffWithProgressbar();
                            continue;
                        }

                        ms.Position = msPos;
                        Send();

                        byte[] data = ms.ToArray().Skip((int)ms.Position).ToArray();
                        ms = new MemoryStream();
                        ms.Write(data, 0, data.Length);
                    }
                    catch (Exception e)
                    {
                        if (!client.Client.Connected)
                        {
                            WriteLine($"{client.Client.RemoteEndPoint} has disconnected");
                            clients.Remove(new Tuple<TcpClient, NetworkStream>(client, clients[clientPos].Item2));
                        }
                        else
                            WriteLine(e.Message);
                    }
                }

                var oldpercentage = InternetProtocol.Percentage;
                DoStuffWithProgressbar();

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && oldpercentage != InternetProtocol.Percentage)
                    WriteLine($"BFs {InternetProtocol.Percentage}%");
            }

            ms.Close();

            clients.Clear();
            WriteLine("Done!");
            FinishedTransfer = true;
        }
    }
}
