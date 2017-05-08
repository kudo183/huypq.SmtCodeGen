using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class ControllersCodeGenerator
    {
        public static void GenControllersClass(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#ControllerTemplate.txt")))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<InitDtoProperties>")
                    {
                        result.AppendLine(InitDtoProperties(table.Columns, baseTab));
                    }
                    else if (trimmed == "<InitEntityProperties>")
                    {
                        result.AppendLine(InitEntityProperties(table.Columns, baseTab));
                    }
                    else
                    {
                        result.AppendLine(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + "Controller.cs"), result.Value.ToString());
            }

            GenControllersPartialClass(tables, outputPath);
        }

        private static void GenControllersPartialClass(IEnumerable<DbTable> tables, string outputPath)
        {
            var referencedTable = tables.Where(p => p.ReferencesToThisTable.Count > 0);
            if (referencedTable.Count() == 0)
            {
                return;
            }

            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in referencedTable)
            {
                results.Add(table.TableName, new StringBuilder());
            }

            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#ControllerPartTemplate.txt")))
            {
                foreach (var table in referencedTable)
                {
                    var result = results[table.TableName];
                    result.AppendLine(line.Replace("<EntityName>", table.TableName));
                }
            }

            foreach (var result in results)
            {
                var filePath = System.IO.Path.Combine(outputPath, result.Key + "Controller.part.cs");
                if (System.IO.File.Exists(filePath) == false)
                {
                    FileUtils.WriteAllTextInUTF8(filePath, result.Value.ToString());
                }
            }
        }

        private static string InitEntityProperties(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendFormat("{0}{1} = dto.{1},{2}", baseTab, item.ColumnName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length - ",".Length);
        }

        private static string InitDtoProperties(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendFormat("{0}{1} = entity.{1},{2}", baseTab, item.ColumnName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length - ",".Length);
        }
    }
}
