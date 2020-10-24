using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Badger2018.Properties;

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
            return GetInt32ByColName(reader, colName) == 0;
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

            try
            {
                foreach (string line in fileContent)
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

    }
}
