namespace ConversorSQL
{
    class Program
    {
        static void Main(string[] args)
        {
            string? filePath = "";
            string? tableName = "";
            if (args.Length < 2)
            {
                //Console.WriteLine("Faltam argumentos, use: ConversorSQL <caminho do arquivo> <nome da tabela>");
                //return;
                do {
                    Console.WriteLine("Digite o caminho do arquivo:");
                    filePath = Console.ReadLine();
                } while (checkIfFileExists(filePath, "Arquivo não encontrado") == false);

                do
                {
                    Console.WriteLine("Digite o nome da tabela:");
                    tableName = Console.ReadLine();
                } while (tableName == null || tableName == "");
            }
            else
            {

                filePath = args[0];
                tableName = args[1];
            }

            if (!checkIfFileExists(filePath, "Arquivo não encontrado"))
            {
                return;
            }

            string fileType = filePath.Substring(filePath.LastIndexOf('.') + 1);

            if (fileType != "csv" && fileType != "txt")
            {
                Console.WriteLine("Arquivo não é um CSV ou txt");
                return;
            }

            Console.Clear();

            Console.WriteLine("Arquivo de origem: " + filePath);
            Console.WriteLine("Tipo do arquivo: " + fileType);

            using(var reader = new StreamReader(filePath))
            {
                bool firstLine = true;
                var columnsName = new List<string>();
                var columnsType = new List<string>();
                var values = new List<List<string>>();

                while(!reader.EndOfStream)
                {
                    var line = "";
                    
                    line = reader.ReadLine();
                    
                    if (line == null || line == "")
                    {
                        continue;
                    }

                    bool hasDotComma = line.Contains(";");
                    
                    var valuesLine = (hasDotComma == true)?line.Split(';'):line.Split(",");
                    if (valuesLine.Length == 0)
                    {
                        continue;
                    }
                    
                    for(int i = 0; i < valuesLine.Length; i++)
                    {
                        valuesLine[i] = valuesLine[i].Trim();
                    }

                    if(firstLine)
                    {
                        columnsName.AddRange(valuesLine);
                        firstLine = false;
                        continue;
                    }

                    if (columnsType.Count == 0)
                    {
                        for (int index = 0; index < columnsName.Count; index++)
                        {
                            if (valuesLine[index].Replace(".",string.Empty).All(char.IsDigit)) {
                                if (valuesLine[index].Contains("."))
                                {
                                    columnsType.Add("FLOAT");
                                }
                                else
                                {
                                    columnsType.Add("INT");
                                }
                            }
                            else
                            {
                                string[] booleanValues = { "true", "false" };
                                if (booleanValues.Contains(valuesLine[index].ToLower()))
                                {
                                    columnsType.Add("BIT");
                                }
                                else
                                {
                                    columnsType.Add("VARCHAR(255)");
                                }
                            }
                        }
                    }
                    values.Add(valuesLine.ToList());
                }

                var sqlTable = CreateSQLTable(columnsName, columnsType, tableName);
                
                var sqlInserts = CreateSQLInserts(columnsName, columnsType, values, tableName);

                Console.WriteLine("Gerando Tabela SQL:");
                Console.WriteLine(sqlTable);

                saveSQLFile(sqlTable, tableName+"_table");

                Console.WriteLine("Gerando Inserts SQL:");
                Console.WriteLine(sqlInserts);
                
                saveSQLFile(sqlInserts, tableName+"_inserts");
            }
        }
        static bool checkIfFileExists(string? filePath, string errorMessage)
        {
            if(filePath == null)
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            if (File.Exists(filePath))
            {
                return true;
            }
            else
            {
                Console.WriteLine(errorMessage);
                return false;
            }
        }

        static bool saveSQLFile(string sqlText, string fileName)
        {

            var filePath = fileName+".sql";
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.Write(sqlText);

                    return true;
                }
            } catch(Exception e) {
                Console.WriteLine("Erro ao salvar arquivo: " + e.Message);
                return false;
            }
        }
        static string CreateSQLTable(List<string> columnsName, List<string> columnsType, string tableName)
        {
            var sql = "CREATE TABLE "+tableName+" (";
            for(int i = 0; i < columnsName.Count; i++)
            {
                sql += "\n\t" + columnsName[i] + " " + columnsType[i] + ",";
            }
            sql = sql.Substring(0, sql.Length - 1);
            sql += "\n);";
            return sql;
        }
        static string CreateSQLInserts(List<string> columnsName, List<string> columnsType, List<List<string>> values, string tableName)
        {
            var sql = "INSERT INTO "+tableName+" (";
            foreach(var column in columnsName)
            {
                sql += column + ",";
            }
            sql = sql.Substring(0, sql.Length - 1);
            sql += ") VALUES ";
            
            for(int indexLine = 0; indexLine < values.Count; indexLine++)
            {
                sql += "\n\t(";
                for(int indexColumn = 0; indexColumn < columnsName.Count; indexColumn++)
                {
                    var value = values[indexLine][indexColumn];
                    switch(columnsType[indexColumn])
                    {
                        case "INT":
                            sql += value + ",";
                            break;
                        case "FLOAT":
                            sql += value + ",";
                            break;
                        case "BIT":
                            sql +="'"+ value + "',";
                            break;
                        case "VARCHAR(255)":
                            sql += "'" + value + "',";
                            break;
                    }
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += "),";
            }
            sql = sql.Substring(0, sql.Length - 1);
            sql += ";";
            return sql;
        }

    }
}