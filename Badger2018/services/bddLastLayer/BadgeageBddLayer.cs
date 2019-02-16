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
        private const String TableBadgeages = Cst.TableBadgeages;
        private const String ViewBadgeagesFull = "BADGEAGES_TYPE";

        private static readonly Logger _logger = Logger.LastLoggerInstance;


        public static void InsertNewBadgeage(DbbAccessManager dbbManager, int typeBadgeage, DateTime dateTime, String relationKey, TimeSpan? cdToSave = null)
        {
            SQLiteCommand command = null;

            ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();
            lstUpd.Add("DATE_BADGE", dateTime);
            lstUpd.Add("TYPE_BADGE", typeBadgeage);
            lstUpd.Add("TIME_BADGE", dateTime.TimeOfDay);
            lstUpd.Add("RELATION_KEY", relationKey);
            lstUpd.Add("DT_ADDED", AppDateUtils.DtNow(), ListSqlLiteKVPair.AddOptions.DateTimeToStrDateAndTime);
            if (cdToSave.HasValue)
            {
                lstUpd.Add("CD_AT_TIME", cdToSave.Value);
            }

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

            string from = TableBadgeages;
            string where = "TYPE_BADGE = @TYPE_BADGE";
            if (typeBadgeage == 1 || typeBadgeage == 3)
            {
                from = String.Format("{0} b, {1} j", TableBadgeages, Cst.TableJours);

                int typeJour = 0;
                int otherTypeBadgeage = 3;
                int otherTypeJour = 1;
                if (typeBadgeage == 3)
                {
                    otherTypeJour = 2;                    
                }
                where = String.Format( 
                    "b.DATE_BADGE = j.DATE_JOUR and ((  b.TYPE_BADGE = {0} and j.TYPE_JOUR = {1}) or ( b.TYPE_BADGE = {2} and j.TYPE_JOUR = {3}))",
                    otherTypeBadgeage,
                    otherTypeJour,
                    typeBadgeage,
                    typeJour
                    );
            } 
            

            
            string sql = String.Format(SqlConstants.SELECT_ALL_WHERE, from, where + " order by date_badge desc limit @LIMIT");

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

        public static void RemoveDuplicatesBadgeages(DbbAccessManager dbbManager)
        {
            SQLiteCommand command = null;
            String sql =
                "SELECT DATE_BADGE, TYPE_BADGE, count(*) AS C FROM BADGEAGES GROUP BY DATE_BADGE, TYPE_BADGE HAVING COUNT(*) > 1";
            command = new SQLiteCommand(sql, dbbManager.Connection);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {

                    ListSqlLiteKVPair lstUpd = new ListSqlLiteKVPair();

                    string date = reader.GetStringByColName("DATE_BADGE");
                    int tyBadge = reader.GetInt32ByColName("TYPE_BADGE");
                    int count = reader.GetInt32ByColName("C");


                    _logger.Debug("Ligne dupliquée [DATE_BADGE:{0}][TYPE_BADGE:{1}][COUNT:{2}]", date, tyBadge, count);

                    lstUpd.Add("DATE_BADGE", date);
                    lstUpd.Add("TYPE_BADGE", tyBadge);
                    //lstUpd.Add("LIMIT", count - 1);

                    SQLiteCommand commandDelete = null;
                    String sqlA = "delete from BADGEAGES where (DATE_BADGE, TYPE_BADGE) " +
                                  "IN (SELECT DATE_BADGE, TYPE_BADGE WHERE DATE_BADGE = @DATE_BADGE and TYPE_BADGE = @TYPE_BADGE) " +
                                  "and _rowid_ != (SELECT _rowid_ from BADGEAGES WHERE DATE_BADGE = @DATE_BADGE and TYPE_BADGE = @TYPE_BADGE LIMIT 1)";

                    commandDelete = new SQLiteCommand(sqlA, dbbManager.Connection);
                    lstUpd.AddSqlParams(commandDelete);

                    if (commandDelete.ExecuteNonQuery() == 0)
                    {
                        throw new Exception("Erreur lors de l'ajout du badgeage");
                    }

                }
            }
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


            string timeCdAtTime = reader.GetStringByColName("CD_AT_TIME");
            if (!StringUtils.IsNullOrWhiteSpace(timeCdAtTime))
            {
                b.CdAtTime = TimeSpan.Parse(timeCdAtTime);
            }
            return b;
        }


    }
}
