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

            FileUtils.DeleteAllFileEndWith(outputPath, ViewFileNameSubFix, "Complex" + ViewFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ViewFileNameSubFix), result.Value.ToString());
            }

            GenViewXamlCode(tableSettings, outputPath);
        }

        private static void GenViewXamlCode(IEnumerable<TableSetting> tableSettings, string outputPath)
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
                        result.Append(DataGridColumns(table.ColumnSettings.OrderBy(p => p.Order), baseTab));
                    }
                    else
                    {
                        result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, ViewXamlFileNameSubFix, "Complex" + ViewXamlFileNameSubFix);

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
                var columnName = item.GetColumnNameForCodeGen();
                if (columnName == "TenantID")
                {
                    continue;
                }

                if (columnName == "CreateTime" || columnName == "LastUpdateTime")
                {
                    sb.AppendLineExWithTabAndFormat(baseTab, "<SimpleDataGrid:DataGridTextColumnExt Header=\"{0}\" IsReadOnly=\"True\" Binding=\"{{Binding {0}, UpdateSourceTrigger=PropertyChanged, Converter={{x:Static converter:LongToDateTimeStringConverter.Instance}}}}\"/>", columnName);
                    continue;
                }

                sb.AppendLineEx(GetDataGridColumnFromProperty(item, baseTab));
            }

            return sb.ToString();
        }

        private static string GetDataGridColumnFromProperty(ColumnSetting columnSetting, string baseTab)
        {
            var columnName = columnSetting.GetColumnNameForCodeGen();
            var columnType = columnSetting.DataGridColumnType;
            var header = string.Format(" Header=\"{0}\"", columnName);
            var width = columnSetting.Width > 0 ? string.Format(" Width=\"{0}\"", 80) : "";
            var tabStop = columnSetting.IsReadOnly ? "" : columnSetting.IsTabStop ? "" : " SimpleDataGrid:DataGridColumnAttachedProperty.IsTabStop=\"False\"";

            if (columnType == "DataGridComboBoxColumnExt")
            {
                var tab1 = baseTab + Constant.Tab;
                var sb = new StringBuilder();
                sb.AppendLineExWithTabAndFormat(baseTab, "<SimpleDataGrid:{0}{1}{2}", columnType, header, width);
                if (string.IsNullOrEmpty(tabStop) == false)
                {
                    sb.AppendLineExWithTabAndFormat(tab1, "SimpleDataGrid:DataGridColumnAttachedProperty.IsTabStop=\"False\"");
                }
                sb.AppendLineExWithTab(tab1, "SelectedValuePath=\"ID\"");
                sb.AppendLineExWithTab(tab1, "DisplayMemberPath=\"DisplayText\"");
                sb.AppendLineExWithTabAndFormat(tab1, "SelectedValueBinding=\"{{Binding {0}, UpdateSourceTrigger=PropertyChanged}}\"", columnName);
                sb.AppendTabAndFormat(tab1, "ItemsSource=\"{{Binding {0}DataSource}}\"/>", columnName);
                return sb.ToString();
            }

            if (columnType == "DataGridForeignKeyColumn")
            {
                var tab1 = baseTab + Constant.Tab;
                var tab2 = tab1 + Constant.Tab;
                var sb = new StringBuilder();
                sb.AppendLineExWithTabAndFormat(baseTab, "<SimpleDataGrid:{0}{1}{2}", columnType, header, width);
                if (string.IsNullOrEmpty(tabStop) == false)
                {
                    sb.AppendLineExWithTabAndFormat(tab1, "SimpleDataGrid:DataGridColumnAttachedProperty.IsTabStop=\"False\"");
                }
                sb.AppendLineExWithTabAndFormat(tab1, "Binding=\"{{Binding {0}}}\"", columnName);
                sb.AppendLineExWithTabAndFormat(tab1, "DisplayTextBinding=\"{{Binding {0}Navigation.DisplayText}}\">", columnName);
                sb.AppendLineExWithTabAndFormat(tab1, "<SimpleDataGrid:{0}.PopupView>", columnType);
                sb.AppendLineExWithTabAndFormat(tab2, "<view:{0}View KeepSelectionType=\"KeepSelectedValue\"/>", columnSetting.DbColumn.ForeignKeyTableName);
                sb.AppendLineExWithTabAndFormat(tab1, "</SimpleDataGrid:{0}.PopupView>", columnType);
                sb.AppendTabAndFormat(baseTab, "</SimpleDataGrid:{0}>", columnType);
                return sb.ToString();
            }

            var readOnly = columnSetting.IsReadOnly ? " IsReadOnly=\"True\"" : "";
            var binding = string.Format(" Binding=\"{{Binding {0}{1}, UpdateSourceTrigger=PropertyChanged}}\"", columnName, columnSetting.IsReadOnly ? ", Mode=OneWay" : "");
            var customProperty = string.Empty;

            switch (columnSetting.DataGridColumnType)
            {
                case "DataGridTextColumnExt":
                case "DataGridDateColumn":
                case "DataGridCheckBoxColumnExt":
                    break;
                case "DataGridRightAlignTextColumn":
                    binding = string.Format(" Binding=\"{{Binding {0}, UpdateSourceTrigger=PropertyChanged, StringFormat=\\{{0:N0\\}}{1}}}\"", columnName, columnSetting.IsReadOnly ? ", Mode=OneWay" : "");
                    break;
            }

            return string.Format("{0}<SimpleDataGrid:{1}{2}{3}{4}{5}{6}{7}/>", baseTab, columnType, width, header, tabStop, readOnly, binding, customProperty);
        }
    }
}
