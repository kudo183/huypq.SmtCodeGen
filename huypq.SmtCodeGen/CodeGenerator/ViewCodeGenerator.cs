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

        public static void GenViewCode(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ViewTemplateFileName)))
            {
                foreach (var table in tables)
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

        public static void GenViewXamlCode(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ViewXamlTemplateFileName)))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<DataGridColumns>")
                    {
                        result.Append(DataGridColumns(table.Columns, baseTab));
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

        private static string DataGridColumns(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var tab1 = baseTab + Constant.Tab;

            foreach (var item in columns)
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

        private static string GetDataGridColumnFromProperty(DbTableColumn column, string baseTab)
        {
            if (column.IsIdentity == true)
            {
                return string.Format("{0}<SimpleDataGrid:DataGridTextColumnExt Width =\"80\" Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, Mode=OneWay}}\"/>", baseTab, column.ColumnName);
            }
            if (column.IsForeignKey == true)
            {
                if (column.IsReferenceToLargeTable)
                {
                    return string.Format("{0}<SimpleDataGrid:DataGridTextColumnExt Width=\"80\" Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, Mode=OneWay}}\"/>", baseTab, column.ColumnName);
                }
                else
                {
                    var tab1 = baseTab + Constant.Tab;
                    var sb = new StringBuilder();
                    sb.AppendLineExWithTabAndFormat(baseTab, "<SimpleDataGrid:DataGridComboBoxColumnExt Header=\"{0}\"", column.ColumnName);
                    sb.AppendLineExWithTab(tab1, "SelectedValuePath=\"ID\"");
                    sb.AppendLineExWithTab(tab1, "DisplayMemberPath=\"DisplayText\"");
                    sb.AppendLineExWithTabAndFormat(tab1, "SelectedValueBinding=\"{{Binding {0}, UpdateSourceTrigger=PropertyChanged}}\"", column.ColumnName);
                    sb.AppendTabAndFormat(tab1, "ItemsSource=\"{{Binding {0}DataSource}}\"/>", column.ColumnName);
                    return sb.ToString();
                }
            }

            var dataType = column.DataType;
            if (dataType == "int" || dataType == "int?" || dataType == "long" || dataType == "long?")
            {
                return string.Format("{0}<SimpleDataGrid:DataGridRightAlignTextColumn Header=\"{1}\" Binding=\"{{Binding {1}, StringFormat=\\{{0:N0\\}}}}\"/>", baseTab, column.ColumnName);
            }
            else if (dataType == "bool" || dataType == "bool?")
            {
                return string.Format("{0}<SimpleDataGrid:DataGridCheckBoxColumnExt Header=\"{1}\" Binding=\"{{Binding {1}}}\"/>", baseTab, column.ColumnName);
            }
            else if (dataType == "System.DateTime" || dataType == "System.DateTime?")
            {
                return string.Format("{0}<SimpleDataGrid:DataGridDateColumn Header=\"{1}\" Binding=\"{{Binding {1}}}\"/>", baseTab, column.ColumnName);
            }

            return string.Format("{0}<SimpleDataGrid:DataGridTextColumnExt Header=\"{1}\" Binding=\"{{Binding {1}}}\"/>", baseTab, column.ColumnName);
        }
    }
}
