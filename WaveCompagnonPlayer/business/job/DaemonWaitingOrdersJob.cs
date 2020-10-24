using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AudioSwitcher.AudioApi.CoreAudio;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WaveCompagnonPlayer.dto;
using WaveCompagnonPlayer.utils;
using ExceptionHandlingUtils = BadgerCommonLibrary.utils.ExceptionHandlingUtils;

namespace WaveCompagnonPlayer.business.job
{
    class DaemonWaitingOrdersJob : IJobInterface
    {
        private TcpListener MonTcpListener;

        private static readonly Logger _logger = Logger.LastLoggerInstance;
        private bool isDaemonOkToRun = true;

        public void DoJob(AppArgsDto prgOptions)
        {
            int port = 49152;
            while(IsPortUsed(port))
            {
                port++;
            }

            FileInfo f = new FileInfo("wavePort.txt");
            if (f.Exists)
            {
                f.Delete();
            }
            File.WriteAllText(f.FullName, "" + port);

            MonTcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);




            try
            {
                using (CoreAudioController coreAudioCtrler = new CoreAudioController())
                {
                    MonTcpListener.Start();
                    while (isDaemonOkToRun)
                    {
                        using (TcpClient client = MonTcpListener.AcceptTcpClient())
                        {

                            NetworkStream nwStream = null;
                            try
                            {
                                _logger.Debug("Connection accepted.");
                               
                                nwStream = client.GetStream();

                                byte[] buffer = new byte[client.ReceiveBufferSize];

                                //---read incoming stream---
                                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                                //---convert the data received into a string---
                                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                _logger.Debug("Message reçu: '{0}'", dataReceived);

                                if (!StringUtils.IsNullOrWhiteSpace(dataReceived))
                                {
                                    RouteMessage(dataReceived, prgOptions, coreAudioCtrler);
                                }

                                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(Boolean.TrueString);
                                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                            } catch (Exception ex)
                            {
                                if (client != null && client.Connected && nwStream != null )
                                {
                                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(Boolean.FalseString);
                                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                                }

                                ExceptionHandlingUtils.LogAndHideException(ex);
                                throw ex;
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
                isDaemonOkToRun = false;
            }
            finally
            {
                if (MonTcpListener != null)
                {
                    if (MonTcpListener.Server.Connected)
                    {
                        MonTcpListener.Server.Disconnect(true);
                    }
                    MonTcpListener.Stop();
                }
                
        
                Console.WriteLine("Listener stopped.");

                if (f != null && f.Exists)
                {
                    f.Delete();
                }
            }

            Thread.Sleep(1000);

        }

        private void RouteMessage(string dataReceived, AppArgsDto prgOptions, CoreAudioController coreAudioCtrler)
        {
            if ("EXIT".Equals(dataReceived))
            {
                _logger.Debug("EXIT");
                isDaemonOkToRun = false;
            }
            else if (dataReceived.StartsWith("PLAY"))
            {
                string[] args = dataReceived.Split('#');
                prgOptions.SoundToPlay = EnumSonWindows.GetFromIndex(int.Parse(args[1]));
                prgOptions.SoundVolume = int.Parse(args[2]);
                prgOptions.SoundDevice = args[3];

                SoundUtils.PlaySound(prgOptions, coreAudioCtrler);
            }
        }

        private static bool IsPortUsed(int portNbr)
        {
            

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == portNbr)
                {
                    return true;
                }
            }

            return false;
        }
  
    }
}
