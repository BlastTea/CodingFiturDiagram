using Npgsql;
using System.Data;

namespace Logitop.Services
{
    public class DbHelper
    {
        private string Schema;
        private NpgsqlConnection Connection;
        private static DbHelper? Instance;

        /// <summary>
        /// Initialize a new instance of the <see cref="DbHelper" class/>
        /// </summary>
        /// <param name="host">Host of the database, usually localhost</param>
        /// <param name="port">Port of the database, usually 5432</param>
        /// <param name="username">Username of the database, usually postgres</param>
        /// <param name="password">Password of the database</param>
        /// <param name="database">Database name</param>
        /// <param name="schema">Schema name</param>
        private DbHelper(string host, int port, string username, string password, string database, string schema)
        {
            Schema = schema;
            Connection = new NpgsqlConnection()
            {
                ConnectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database};"
            };
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="DbHelper" class/>
        /// </summary>
        /// <param name="host">Host of the database, usually localhost</param>
        /// <param name="port">Port of the database, usually 5432</param>
        /// <param name="username">Username of the database, usually postgres</param>
        /// <param name="password">Password of the database</param>
        /// <param name="database">Database name</param>
        /// <param name="schema">Schema name</param>
        /// <returns>
        /// <see cref="DbHelper" instance/>
        /// </returns>
        public static DbHelper Initialize(string host, int port, string username, string password, string database, string schema)
        {
            if (Instance == null)
            {
                Instance = new DbHelper(host, port, username, password, database, schema);
            }
            return Instance;
        }

        /// <summary>
        /// Get the instance of <see cref="DbHelper"/> class
        /// </summary>
        /// <returns>
        /// null if <see cref="DbHelper"/> is not initialized, otherwise return <see cref="DbHelper"/> instance
        /// </returns>
        public static DbHelper GetInstance()
        {
            if (Instance == null)
            {
                throw new InvalidOperationException("DbHelper has not been initialized.");
            }
            return Instance;
        }

        /// <summary>
        /// Perform execute non query e.g. Create, Update, or Delete.
        /// </summary>
        /// <param name="sql">The sql command to be execute</param>
        /// <returns>
        /// Affected rows, if the affected rows more than one, then the execution is succeeded
        /// </returns>
        public int ExecuteNonQuery(string sql)
        {
            int affectedRows;
            try
            {
                Connection.Open();
                affectedRows = new NpgsqlCommand()
                {
                    Connection = Connection,
                    CommandText = sql
                }.ExecuteNonQuery();
                Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            return affectedRows;
        }

        /// <summary>
        /// Perform execute query e.g. Read
        /// </summary>
        /// <param name="sql">The sql command to be execute</param>
        /// <returns>
        /// <see cref="DataTable"/>
        /// </returns>
        public DataTable ExecuteQuery(string sql)
        {
            DataTable table = new DataTable();
            try
            {
                Connection.Open();
                new NpgsqlDataAdapter(sql, Connection).Fill(table);
                Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            return table;
        }

        public int Create(string table, Dictionary<string, object> data)
        {
            string column = "";
            string value = "";

            foreach (String key in data.Keys)
            {
                column += key;
                value += $"'{data[key]}'";

                if (key != data.Keys.Last())
                {
                    column += ", ";
                    value += ", ";
                }
            }

            int affectedRows;
            try
            {
                affectedRows = ExecuteNonQuery($"INSERT INTO {Schema}.{table} ({column}) VALUES ({value})");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            return affectedRows;
        }

        public DataTable Read(string table, string? orderBy)
        {
            // String options = "";


            return ExecuteQuery($"SELECT * FROM {Schema}.{table} {(orderBy != null ? $"ORDER BY {orderBy}" : "")}");
        }

        public int Update(string table, Dictionary<string, object> data, string whereColumn, int? whereValue)
        {
            string set = "";
            foreach (string key in data.Keys)
            {
                set += $"{key} = '{data[key]}'";
                if (key != data.Keys.Last())
                {
                    set += ", ";
                }
            }
            int affectedRows;
            try
            {
                affectedRows = ExecuteNonQuery($"UPDATE {Schema}.{table} SET {set} WHERE {whereColumn} = {whereValue}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;

            }
            return affectedRows;
        }

        public int Delete(string table, string whereColumn, int? whereValue)
        {
            int affectedRows;
            try
            {
                affectedRows = ExecuteNonQuery($"DELETE FROM {Schema}.{table} WHERE {whereColumn} = {whereValue}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            return affectedRows;
        }
    }
}