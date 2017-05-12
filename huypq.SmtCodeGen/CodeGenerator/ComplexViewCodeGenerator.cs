using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huypq.SmtCodeGen
{
    public static class ComplexViewCodeGenerator
    {
        public static void GenComplexViewCode(IEnumerable<MasterDetail> complexViews, IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var cv in complexViews)
            {
                results.Add(cv.ViewName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#ComplexViewTemplate.xaml.cs.txt")))
            {
                foreach (var cv in complexViews)
                {
                    var result = results[cv.ViewName];

                    result.AppendLine(line.Replace("<ViewName>", cv.ViewName));
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + "ComplexView.xaml.cs"), result.Value.ToString());
            }
        }

        public static void GenComplexViewXamlCode(IEnumerable<MasterDetail> complexViews, IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var cv in complexViews)
            {
                results.Add(cv.ViewName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#ComplexViewTemplate.xaml.txt")))
            {
                foreach (var cv in complexViews)
                {
                    var result = results[cv.ViewName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<GridContent>")
                    {
                        result.AppendLine(GridContent(cv, tables, baseTab));
                    }
                    else
                    {
                        result.AppendLine(line.Replace("<ViewName>", cv.ViewName));
                    }
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + "ComplexView.xaml"), result.Value.ToString());
            }
        }

        private static string GridContent(MasterDetail view, IEnumerable<DbTable> tables, string baseTab)
        {
            var sb = new StringBuilder();

            var tab1 = baseTab + Constant.Tab;

            sb.AppendFormat("{0}<Grid.RowDefinitions>{1}", baseTab, Constant.LineEnding);
            for (int i = 0; i < view.Levels.Count; i++)
            {
                sb.AppendFormat("{0}<RowDefinition/>{1}", tab1, Constant.LineEnding);
            }
            sb.AppendFormat("{0}</Grid.RowDefinitions>{1}", baseTab, Constant.LineEnding);

            sb.AppendFormat("{0}<view:{1}View Grid.Row=\"0\" Abstraction:BaseComplexView.ViewLevel=\"0\"/>{2}", baseTab, view.Levels[0], Constant.LineEnding);

            for (int i = 1; i < view.Levels.Count; i++)
            {
                var parentTable = tables.First(p => p.TableName == view.Levels[i - 1]);
                var foreignKey = parentTable.ReferencesToThisTable.First(p => p.ReferenceTableName == view.Levels[i]).PropertyName;
                sb.AppendFormat("{0}<view:{1}View Grid.Row=\"{2}\" Abstraction:BaseComplexView.ViewLevel=\"{2}\" Abstraction:BaseComplexView.FilterProperty=\"{3}\"/>{4}", baseTab, view.Levels[i], i, foreignKey, Constant.LineEnding);
            }

            for (int i = 0; i < view.Levels.Count - 1; i++)
            {
                sb.AppendFormat("{0}<GridSplitter Grid.Row=\"{1}\" VerticalAlignment=\"Bottom\" HorizontalAlignment=\"Stretch\" Height=\"2\"/>{2}", baseTab, i, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }
    }
}
