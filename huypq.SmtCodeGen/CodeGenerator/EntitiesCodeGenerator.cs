using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class EntitiesCodeGenerator
    {
        public static void GenDbContextClass(IEnumerable<DbTable> tables, string outputPath)
        {
            var result = new StringBuilder();
            var classKeyword = " class ";
            var contextName = "";
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#DbContextTemplate.txt")))
            {
                var indexOfClass = line.IndexOf(classKeyword);
                if (indexOfClass != -1)
                {
                    var afterClassKeywordIndex = indexOfClass + classKeyword.Length;
                    var nextSpaceIndex = line.IndexOf(' ', afterClassKeywordIndex);
                    contextName = line.Substring(afterClassKeywordIndex, nextSpaceIndex - afterClassKeywordIndex);
                    result.AppendLine(line);
                    continue;
                }

                var trimmedEnd = line.TrimEnd();
                var trimmed = trimmedEnd.TrimStart();
                var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                if (trimmed == "<ModelBuilderConfix>")
                {
                    result.AppendLine(ModelBuilderConfix(tables, baseTab));
                }
                else if (trimmed == "<DbSetProperties>")
                {
                    result.AppendLine(DbSetProperties(tables, baseTab));
                }
                else
                {
                    result.AppendLine(line);
                }
            }

            FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, contextName + ".cs"), result.ToString());
        }

        public static void GenEntitiesClass(IEnumerable<DbTable> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, "#EntityTemplate.txt")))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<InitEntityCollectionNavigationProperties>")
                    {
                        result.AppendLine(InitEntityCollectionNavigationProperties(table.ReferencesToThisTable, baseTab));
                    }
                    else if (trimmed == "<EntityProperties>")
                    {
                        result.AppendLine(EntityProperties(table.Columns, baseTab));
                    }
                    else if (trimmed == "<EntityNavigationProperties>")
                    {
                        result.AppendLine(EntityNavigationProperties(table.Columns, baseTab));
                    }
                    else if (trimmed == "<EntityCollectionNavigationProperties>")
                    {
                        result.AppendLine(EntityCollectionNavigationProperties(table.ReferencesToThisTable, baseTab));
                    }
                    else
                    {
                        if (table.TableName == "SmtUser")
                        {
                            result.AppendLine(line.Replace("IEntity", "IUser").Replace("<EntityName>", table.TableName));
                        }
                        else if (table.TableName == "SmtTenant")
                        {
                            result.AppendLine(line.Replace("IEntity", "ITenant").Replace("<EntityName>", table.TableName));
                        }
                        else if (table.TableName == "SmtUserClaim")
                        {
                            result.AppendLine(line.Replace("IEntity", "IUserClaim").Replace("<EntityName>", table.TableName));
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
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + ".cs"), result.Value.ToString());
            }
        }

        private static string InitEntityCollectionNavigationProperties(IEnumerable<Reference> references, string baseTab)
        {
            if (references.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in references)
            {
                sb.AppendFormat("{0}{1}Navigation = new HashSet<{2}>();{3}", baseTab,
                    DatabaseUtils.UpperFirstLetter(item.PropertyName), DatabaseUtils.UpperFirstLetter(item.ReferenceTableName), Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string EntityProperties(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendFormat("{0}public {1} {2} {{ get; set; }}{3}", baseTab,
                    item.DataType, DatabaseUtils.UpperFirstLetter(item.ColumnName), Constant.LineEnding);
            }

            var pkName = columns.First(p => p.IsIdentity).ColumnName;
            if (pkName != "ID")
            {
                sb.AppendLine();
                sb.AppendLine(baseTab + "[System.ComponentModel.DataAnnotations.Schema.NotMapped]");
                sb.AppendFormat("{0}public int ID {{ get {{ return {1}; }} set {{ {1} = value;}} }}{2}", baseTab, pkName, Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string EntityNavigationProperties(IEnumerable<DbTableColumn> columns, string baseTab)
        {
            var foreignKeyColumns = columns.Where(p => p.IsForeignKey == true);
            if (foreignKeyColumns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in foreignKeyColumns)
            {
                sb.AppendFormat("{0}public {1} {2}Navigation {{ get; set; }}{3}", baseTab,
                    DatabaseUtils.UpperFirstLetter(item.ForeignKeyTableName), DatabaseUtils.UpperFirstLetter(item.ColumnName), Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string EntityCollectionNavigationProperties(IEnumerable<Reference> references, string baseTab)
        {
            if (references.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in references)
            {
                sb.AppendFormat("{0}public ICollection<{1}> {2}Navigation {{ get; set; }}{3}", baseTab,
                    DatabaseUtils.UpperFirstLetter(item.ReferenceTableName), DatabaseUtils.UpperFirstLetter(item.PropertyName), Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string ModelBuilderConfix(IEnumerable<DbTable> tables, string baseTab)
        {
            if (tables.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var tab1 = baseTab + Constant.Tab;
            var tab2 = tab1 + Constant.Tab;
            foreach (var table in tables)
            {
                var UpperedTableName = DatabaseUtils.UpperFirstLetter(table.TableName);
                sb.AppendFormat("{0}modelBuilder.Entity<{1}>(entity =>{2}", baseTab, UpperedTableName, Constant.LineEnding);
                sb.AppendLine(baseTab + "{");
                foreach (var index in table.Indexes)
                {
                    switch (index.IndexType)
                    {
                        case 0:
                            sb.AppendFormat("{0}entity.HasIndex(e => e.{1}){2}", tab1, index.PropertyName, Constant.LineEnding);
                            sb.AppendFormat("{0}.HasName(\"{1}\");{2}", tab2, index.IX_Name, Constant.LineEnding);
                            break;
                        case 1:
                            sb.AppendFormat("{0}entity.HasKey(e => e.{1}){2}", tab1, index.PropertyName, Constant.LineEnding);
                            sb.AppendFormat("{0}.HasName(\"{1}\");{2}", tab2, index.IX_Name, Constant.LineEnding);
                            if (table.TableName != UpperedTableName)
                            {
                                sb.AppendLine();
                                sb.AppendFormat("{0}entity.ToTable(\"{1}\");{2}", tab1, table.TableName, Constant.LineEnding);
                            }
                            break;
                        case 2:
                            sb.AppendFormat("{0}entity.HasIndex(e => e.{1}){2}", tab1, index.PropertyName, Constant.LineEnding);
                            sb.AppendFormat("{0}.HasName(\"{1}\"){2}", tab2, index.IX_Name, Constant.LineEnding);
                            sb.AppendLine(tab2 + ".IsUnique();");
                            break;
                    }
                    sb.AppendLine();
                }
                foreach (var defaultValue in table.DefaultValues)
                {
                    var value = defaultValue.Value.Substring(1, defaultValue.Value.Length - 2);
                    if (string.IsNullOrEmpty(value) == true)
                    {
                        value = "''";
                    }
                    sb.AppendFormat("{0}entity.Property(e => e.{1}).HasDefaultValueSql(\"{2}\");{3}", tab1, defaultValue.PropertyName, value, Constant.LineEnding);
                    sb.AppendLine();
                }
                foreach (var hasColumnType in table.HasColumnTypes)
                {
                    sb.AppendFormat("{0}entity.Property(e => e.{1}).HasColumnType(\"{2}\");{3}", tab1, hasColumnType.PropertyName, hasColumnType.TypeName, Constant.LineEnding);
                }
                foreach (var requiredMaxLength in table.RequiredMaxLengths)
                {
                    sb.AppendFormat("{0}entity.Property(e => e.{1})", tab1, requiredMaxLength.PropertyName);
                    if (requiredMaxLength.NeedIsRequired == true)
                    {
                        sb.AppendLine();
                        sb.Append(tab2 + ".IsRequired()");
                    }
                    if (requiredMaxLength.MaxLength > 0)
                    {
                        sb.AppendLine();
                        sb.AppendFormat("{0}.HasMaxLength({1})", tab2, requiredMaxLength.MaxLength);
                    }
                    sb.AppendLine(";");
                }
                sb.AppendLine();
                foreach (var foreignKey in table.ForeignKeys)
                {
                    sb.AppendFormat("{0}entity.HasOne(d => d.{1}Navigation){2}", tab1, foreignKey.PropertyName, Constant.LineEnding);
                    sb.AppendFormat("{0}.WithMany(p => p.{1}{2}Navigation){3}", tab2, UpperedTableName, foreignKey.PropertyName, Constant.LineEnding);
                    sb.AppendFormat("{0}.HasForeignKey(d => d.{1}){2}", tab2, foreignKey.PropertyName, Constant.LineEnding);
                    if (foreignKey.DeleteAction == 0)
                    {
                        sb.AppendLine(tab2 + ".OnDelete(DeleteBehavior.Restrict)");
                    }
                    else if (foreignKey.DeleteAction == 1)
                    {
                        sb.AppendLine(tab2 + ".OnDelete(DeleteBehavior.SetNull)");
                    }
                    else if (foreignKey.DeleteAction == 2)
                    {
                        sb.AppendLine(tab2 + ".OnDelete(DeleteBehavior.Cascade)");
                    }
                    sb.AppendFormat("{0}.HasConstraintName(\"{1}\");{2}", tab2, foreignKey.FK_Name, Constant.LineEnding);
                    sb.AppendLine();
                }
                sb.AppendLine(baseTab + "});");
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }

        private static string DbSetProperties(IEnumerable<DbTable> tables, string baseTab)
        {
            if (tables.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                sb.AppendFormat("{0}public DbSet<{1}> {1} {{ get; set; }}{2}",
                    baseTab, DatabaseUtils.UpperFirstLetter(table.TableName), Constant.LineEnding);
            }

            return sb.ToString(0, sb.Length - Constant.LineEnding.Length);
        }
    }
}
