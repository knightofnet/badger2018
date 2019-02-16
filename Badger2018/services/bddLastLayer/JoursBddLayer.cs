using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.utils;
using Badger2018.utils.sqlite;
using BadgerCommonLibrary.utils;

namespace Badger2018.services.bddLastLayer
{
    class JoursBddLayer
    {

        private const String TableBadgeages = Cst.TableJours;

        public static bool IsJourExistFor(DbbAccessManager dbbManager, DateTime date)
        {
            SQLiteCommand command = null;

            string sql = String.Format(SqlConstants.SELECT_COUNT_ALL_WHERE, TableBadgeages, "DATE_JOUR=@DATEJOUR");

            command = new SQLiteCommand(sql, dbbManager.Connection);

            command.Parameters.Add(new SQLiteParameter("@DATEJOUR", date.ToString("yyyy-MM-dd")));

            return (long)command.ExecuteScalar() >= 1;
        }

        public static bool UpdateJour(DbbAccessManager dbbManager, DateTime date, JourEntryDto jourEntryDto)
        {
            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("TYPE_JOUR", jourEntryDto.TypeJour.Index);
            lstUpd.Add("ETAT_BADGER", jourEntryDto.EtatBadger);
            lstUpd.Add("IS_COMPLETE", jourEntryDto.IsComplete);
            lstUpd.Add("OLD_ETAT_BADGER", jourEntryDto.OldEtatBadger);
            lstUpd.Add("TPS_TRAV_SECONDE", jourEntryDto.TpsTravaille);

            string sql = String.Format(SqlConstants.UPDATE_WHERE, TableBadgeages, lstUpd.UpdateClauseStr(), "DATE_JOUR=@DATE_JOUR");

            command = new SQLiteCommand(sql, dbbManager.Connection);

            lstUpd.AddSqlParams(command);
            command.Parameters.Add(new SQLiteParameter("@DATE_JOUR", date.ToString("yyyy-MM-dd")));

            return command.ExecuteNonQuery() == 1;
        }

        public static bool InsertNewJour(DbbAccessManager dbbManager, DateTime date, PointageElt pointageElt)
        {
            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("DATE_JOUR", date);
            lstUpd.Add("TYPE_JOUR", pointageElt.TypeJournee);
            lstUpd.Add("ETAT_BADGER", pointageElt.EtatBadger);
            lstUpd.Add("IS_COMPLETE", pointageElt.IsComplete);
            lstUpd.Add("OLD_ETAT_BADGER", pointageElt.OldEtatBadger);

            string sql = lstUpd.InserOrderStr(TableBadgeages);

            command = new SQLiteCommand(sql, dbbManager.Connection);

            lstUpd.AddSqlParams(command);

            return command.ExecuteNonQuery() == 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbbManager"></param>
        /// <param name="date"></param>
        /// <param name="isLtOrGt">true (DATE_JOUR<@DATE_JOUR), false (DATE_JOUR>@DATE_JOUR)</param>
        /// <returns></returns>
        internal static JourEntryDto GetPreviousOrNextDayOf(DbbAccessManager dbbManager, DateTime date, bool isLtOrGt)
        {
            // select * from JOURS where date_jour < "2018-10-22" order by date_jour desc limit 1

            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("DATE_JOUR", date);


            string sql = String.Format(SqlConstants.SELECT_ALL_WHERE, TableBadgeages,
                String.Format("DATE_JOUR{0}@DATE_JOUR order by date_jour {1} limit 1",
                isLtOrGt ? "<" : ">",
                isLtOrGt ? "desc" : "asc"));

            command = new SQLiteCommand(sql, dbbManager.Connection);

            lstUpd.AddSqlParams(command);

            JourEntryDto jourEntryDto = new JourEntryDto();
            jourEntryDto.IsHydrated = false;
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {

                    jourEntryDto.IsComplete = reader.GetBooleanByColName("IS_COMPLETE");
                    jourEntryDto.EtatBadger = reader.GetInt32ByColName("ETAT_BADGER");
                    jourEntryDto.OldEtatBadger = reader.GetInt32ByColName("OLD_ETAT_BADGER");
                    jourEntryDto.TypeJour = EnumTypesJournees.GetFromIndex(reader.GetInt32ByColName("TYPE_JOUR"));
                    jourEntryDto.DateJour = reader.GetDatetimeByColName("DATE_JOUR").Value;
                    jourEntryDto.TpsTravaille = reader.GetTimeSpanByColName("TPS_TRAV_SECONDE");

                    jourEntryDto.IsHydrated = true;
                    break;
                }
            }

            return jourEntryDto;

        }

