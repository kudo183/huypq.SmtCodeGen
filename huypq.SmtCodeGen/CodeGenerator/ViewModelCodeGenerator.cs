using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class ViewModelCodeGenerator
    {
        private const string ViewModelTemplateFileName = "#ViewModelTemplate.txt";
        private const string ViewModelFileNameSubFix = "ViewModel.cs";

        public static void GenViewModelCode(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ViewModelTemplateFileName)))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<DeclareHeaderFilters>")
                    {
                        result.Append(DeclareHeaderFilters(table.Columns, baseTab));
                    }
                    else if (trimmed == "<InitHeaderFilters>")
                    {
                        result.Append(InitHeaderFilters(table.Columns, table.TableName, baseTab));
                    }
                    else if (trimmed == "<AddHeaderFiltersToHeaderFilterCollection>")
                    {
                        result.Append(AddHeaderFiltersToHeaderFilterCollection(table.Columns, baseTab));
                    }
                    else if (trimmed == "<LoadReferenceDatas>")
                    {
                        result.Append(LoadReferenceDatas(table.Columns, baseTab));
                    }
                    else if (trimmed == "<SetDtosReferenceDataSource>")
                    {
                        result.Append(SetDtosReferenceDataSource(table.Columns, baseTab));
                    }
                    else if (trimmed == "<SetDtosDefaultValue>")
                    {
                        result.Append(SetDtosDefaultValue(table.Columns, baseTab));
                    }
                    else
                    {
                        result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ViewModelFileNameSubFix), result.Value.ToString());
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
                sb.AppendLineExWithTabAndFormat(baseTab, "HeaderFilterBaseModel _{0}Filter;", item.ColumnName);
            }

            return sb.ToString();
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
                    if (item.IsReferenceToLargeTable)
                    {
                        var filterType = "HeaderTextFilterModel";
                        sb.AppendLineExWithTabAndFormat(baseTab, "_{0}Filter = new {1}(TextManager.{2}_{0}, nameof({2}Dto.{0}), typeof({3}));",
                        item.ColumnName, filterType, tableName, item.DataType);
                    }
                    else
                    {
                        sb.AppendLineExWithTabAndFormat(baseTab, "_{0}Filter = new HeaderComboBoxFilterModel(", item.ColumnName);
                        sb.AppendLineExWithTabAndFormat(tab1, "TextManager.{0}_{1}, HeaderComboBoxFilterModel.ComboBoxFilter,", tableName, item.ColumnName);
                        sb.AppendLineExWithTabAndFormat(tab1, "nameof({0}Dto.{1}),", tableName, item.ColumnName);
                        sb.AppendLineExWithTabAndFormat(tab1, "typeof({0}),", item.DataType);
                        sb.AppendLineExWithTabAndFormat(tab1, "nameof({0}Dto.DisplayText),", item.ForeignKeyTableName);
                        sb.AppendLineExWithTabAndFormat(tab1, "nameof({0}Dto.ID))", item.ForeignKeyTableName);
                        sb.AppendLineExWithTab(baseTab, "{");
                        sb.AppendLineExWithTabAndFormat(tab1, "AddCommand = new SimpleCommand(\"{0}AddCommand\",", item.ColumnName);
                        sb.AppendLineExWithTab(tab2, "() => base.ProccessHeaderAddCommand(");
                        sb.AppendLineExWithTabAndFormat(tab2, "new View.{0}View(), \"{0}\", ReferenceDataManager<{0}Dto>.Instance.LoadOrUpdate)),", item.ForeignKeyTableName);
                        sb.AppendLineExWithTabAndFormat(tab1, "ItemSource = ReferenceDataManager<{0}Dto>.Instance.Get()", item.ForeignKeyTableName);
                        sb.AppendLineExWithTab(baseTab, "};");
                    }
                }
                else
                {
                    var filterType = GetFilterTypeFromProperty(item);
                    sb.AppendLineExWithTabAndFormat(baseTab, "_{0}Filter = new {1}(TextManager.{2}_{0}, nameof({2}Dto.{0}), typeof({3}));",
                        item.ColumnName, filterType, tableName, item.DataType);
                }
            }

            return sb.ToString();
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
                sb.AppendLineExWithTabAndFormat(baseTab, "AddHeaderFilter(_{0}Filter);", item.ColumnName);
            }

            return sb.ToString();
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
                if (item.IsReferenceToLargeTable == true)
                {
                    continue;
                }

                sb.AppendLineExWithTabAndFormat(baseTab, "ReferenceDataManager<{0}Dto>.Instance.LoadOrUpdate();", item.ForeignKeyTableName);
            }

            return sb.ToString();
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
                if (item.IsReferenceToLargeTable == true)
                {
                    continue;
                }

                sb.AppendLineExWithTabAndFormat(baseTab, "dto.{0}DataSource = ReferenceDataManager<{1}Dto>.Instance.Get();",
                    item.ColumnName, item.ForeignKeyTableName);
            }

            return sb.ToString();
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
                sb.AppendLineExWithTabAndFormat(baseTab, "if (_{0}Filter.FilterValue != null)", item.ColumnName);
                sb.AppendLineExWithTab(baseTab, "{");
                sb.AppendLineExWithTabAndFormat(tab1, "dto.{0} = ({1})_{0}Filter.FilterValue;", item.ColumnName, item.DataType);
                sb.AppendLineExWithTab(baseTab, "}");
            }

            return sb.ToString();
        }

        private static string GetFilterTypeFromProperty(DbTableColumn column)
        {
            if (column.IsForeignKey == true)
            {
                return "HeaderComboBoxFilterModel";
            }
            var dataType = column.DataType;
            if (dataType == "int" || dataType == "int?" || dataType == "long" || dataType == "long?")
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
