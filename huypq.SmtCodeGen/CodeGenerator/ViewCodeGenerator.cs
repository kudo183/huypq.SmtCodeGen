using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class ViewCodeGenerator
    {
        public static void GenViewCode(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#ViewTemplate.xaml.cs.txt")))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    result.AppendFormat("{0}{1}", line.Replace("<EntityName>", table.TableName), Constant.LineEnding);
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + "View.xaml.cs"), result.Value.ToString());
            }
        }

        public static void GenViewXamlCode(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#ViewTemplate.xaml.txt")))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<DataGridColumns>")
                    {
                        result.AppendFormat("{0}{1}", DataGridColumns(table.Columns, baseTab), Constant.LineEnding);
                    }
                    else
                    {
                        result.AppendFormat("{0}{1}", line.Replace("<EntityName>", table.TableName), Constant.LineEnding);
                    }
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + "View.xaml"), result.Value.ToString());
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
                    sb.AppendFormat("{0}<SimpleDataGrid:DataGridTextColumnExt Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, UpdateSourceTrigger=PropertyChanged, Converter={{x:Static converter:LongToDateTimeStringConverter.Instance}}}}\"/>{2}", baseTab, item.ColumnName, Constant.LineEnding);
                    continue;
                }

                sb.AppendFormat("{0}{1}", GetDataGridColumnFromProperty(item, baseTab), Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
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
                    sb.AppendFormat("{0}<SimpleDataGrid:DataGridComboBoxColumnExt Header=\"{1}\"{2}", baseTab, column.ColumnName, Constant.LineEnding);
                    sb.AppendFormat("{0}SelectedValuePath=\"ID\"{1}", tab1, Constant.LineEnding);
                    sb.AppendFormat("{0}DisplayMemberPath=\"DisplayText\"{1}", tab1, Constant.LineEnding);
                    sb.AppendFormat("{0}SelectedValueBinding=\"{{Binding {1}, UpdateSourceTrigger=PropertyChanged}}\"{2}", tab1, column.ColumnName, Constant.LineEnding);
                    sb.AppendFormat("{0}ItemsSource=\"{{Binding {1}DataSource}}\"/>", tab1, column.ColumnName);
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
