using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class DtosCodeGenerator
    {
        private const string DtoTemplateFileName = "#DtoTemplate.txt";
        private const string DtoFileNameSubFix = "Dto.cs";

        private const string DtoPartTemplateFileName = "#DtoPartTemplate.txt";
        private const string DtoPartFileNameSubFix = "Dto.part.cs";

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
                    if (trimmed == "<PrivateFields>")
                    {
                        result.Append(PrivateFields(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<PublicProperties>")
                    {
                        result.Append(PublicProperties(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<SetCurrentValueAsOriginalValue>")
                    {
                        result.Append(SetCurrentValueAsOriginalValue(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<Update>")
                    {
                        result.Append(Update(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<HasChange>")
                    {
                        result.Append(HasChange(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<ForeignKeyDto>")
                    {
                        result.Append(ForeignKeyDto(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<ReferenceDataSource>")
                    {
                        result.Append(ReferenceDataSource(table.ColumnSettings, baseTab));
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

            FileUtils.DeleteAllFileEndWith(outputPath, DtoFileNameSubFix, DtoPartFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + DtoFileNameSubFix), result.Value.ToString());
            }

            GenDtosPartialClass(tables, outputPath);
        }

        private static void GenDtosPartialClass(IEnumerable<TableSetting> tables, string outputPath)
        {
            var referencedTable = tables.Where(p => p.DbTable.ReferencesToThisTable.Count > 0);
            if (referencedTable.Count() == 0)
            {
                return;
            }

            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in referencedTable)
            {
                results.Add(table.TableName, new StringBuilder());
            }

            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, DtoPartTemplateFileName)))
            {
                foreach (var table in referencedTable)
                {
                    var result = results[table.TableName];
                    result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                }
            }

            foreach (var result in results)
            {
                var filePath = System.IO.Path.Combine(outputPath, result.Key + DtoPartFileNameSubFix);
                if (System.IO.File.Exists(filePath) == false)
                {
                    FileUtils.WriteAllTextInUTF8(filePath, result.Value.ToString());
                }
            }
        }

        private static string PrivateFields(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "{0} o{1};", item.DbColumn.DataType, item.ColumnName);
            }
            sb.AppendLine();
            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "{0} _{1};", item.DbColumn.DataType, item.ColumnName);
            }

            return sb.ToString();
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
                sb.AppendLineExWithTabAndFormat(baseTab, "[ProtoBuf.ProtoMember({0})]", i);
                sb.AppendLineExWithTabAndFormat(baseTab, "public {0} {1} {{ get {{ return _{1}; }} set {{ _{1} = value; OnPropertyChanged(); }} }}", item.DbColumn.DataType, item.ColumnName);
                i++;
            }

            return sb.ToString();
        }

        private static string SetCurrentValueAsOriginalValue(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "o{0} = {0};", item.ColumnName);
            }

            return sb.ToString();
        }

        private static string Update(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "{0} = dto.{0};", item.ColumnName);
            }

            return sb.ToString();
        }

        private static string HasChange(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendLineExWithTab(baseTab, "return");
            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "(o{0} != {0})||", item.ColumnName);
            }

            var l = "||".Length + Constant.LineEnding.Length;
            sb.Remove(sb.Length - l, l);

            sb.AppendLineEx(";");
            return sb.ToString();
        }

        private static string ForeignKeyDto(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            var foreignKeys = columnSettings.Where(p => p.DbColumn.IsForeignKey == true);
            if (foreignKeys.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in foreignKeys)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "public {0}Dto {1}{0}Dto {{ get; set; }}", item.DbColumn.ForeignKeyTableName, item.ColumnName);
            }

            return sb.ToString();
        }

        private static string ReferenceDataSource(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var foreignKeys = columnSettings.Where(p => p.DbColumn.IsForeignKey && p.IsNeedReferenceData);
            foreach (var item in foreignKeys)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "object _{0}DataSource;", item.ColumnName);
            }
            sb.AppendLineEx();
            foreach (var item in foreignKeys)
            {
                sb.AppendLineExWithTab(baseTab, "[Newtonsoft.Json.JsonIgnore]");
                sb.AppendLineExWithTabAndFormat(baseTab, "public object {0}DataSource {{ get {{ return _{0}DataSource; }} set {{ _{0}DataSource = value; OnPropertyChanged(); }} }}", item.ColumnName);
            }

            var pkName = columnSettings.First(p => p.DbColumn.IsIdentity).ColumnName;
            if (pkName != "ID")
            {
                sb.AppendLineEx();
                sb.AppendLineExWithTab(baseTab, "[Newtonsoft.Json.JsonIgnore]");
                sb.AppendLineExWithTabAndFormat(baseTab, "public int ID {{ get {{ return {0}; }} set {{ {0} = value;}} }}", pkName);
            }

            return sb.ToString();
        }
    }
}
