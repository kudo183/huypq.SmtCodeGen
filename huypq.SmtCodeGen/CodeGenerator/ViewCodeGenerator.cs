using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class ViewCodeGenerator
    {
        private const string ViewTemplateFileName = "#ViewTemplate.xaml.cs.txt";
        private const string ViewFileNameSubFix = "View.xaml.cs";

        private const string ViewXamlTemplateFileName = "#ViewTemplate.xaml.txt";
        private const string ViewXamlFileNameSubFix = "View.xaml";

        public static void GenViewCode(IEnumerable<TableSetting> tableSettings, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tableSettings)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ViewTemplateFileName)))
            {
                foreach (var table in tableSettings)
                {
                    var result = results[table.TableName];

                    result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ViewFileNameSubFix), result.Value.ToString());
            }
        }

        public static void GenViewXamlCode(IEnumerable<TableSetting> tableSettings, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tableSettings)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ViewXamlTemplateFileName)))
            {
                foreach (var table in tableSettings)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<DataGridColumns>")
                    {
                        result.Append(DataGridColumns(table.ColumnSettings, baseTab));
                    }
                    else
                    {
                        result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ViewXamlFileNameSubFix), result.Value.ToString());
            }
        }

        private static string DataGridColumns(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var tab1 = baseTab + Constant.Tab;

            foreach (var item in columnSettings)
            {
                if (item.ColumnName == "TenantID")
                {
                    continue;
                }

                if (item.ColumnName == "CreateTime" || item.ColumnName == "LastUpdateTime")
                {
                    sb.AppendLineExWithTabAndFormat(baseTab, "<SimpleDataGrid:DataGridTextColumnExt Header=\"{0}\" IsReadOnly=\"True\" Binding=\"{{Binding {0}, UpdateSourceTrigger=PropertyChanged, Converter={{x:Static converter:LongToDateTimeStringConverter.Instance}}}}\"/>", item.ColumnName);
                    continue;
                }

                sb.AppendLineEx(GetDataGridColumnFromProperty(item, baseTab));
            }

            return sb.ToString();
        }

        private static string GetDataGridColumnFromProperty(ColumnSetting columnSetting, string baseTab)
        {
            var columnName = columnSetting.ColumnName;
            if (columnSetting.IsReadOnly == true)
            {
                return GetDataGridReadOnlyColumn(columnName, columnSetting.DataGridColumnType, baseTab);
            }

            switch (columnSetting.DataGridColumnType)
            {
                case "DataGridTextColumnExt":
                    return string.Format("{0}<SimpleDataGrid:DataGridTextColumnExt Width =\"80\" Header=\"{1}\" Binding=\"{{Binding {1}}}\"/>", baseTab, columnName);
                case "DataGridComboBoxColumnExt":
                    var tab1 = baseTab + Constant.Tab;
                    var sb = new StringBuilder();
                    sb.AppendLineExWithTabAndFormat(baseTab, "<SimpleDataGrid:DataGridComboBoxColumnExt Header=\"{0}\"", columnName);
                    sb.AppendLineExWithTab(tab1, "SelectedValuePath=\"ID\"");
                    sb.AppendLineExWithTab(tab1, "DisplayMemberPath=\"DisplayText\"");
                    sb.AppendLineExWithTabAndFormat(tab1, "SelectedValueBinding=\"{{Binding {0}, UpdateSourceTrigger=PropertyChanged}}\"", columnName);
                    sb.AppendTabAndFormat(tab1, "ItemsSource=\"{{Binding {0}DataSource}}\"/>", columnName);
                    return sb.ToString();
                case "DataGridRightAlignTextColumn":
                    return string.Format("{0}<SimpleDataGrid:DataGridRightAlignTextColumn Header=\"{1}\" Binding=\"{{Binding {1}, StringFormat=\\{{0:N0\\}}}}\"/>", baseTab, columnName);
                case "DataGridCheckBoxColumnExt":
                    return string.Format("{0}<SimpleDataGrid:DataGridCheckBoxColumnExt Header=\"{1}\" Binding=\"{{Binding {1}}}\"/>", baseTab, columnName);
                case "DataGridDateColumn":
                    return string.Format("{0}<SimpleDataGrid:DataGridDateColumn Header=\"{1}\" Binding=\"{{Binding {1}}}\"/>", baseTab, columnName);
                case "DataGridForeignKeyColumn":
                    return string.Format("{0}<SimpleDataGrid:DataGridForeignKeyColumn Header=\"{1}\" ReferenceType=\"{{x:Type view:{2}View}}\" Binding=\"{{Binding {1}}}\"/>",
                        baseTab, columnName, columnSetting.DbColumn.ForeignKeyTableName);
            }

            return string.Format("{0}<SimpleDataGrid:DataGridTextColumnExt Header=\"{1}\" Binding=\"{{Binding {1}}}\"/>", baseTab, columnName);
        }

        private static string GetDataGridReadOnlyColumn(string columnName, string columnType, string baseTab)
        {
            switch (columnType)
            {
                case "DataGridTextColumnExt":
                    return string.Format("{0}<SimpleDataGrid:DataGridTextColumnExt Width =\"80\" Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, Mode=OneWay}}\"/>", baseTab, columnName);
                case "DataGridRightAlignTextColumn":
                    return string.Format("{0}<SimpleDataGrid:DataGridRightAlignTextColumn Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, Mode=OneWay, StringFormat=\\{{0:N0\\}}}}\"/>", baseTab, columnName);
                case "DataGridCheckBoxColumnExt":
                    return string.Format("{0}<SimpleDataGrid:DataGridCheckBoxColumnExt Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, Mode=OneWay}}\"/>", baseTab, columnName);
            }

            return string.Format("{0}<SimpleDataGrid:DataGridTextColumnExt Width =\"80\" Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, Mode=OneWay}}\"/>", baseTab, columnName);
        }
    }
}
