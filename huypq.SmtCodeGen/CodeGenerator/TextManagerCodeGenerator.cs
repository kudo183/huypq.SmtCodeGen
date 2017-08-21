using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class TextManagerCodeGenerator
    {
        private const string TextManagerTemplateFileName = "#TextManagerTemplate.txt";
        private const string TextManagerFileNameSubFix = ".cs";

        public static void GenTextManagerCode(IEnumerable<TableSetting> tables, string outputPath)
        {
            var result = new StringBuilder();
            var classKeyword = " class ";
            var className = "";
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, TextManagerTemplateFileName)))
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
                    result.AppendLineEx(line);
                    continue;
                }

                var trimmedEnd = line.TrimEnd();
                var trimmed = trimmedEnd.TrimStart();
                var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                if (trimmed == "<TextStaticProperties>")
                {
                    result.Append(TextStaticProperties(tables, baseTab));
                }
                else if (trimmed == "<InitDefaultTextData>")
                {
                    result.Append(InitDefaultTextData(tables, baseTab));
                }
                else
                {
                    result.AppendLineEx(line);
                }
            }

            FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, className + TextManagerFileNameSubFix), result.ToString());
        }

        private static string TextStaticProperties(IEnumerable<TableSetting> tables, string baseTab)
        {
            if (tables.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                foreach (var column in table.ColumnSettings)
                {
                    sb.AppendLineExWithTabAndFormat(baseTab, "public static string {0}_{1} {{ get {{ return GetText(); }} }}", table.TableName, column.GetColumnNameForCodeGen());
                }
            }

            return sb.ToString();
        }

        private static string InitDefaultTextData(IEnumerable<TableSetting> tables, string baseTab)
        {
            if (tables.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                foreach (var column in table.ColumnSettings)
                {
                    sb.AppendLineExWithTabAndFormat(baseTab, "_dic.Add(\"{0}_{1}\", \"{1}\");", table.TableName, column.GetColumnNameForCodeGen());
                }
            }

            return sb.ToString();
        }
    }
}
