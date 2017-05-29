using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class ViewModelCodeGenerator
    {
        private const string ViewModelTemplateFileName = "#ViewModelTemplate.txt";
        private const string ViewModelFileNameSubFix = "ViewModel.cs";

        public static void GenViewModelCode(IEnumerable<TableSetting> tables, string outputPath)
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
                        result.Append(DeclareHeaderFilters(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<InitHeaderFilters>")
                    {
                        result.Append(InitHeaderFilters(table.ColumnSettings, table.TableName, baseTab));
                    }
                    else if (trimmed == "<AddHeaderFiltersToHeaderFilterCollection>")
                    {
                        result.Append(AddHeaderFiltersToHeaderFilterCollection(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<LoadReferenceDatas>")
                    {
                        result.Append(LoadReferenceDatas(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<SetDtosReferenceDataSource>")
                    {
                        result.Append(SetDtosReferenceDataSource(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<SetDtosDefaultValue>")
                    {
                        result.Append(SetDtosDefaultValue(table.ColumnSettings, baseTab));
                    }
                    else
                    {
                        result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, ViewModelFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ViewModelFileNameSubFix), result.Value.ToString());
            }
        }

        private static string DeclareHeaderFilters(IEnumerable<ColumnSetting> columns, string baseTab)
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

        private static string InitHeaderFilters(IEnumerable<ColumnSetting> columns, string tableName, string baseTab)
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
                var columnName = item.ColumnName;
                var dataType = item.DbColumn.DataType;
                var headerFilterModelType = GetHeaderFilterModelType(item);
                if (headerFilterModelType == "HeaderComboBoxFilterModel")
                {
                    var foreignKeyTableName = item.DbColumn.ForeignKeyTableName;
                    sb.AppendLineExWithTabAndFormat(baseTab, "_{0}Filter = new HeaderComboBoxFilterModel(", columnName);
                    sb.AppendLineExWithTabAndFormat(tab1, "TextManager.{0}_{1}, HeaderComboBoxFilterModel.ComboBoxFilter,", tableName, columnName);
                    sb.AppendLineExWithTabAndFormat(tab1, "nameof({0}Dto.{1}),", tableName, columnName);
                    sb.AppendLineExWithTabAndFormat(tab1, "typeof({0}),", dataType);
                    sb.AppendLineExWithTabAndFormat(tab1, "nameof({0}Dto.DisplayText),", foreignKeyTableName);
                    sb.AppendLineExWithTabAndFormat(tab1, "nameof({0}Dto.ID))", foreignKeyTableName);
                    sb.AppendLineExWithTab(baseTab, "{");
                    sb.AppendLineExWithTabAndFormat(tab1, "AddCommand = new SimpleCommand(\"{0}AddCommand\",", columnName);
                    sb.AppendLineExWithTab(tab2, "() => base.ProccessHeaderAddCommand(");
                    sb.AppendLineExWithTabAndFormat(tab2, "new View.{0}View(), \"{0}\", ReferenceDataManager<{0}Dto>.Instance.LoadOrUpdate)),", foreignKeyTableName);
                    sb.AppendLineExWithTabAndFormat(tab1, "ItemSource = ReferenceDataManager<{0}Dto>.Instance.Get()", foreignKeyTableName);
                    sb.AppendLineExWithTab(baseTab, "};");
                }
                else
                {
                    sb.AppendLineExWithTabAndFormat(baseTab, "_{0}Filter = new {1}(TextManager.{2}_{0}, nameof({2}Dto.{0}), typeof({3}));",
                        item.ColumnName, headerFilterModelType, tableName, dataType);
                }
            }

            return sb.ToString();
        }

        private static string AddHeaderFiltersToHeaderFilterCollection(IEnumerable<ColumnSetting> columns, string baseTab)
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

        private static string LoadReferenceDatas(IEnumerable<ColumnSetting> columns, string baseTab)
        {
            var needReferenceDataColumn = columns.Where(p => p.DbColumn.IsForeignKey && p.IsNeedReferenceData == true);
            if (needReferenceDataColumn.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in needReferenceDataColumn)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "ReferenceDataManager<{0}Dto>.Instance.LoadOrUpdate();", item.DbColumn.ForeignKeyTableName);
            }

            return sb.ToString();
        }

        private static string SetDtosReferenceDataSource(IEnumerable<ColumnSetting> columns, string baseTab)
        {
            var needReferenceDataColumn = columns.Where(p => p.DbColumn.IsForeignKey && p.IsNeedReferenceData == true);
            if (needReferenceDataColumn.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in needReferenceDataColumn)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "dto.{0}DataSource = ReferenceDataManager<{1}Dto>.Instance.Get();",
                    item.ColumnName, item.DbColumn.ForeignKeyTableName);
            }

            return sb.ToString();
        }

        private static string SetDtosDefaultValue(IEnumerable<ColumnSetting> columns, string baseTab)
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
                sb.AppendLineExWithTabAndFormat(tab1, "dto.{0} = ({1})_{0}Filter.FilterValue;", item.ColumnName, item.DbColumn.DataType);
                sb.AppendLineExWithTab(baseTab, "}");
            }

            return sb.ToString();
        }

        private static string GetHeaderFilterModelType(ColumnSetting columnSetting)
        {
            switch (columnSetting.DataGridColumnType)
            {
                case "DataGridTextColumnExt":
                case "DataGridRightAlignTextColumn":
                case "DataGridForeignKeyColumn":
                    return "HeaderTextFilterModel";
                case "DataGridComboBoxColumnExt":
                    return "HeaderComboBoxFilterModel";
                case "DataGridCheckBoxColumnExt":
                    return "HeaderCheckFilterModel";
                case "DataGridDateColumn":
                    return "HeaderDateFilterModel";
            }

            return "HeaderTextFilterModel";
        }
    }
}
