using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class DtosCodeGenerator
    {
        private const string DtoTemplateFileName = "#DtoTemplate.txt";
        private const string DtoFileNameSubFix = "Dto.cs";

        public static void GenDtosClass(IEnumerable<TableSetting> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }

            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, DtoTemplateFileName)))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<PublicProperties>")
                    {
                        result.Append(PublicProperties(table.ColumnSettings, baseTab));
                    }
                    else
                    {
                        if (table.TableName == "SmtUser")
                        {
                            result.AppendLineEx(line.Replace("IDto", "IUserDto").Replace("<EntityName>", table.TableName));
                        }
                        else if (table.TableName == "SmtUserClaim")
                        {
                            result.AppendLineEx(line.Replace("IDto", "IUserClaimDto").Replace("<EntityName>", table.TableName));
                        }
                        else
                        {
                            result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                        }
                    }
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, DtoFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + DtoFileNameSubFix), result.Value.ToString());
            }
        }

        private static string PublicProperties(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var i = 1;
            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "[Newtonsoft.Json.JsonProperty]");
                sb.AppendLineExWithTabAndFormat(baseTab, "[ProtoBuf.ProtoMember({0})]", i);
                sb.AppendLineExWithTabAndFormat(baseTab, "public {0} {1} {{ get; set;}}", item.DbColumn.DataType, item.GetColumnNameForCodeGen());
                i++;
            }

            return sb.ToString();
        }
    }
}
