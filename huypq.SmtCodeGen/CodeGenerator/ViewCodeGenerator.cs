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

                    result.AppendLine(line.Replace("<EntityName>", table.TableName));
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
                        result.AppendLine(DataGridColumns(table.Columns, baseTab));
                    }
                    else
                    {
                        result.AppendLine(line.Replace("<EntityName>", table.TableName));
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
                if (item.IsIdentity == true)
                {
                    sb.AppendFormat("{0}<SimpleDataGrid:DataGridTextColumnExt Width=\"80\" Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, Mode=OneWay}}\"/>{2}", baseTab, item.ColumnName, Constant.LineEnding);
                }
                else if (item.IsForeignKey == true)
                {
                    sb.AppendFormat("{0}<SimpleDataGrid:DataGridComboBoxColumnExt Header=\"{1}\"{2}", baseTab, item.ColumnName, Constant.LineEnding);
                    sb.AppendFormat("{0}SelectedValuePath=\"ID\"{1}", tab1, Constant.LineEnding);
                    sb.AppendFormat("{0}DisplayMemberPath=\"DisplayText\"{1}", tab1, Constant.LineEnding);
                    sb.AppendFormat("{0}SelectedValueBinding=\"{{Binding {1}, UpdateSourceTrigger=PropertyChanged}}\"{2}", tab1, item.ColumnName, Constant.LineEnding);
                    sb.AppendFormat("{0}ItemsSource=\"{{Binding {1}DataSource}}\"/>{2}", tab1, item.ColumnName, Constant.LineEnding);
                }
                else
                {
                    var columnType = GetDataGridColumnTypeFromProperty(item);
                    sb.AppendFormat("{0}<SimpleDataGrid:{1} Header=\"{2}\" Binding=\"{{Binding {2}, UpdateSourceTrigger=PropertyChanged}}\"/>{3}", baseTab, columnType, item.ColumnName, Constant.LineEnding);
                }
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string GetDataGridColumnTypeFromProperty(DbTableColumn column)
        {
            if (column.IsIdentity == true)
            {
                return "DataGridTextColumnExt";
            }
            if (column.IsForeignKey == true)
            {
                return "DataGridComboBoxColumnExt";
            }
            var dataType = column.DataType;
            if (dataType == "string" || dataType == "int" || dataType == "int?")
            {
                return "DataGridTextColumnExt";
            }
            else if (dataType == "bool" || dataType == "bool?")
            {
                return "DataGridCheckBoxColumnExt";
            }
            else if (dataType == "System.DateTime" || dataType == "System.DateTime?")
            {
                return "DataGridDateColumn";
            }

            return "DataGridTextColumnExt";
        }
    }
}
