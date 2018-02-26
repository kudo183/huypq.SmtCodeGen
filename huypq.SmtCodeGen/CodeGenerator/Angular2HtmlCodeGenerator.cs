using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huypq.SmtCodeGen
{
    public static class Angular2HtmlCodeGenerator
    {
        private const string Angular2HtmlTemplateFileName = "#Angular2HtmlTemplate.html.txt";
        private const string Angular2HtmlFileNameSubFix = ".component.html";

        public static void GenAngular2Html(IEnumerable<TableSetting> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }

            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, Angular2HtmlTemplateFileName)))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);

                    var columnSettings = table.ColumnSettings;
                    if (trimmed == "<DataGridColumns>")
                    {
                        result.Append(DataGridColumns(table.ColumnSettings.OrderBy(p => p.Order), baseTab));
                    }
                    else if (trimmed == "<ForeignKeyPickerWindows>")
                    {
                        result.Append(ForeignKeyPickerWindows(table, baseTab));
                    }
                    else
                    {
                        result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, Angular2HtmlFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + Angular2HtmlFileNameSubFix), result.Value.ToString());
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
                if (columnName == "ID" || columnName == "TenantID" || columnName == "CreateTime" || columnName == "LastUpdateTime")
                {
                    continue;
                }

                sb.AppendLineEx(GetDataGridColumnFromProperty(item, baseTab));
            }

            return sb.ToString();
        }

        private static string ForeignKeyPickerWindows(TableSetting tableSetting, string baseTab)
        {
            var columnSettings = tableSetting.ColumnSettings.Where(p => p.IsNeedNavigationData).OrderBy(p => p.Order);
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var tab1 = baseTab + Constant.Tab;

            foreach (var item in columnSettings)
            {
                var columnName = item.GetColumnNameForCodeGen();
                if (columnName == "ID" || columnName == "TenantID" || columnName == "CreateTime" || columnName == "LastUpdateTime")
                {
                    continue;
                }

                var lowerFirstCharColumnName = ToLowerFirstChar(columnName);
                var foreignKeyTableName = item.DbColumn.ForeignKeyTableName;
                var tableName = tableSetting.TableName;
                sb.AppendLineExWithTab(baseTab, $"<h-window [name]=\"'window_{lowerFirstCharColumnName}Of{tableName}'\" #{lowerFirstCharColumnName}Window [visible]=\"false\" [ngContent]=\"{lowerFirstCharColumnName}\" [hasOkButton]=\"true\" [hasCancelButton]=\"true\" [hasCloseButton]=\"false\" [width]=\"500\" [height]=\"200\">");

                sb.AppendLineExWithTab(tab1, $"<app-{foreignKeyTableName} [name]=\"'view_{foreignKeyTableName}Of{tableName}'\" #{lowerFirstCharColumnName}></app-{foreignKeyTableName}>");
                sb.AppendLineExWithTab(baseTab, "</h-window>");

                sb.AppendLineEx();
            }

            return sb.ToString();
        }

        private static string GetDataGridColumnFromProperty(ColumnSetting columnSetting, string baseTab)
        {
            var columnName = columnSetting.GetColumnNameForCodeGen();
            var lowerFirstCharColumnName = ToLowerFirstChar(columnName);
            var editorType = "";
            var dataType = "";
            var filterOperatorType = "";
            var tab1 = baseTab + Constant.Tab;

            var sb = new StringBuilder();
            if (columnSetting.DataGridColumnType == "DataGridComboBoxColumnExt")
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "<h-column [type]=\"EditorTypeEnum.HComboBox\" cellValueProperty=\"{0}\" [cellValueType]=\"DataTypeEnum.Int\" itemsSourceName=\"{0}Source\" itemTextPath=\"displayText\" itemValuePath=\"id\">", lowerFirstCharColumnName);
                sb.AppendLineExWithTabAndFormat(tab1, "<h-header headerText=\"{0}\" [filterOperatorType]=\"FilterOperatorTypeEnum.NUMBER\" [filterType]=\"EditorTypeEnum.TextBox\"></h-header>", columnName);
                sb.AppendTab(baseTab, "</h-column>");
                return sb.ToString();
            }

            if (columnSetting.DataGridColumnType == "DataGridForeignKeyColumn")
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "<h-column [type]=\"EditorTypeEnum.HForeignKeyPicker\" cellValueProperty=\"{0}\" [cellValueType]=\"DataTypeEnum.Int\" itemValuePath=\"id\" [component]=\"{0}Window\">", lowerFirstCharColumnName);
                sb.AppendLineExWithTabAndFormat(tab1, "<h-header headerText=\"{0}\" [filterOperatorType]=\"FilterOperatorTypeEnum.NUMBER\" [filterType]=\"EditorTypeEnum.HComboBox\" itemTextPath=\"displayText\" itemValuePath=\"id\"></h-header>", columnName);
                sb.AppendTab(baseTab, "</h-column>");
                return sb.ToString();
            }

            switch (columnSetting.DataGridColumnType)
            {
                case "DataGridTextColumnExt":
                    editorType = "TextBox";
                    dataType = "String";
                    filterOperatorType = "TEXT";
                    break;
                case "DataGridDateColumn":
                    editorType = "DatePicker";
                    dataType = "Date";
                    filterOperatorType = "NUMBER";
                    break;
                case "DataGridCheckBoxColumnExt":
                    editorType = "CheckBox";
                    dataType = "Bool";
                    filterOperatorType = "Number";
                    break;
                case "DataGridRightAlignTextColumn":
                    editorType = "HNumber";
                    dataType = "Int";
                    filterOperatorType = "Number";
                    break;
            }
            sb.AppendLineExWithTabAndFormat(baseTab, "<h-column [type]=\"EditorTypeEnum.{0}\" cellValueProperty=\"{1}\" [cellValueType]=\"DataTypeEnum.{2}\">", editorType, lowerFirstCharColumnName, dataType);
            sb.AppendLineExWithTabAndFormat(tab1, "<h-header headerText=\"{0}\" [filterOperatorType]=\"FilterOperatorTypeEnum.{1}\" [filterType]=\"EditorTypeEnum.{2}\"></h-header>", columnName, filterOperatorType, editorType);
            sb.AppendTab(baseTab, "</h-column>");
            return sb.ToString();
        }

        private static string ToLowerFirstChar(string text)
        {
            return text[0].ToString().ToLower() + text.Substring(1);
        }
    }
}
