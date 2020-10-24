using AryxDevLibrary.utils.logger;
using BadgerCommonLibrary.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Badger2018.utils
{
    public static class TcpRequestsStore
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public static bool CloseWaveCompagnon()
        {

            TcpClient client = null;
            try
            {
                string message = "EXIT";

                client = InitClient();
                using (NetworkStream nwStream = client.GetStream())
                {

                    SendMessage(client, message, nwStream);

                }
                Thread.Sleep(500);

                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndRethrows(ex);


            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }

            return true;
        }

        public static bool PlaySoundTcp(int soundIndex, int volume, string deviceName)
        {
            TcpClient client = null;
            try
            {
                string message = String.Format("PLAY#{0}#{1}#{2}", soundIndex, volume, deviceName);

                client = InitClient();
                using (NetworkStream nwStream = client.GetStream())
                {

                    SendMessage(client, message, nwStream);

                    string returnMsg = ReadMessage(client, nwStream);


                    if (returnMsg == null || Boolean.FalseString.Equals(returnMsg))
                    {
                        throw new Exception("Play Sound Fail");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndRethrows(ex);


            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }

            return true;
        }

        private static string ReadMessage(TcpClient client, NetworkStream nwStream)
        {

            //---read back the text---
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
            string returnMsg = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
            _logger.Debug("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
            return returnMsg;

        }

        private static void SendMessage(TcpClient client, string message, NetworkStream nwStream)
        {

            byte[] bytesToSend = Encoding.ASCII.GetBytes(message);

            //---send the text---
            _logger.Debug("Sending : " + message);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);

        }

        private static TcpClient InitClient()
        {
            TcpClient client;
            int port = 49152;
            if (System.IO.File.Exists("wavePort.txt"))
            {
                string portRaw = System.IO.File.ReadAllText("wavePort.txt");
                int.TryParse(portRaw, out port);
            }

            //---create a TCPClient object at the IP and port no.---
            client = new TcpClient("127.0.0.1", port);
            return client;
        }
    }
}
