using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace BFs
{
    public class ShareViaServer
    {
        private TcpListener listener;

        private List<TcpClient> clients = new List<TcpClient>();
        private List<NetworkStream> nwStream = new List<NetworkStream>();
        private bool FinishedDoingTranser = true;

        public ShareViaServer(bool Connect)
        {
            Clear();

            if (Connect)
            {
                Send();
                ReadLine();
            }
            else
            {
                Receive();
                ReadLine();
            }
        }

        private void Send()
        {
            Write("Do you want to send or receive a file? [1/2]: ");
            string input = ReadLine();

            if (input == "1")
            {
                new Send();
            }
            else if (input == "2")
            {
                new ReceiverWPort();
            }
        }

        private async void Receive()
        {
            while (true)
            {
                string input = ReadLine();

                if (input == "Start")
                {
                    Write("Number of participants (receiver + sender) : ");
                    int participants = int.Parse(Console.ReadLine());

                    WriteLine("Listening...");

                    listener = TcpListener.Create(1604);
                    listener.Start();

                    try
                    {
                        int i = 0;

                        while (FinishedDoingTranser)
                        {
                            if (i < participants)
                            {
                                WriteLine("Waiting for a connection...");

                                clients.Add(await listener.AcceptTcpClientAsync());
                                nwStream.Add(clients[clients.Count() - 1].GetStream());

                                int clientPos = clients.Count() - 1;
                                WriteLine($"{clientPos} | {clients[clientPos].Client.RemoteEndPoint} has connected");
                            }
                            else if (i == participants)
                            {
                                new Thread(new ParameterizedThreadStart(HandleClient)).Start(new object[3]
                                {
                                    clients,
                                    nwStream,
                                    clients.Count() - 1,
                                });

                                FinishedDoingTranser = false;
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
                        listener.Stop();
                    }
                }
                else if (input == "Stop")
                {
                    listener.Stop();
                    ClearLists();
                }

                Thread.Sleep(60000);
            }
        }

        public async void HandleClient(object args)
        {
            Array array = (Array)args;

            List<TcpClient> clients = (List<TcpClient>)array.GetValue(0);
            List<NetworkStream> nwStream = (List<NetworkStream>)array.GetValue(1);

            int clientPos = (int)array.GetValue(2);
            using TcpClient client = clients[clientPos];

            InternetProtocol.GetFileSize(nwStream[clientPos], client);
            await Task.Delay(1000);

            InternetProtocol.GetFileName(nwStream[clientPos], client);
            await Task.Delay(1000);

            for (int i = 0; i < clients.Count(); i++)
            {
                if (i == clientPos)
                    continue;

                InternetProtocol.SendFileSize(nwStream[i], InternetProtocol.Filesize);
                await Task.Delay(1000);

                InternetProtocol.SendFileName(nwStream[i], InternetProtocol.Filename);
                await Task.Delay(1000);
            }

            using MemoryStream ms = new MemoryStream();

            WriteLine("Receiving and sending the file...");

            bool SendMissingParts = true;

            while (true)
            {
                int num = nwStream[clientPos].Read(InternetProtocol.buffersize, 0, InternetProtocol.buffersize.Length);

                if (num <= 0 && ms.Length == InternetProtocol.Filesize)
                    break;

                long msPos1 = ms.Position;

                if (num > 0)
                    ms.Write(InternetProtocol.buffersize, 0, num);

                if (true)
                {
                    for (int i = 0; i < clients.Count(); i++)
                    {
                        try
                        {
                            if (i == clientPos)
                                continue;

                            //if (SendMissingParts)
                            //{
                            //    SendMissingParts = false;

                            //    ms.Position = 0;

                            //    while (ms.Position < ms.Length)
                            //    {
                            //        int num2 = ms.Read(InternetProtocol.buffersize, 0, InternetProtocol.buffersize.Length);
                            //        nwStream[i].Write(InternetProtocol.buffersize, 0, num2);
                            //    }
                            //    continue;
                            //}

                            ms.Position = msPos1;

                            int num3 = ms.Read(InternetProtocol.buffersize, 0, InternetProtocol.buffersize.Length);
                            nwStream[i].Write(InternetProtocol.buffersize, 0, num3);
                        }
                        catch (Exception e)
                        {
                            if (!client.Client.Connected)
                            {
                                WriteLine($"{client.Client.RemoteEndPoint} has disconnected");
                                clients.Remove(client);
                                nwStream.Remove(nwStream[i]);
                            }
                            else
                            {
                                WriteLine(e.Message);
                            }
                        }
                    }
                }

                InternetProtocol.UpdateProgressbar(num, InternetProtocol.Filesize);
            }

            ClearLists();
            WriteLine("Done!");
            FinishedDoingTranser = true;
        }

        private void ClearLists()
        {
            for (int i = 0; i < clients.Count(); i++)
            {
                nwStream[i].Close();
                clients[i].Close();
            }

            nwStream.Clear();
            clients.Clear();
        }
    }
}
