using System.Data.SqlClient;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AItoSQL.Services
{
    public class DbService
    {
        public static string ModelsToText(string connectionString)
        {
            List<Table> tables = new();

            // Create a SqlConnection object
            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();

                // Get the database definition using SQL query
                string sql = "SELECT TABLE_NAME FROM information_schema.tables";
                using SqlCommand command = new(sql, connection);
                // Execute the query and read the results
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    List<Field> fields = new();

                    // Retrieve the table definition from the reader
                    string tableName = reader["TABLE_NAME"].ToString()!;

                    // Retrieve the field definition using SQL query
                    string fieldSql = $"SELECT TABLE_NAME,COLUMN_NAME,IS_NULLABLE,DATA_TYPE FROM information_schema.columns WHERE TABLE_NAME = '{tableName}'";
                    using (SqlCommand fieldCommand = new(fieldSql, connection))
                    {
                        // Execute the field query and read the results
                        using SqlDataReader fieldReader = fieldCommand.ExecuteReader();
                        while (fieldReader.Read())
                        {
                            // Retrieve the field definition from the fieldReader
                            string fieldName = fieldReader["COLUMN_NAME"].ToString()!;
                            string dataType = fieldReader["DATA_TYPE"].ToString()!;

                            // Save field information
                            fields.Add(new Field() { Name = fieldName, DataType = dataType });
                        }
                    }

                    // Save table information
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

            using (SqlConnection connection = new(connectionString))
            {

                try
                {
                    connection.Open();

                    using SqlCommand command = new(sqlQuery, connection);
                    using SqlDataReader reader = command.ExecuteReader();
                    List<dynamic> resultList = new();

                    while (reader.Read())
                    {
                        // Create a dynamic object for each row
                        dynamic result = new System.Dynamic.ExpandoObject();

                        // Read each column and add it to the dynamic object
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            ((IDictionary<string, object>)result)[columnName] = reader.GetValue(i);
                        }

                        resultList.Add(result);
                    }

                    results = JsonSerializer.Serialize(resultList);
                }
                catch (SqlException e)
                {
                    var errorObject = new { error = $"ERROR! {e.Message} {sqlQuery}" };
                    return JsonSerializer.Serialize(errorObject);
                }
            }

            return results;
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