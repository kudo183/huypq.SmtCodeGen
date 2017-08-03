using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class DatabaseUtils
    {
        private static string serverName = ".";
        private static Dictionary<string, string> _typeMapping = new Dictionary<string, string>()
        {
            {"bigint", "long" },
            {"binary", "byte[]" },
            {"bit", "bool" },
            {"char", "string" },
            {"date", "System.DateTime" },
            {"datetime", "System.DateTime" },
            {"datetime2", "System.DateTime" },
            {"datetimeoffset", "System.DateTimeOffset" },
            {"decimal", "decimal" },
            {"float", "double" },
            //{"geography", ""},
            //{"geometry", ""},
            //{"hierarchyid", ""},
            {"image", "byte[]"},
            {"int", "int" },
            {"money", "decimal"},
            {"nchar", "string" },
            {"ntext", "string" },
            {"numeric", "decimal"},
            {"nvarchar", "string" },
            {"real", "System.Single"},
            //{"rowversion", "byte[]"},
            {"smalldatetime", "System.DateTime" },
            {"smallint", "System.Int16" },
            {"smallmoney", "decimal"},
            //{"sql_variant", ""},
            {"text", "string" },
            {"time","System.TimeSpan" },
            {"timestamp", "byte[]"},
            {"tinyint", "byte" },
            {"uniqueidentifier", "System.Guid" },
            {"varbinary", "byte[]" },
            {"varchar", "string" },
            //{"xml", "System.Data.SqlTypes.SqlXml" }
        };

        public static List<DbTable> FromDB(string dbName)
        {
            var tables = new List<DbTable>();
             var dicReferenceTable = new Dictionary<string, List<Reference>>();

            var server = new Microsoft.SqlServer.Management.Smo.Server(serverName);
            var db = new Microsoft.SqlServer.Management.Smo.Database(server, dbName);
            db.Refresh();
            foreach (Microsoft.SqlServer.Management.Smo.Table table in db.Tables)
            {
                table.Refresh();

                var dic = new Dictionary<string, string>();

                foreach (Microsoft.SqlServer.Management.Smo.ForeignKey item in table.ForeignKeys)
                {
                    dic.Add(item.Columns[0].Name, item.ReferencedTable);

                    if (dicReferenceTable.ContainsKey(item.ReferencedTable) == false)
                    {
                        dicReferenceTable.Add(item.ReferencedTable, new List<Reference>());
                    }

                    dicReferenceTable[item.ReferencedTable].Add(new Reference()
                    {
                        PropertyName = item.Columns[0].Name,
                        ReferenceTableName = table.Name
                    });
                }

                var indexes = GetIndexes(table);
                var foreignKeys = GetForeignKey(table);

                var columns = new List<DbTableColumn>();
                var requiredMaxLengths = new List<RequiredMaxLength>();
                var defaultValues = new List<DefaultValue>();
                var hasColumnTypes = new List<HasColumnType>();

                foreach (Microsoft.SqlServer.Management.Smo.Column item in table.Columns)
                {
                    var sqlDataType = item.DataType.Name;
                    var dotnetType = _typeMapping[sqlDataType];
                    if (item.Nullable == true && dotnetType != "string")
                        dotnetType = dotnetType + "?";

                    var entityProperty = new DbTableColumn()
                    {
                        DataType = dotnetType,
                        ColumnName = item.Name,
                        IsForeignKey = item.IsForeignKey,
                        IsPrimaryKey = item.InPrimaryKey,
                        IsIdentity = item.Identity
                    };
                    if (item.IsForeignKey == true)
                    {
                        entityProperty.ForeignKeyTableName = dic[item.Name];
                    }
                    columns.Add(entityProperty);

                    //hascolumntype
                    if (sqlDataType == "datetime2"
                        || sqlDataType == "datetimeoffset"
                        || sqlDataType == "time")
                    {
                        hasColumnTypes.Add(new HasColumnType()
                        {
                            PropertyName = item.Name,
                            TypeName = sqlDataType + "(" + item.DataType.NumericScale + ")"
                        });
                    }
                    else if (sqlDataType == "decimal" || sqlDataType == "numeric")
                    {
                        hasColumnTypes.Add(new HasColumnType()
                        {
                            PropertyName = item.Name,
                            TypeName = sqlDataType + "(" + item.DataType.NumericPrecision + "," + item.DataType.NumericScale + ")"
                        });
                    }

                    //requiredmaxlength
                    var requiredMaxLength = new RequiredMaxLength() { PropertyName = item.Name, MaxLength = -1 };
                    if (item.Nullable == false && dotnetType == "string")
                    {
                        requiredMaxLength.NeedIsRequired = true;
                    }
                    if (dotnetType == "string" || dotnetType == "byte[]")
                    {
                        requiredMaxLength.MaxLength = item.DataType.MaximumLength;
                    }
                    if (requiredMaxLength.NeedIsRequired == true || requiredMaxLength.MaxLength > 0)
                    {
                        requiredMaxLengths.Add(requiredMaxLength);
                    }

                    //defaultvalue
                    if (item.DefaultConstraint != null)
                    {
                        defaultValues.Add(new DefaultValue()
                        {
                            PropertyName = item.Name,
                            Value = item.DefaultConstraint.Text
                        });
                    }
                }

                var t = new DbTable()
                {
                    TableName = table.Name,
                    Columns = new ObservableCollection<DbTableColumn>(columns),
                    ForeignKeys = new ObservableCollection<ForeignKey>(foreignKeys),
                    Indexes = new ObservableCollection<Index>(indexes),
                    RequiredMaxLengths = new ObservableCollection<RequiredMaxLength>(requiredMaxLengths),
                    DefaultValues = new ObservableCollection<DefaultValue>(defaultValues),
                    HasColumnTypes = new ObservableCollection<HasColumnType>(hasColumnTypes)
                };
                if (table.Name == "SmtDeletedItem"
                    || table.Name == "SmtTable"
                    || table.Name == "SmtTenant"
                    || table.Name == "SmtUser"
                    || table.Name == "SmtUserClaim")
                {
                    t.IsSelected = false;
                }
                else
                {
                    t.IsSelected = true;
                }
                tables.Add(t);
            }

            foreach (var table in tables)
            {
                List<Reference> reference;
                if (dicReferenceTable.TryGetValue(table.TableName, out reference) == true)
                {
                    table.ReferencesToThisTable = new ObservableCollection<Reference>(reference);
                }
                else
                {
                    table.ReferencesToThisTable = new ObservableCollection<Reference>();
                }
            }

            CalculateReferenceLevel(tables);

            return tables;
        }

        public static string UpperFirstLetter(string text)
        {
            return text;
            //if (string.IsNullOrEmpty(text) == true)
            //    return text;

            //return text[0].ToString().ToUpper() + text.Substring(1);
        }

        private static List<Index> GetIndexes(Microsoft.SqlServer.Management.Smo.Table table)
        {
            var indexes = new List<Index>();
            foreach (Microsoft.SqlServer.Management.Smo.Index item in table.Indexes)
            {
                if (item.IndexKeyType == Microsoft.SqlServer.Management.Smo.IndexKeyType.None)
                {
                    if (item.IsUnique == true)
                    {
                        indexes.Add(new Index()
                        {
                            PropertyName = item.IndexedColumns[0].Name,
                            IX_Name = item.Name,
                            IndexType = 2
                        });
                    }
                    else
                    {
                        indexes.Add(new Index()
                        {
                            PropertyName = item.IndexedColumns[0].Name,
                            IX_Name = item.Name,
                            IndexType = 0
                        });
                    }
                }
                else if (item.IndexKeyType == Microsoft.SqlServer.Management.Smo.IndexKeyType.DriPrimaryKey)
                {
                    indexes.Insert(0, new Index()
                    {
                        PropertyName = item.IndexedColumns[0].Name,
                        IX_Name = item.Name,
                        IndexType = 1
                    });
                }
            }

            return indexes;
        }

        private static List<ForeignKey> GetForeignKey(Microsoft.SqlServer.Management.Smo.Table table)
        {
            var foreignKeys = new List<ForeignKey>();

            foreach (Microsoft.SqlServer.Management.Smo.ForeignKey item in table.ForeignKeys)
            {
                int action = -1;
                switch (item.DeleteAction)
                {
                    case Microsoft.SqlServer.Management.Smo.ForeignKeyAction.NoAction:
                        action = 0;
                        break;
                    case Microsoft.SqlServer.Management.Smo.ForeignKeyAction.SetNull:
                        action = 1;
                        break;
                    case Microsoft.SqlServer.Management.Smo.ForeignKeyAction.Cascade:
                        action = 2;
                        break;
                    case Microsoft.SqlServer.Management.Smo.ForeignKeyAction.SetDefault:
                        action = 3;
                        break;
                }
                foreignKeys.Add(new ForeignKey()
                {
                    PropertyName = item.Columns[0].Name,
                    ForeignKeyTableName = item.ReferencedTable,
                    FK_Name = item.Name,
                    DeleteAction = action
                });
            }

            return foreignKeys;
        }

        private static void CalculateReferenceLevel(List<DbTable> tables)
        {
            var temp = new List<DbTable>(tables);

            int level = 0;
            var previousLevelTables = new List<DbTable>();
            var removedTables = new List<DbTable>();
            while (temp.Count > 0)
            {
                for (int i = 0; i < temp.Count; i++)
                {
                    bool isOnlyReferenceToPreviousLevel = true;

                    foreach (var f in temp[i].ForeignKeys)
                    {
                        if (previousLevelTables.Any(p => p.TableName == f.ForeignKeyTableName) == false)
                        {
                            isOnlyReferenceToPreviousLevel = false;
                        }
                    }

                    if (isOnlyReferenceToPreviousLevel == true)
                    {
                        temp[i].ReferenceLevel = level;
                        removedTables.Add(temp[i]);
                        temp.RemoveAt(i);
                        i--;
                    }
                }
                previousLevelTables.AddRange(removedTables);
                removedTables.Clear();
                level++;
            }
        }
    }
}
