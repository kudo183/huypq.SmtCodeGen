using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huypq.SmtCodeGen
{
    public static class Angular2TSCodeGenerator
    {
        private const string Angular2TSTemplateFileName = "#Angular2TSTemplate.ts.txt";
        private const string Angular2TSFileNameSubFix = ".component.ts";

        private const string Angular2IndexTemplate = "#Angular2IndexTemplate.ts.txt";
        private const string Angular2TestGenTemplate = "#Angular2TestGenTemplate.ts.txt";

        public static void GenAngular2TS(IEnumerable<TableSetting> tables, string outputPath)
        {
            GenAngular2ComponentTS(tables, outputPath);
            GenAngular2IndexTS(tables, outputPath);
            GenAngular2TestGenTS(tables, outputPath);
        }

        #region index.ts
        public static void GenAngular2IndexTS(IEnumerable<TableSetting> tables, string outputPath)
        {
            var result = new StringBuilder();
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, Angular2IndexTemplate)))
            {
                var trimmedEnd = line.TrimEnd();
                var trimmed = trimmedEnd.TrimStart();
                var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);

                if (trimmed == "<Imports>")
                {
                    result.Append(Imports(tables, baseTab));
                }
                else if (trimmed == "<Exports>")
                {
                    result.Append(Exports(tables, baseTab));
                }
                else if (trimmed == "<Declarations_exports>")
                {
                    result.Append(Declarations_exports(tables, baseTab));
                }
                else
                {
                    result.AppendLineEx(line);
                }
            }

            FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, "index.ts"), result.ToString());
        }

        public static string Imports(IEnumerable<TableSetting> tables, string baseTab)
        {
            var sb = new StringBuilder();

            foreach (var item in tables)
            {
                sb.AppendLineEx($"import {{ {item.TableName}Component }} from './{item.TableName}.component';");
            }

            return sb.ToString();
        }

        public static string Exports(IEnumerable<TableSetting> tables, string baseTab)
        {
            var sb = new StringBuilder();

            foreach (var item in tables)
            {
                sb.AppendLineEx($"export * from './{item.TableName}.component';");
            }

            return sb.ToString();
        }

        public static string Declarations_exports(IEnumerable<TableSetting> tables, string baseTab)
        {
            var sb = new StringBuilder();

            foreach (var item in tables)
            {
                sb.AppendLineExWithTab(baseTab, $"{item.TableName}Component,");
            }

            return sb.ToString();
        }
        #endregion

        #region test-gen.component.ts
        public static void GenAngular2TestGenTS(IEnumerable<TableSetting> tables, string outputPath)
        {
            var result = new StringBuilder();
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, Angular2TestGenTemplate)))
            {
                var trimmedEnd = line.TrimEnd();
                var trimmed = trimmedEnd.TrimStart();
                var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);

                if (trimmed == "<NgSwitchCases>")
                {
                    result.Append(NgSwitchCases(tables, baseTab));
                }
                else if (trimmed == "<DeclareItems>")
                {
                    result.Append(DeclareItems(tables, baseTab));
                }
                else
                {
                    result.AppendLineEx(line);
                }
            }

            FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, "test-gen.component.ts"), result.ToString());
        }

        public static string NgSwitchCases(IEnumerable<TableSetting> tables, string baseTab)
        {
            var sb = new StringBuilder();

            foreach (var item in tables)
            {
                sb.AppendLineExWithTab(baseTab, $"<app-{item.TableName} *ngSwitchCase=\"'{item.TableName}'\"></app-{item.TableName}>");
            }

            return sb.ToString();
        }

        public static string DeclareItems(IEnumerable<TableSetting> tables, string baseTab)
        {
            var sb = new StringBuilder();
            sb.AppendTab(baseTab, "items = [");
            foreach (var item in tables)
            {
                sb.Append($"'{item.TableName}', ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.AppendLineEx("];");
            return sb.ToString();
        }
        #endregion

        #region component.ts
        public static void GenAngular2ComponentTS(IEnumerable<TableSetting> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }

            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, Angular2TSTemplateFileName)))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);

                    var columnSettings = table.ColumnSettings;
                    if (trimmed == "<DeclareReferenceDataSource>")
                    {
                        result.Append(DeclareReferenceDataSource(columnSettings, baseTab));
                    }
                    else if (trimmed == "<LoadReferenceDatas>")
                    {
                        result.Append(LoadReferenceDatas(columnSettings, baseTab));
                    }
                    else if (trimmed == "<SetReferenceDataSource>")
                    {
                        result.Append(SetReferenceDataSource(columnSettings, baseTab));
                    }
                    else
                    {
                        result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, Angular2TSFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + Angular2TSFileNameSubFix), result.Value.ToString());
            }
        }

        private static string DeclareReferenceDataSource(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            var needReferenceDataColumn = columnSettings.Where(p => p.IsNeedReferenceData).ToList();

            if (needReferenceDataColumn.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in needReferenceDataColumn)
            {
                var columnName = item.GetColumnNameForCodeGen();

                sb.AppendLineExWithTab(baseTab, $"{ToLowerFirstChar(columnName)}Source = [];");
            }

            return sb.ToString();
        }

        private static string LoadReferenceDatas(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            var sb = new StringBuilder();

            var tab1 = baseTab + Constant.JsTab;
            var tab2 = tab1 + Constant.JsTab;

            var needReferenceDataColumn = columnSettings.Where(p => p.IsNeedReferenceData).ToList();

            if (needReferenceDataColumn.Count() == 0)
            {
                sb.AppendLineExWithTab(baseTab, "this.partialMethodService.loadReferenceDataPartial(this.className, [this]).subscribe(event => {");
                sb.AppendLineExWithTab(tab1, "if (this.autoLoad === true) { this.onLoad(undefined); }");
                sb.AppendLineExWithTab(baseTab, "});");
                return sb.ToString();
            }

            var columnNameArray = needReferenceDataColumn.Select(p => $"'{p.DbColumn.ForeignKeyTableName}'").Aggregate((i, j) => $"{i}, {j}");

            sb.AppendLineExWithTab(baseTab, $"this.refDataService.gets([{columnNameArray}]).subscribe(data => {{");

            var index = 0;
            foreach (var item in needReferenceDataColumn)
            {
                var columnName = item.GetColumnNameForCodeGen();

                sb.AppendLineExWithTab(tab1, $"this.{ToLowerFirstChar(columnName)}Source = data[{index}].items;");
                sb.AppendLineExWithTab(tab1, $"this.grid.setHeaderItems({item.Order}, data[{index}].items);");
                index++;
            }

            sb.AppendLineExWithTab(tab1, "this.partialMethodService.loadReferenceDataPartial(this.className, [this]).subscribe(event => {");
            sb.AppendLineExWithTab(tab2, "if (this.autoLoad === true) { this.onLoad(undefined); }");
            sb.AppendLineExWithTab(tab1, "});");
            sb.AppendLineExWithTab(baseTab, "});");
            return sb.ToString();
        }

        private static string SetReferenceDataSource(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            var needReferenceDataColumn = columnSettings.Where(p => p.IsNeedReferenceData).ToList();

            if (needReferenceDataColumn.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in needReferenceDataColumn)
            {
                var columnName = item.GetColumnNameForCodeGen();
                var lowerFirstChar = ToLowerFirstChar(columnName);
                sb.AppendLineExWithTab(baseTab, $"item.{lowerFirstChar}Source = this.{lowerFirstChar}Source;");
            }

            return sb.ToString();
        }
        #endregion

        private static string ToLowerFirstChar(string text)
        {
            return text[0].ToString().ToLower() + text.Substring(1);
        }
    }
}
