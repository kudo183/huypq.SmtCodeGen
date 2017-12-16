using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class ControllersCodeGenerator
    {
        private const string ControllerTemplateFileName = "#ControllerTemplate.txt";
        private const string ControllerFileNameSubFix = "Controller.cs";

        //private const string ControllerPartTemplateFileName = "#ControllerPartTemplate.txt";
        //private const string ControllerPartFileNameSubFix = "Controller.part.cs";

        public static void GenControllersClass(IEnumerable<TableSetting> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ControllerTemplateFileName)))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<InitDtoProperties>")
                    {
                        result.Append(InitDtoProperties(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<InitEntityProperties>")
                    {
                        result.Append(InitEntityProperties(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<OverrideUpdateEntityEFCore>")
                    {
                        result.Append(OverrideUpdateEntityEFCore(table, baseTab));
                    }
                    else if (trimmed == "<OverrideUpdateEntityEFFull>")
                    {
                        result.Append(OverrideUpdateEntityEFFull(table, baseTab));
                    }
                    else
                    {
                        result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                    }
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, ControllerFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ControllerFileNameSubFix), result.Value.ToString());
            }

            //GenControllersPartialClass(tables, outputPath);
        }

        //private static void GenControllersPartialClass(IEnumerable<TableSetting> tables, string outputPath)
        //{
        //    var referencedTable = tables.Where(p => p.DbTable.ReferencesToThisTable.Count > 0);
        //    if (referencedTable.Count() == 0)
        //    {
        //        return;
        //    }

        //    var results = new Dictionary<string, StringBuilder>();
        //    foreach (var table in referencedTable)
        //    {
        //        results.Add(table.TableName, new StringBuilder());
        //    }

        //    foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ControllerPartTemplateFileName)))
        //    {
        //        foreach (var table in referencedTable)
        //        {
        //            var result = results[table.TableName];
        //            result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
        //        }
        //    }

        //    foreach (var result in results)
        //    {
        //        var filePath = System.IO.Path.Combine(outputPath, result.Key + ControllerPartFileNameSubFix);
        //        if (System.IO.File.Exists(filePath) == false)
        //        {
        //            FileUtils.WriteAllTextInUTF8(filePath, result.Value.ToString());
        //        }
        //    }
        //}

        private static string InitDtoProperties(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columnSettings)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "{0} = entity.{0},", item.GetColumnNameForCodeGen());
            }

            sb.Remove(sb.Length - Constant.LineEnding.Length - ",".Length, 1);
            return sb.ToString();
        }

        private static string InitEntityProperties(IEnumerable<ColumnSetting> columnSettings, string baseTab)
        {
            if (columnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columnSettings)
            {
                if (item.IsReadOnly == true && item.DbColumn.IsIdentity == false && item.IsSmtColumn() == false)
                {
                    sb.AppendLineExWithTabAndFormat(baseTab, "//{0} = dto.{0},", item.GetColumnNameForCodeGen());
                }
                else
                {
                    sb.AppendLineExWithTabAndFormat(baseTab, "{0} = dto.{0},", item.GetColumnNameForCodeGen());
                }
            }

            sb.Remove(sb.Length - Constant.LineEnding.Length - ",".Length, 1);
            return sb.ToString();
        }

        private static string OverrideUpdateEntityEFCore(TableSetting tableSetting, string baseTab)
        {
            if (tableSetting.ColumnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var list = tableSetting.ColumnSettings.Where(item => item.IsReadOnly == true && item.DbColumn.IsIdentity == false && item.IsSmtColumn() == false).ToList();
            if (list.Count > 0)
            {
                var tab1 = baseTab + Constant.Tab;

                sb.AppendLineExWithTabAndFormat(baseTab, "protected override void UpdateEntity(SqlDbContext context, {0} entity)", tableSetting.TableName);
                sb.AppendLineExWithTab(baseTab, "{");
                sb.AppendLineExWithTabAndFormat(tab1, "var entry = context.{0}.Update(entity);", tableSetting.TableName);
                foreach (var item in list)
                {
                    sb.AppendLineExWithTabAndFormat(tab1, "entry.Property(p => p.{0}).IsModified = false;", item.ColumnName);
                }
                sb.AppendLineExWithTab(baseTab, "}");
            }
            return sb.ToString();
        }

        private static string OverrideUpdateEntityEFFull(TableSetting tableSetting, string baseTab)
        {
            if (tableSetting.ColumnSettings.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var list = tableSetting.ColumnSettings.Where(item => item.IsReadOnly == true && item.DbColumn.IsIdentity == false && item.IsSmtColumn() == false).ToList();
            if (list.Count > 0)
            {
                var tab1 = baseTab + Constant.Tab;

                sb.AppendLineExWithTabAndFormat(baseTab, "protected override void UpdateEntity(SqlDbContext context, {0} entity)", tableSetting.TableName);
                sb.AppendLineExWithTab(baseTab, "{");
                sb.AppendLineExWithTabAndFormat(tab1, "var entry = context.Entry(entity);");
                foreach (var item in list)
                {
                    sb.AppendLineExWithTabAndFormat(tab1, "entry.Property(p => p.{0}).IsModified = false;", item.ColumnName);
                }
                sb.AppendLineExWithTab(baseTab, "}");
            }
            return sb.ToString();
        }
    }
}
