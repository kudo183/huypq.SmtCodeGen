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
            var columnType = columnSetting.DataGridColumnType;
            var header = string.Format(" Header=\"{0}\"", columnName);
            var width = columnSetting.Width > 0 ? string.Format(" Width =\"{0}\"", 80) : "";

            if (columnType == "DataGridComboBoxColumnExt")
            {
                var tab1 = baseTab + Constant.Tab;
                var sb = new StringBuilder();
                sb.AppendLineExWithTabAndFormat(baseTab, "<SimpleDataGrid:{0}{1}{2}", columnType, header, width);
                sb.AppendLineExWithTab(tab1, "SelectedValuePath=\"ID\"");
                sb.AppendLineExWithTab(tab1, "DisplayMemberPath=\"DisplayText\"");
                sb.AppendLineExWithTabAndFormat(tab1, "SelectedValueBinding=\"{{Binding {0}, UpdateSourceTrigger=PropertyChanged}}\"", columnName);
                sb.AppendTabAndFormat(tab1, "ItemsSource=\"{{Binding {0}DataSource}}\"/>", columnName);
                return sb.ToString();
            }

            var tabStop = columnSetting.IsTabStop ? " SimpleDataGrid:DataGridColumnAttachedProperty.IsTabStop =\"True\"" : "";
            var readOnly = columnSetting.IsReadOnly ? " IsReadOnly=\"True\"" : "";
            var binding = string.Format(" Binding=\"{{Binding {0}{1}}}\"", columnName, columnSetting.IsReadOnly ? ", Mode=OneWay" : "");
            var customProperty = string.Empty;
            switch (columnSetting.DataGridColumnType)
            {
                case "DataGridTextColumnExt":
                case "DataGridDateColumn":
                case "DataGridCheckBoxColumnExt":
                    break;
                case "DataGridRightAlignTextColumn":
                    binding = string.Format(" Binding=\"{{Binding {0}, StringFormat=\\{{0:N0\\}}{1}}}\"", columnName, columnSetting.IsReadOnly ? ", Mode=OneWay" : "");
                    break;
                case "DataGridForeignKeyColumn":
                    customProperty = string.Format(" ReferenceType=\"{{x:Type view:{0}View}}\"", columnSetting.DbColumn.ForeignKeyTableName);
                    break;
            }

            return string.Format("{0}<SimpleDataGrid:{1}{2}{3}{4}{5}{6}{7}/>", baseTab, columnType, width, header, tabStop, readOnly, binding, customProperty);
        }
    }
}
