using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class EntitiesCodeGenerator
    {
        private const string DbContextTemplateFileName = "#DbContextTemplate.txt";
        private const string FileNameSubFix = ".cs";

        private const string EntityTemplateTemplateFileName = "#EntityTemplate.txt";

        public static void GenDbContextAndEntitiesClass(IEnumerable<TableSetting> tables, string outputPath)
        {
            var result = new StringBuilder();
            var classKeyword = " class ";
            var contextName = "";
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, DbContextTemplateFileName)))
            {
                var indexOfClass = line.IndexOf(classKeyword);
                if (indexOfClass != -1)
                {
                    var afterClassKeywordIndex = indexOfClass + classKeyword.Length;
                    var nextSpaceIndex = line.IndexOf(' ', afterClassKeywordIndex);
                    contextName = line.Substring(afterClassKeywordIndex, nextSpaceIndex - afterClassKeywordIndex);
                    result.AppendLineEx(line);
                    continue;
                }

                var trimmedEnd = line.TrimEnd();
                var trimmed = trimmedEnd.TrimStart();
                var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                if (trimmed == "<ModelBuilderConfix>")
                {
                    result.Append(ModelBuilderConfix(tables, baseTab));
                }
                else if (trimmed == "<DbSetProperties>")
                {
                    result.Append(DbSetProperties(tables, baseTab));
                }
                else
                {
                    result.AppendLineEx(line);
                }
            }

            FileUtils.DeleteAllFileEndWith(outputPath, FileNameSubFix);

            FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, contextName + FileNameSubFix), result.ToString());

            GenEntitiesClass(tables, outputPath);
        }

        private static void GenEntitiesClass(IEnumerable<TableSetting> tables, string outputPath)
        {
            var results = new Dictionary<string, StringBuilder>();
            foreach (var table in tables)
            {
                results.Add(table.TableName, new StringBuilder());
            }
            foreach (var line in System.IO.File.ReadLines(System.IO.Path.Combine(outputPath, EntityTemplateTemplateFileName)))
            {
                foreach (var table in tables)
                {
                    var result = results[table.TableName];

                    var trimmedEnd = line.TrimEnd();
                    var trimmed = trimmedEnd.TrimStart();
                    var baseTab = trimmedEnd.Substring(0, trimmedEnd.Length - trimmed.Length);
                    if (trimmed == "<InitEntityCollectionNavigationProperties>")
                    {
                        result.Append(InitEntityCollectionNavigationProperties(table.DbTable.ReferencesToThisTable, baseTab));
                    }
                    else if (trimmed == "<EntityProperties>")
                    {
                        result.Append(EntityProperties(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<EntityNavigationProperties>")
                    {
                        result.Append(EntityNavigationProperties(table.ColumnSettings, baseTab));
                    }
                    else if (trimmed == "<EntityCollectionNavigationProperties>")
                    {
                        result.Append(EntityCollectionNavigationProperties(table.DbTable.ReferencesToThisTable, baseTab));
                    }
                    else
                    {
                        if (table.TableName == "SmtUser")
                        {
                            result.AppendLineEx(line.Replace("IEntity", "IUser").Replace("<EntityName>", table.TableName));
                        }
                        else if (table.TableName == "SmtTenant")
                        {
                            result.AppendLineEx(line.Replace("IEntity", "ITenant").Replace("<EntityName>", table.TableName));
                        }
                        else if (table.TableName == "SmtUserClaim")
                        {
                            result.AppendLineEx(line.Replace("IEntity", "IUserClaim").Replace("<EntityName>", table.TableName));
                        }
                        else
                        {
                            result.AppendLineEx(line.Replace("<EntityName>", table.TableName));
                        }
                    }
                }
            }

            foreach (var result in results)
            {
                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(outputPath, result.Key + FileNameSubFix), result.Value.ToString());
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
                sb.AppendLineExWithTabAndFormat(baseTab, "{0}{1}Navigation = new HashSet<{0}>();",
                    DatabaseUtils.UpperFirstLetter(item.ReferenceTableName), DatabaseUtils.UpperFirstLetter(item.PropertyName));
            }

            return sb.ToString();
        }

        private static string EntityProperties(IEnumerable<ColumnSetting> columns, string baseTab)
        {
            if (columns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in columns)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "public {0} {1} {{ get; set; }}",
                    item.DbColumn.DataType, DatabaseUtils.UpperFirstLetter(item.GetColumnNameForCodeGen()));
            }

            return sb.ToString();
        }

        private static string EntityNavigationProperties(IEnumerable<ColumnSetting> columns, string baseTab)
        {
            var foreignKeyColumns = columns.Where(p => p.DbColumn.IsForeignKey == true);
            if (foreignKeyColumns.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var item in foreignKeyColumns)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "public {0} {1}Navigation {{ get; set; }}",
                    DatabaseUtils.UpperFirstLetter(item.DbColumn.ForeignKeyTableName), DatabaseUtils.UpperFirstLetter(item.GetColumnNameForCodeGen()));
            }

            return sb.ToString();
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
                sb.AppendLineExWithTabAndFormat(baseTab, "public ICollection<{0}> {0}{1}Navigation {{ get; set; }}",
                    DatabaseUtils.UpperFirstLetter(item.ReferenceTableName), DatabaseUtils.UpperFirstLetter(item.PropertyName));
            }

            return sb.ToString();
        }

        private static string ModelBuilderConfix(IEnumerable<TableSetting> tables, string baseTab)
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
                sb.AppendLineExWithTabAndFormat(baseTab, "modelBuilder.Entity<{0}>(entity =>", UpperedTableName);
                sb.AppendLineExWithTab(baseTab, "{");
                if (table.TableName != UpperedTableName)
                {
                    sb.AppendLineExWithTabAndFormat(tab1, "entity.ToTable(\"{0}\");", table.TableName);
                    sb.AppendLineEx();
                }
                var pkColumn = table.ColumnSettings.First(p => p.DbColumn.IsIdentity);
                if (pkColumn.DbColumn.ColumnName != "ID")
                {
                    sb.AppendLineExWithTabAndFormat(tab1, "entity.Property(p => p.ID).HasColumnName(\"{0}\");", pkColumn.DbColumn.ColumnName);
                    sb.AppendLineEx();
                }
                foreach (var index in table.DbTable.Indexes)
                {
                    switch (index.IndexType)
                    {
                        case 0:
                            sb.AppendLineExWithTabAndFormat(tab1, "entity.HasIndex(e => e.{0})", index.PropertyName);
                            sb.AppendLineExWithTabAndFormat(tab2, ".HasName(\"{1}\");{2}", index.IX_Name);
                            break;
                        case 1:
                            //sb.AppendLineExWithTabAndFormat(tab1, "entity.HasKey(e => e.{0})", index.PropertyName);
                            sb.AppendLineExWithTabAndFormat(tab1, "entity.HasKey(e => e.{0})", "ID");
                            sb.AppendLineExWithTabAndFormat(tab2, ".HasName(\"{0}\");", index.IX_Name);
                            break;
                        case 2:
                            sb.AppendLineExWithTabAndFormat(tab1, "entity.HasIndex(e => e.{0})", index.PropertyName);
                            sb.AppendLineExWithTabAndFormat(tab2, ".HasName(\"{0}\")", index.IX_Name);
                            sb.AppendLineExWithTab(tab2, ".IsUnique();");
                            break;
                    }
                    sb.AppendLineEx();
                }
                foreach (var defaultValue in table.DbTable.DefaultValues)
                {
                    var value = defaultValue.Value.Substring(1, defaultValue.Value.Length - 2);
                    if (string.IsNullOrEmpty(value) == true)
                    {
                        value = "''";
                    }
                    sb.AppendLineExWithTabAndFormat(tab1, "entity.Property(e => e.{0}).HasDefaultValueSql(\"{1}\");", defaultValue.PropertyName, value);
                    sb.AppendLineEx();
                }
                foreach (var hasColumnType in table.DbTable.HasColumnTypes)
                {
                    sb.AppendLineExWithTabAndFormat(tab1, "entity.Property(e => e.{0}).HasColumnType(\"{1}\");", hasColumnType.PropertyName, hasColumnType.TypeName);
                }
                foreach (var requiredMaxLength in table.DbTable.RequiredMaxLengths)
                {
                    sb.AppendTabAndFormat(tab1, "entity.Property(e => e.{0})", requiredMaxLength.PropertyName);
                    if (requiredMaxLength.NeedIsRequired == true)
                    {
                        sb.AppendLineEx();
                        sb.AppendTab(tab2, ".IsRequired()");
                    }
                    if (requiredMaxLength.MaxLength > 0)
                    {
                        sb.AppendLineEx();
                        sb.AppendTabAndFormat(tab2, ".HasMaxLength({0})", requiredMaxLength.MaxLength);
                    }
                    sb.AppendLineEx(";");
                }
                sb.AppendLineEx();
                foreach (var foreignKey in table.DbTable.ForeignKeys)
                {
                    sb.AppendLineExWithTabAndFormat(tab1, "entity.HasOne(d => d.{0}Navigation)", foreignKey.PropertyName);
                    sb.AppendLineExWithTabAndFormat(tab2, ".WithMany(p => p.{0}{1}Navigation)", UpperedTableName, foreignKey.PropertyName);
                    sb.AppendLineExWithTabAndFormat(tab2, ".HasForeignKey(d => d.{0})", foreignKey.PropertyName);
                    if (foreignKey.DeleteAction == 0)
                    {
                        sb.AppendLineExWithTab(tab2, ".OnDelete(DeleteBehavior.Restrict)");
                    }
                    else if (foreignKey.DeleteAction == 1)
                    {
                        sb.AppendLineExWithTab(tab2, ".OnDelete(DeleteBehavior.SetNull)");
                    }
                    else if (foreignKey.DeleteAction == 2)
                    {
                        sb.AppendLineExWithTab(tab2, ".OnDelete(DeleteBehavior.Cascade)");
                    }
                    sb.AppendLineExWithTabAndFormat(tab2, ".HasConstraintName(\"{0}\");", foreignKey.FK_Name);
                    sb.AppendLineEx();
                }
                sb.AppendLineExWithTab(baseTab, "});");
            }

            return sb.ToString();
        }

        private static string DbSetProperties(IEnumerable<TableSetting> tables, string baseTab)
        {
            if (tables.Count() == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                sb.AppendLineExWithTabAndFormat(baseTab, "public DbSet<{0}> {0} {{ get; set; }}",
                    DatabaseUtils.UpperFirstLetter(table.TableName), Constant.LineEnding);
            }

            return sb.ToString();
        }
    }
}
