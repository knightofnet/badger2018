using System;
using System.Data.SQLite;
using AryxDevLibrary.utils.logger;
using Badger2018.business.dbb;
using Badger2018.constants;
using Badger2018.dto.bdd;
using Badger2018.utils.sqlite;

namespace Badger2018.services.bddLastLayer
{
    class AbsencesBddLayer
    {

        private const String TableAbsences = Cst.TableAbsences;


        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public static void InsertAbsence(DbbAccessManager dbbManager, AbsencesEntryDto absence)
        {
            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("DATE_JOUR", absence.DateJour);
            lstUpd.Add("PART_JOUR", absence.PartJour.Index);
            lstUpd.Add("TY_ABS", absence.TyAbs.Index);

            _logger.Debug("InsertNewBadgeage : " + lstUpd.ToString());

            string sql = lstUpd.InserOrderStr(TableAbsences);
            command = new SQLiteCommand(sql, dbbManager.Connection);
            lstUpd.AddSqlParams(command);

            if (command.ExecuteNonQuery() != 1)
            {
                throw new Exception("Erreur lors de l'ajout d'une nouvelle absence");
            }
        }

        public static void RemoveAbsence(DbbAccessManager dbbManager, AbsencesEntryDto absence)
        {
            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("DATE_JOUR", absence.DateJour);
            lstUpd.Add("PART_JOUR", absence.PartJour.Index);
            lstUpd.Add("TY_ABS", absence.TyAbs.Index);

            _logger.Debug("InsertNewBadgeage : " + lstUpd.ToString());

            string sql = lstUpd.DeleteWhereStr(TableAbsences);
            command = new SQLiteCommand(sql, dbbManager.Connection);
            lstUpd.AddSqlParams(command);

            if (command.ExecuteNonQuery() != 1)
            {
                throw new Exception("Erreur lors de l'ajout d'une nouvelle absence");
            }
        }


    }
}
