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

        private readonly List<TcpClient> clients = new List<TcpClient>();
        private readonly List<NetworkStream> nwStream = new List<NetworkStream>();
        private bool FinishedDoingTranser = false;

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
            WriteLine("input: ");
            string input = ReadLine();

            if (input == "Start")
            {
                Write("Number of participants (receiver + sender) : ");
                int participants = int.Parse(ReadLine());

                WriteLine("Listening...");

                listener = TcpListener.Create(1604);
                listener.Start();

                try
                {
                    int i = 0;

                    while (true)
                    {
                        if (FinishedDoingTranser)
                            break;

                        else if (i < participants)
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
                        }
                        else if (i > participants)
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
                    listener.Stop();
                }

                listener.Stop();
                ClearLists();
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

            MemoryStream ms = new MemoryStream();

            WriteLine("Receiving and sending the file...");
            bool SendMissingParts = true;

            while (true)
            {
                int num = nwStream[clientPos].Read(InternetProtocol.buffersize, 0, InternetProtocol.buffersize.Length);

                if (num <= 0 && InternetProtocol.Current == InternetProtocol.Filesize)
                    break;

                long msPos = ms.Position;

                if (num > 0)
                    ms.Write(InternetProtocol.buffersize, 0, num);

                if (InternetProtocol.Percentage >= 10)
                {
                    void Send()
                    {
                        int num2 = ms.Read(InternetProtocol.buffersize, 0, InternetProtocol.buffersize.Length);

                        for (int i = 0; i < clients.Count(); i++)
                        {
                            if (i == clientPos)
                                continue;

                            nwStream[i].Write(InternetProtocol.buffersize, 0, num2);
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

                            InternetProtocol.UpdateProgressbar(num, InternetProtocol.Filesize);
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
                            clients.Remove(client);
                            nwStream.Remove(nwStream[clientPos]);
                        }
                        else
                        {
                            WriteLine(e.Message);
                        }
                    }
                }

                InternetProtocol.UpdateProgressbar(num, InternetProtocol.Filesize);

                WriteLine($"BFs {InternetProtocol.Percentage}%");
            }

            ms.Close();

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
