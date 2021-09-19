using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Badger2018.Properties;
using Badger2018.utils.sqlite;

namespace Badger2018.utils
{
    public static class SqlLiteUtils
    {

        private static SQLiteConnection _connection = null;

        public static int GetInt32ByColName(this SQLiteDataReader reader, String colName)
        {


            return reader.GetInt32(reader.GetOrdinal(colName));
        }

        public static bool GetBooleanByColName(this SQLiteDataReader reader, String colName)
        {
            return GetInt32ByColName(reader, colName) == 1;
        }


        public static String GetStringByColName(this SQLiteDataReader reader, String colName)
        {
            int colIndex = reader.GetOrdinal(colName);

            return reader.IsDBNull(colIndex) ? null : reader.GetString(colIndex);
        }

        public static DateTime? GetDatetimeByColName(this SQLiteDataReader reader, String colName)
        {
            int colIndex = reader.GetOrdinal(colName);

            return reader.IsDBNull(colIndex) ? (DateTime?)null : reader.GetDateTime(colIndex);
        }

        public static TimeSpan? GetTimeSpanByColName(this SQLiteDataReader reader, String colName)
        {
            int colIndex = reader.GetOrdinal(colName);

            return reader.IsDBNull(colIndex) ? (TimeSpan?)null : reader.GetDateTime(colIndex).TimeOfDay;
        }

        public static Decimal GetDecimalByColName(this SQLiteDataReader reader, String colName)
        {
            int colIndex = reader.GetOrdinal(colName);

            return (Decimal)reader.GetDecimal(colIndex);
        }

        public static Decimal? GetDecimalNByColName(this SQLiteDataReader reader, String colName)
        {
            int colIndex = reader.GetOrdinal(colName);

            return reader.IsDBNull(colIndex) ? (Decimal?)null : (Decimal)reader.GetDecimal(colIndex);
        }

        public static Object GetValueByColName(this SQLiteDataReader reader, String colName)
        {
            int colIndex = reader.GetOrdinal(colName);

            return reader.GetValue(colIndex);
        }

        


        public static SQLiteConnection InitAndGetConnection(String file, String password=null)
        {
            if (_connection != null)
            {

                return _connection;
            }

            try
            {

                if (!File.Exists(file))
                {
                    throw new FileNotFoundException("Le fichier de base de donnée n'existe pas");
                }

                if (password != null)
                {
                    _connection = new SQLiteConnection(String.Format("Data Source={0};Version=3;New=False;Password={1}", file, password));
                }
                else
                {
                    _connection = new SQLiteConnection(String.Format("Data Source={0};Version=3;New=False", file));
                }
                _connection.Open();

                return _connection;
            }
            catch (Exception e)
            {
                throw new Exception("Impossible de se connecter à la base de donnée applicative", e);
            }
        }

        public static SQLiteConnection GetConnection()
        {


            return _connection;

        }



        internal static void CreateDb(string dbFileName)
        {

            try
            {
                using (
                    SQLiteConnection connection =
                        new SQLiteConnection(String.Format("Data Source={0};Version=3;New=True", dbFileName)))
                {
                    using (SQLiteCommand command = connection.CreateCommand())
                    {

                        connection.Open();
                        command.CommandText = Resources.dbbCreate;
                        command.ExecuteNonQuery();


                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de la création de la base de données", e);
            }


        }

        public static void ExecuteContentFile(SQLiteConnection connection, string file)
        {
            if (connection == null || connection.State != ConnectionState.Open)
            {
                throw new SQLiteException("La connexion n'a pas été initalisée");
            }

            String[] fileContent = File.ReadAllLines(file);

            ExecuteSqlOrdersArray(connection, fileContent);
        }

        public static void ExecuteSqlOrdersArray(SQLiteConnection connection, string[] sqlOrders)
        {
            try
            {
                foreach (string line in sqlOrders)
                {
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = line;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de l'exécution des commandes", e);
            }
        }

        public static List<TableDef> GetTableDefinition(SQLiteConnection connection, string tableName)
        {
            String sqlReq = String.Format("PRAGMA TABLE_INFO(\"{0}\")", tableName);
            SQLiteCommand command = new SQLiteCommand(sqlReq, connection);

            List<TableDef> listT = new List<TableDef>();

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    TableDef col = new TableDef();
                    col.ColumnId = reader.GetInt32ByColName("cid");
                    col.Name = reader.GetStringByColName("name");
                    col.Type = reader.GetStringByColName("type");
                    col.IsNotNull = reader.GetBooleanByColName("notnull");
                    col.DefaultValue = reader.GetValueByColName("dflt_value");
                    col.PrimaryKey = reader.GetInt32ByColName("pk");
                    
                    listT.Add(col);
                }
            }

            return listT;

        }
    }
}
