using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class DataModelCodeGenerator
    {
        private const string DataModelTemplateFileName = "#DataModelTemplate.txt";
        private const string DataModelFileNameSubFix = "DataModel.cs";

        public static void GenDataModelClass(IEnumerable<TableSetting> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }

            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, DataModelTemplateFileName)))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);

                    var columnSettings = table.ColumnSettings.Where(p => IsSkippedProperty(p.GetColumnNameForCodeGen()) == false);
                    if (trimmed == "<DefaultValues>")
                    {
                        result.Append(DefaultValues(columnSettings, baseTab));
                    }
                    else if (trimmed == "<PrivateFields>")
                    {
                        result.Append(PrivateFields(columnSettings, baseTab));
                    }
                    else if (trimmed == "<PublicProperties>")
                    {
                        result.Append(PublicProperties(columnSettings, baseTab));
                    }
                    else if (trimmed == "<SetCurrentValueAsOriginalValue>")
                    {
                        result.Append(SetCurrentValueAsOriginalValue(columnSettings, baseTab));
                    }
                    else if (trimmed == "<Update>")
                    {
                        result.Append(Update(columnSettings, baseTab));
                    }
                    else if (trimmed == "<HasChange>")
                    {
                        result.Append(HasChange(columnSettings, baseTab));
                    }
                    else if (trimmed == "<ForeignKeyDataModel>")
                    {
                        result.Append(ForeignKeyDataModel(columnSettings, baseTab));
                    }
                    else if (trimmed == "<ReferenceDataSource>")
                    {
                        result.Append(ReferenceDataSource(columnSettings, baseTab));
                    }
                    else if (trimmed == "<ToDto>")
                    {
                        result.Append(ToDto(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<FromDto>")
                    {
                        result.Append(FromDto(table.ColumnSettings, baseTab));
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

            FileUtils.DeleteAllFileEndWith(outputPath, DataModelFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + DataModelFileNameSubFix), result.Value.ToString());
            }
        }

        private static string DefaultValues(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "public static {0} D{1};", item.DbColumn.DataType, item.GetColumnNameForCodeGen());
            }

            return sb.ToString();
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
                sb.AppendLineExWithTabAndFormat(baseTab, "{0} o{1};", item.DbColumn.DataType, item.GetColumnNameForCodeGen());
            }
            sb.AppendLine();
            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "{0} _{1} = D{1};", item.DbColumn.DataType, item.GetColumnNameForCodeGen());
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
                sb.AppendLineExWithTabAndFormat(baseTab, "public {0} {1} {{ get {{ return _{1}; }} set {{ SetField(ref _{1}, value); }} }}", item.DbColumn.DataType, item.GetColumnNameForCodeGen());
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
                sb.AppendLineExWithTabAndFormat(baseTab, "o{0} = {0};", item.GetColumnNameForCodeGen());
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
                sb.AppendLineExWithTabAndFormat(baseTab, "{0} = dataModel.{0};", item.GetColumnNameForCodeGen());
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
                sb.AppendLineExWithTabAndFormat(baseTab, "(o{0} != {0}) ||", item.GetColumnNameForCodeGen());
            }

            var l = " ||".Length + Constant.LineEnding.Length;
            sb.Remove(sb.Length - l, l);

            sb.AppendLineEx(";");
            return sb.ToString();
        }

        private static string ForeignKeyDataModel(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            var foreignKeys = columnSettings.Where(p => p.DbColumn.IsForeignKey == true);
            if (foreignKeys.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in foreignKeys)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "public {0}DataModel {1}Navigation {{ get; set; }}", item.DbColumn.ForeignKeyTableName, item.GetColumnNameForCodeGen());
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
                sb.AppendLineExWithTabAndFormat(baseTab, "object _{0}DataSource;", item.GetColumnNameForCodeGen());
            }
            sb.AppendLineEx();
            foreach (var item in foreignKeys)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "public object {0}DataSource {{ get {{ return _{0}DataSource; }} set {{ SetField(ref _{0}DataSource, value); }} }}", item.GetColumnNameForCodeGen());
            }

            return sb.ToString();
        }

        private static string ToDto(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "dto.{0} = {0};", item.GetColumnNameForCodeGen());
            }

            return sb.ToString();
        }

        private static string FromDto(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "{0} = dto.{0};", item.GetColumnNameForCodeGen());
            }

            return sb.ToString();
        }

        private static bool IsSkippedProperty(string propertyName)
        {
            if (propertyName == "ID"
                || propertyName == "TenantID"
                || propertyName == "CreateTime"
                || propertyName == "LastUpdateTime")
            {
                return true;
            }

            return false;
        }
    }
}
