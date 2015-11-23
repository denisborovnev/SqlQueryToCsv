using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace sqltocsv
{
    class Program
    {
        static void Main(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var inputFile = Path.Combine(basePath, "input.txt");
            var outputFile = Path.Combine(basePath, "output.csv");

            var query = ReadQueryFrom(inputFile);
            var dataTable = ExecuteQuery(query);
            SaveDatatableAsCsv(dataTable, outputFile);
        }

        private static void SaveDatatableAsCsv(DataTable dataTable, string outputFile)
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            using (var fileStream = File.Create(outputFile))
            using (var textWriter = new StreamWriter(fileStream, Encoding.UTF8))
            using(var csvWriter = new CsvWriter(textWriter))
            {
                //header
                foreach (DataColumn column in dataTable.Columns)
                {
                    csvWriter.WriteField(column.ColumnName);
                }
                csvWriter.NextRecord();

                //rows
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        var rawValue = dataRow[column];
                        var value = ConvertToString(rawValue);
                        
                        csvWriter.WriteField(value);
                    }
                    csvWriter.NextRecord();
                }
                
            }
        }

        private static string ConvertToString(object rawValue)
        {
            if (DBNull.Value == rawValue)
            {
                return string.Empty;
            }

            if (rawValue is bool)
            {
                return ((bool)rawValue) ? "Yes" : "No";
            }

            if (rawValue is DateTime)
            {
                return ((DateTime)rawValue).ToString("G");
            }

            return rawValue.ToString();
        }

        private static DataTable ExecuteQuery(QueryInfo query)
        {
            using (var conn = new SqlConnection(query.ConnectionString))
            {
                var dataAdapter = new SqlDataAdapter(query.SqlText, conn);
                var result = new DataTable();
                dataAdapter.Fill(result);
                return result;
            }
        }

        static QueryInfo ReadQueryFrom(string file)
        {
            if (!File.Exists(file))
            {
                throw new Exception("Can't find input file: " + file);
            }

            var lines = File.ReadAllLines(file).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (lines.Length < 2)
            {
                throw new Exception(
@"input file should be in the following format:
connectionstring
sql query
");
            }

            return new QueryInfo(lines[0], string.Join(Environment.NewLine, lines.Skip(1)));
        }
    }
}
