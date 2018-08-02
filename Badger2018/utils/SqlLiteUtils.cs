using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Badger2018.utils
{
    public static class SqlLiteUtils
    {

        private static SQLiteConnection _connection = null;

        public static int GetInt32ByColName(this SQLiteDataReader reader, String colName)
        {


            return reader.GetInt32(reader.GetOrdinal(colName));
        }



        public static String GetStringByColName(this SQLiteDataReader reader, String colName)
        {
            int colIndex = reader.GetOrdinal(colName);

            return reader.IsDBNull(colIndex) ? null : reader.GetString(colIndex);
        }


        public static SQLiteConnection InitAndGetConnection(String file)
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


                _connection = new SQLiteConnection(String.Format("Data Source={0};Version=3;New=False", file));
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
                        command.CommandText = Properties.Resources.dbbCreate;
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


            try
            {

                using (SQLiteCommand command = connection.CreateCommand())
                {


                    command.CommandText = file;
                    command.ExecuteNonQuery();

                }

            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de l'exécution des commandes", e);
            }
        }

    }
}
