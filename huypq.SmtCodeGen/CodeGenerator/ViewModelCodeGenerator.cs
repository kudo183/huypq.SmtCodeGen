using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class ViewModelCodeGenerator
    {
        public static void GenViewModelCode(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#ViewModelTemplate.cs")))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<DeclareHeaderFilters>")
                    {
                        result.AppendLine(DeclareHeaderFilters(table.Columns, baseTab));
                    }
                    else if (trimmed == "<InitHeaderFilters>")
                    {
                        result.AppendLine(InitHeaderFilters(table.Columns, table.TableName, baseTab));
                    }
                    else if (trimmed == "<AddHeaderFiltersToHeaderFilterCollection>")
                    {
                        result.AppendLine(AddHeaderFiltersToHeaderFilterCollection(table.Columns, baseTab));
                    }
                    else if (trimmed == "<LoadReferenceDatas>")
                    {
                        result.AppendLine(LoadReferenceDatas(table.Columns, baseTab));
                    }
                    else if (trimmed == "<SetDtosReferenceDataSource>")
                    {
                        result.AppendLine(SetDtosReferenceDataSource(table.Columns, baseTab));
                    }
                    else if (trimmed == "<SetDtosDefaultValue>")
                    {
                        result.AppendLine(SetDtosDefaultValue(table.Columns, baseTab));
                    }
                    else
                    {
                        result.AppendLine(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + "ViewModel.cs"), result.Value.ToString());
            }
        }

        private static string DeclareHeaderFilters(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendFormat("{0}HeaderFilterBaseModel _{1}Filter;{2}",
                    baseTab, item.ColumnName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string InitHeaderFilters(IEnumerable<DbTableColumn> columns, string tableName, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var tab1 = baseTab + Constant.Tab;
            var tab2 = tab1 + Constant.Tab;
            foreach (var item in columns)
            {
                if (item.IsForeignKey == true)
                {
                    sb.AppendFormat("{0}_{1}Filter = new HeaderComboBoxFilterModel({2}", baseTab, item.ColumnName, Constant.LineEnding);
                    sb.AppendFormat("{0}TextManager.{1}_{2}, HeaderComboBoxFilterModel.ComboBoxFilter,{3}", tab1, tableName, item.ColumnName, Constant.LineEnding);
                    sb.AppendFormat("{0}nameof({1}Dto.{2}),{3}", tab1, tableName, item.ColumnName, Constant.LineEnding);
                    sb.AppendFormat("{0}typeof({1}),{2}", tab1, item.DataType, Constant.LineEnding);
                    sb.AppendFormat("{0}nameof({1}Dto.TenHienThi),{2}", tab1, item.ForeignKeyTableName, Constant.LineEnding);
                    sb.AppendFormat("{0}nameof({1}Dto.ID)){2}", tab1, item.ForeignKeyTableName, Constant.LineEnding);
                    sb.AppendLine(baseTab + "{");
                    sb.AppendFormat("{0}AddCommand = new SimpleCommand(\"{1}AddCommand\",{2}", tab1, item.ColumnName, Constant.LineEnding);
                    sb.AppendLine(tab2 + "() => base.ProccessHeaderAddCommand(");
                    sb.AppendFormat("{0}new View.{1}View(), \"{1}\", ReferenceDataManager<{1}Dto>.Instance.Update)),{2}", tab2, item.ForeignKeyTableName, Constant.LineEnding);
                    sb.AppendFormat("{0}ItemSource = ReferenceDataManager<{1}Dto>.Instance.Get(){2}", tab1, item.ForeignKeyTableName, Constant.LineEnding);
                    sb.AppendLine(baseTab + "};");
                }
                else
                {
                    var filterType = GetFilterTypeFromProperty(item);
                    sb.AppendFormat("{0}_{1}Filter = new {2}(TextManager.{3}_{1}, nameof({3}Dto.{1}), typeof({4}));{5}",
                        baseTab, item.ColumnName, filterType, tableName, item.DataType, Constant.LineEnding);
                }
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string AddHeaderFiltersToHeaderFilterCollection(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendFormat("{0}AddHeaderFilter(_{1}Filter);{2}", baseTab, item.ColumnName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string LoadReferenceDatas(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            var foreignKeys = columns.Where(p => p.IsForeignKey);
            if (foreignKeys.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in foreignKeys)
            {
                sb.AppendFormat("{0}ReferenceDataManager<{1}Dto>.Instance.Update();{2}", baseTab, item.ForeignKeyTableName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string SetDtosReferenceDataSource(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            var foreignKeys = columns.Where(p => p.IsForeignKey);
            if (foreignKeys.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in foreignKeys)
            {
                sb.AppendFormat("{0}dto.{1}DataSources = ReferenceDataManager<{2}Dto>.Instance.Get();{3}",
                    baseTab, item.ColumnName, item.ForeignKeyTableName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string SetDtosDefaultValue(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var tab1 = baseTab + Constant.Tab;
            foreach (var item in columns)
            {
                sb.AppendFormat("{0}if (_{1}Filter.FilterValue != null){2}", baseTab, item.ColumnName, Constant.LineEnding);
                sb.AppendFormat("{0}{{{1}",baseTab,Constant.LineEnding);
                sb.AppendFormat("{0}dto.{1} = ({2})_{1}Filter.FilterValue;{3}",
                    tab1, item.ColumnName, item.DataType, Constant.LineEnding);
                sb.AppendFormat("{0}}}{1}", baseTab, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string GetFilterTypeFromProperty(DbTableColumn column)
        {
            if (column.IsForeignKey == true)
            {
                return "HeaderComboBoxFilterModel";
            }
            var dataType = column.DataType;
            if (dataType == "string" || dataType == "int" || dataType == "int?")
            {
                return "HeaderTextFilterModel";
            }
            else if (dataType == "bool" || dataType == "bool?")
            {
                return "HeaderCheckFilterModel";
            }
            else if (dataType == "System.DateTime" || dataType == "System.DateTime?")
            {
                return "HeaderDateFilterModel";
            }

            return "HeaderTextFilterModel";
        }
    }
}
