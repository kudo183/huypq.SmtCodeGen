using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class TextManagerCodeGenerator
    {
        public static void GenTextManagerCode(IEnumerable<DbTable> tables, string outputPath)
        {
            var result = new StringBuilder();
            var classKeyword = " class ";
            var className = "";
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#TextManagerTemplate.cs")))
            {
                var indexOfClass = line.IndexOf(classKeyword);
                if (indexOfClass != -1)
                {
                    var afterClassKeywordIndex = indexOfClass + classKeyword.Length;
                    var nextSpaceIndex = line.IndexOf(' ', afterClassKeywordIndex);
                    if (nextSpaceIndex == -1)
                    {
                        className = line.Substring(afterClassKeywordIndex);
                    }
                    else
                    {
                        className = line.Substring(afterClassKeywordIndex, nextSpaceIndex - afterClassKeywordIndex);
                    }
                    result.AppendLine(line);
                    continue;
                }

                var trimmedEnd = line.TrimEnd();
                var trimmed = trimmedEnd.TrimStart();
                var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                if (trimmed == "<TextStaticProperties>")
                {
                    result.AppendLine(TextStaticProperties(tables, baseTab));
                }
                else if (trimmed == "<InitDefaultTextData>")
                {
                    result.AppendLine(InitDefaultTextData(tables, baseTab));
                }
                else
                {
                    result.AppendLine(line);
                }
            }

            FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, className + ".cs"), result.ToString());
        }

        private static string TextStaticProperties(IEnumerable<DbTable> tables, string baseTab)
        {
            if (tables.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                foreach (var column in table.Columns)
                {
                    sb.AppendFormat("{0}public static string {1}_{2} {{ get {{ return GetText(); }} }}{3}", baseTab, table.TableName, column.ColumnName, Constant.LineEnding);
                }
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string InitDefaultTextData(IEnumerable<DbTable> tables, string baseTab)
        {
            if (tables.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                foreach (var column in table.Columns)
                {
                    sb.AppendFormat("{0}_dic.Add(\"{1}_{2}\", \"{2}\");{3}", baseTab, table.TableName, column.ColumnName, Constant.LineEnding);
                }
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }
    }
}
