using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class DtosCodeGenerator
    {
        public static void GenDtosClass(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#DtoTemplate.txt")))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<PrivateFields>")
                    {
                        result.AppendLine(PrivateFields(table.Columns, baseTab));
                    }
                    else if (trimmed == "<PublicProperties>")
                    {
                        result.AppendLine(PublicProperties(table.Columns, baseTab));
                    }
                    else if (trimmed == "<SetCurrentValueAsOriginalValue>")
                    {
                        result.AppendLine(SetCurrentValueAsOriginalValue(table.Columns, baseTab));
                    }
                    else if (trimmed == "<Update>")
                    {
                        result.AppendLine(Update(table.Columns, baseTab));
                    }
                    else if (trimmed == "<HasChange>")
                    {
                        result.AppendLine(HasChange(table.Columns, baseTab));
                    }
                    else if (trimmed == "<ReferenceDataSource>")
                    {
                        result.AppendLine(ReferenceDataSource(table.Columns, baseTab));
                    }
                    else
                    {
                        if (table.TableName == "SmtUser")
                        {
                            result.AppendLine(line.Replace("IDto", "IUserDto").Replace("<EntityName>", table.TableName));
                        }
                        else if (table.TableName == "SmtUserClaim")
                        {
                            result.AppendLine(line.Replace("IDto", "IUserClaimDto").Replace("<EntityName>", table.TableName));
                        }
                        else
                        {
                            result.AppendLine(line.Replace("<EntityName>", table.TableName));
                        }
                    }
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + "Dto.cs"), result.Value.ToString());
            }
        }

        private static string PrivateFields(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendFormat("{0}{1} o{2};{3}", baseTab, item.DataType, item.ColumnName, Constant.LineEnding);
            }
            sb.AppendLine();
            foreach (var item in columns)
            {
                sb.AppendFormat("{0}{1} _{2};{3}", baseTab, item.DataType, item.ColumnName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string PublicProperties(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var i = 1;
            foreach (var item in columns)
            {
                sb.AppendFormat("{0}[ProtoBuf.ProtoMember({1})]{2}", baseTab, i, Constant.LineEnding);
                sb.AppendLine(string.Format("{0}public {1} {2} {{ get {{ return _{2}; }} set {{ _{2} = value; OnPropertyChanged(); }} }}",
                    baseTab, item.DataType, item.ColumnName));
                i++;
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string SetCurrentValueAsOriginalValue(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendFormat("{0}o{1} = {1};{2}", baseTab, item.ColumnName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string Update(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendFormat("{0}{1} = dto.{1};{2}", baseTab, item.ColumnName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string HasChange(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendFormat("{0}return{1}", baseTab, Constant.LineEnding);
            foreach (var item in columns)
            {
                sb.AppendFormat("{0}(o{1} != {1})||{2}", baseTab, item.ColumnName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length - "||".Length) + ";";
        }

        private static string ReferenceDataSource(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var foreignKeys = columns.Where(p => p.IsForeignKey);
            foreach (var item in foreignKeys)
            {
                sb.AppendFormat("{0}object _{1}DataSource;{2}", baseTab, item.ColumnName, Constant.LineEnding);
            }
            sb.AppendLine();
            foreach (var item in foreignKeys)
            {
                sb.AppendFormat("{0}[Newtonsoft.Json.JsonIgnore]{1}", baseTab, Constant.LineEnding);
                sb.AppendFormat("{0}public object {1}DataSource {{ get {{ return _{1}DataSource; }} set {{ _{1}DataSource = value; OnPropertyChanged(); }} }}{2}",
                    baseTab, item.ColumnName, Constant.LineEnding);
            }

            var pkName = columns.First(p => p.IsIdentity).ColumnName;
            if (pkName != "ID")
            {
                sb.AppendLine();
                sb.AppendFormat("{0}[Newtonsoft.Json.JsonIgnore]{1}", baseTab, Constant.LineEnding);
                sb.AppendFormat("{0}public int ID {{ get {{ return {1}; }} set {{ {1} = value;}} }}{2}", baseTab, pkName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }
    }
}
