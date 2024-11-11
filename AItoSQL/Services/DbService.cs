using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Npgsql;
using Microsoft.Data.Sqlite;

namespace AItoSQL.Services
{
    public class DbService
    {
        public static string ModelsToText(string connectionString)
        {
            List<Table> tables = new();

            using (DbConnection connection = CreateConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT TABLE_NAME FROM information_schema.tables";
                using DbCommand command = connection.CreateCommand();
                command.CommandText = sql;

                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    List<Field> fields = new();
                    string tableName = reader["TABLE_NAME"].ToString()!;

                    string fieldSql = $"SELECT TABLE_NAME,COLUMN_NAME,IS_NULLABLE,DATA_TYPE FROM information_schema.columns WHERE TABLE_NAME = '{tableName}'";
                    using (DbCommand fieldCommand = connection.CreateCommand())
                    {
                        fieldCommand.CommandText = fieldSql;

                        using DbDataReader fieldReader = fieldCommand.ExecuteReader();
                        while (fieldReader.Read())
                        {
                            string fieldName = fieldReader["COLUMN_NAME"].ToString()!;
                            string dataType = fieldReader["DATA_TYPE"].ToString()!;

                            fields.Add(new Field() { Name = fieldName, DataType = dataType });
                        }
                    }

                    tables.Add(new Table() { Name = tableName, Fields = fields });
                }
            }

            return JsonSerializer.Serialize(tables);
        }

        public static string QueryDatabase(string sqlQuery, string connectionString)
        {
            if (!IsSafeQuery(sqlQuery))
            {
                var errorObject = new { error = $"ERROR! not safe query: {sqlQuery}" };
                return JsonSerializer.Serialize(errorObject);
            }

            string results = "";

            using (DbConnection connection = CreateConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using DbCommand command = connection.CreateCommand();
                    command.CommandText = sqlQuery;

                    using DbDataReader reader = command.ExecuteReader();
                    List<dynamic> resultList = new();

                    while (reader.Read())
                    {
                        dynamic result = new System.Dynamic.ExpandoObject();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            ((IDictionary<string, object>)result)[columnName] = reader.GetValue(i);
                        }

                        resultList.Add(result);
                    }

                    results = JsonSerializer.Serialize(resultList);
                }
                catch (DbException e)
                {
                    var errorObject = new { error = $"ERROR! {e.Message} {sqlQuery}" };
                    return JsonSerializer.Serialize(errorObject);
                }
            }

            return results;
        }

        private static DbConnection CreateConnection(string connectionString)
        {
            if (connectionString.Contains("Server=") && connectionString.Contains("Database="))
            {
                return new MySqlConnection(connectionString);
            }
            else if (connectionString.Contains("Host=") && connectionString.Contains("Database="))
            {
                return new NpgsqlConnection(connectionString);
            }
            else if (connectionString.Contains("Data Source="))
            {
                return new SqliteConnection(connectionString);
            }
            else
            {
                return new SqlConnection(connectionString);
            }
        }

        public static bool IsSafeQuery(string sqlQuery)
        {
            string pattern = @"^\s*SELECT\s+.*$";
            string[] dangerousKeywords = { "DELETE", "DROP", "TRUNCATE", "UPDATE", "INSERT", "ALTER", "EXEC", "MERGE", "REPLACE", "CREATE", "GRANT", "REVOKE" };

            if (Regex.IsMatch(sqlQuery, pattern, RegexOptions.IgnoreCase))
            {
                foreach (string keyword in dangerousKeywords)
                {
                    if (Regex.IsMatch(sqlQuery, @"\b" + keyword + @"\b", RegexOptions.IgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Models
        public class Table
        {
            public string Name { get; set; } = string.Empty;
            public virtual List<Field> Fields { get; set; } = new();
        }

        public class Field
        {
            public string Name { get; set; } = string.Empty;
            public string DataType { get; set; } = string.Empty;
        }
        #endregion
    }
}
