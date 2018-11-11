using System;
using System.Collections.Generic;
using System.Data.SQLite;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto.bdd;
using Badger2018.utils;
using Badger2018.utils.sqlite;
using BadgerCommonLibrary.utils;

namespace Badger2018.services.bddLastLayer
{
    public class BadgeageBddLayer
    {
        private const String TableBadgeages = "BADGEAGES";
        private const String ViewBadgeagesFull = "BADGEAGES_TYPE";

        private static readonly Logger _logger = Logger.LastLoggerInstance;


        public static void InsertNewBadgeage(DbbAccessManager dbbManager, int typeBadgeage, DateTime dateTime, String relationKey)
        {
            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("DATE_BADGE", dateTime);
            lstUpd.Add("TYPE_BADGE", typeBadgeage);
            lstUpd.Add("TIME_BADGE", dateTime.TimeOfDay);
            lstUpd.Add("RELATION_KEY", relationKey);
            lstUpd.Add("DT_ADDED", AppDateUtils.DtNow(), ListSqlLiteKVPair.AddOptions.DateTimeToStrDateAndTime);

            _logger.Debug("InsertNewBadgeage : " + lstUpd.ToString());

            string sql = lstUpd.InserOrderStr(TableBadgeages);
            command = new SQLiteCommand(sql, dbbManager.Connection);
            lstUpd.AddSqlParams(command);

            if (command.ExecuteNonQuery() != 1)
            {
                throw new Exception("Erreur lors de l'ajout du badgeage");
            }
        }

        public static void RemoveBadgeagesOfAday(DbbAccessManager dbbManager, DateTime dateTime)
        {
            SQLiteCommand command = null;

            string sql = String.Format(SqlConstants.DELETE_WHERE, TableBadgeages, "DATE_BADGE=@DATEBADGE");

            command = new SQLiteCommand(sql, dbbManager.Connection);

            command.Parameters.Add(new SQLiteParameter("@DATEBADGE", dateTime.ToString("yyyy-MM-dd")));


            if (command.ExecuteNonQuery() == -1)
            {
                throw new Exception("Erreur lors de l'ajout de la suppression des badgeages du jours");
            }
        }

        internal static bool IsBadgeageExistFor(DbbAccessManager dbbManager, DateTime date, int index = -1, string relationKey = null)
        {
            SQLiteCommand command = null;
            string whereClause = "DATE_BADGE=@DATEBADGE";



            if (relationKey == null && index == -1)
            {
                whereClause += " AND TYPE_BADGE BETWEEN 0 AND 4";
            }
            else if (index > -1)
            {
                whereClause += " AND TYPE_BADGE=@TYPE_BADGE";
            }

            if (relationKey != null)
            {
                whereClause += " AND RELATION_KEY=@RELATION_KEY";
            }

            string sql = String.Format(SqlConstants.SELECT_COUNT_ALL_WHERE, TableBadgeages, whereClause);

            command = new SQLiteCommand(sql, dbbManager.Connection);

            command.Parameters.Add(new SQLiteParameter("@DATEBADGE", date.ToString("yyyy-MM-dd")));
            if (relationKey != null)
            {
                command.Parameters.Add(new SQLiteParameter("@RELATION_KEY", relationKey));
            }
            if (index > -1)
            {
                command.Parameters.Add(new SQLiteParameter("@TYPE_BADGE", index));
            }


            return (long)command.ExecuteScalar() >= 1;
            ;
        }



        internal static string GetBadgeageTimeStrFor(DbbAccessManager dbbManager, int index, DateTime date, string relationKey = null)
        {
            SQLiteCommand command = null;
            string whereClause = "DATE_BADGE=@DATEBADGE AND TYPE_BADGE=@TYPE_BADGE";
            if (relationKey != null)
            {
                whereClause += " AND RELATION_KEY=@RELATION_KEY";
            }
            string sql = String.Format(SqlConstants.SELECT_COL_WHERE, "TIME_BADGE", TableBadgeages, whereClause);

            command = new SQLiteCommand(sql, dbbManager.Connection);

            command.Parameters.Add(new SQLiteParameter("@DATEBADGE", date.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new SQLiteParameter("@TYPE_BADGE", index));
            if (relationKey != null)
            {
                command.Parameters.Add(new SQLiteParameter("@RELATION_KEY", relationKey));
            }

            return (string)command.ExecuteScalar();

        }

        public static List<BadgeageEntryDto> GetListBadgeagesOf(DbbAccessManager dbbManager, DateTime date, int index, string relationKey)
        {
            List<BadgeageEntryDto> lstRet = new List<BadgeageEntryDto>();

            SQLiteCommand command = null;
            string whereClause = "DATE_BADGE=@DATEBADGE AND TYPE_BADGE=@TYPE_BADGE";
            if (relationKey != null)
            {
                whereClause += " AND RELATION_KEY=@RELATION_KEY";
            }
            string sql = String.Format(SqlConstants.SELECT_ALL_WHERE, TableBadgeages, whereClause);

            command = new SQLiteCommand(sql, dbbManager.Connection);

            command.Parameters.Add(new SQLiteParameter("@DATEBADGE", date.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new SQLiteParameter("@TYPE_BADGE", index));
            if (relationKey != null)
            {
                command.Parameters.Add(new SQLiteParameter("@RELATION_KEY", relationKey));
            }

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    BadgeageEntryDto b = HydrateBadgeageDto(reader);

                    lstRet.Add(b);

                }
            }

            return lstRet;
        }



        internal static List<BadgeageEntryDto> GetBadgeMedianneTime(DbbAccessManager dbbManager, int typeBadgeage, int nbJours)
        {
            List<BadgeageEntryDto> lstRet = new List<BadgeageEntryDto>();

            SQLiteCommand command = null;

            string sql = String.Format(SqlConstants.SELECT_ALL_WHERE, TableBadgeages, "TYPE_BADGE = @TYPE_BADGE order by date_badge desc limit @LIMIT");

            command = new SQLiteCommand(sql, dbbManager.Connection);

            command.Parameters.Add(new SQLiteParameter("@TYPE_BADGE", typeBadgeage));
            command.Parameters.Add(new SQLiteParameter("@LIMIT", nbJours));


            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    BadgeageEntryDto b = HydrateBadgeageDto(reader);

                    lstRet.Add(b);
                }
            }

            return lstRet;
        }



        private static BadgeageEntryDto HydrateBadgeageDto(SQLiteDataReader reader)
        {
            BadgeageEntryDto b = new BadgeageEntryDto();
            string dateStr = reader.GetStringByColName("DATE_BADGE");
            string timeStr = reader.GetStringByColName("TIME_BADGE");
            if (timeStr != null)
            {
                b.DateTime = DateTime.Parse(dateStr + " " + timeStr);
            }
            else
            {
                b.DateTime = DateTime.Parse(dateStr);
            }

            int typeBadgeInt = reader.GetInt32ByColName("TYPE_BADGE");
            b.TypeBadge = EnumBadgeageType.GetFromIndex(typeBadgeInt);

            b.RelationKey = reader.GetStringByColName("RELATION_KEY");

            string dateAddedStr = reader.GetStringByColName("DT_ADDED");
            if (!StringUtils.IsNullOrWhiteSpace(dateAddedStr))
            {
                b.DateAdded = DateTime.Parse(dateAddedStr);
            }
            return b;
        }
    }
}
