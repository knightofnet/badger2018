using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AryxDevLibrary.utils.cliParser;
using AryxDevLibrary.utils.logger;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;
using WaveCompagnonPlayer.business;
using WaveCompagnonPlayer.business.job;
using WaveCompagnonPlayer.dto;

namespace WaveCompagnonPlayer
{
    class Program
    {

        private static Logger _logger = null;
        static void Main(string[] args)
        {
            string asDir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

            _logger = new Logger(Path.Combine(asDir, CommonCst.WavePlayerLogFile), CommonCst.ConsoleLogLvl,
        CommonCst.FileLogLvl, "1 Mo");


            AppArgsParser argsParser = null;
            AppArgsDto prgArgs = null;
            try
            {


                argsParser = new AppArgsParser();
                prgArgs = argsParser.ParseDirect(args);
                if (prgArgs.IsDebugMode)
                {
                    _logger = new Logger(CommonCst.WavePlayerLogFile, Logger.LogLvl.DEBUG,   Logger.LogLvl.DEBUG, "1 Mo");
                }


            }
            catch (CliParsingException cli)
            {
                Console.WriteLine();
                Console.WriteLine("{0} (v. {1})",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version);
                if (args.Length > 0)
                {
                    Console.WriteLine(cli.Message);
                }
                argsParser.ShowSyntax();
                Console.WriteLine();

                Console.WriteLine("  Codes sortie du programme :");
                Console.WriteLine("   {0} : {1}", EnumExitCodes.OK.ExitCodeInt, EnumExitCodes.OK.Libelle);
                Console.WriteLine("   {0} : {1}", EnumExitCodes.W_ERROR_IN_PARAMS.ExitCodeInt, EnumExitCodes.W_ERROR_IN_PARAMS.Libelle);
                Console.WriteLine("   {0} : {1}", EnumExitCodes.W_ERROR_UNKNOW.ExitCodeInt, EnumExitCodes.W_ERROR_UNKNOW.Libelle);

                Console.WriteLine();

                Exit(EnumExitCodes.W_ERROR_IN_PARAMS.ExitCodeInt);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Warn(ex.StackTrace);

                Exit(EnumExitCodes.W_ERROR_IN_PARAMS.ExitCodeInt);
            }


            try
            {
                IJobInterface jobInstance = null;
                if (EnumWaveCompModeTraitement.ShowDevicesMode.Equals(prgArgs.WaveCompModeTraitements))
                {
                    jobInstance = new ShowSoundDevicesJob();
                }
                else if (EnumWaveCompModeTraitement.PlayEnumWaveCompSoundMode.Equals(prgArgs.WaveCompModeTraitements))
                {
                    jobInstance = new PlaySoundJob();
                }
                else if (EnumWaveCompModeTraitement.FullDumpMode.Equals(prgArgs.WaveCompModeTraitements))
                {
                    jobInstance = new FullDumpJob();
                }
                else if (EnumWaveCompModeTraitement.TestAllDeviceMode.Equals(prgArgs.WaveCompModeTraitements))
                {
                    jobInstance = new TestAllPlayerDeviceJob();
                } else if (EnumWaveCompModeTraitement.DaemonWaitingOrders.Equals(prgArgs.WaveCompModeTraitements))
                {
                    jobInstance = new DaemonWaitingOrdersJob();
                }

                jobInstance.DoJob(prgArgs);

            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex);
                Exit(EnumExitCodes.W_ERROR_UNKNOW.ExitCodeInt);

            }


            Exit(EnumExitCodes.OK.ExitCodeInt);
        }

        private static void Exit(int exitCode = 0)
        {
            Environment.Exit(exitCode);
        }
    }
}
