using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Badger2018.business;
using Badger2018.constants;

namespace Badger2018.services
{
    public class BadgeageServices
    {
        private const String TableBadgeages = "BADGEAGES";

        public static void InsertNewBadgeage(DbbAccessManager dbbManager, int typeBadgeage, DateTime dateTime)
        {
            SQLiteCommand command = null;

            string sql = String.Format(SqlConstants.INSERT_INTO, TableBadgeages, "DATEBADGE, TYPEBADGE, TIMEBADGE",
                    "@DATEBADGE, @TYPEBADGE, @TIMEBADGE");

            command = new SQLiteCommand(sql, dbbManager.Connection);

            command.Parameters.Add(new SQLiteParameter("@DATEBADGE", dateTime.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new SQLiteParameter("@TYPEBADGE", typeBadgeage));
            command.Parameters.Add(new SQLiteParameter("@TIMEBADGE", dateTime.ToString("hh:mm")));

            if (command.ExecuteNonQuery() != 1)
            {
                throw new Exception("Erreur lors de l'ajout du badgeage");
            }
        }

        public static void RemoveBadgeagesOfAday(DbbAccessManager dbbManager, DateTime dateTime)
        {
            SQLiteCommand command = null;

            string sql = String.Format(SqlConstants.DELETE_WHERE, TableBadgeages, "DATEBADGE=@DATEBADGE");

            command = new SQLiteCommand(sql, dbbManager.Connection);

            command.Parameters.Add(new SQLiteParameter("@DATEBADGE", dateTime.ToString("yyyy-MM-dd")));


            if (command.ExecuteNonQuery() == -1)
            {
                throw new Exception("Erreur lors de l'ajout de la suppression des badgeages du jours");
            }
        }
    }
}