        public static void GetJourData(DbbAccessManager dbbManager, DateTime date, PointageElt pointageElt)
        {
            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("DATE_JOUR", date);


            string sql = String.Format(SqlConstants.SELECT_ALL_WHERE, TableBadgeages, "DATE_JOUR=@DATE_JOUR");

            command = new SQLiteCommand(sql, dbbManager.Connection);

            lstUpd.AddSqlParams(command);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    pointageElt.IsComplete = ((Int64)reader["IS_COMPLETE"]) == 0;
                    pointageElt.EtatBadger = (int)(Int64)reader["ETAT_BADGER"];
                    pointageElt.OldEtatBadger = (int)(Int64)reader["OLD_ETAT_BADGER"];
                    pointageElt.TypeJournee = (int)(Int64)reader["TYPE_JOUR"];

                }
            }
        }

        public static JourEntryDto GetJourDataNext(DbbAccessManager dbbManager, DateTime date)
        {
            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("DATE_JOUR", date);


            string sql = String.Format(SqlConstants.SELECT_ALL_WHERE, TableBadgeages, "DATE_JOUR=@DATE_JOUR");

            command = new SQLiteCommand(sql, dbbManager.Connection);

            lstUpd.AddSqlParams(command);

            JourEntryDto jourEntryDto = new JourEntryDto();
            jourEntryDto.IsHydrated = false;
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    /*
                    jourEntryDto.IsComplete = ((Int64)reader["IS_COMPLETE"]) == 0;
                    jourEntryDto.EtatBadger = (int)(Int64)reader["ETAT_BADGER"];
                    jourEntryDto.OldEtatBadger = (int)(Int64)reader["OLD_ETAT_BADGER"];
                    jourEntryDto.TypeJour = EnumTypesJournees.GetFromIndex((int)(Int64)reader["TYPE_JOUR"]);
                    jourEntryDto.DateJour = DateTime.Parse((string)reader["DATE_JOUR"]);
                    */
                    jourEntryDto.IsComplete = reader.GetBooleanByColName("IS_COMPLETE");
                    jourEntryDto.EtatBadger = reader.GetInt32ByColName("ETAT_BADGER");
                    jourEntryDto.OldEtatBadger = reader.GetInt32ByColName("OLD_ETAT_BADGER");
                    jourEntryDto.TypeJour = EnumTypesJournees.GetFromIndex(reader.GetInt32ByColName("TYPE_JOUR"));
                    jourEntryDto.DateJour = reader.GetDatetimeByColName("DATE_JOUR").Value;
                    jourEntryDto.TpsTravaille = reader.GetTimeSpanByColName("TPS_TRAV_SECONDE");

                    jourEntryDto.IsHydrated = true;
                    break;
                }
            }

            return jourEntryDto;
        }

        public static DateTime? GetFirstDayOfHistory(DbbAccessManager dbbManager)
        {
            // select DATE_JOUR from JOURS order by DATE_JOUR asc limit 1

            SQLiteCommand command = null;

            DateTime? retDateTime = null;

            string sql = String.Format(SqlConstants.SELECT_COL_ORDERBY, "DATE_JOUR", TableBadgeages, "DATE_JOUR asc LIMIT 1");

            command = new SQLiteCommand(sql, dbbManager.Connection);


            JourEntryDto jourEntryDto = new JourEntryDto();
            jourEntryDto.IsHydrated = false;
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    retDateTime = reader.GetDatetimeByColName("DATE_JOUR");
                    break;
                }

            }

            return retDateTime;
        }
    }
}
