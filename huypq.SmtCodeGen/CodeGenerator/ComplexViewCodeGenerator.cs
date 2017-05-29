using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class ComplexViewCodeGenerator
    {
        private const string ComplexViewTemplateFileName = "#ComplexViewTemplate.xaml.cs.txt";
        private const string ComplexViewFileNameSubFix = "ComplexView.xaml.cs";

        private const string ComplexViewXamlTemplateFileName = "#ComplexViewTemplate.xaml.txt";
        private const string ComplexViewXamlFileNameSubFix = "ComplexView.xaml";

        public static void GenComplexViewCode(IEnumerable<MasterDetail> complexViews, IEnumerable<TableSetting> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var cv in complexViews)
            {
                results.Add(cv.ViewName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ComplexViewTemplateFileName)))
            {
                foreach (var cv in complexViews)
                {
                    var result = results[cv.ViewName];

                    result.AppendLineEx(line.Replace("<ViewName>", cv.ViewName));
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, ComplexViewFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ComplexViewFileNameSubFix), result.Value.ToString());
            }

            GenComplexViewXamlCode(complexViews, tables, outputPath);
        }

        private static void GenComplexViewXamlCode(IEnumerable<MasterDetail> complexViews, IEnumerable<TableSetting> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var cv in complexViews)
            {
                results.Add(cv.ViewName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, ComplexViewXamlTemplateFileName)))
            {
                foreach (var cv in complexViews)
                {
                    var result = results[cv.ViewName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<GridContent>")
                    {
                        result.Append(GridContent(cv, tables, baseTab));
                    }
                    else
                    {
                        result.AppendLineEx(line.Replace("<ViewName>", cv.ViewName));
                    }
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, ComplexViewXamlFileNameSubFix);

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ComplexViewXamlFileNameSubFix), result.Value.ToString());
            }
        }

        private static string GridContent(MasterDetail view, IEnumerable<TableSetting> tables, string baseTab)
        {
            var sb = new StringBuilder();

            var tab1 = baseTab + Constant.Tab;

            sb.AppendLineExWithTab(baseTab, "<Grid.RowDefinitions>");
            for (int i = 0; i < view.Levels.Count; i++)
            {
                sb.AppendLineExWithTab(tab1, "<RowDefinition/>");
            }
            sb.AppendLineExWithTab(baseTab, "</Grid.RowDefinitions>");

            sb.AppendLineExWithTabAndFormat(baseTab, "<view:{0}View Grid.Row=\"0\" Abstraction:BaseComplexView.ViewLevel=\"0\"/>", view.Levels[0]);

            for (int i = 1; i < view.Levels.Count; i++)
            {
                var parentTable = tables.First(p => p.TableName == view.Levels[i - 1]);
                var foreignKey = parentTable.DbTable.ReferencesToThisTable.First(p => p.ReferenceTableName == view.Levels[i]).PropertyName;
                sb.AppendLineExWithTabAndFormat(baseTab, "<view:{0}View Grid.Row=\"{1}\" Abstraction:BaseComplexView.ViewLevel=\"{1}\" Abstraction:BaseComplexView.FilterProperty=\"{2}\"/>", view.Levels[i], i, foreignKey);
            }

            for (int i = 0; i < view.Levels.Count - 1; i++)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "<GridSplitter Grid.Row=\"{0}\" VerticalAlignment=\"Bottom\" HorizontalAlignment=\"Stretch\" Height=\"2\"/>", i);
            }

            return sb.ToString();
        }
    }
}
