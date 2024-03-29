namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System;
    using Newtonsoft.Json.Linq;
    using System.Data.Common;
    using System.Threading.Tasks;

    public static partial class DataReaderHelper
    {
        public static IEnumerable<T> ExecuteRead<T>
                        (
                            this IDataReader target
                            , Func<int, IDataReader, T> onReadProcessFunc
                        )
        {
            try
            {
                int i = 0;
                while (target.Read())
                {
                    if (onReadProcessFunc != null)
                    {
                        yield
                            return
                                onReadProcessFunc(++i, target);
                    }
                }
            }
            finally
            {
                //可能有错 由于 yield 延迟
                target.Close();
                target.Dispose();
            }
        }
        public static IEnumerable<TEntry> GetEnumerable<TEntry>
                (
                    this IDataReader target
                    , Func<IDataReader, TEntry> onReadProcessFunc
                    , bool skipNull = true
                )
                    where TEntry : new()
        {
            while (target.Read())
            {
                var x = onReadProcessFunc(target);
                if (!skipNull)
                {
                    yield
                        return
                                x;
                }
                else
                {
                    if (x != null)
                    {
                        yield
                           return
                                   x;
                    }
                }
            }
        }
        public static JArray GetColumnsJArray
                     (
                        this IDataReader target
                     )
        {
            var fieldsCount = target.FieldCount;
            HashSet<string> hashSet = null;
            JArray r = null;
            for (var i = 0; i < fieldsCount; i++)
            {
                if (r == null)
                {
                    r = new JArray();
                    hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                var fieldType = target.GetFieldType(i);
                var fieldName = target.GetName(i);
                if (fieldName.IsNullOrEmptyOrWhiteSpace())
                {
                    fieldName = $"Column-{i + 1}";
                }
                while (hashSet.Contains(fieldName))
                {
                    fieldName += "_1";
                }
                hashSet.Add(fieldName);
                r.Add
                    (
                        new JObject
                                (
                                    new JProperty
                                        (
                                            "ColumnName"
                                            , fieldName
                                        )
                                    ,
                                    new JProperty
                                        (
                                            "Title"
                                            , fieldName
                                        )
                                    ,
                                    new JProperty
                                        (
                                            "title"
                                            , fieldName
                                        )
                                    ,
                                    new JProperty
                                        (
                                            "data"
                                            , fieldName
                                        )
                                    ,
                                    new JProperty
                                        (
                                            "ColumnType"
                                            , fieldType
                                                .GetJTokenType()
                                                .ToString()
                                        )
                                )
                    );
            }
            return r;
        }
        public static void ReadRows
                             (
                                 this IDataReader target
                                 , JArray columns = null
                                 , Action
                                        <
                                            IDataReader
                                            , JArray        // columns
                                            , int           // row index
                                        >
                                            onReadRowProcessAction = null
                             )
        {
            var dbDataReader = (DbDataReader) target;
            ReadRows
                (
                    dbDataReader
                    , columns
                    , onReadRowProcessAction
                );
        }

        public static void ReadRows
                             (
                                 this DbDataReader target
                                 , JArray columns = null
                                 , Action
                                        <
                                            IDataReader
                                            , JArray        // columns
                                            , int           // row index
                                        >
                                            onReadRowProcessAction = null
                             )
        {
            //var fieldsCount = target.FieldCount;
            int rowIndex = 0;
            if (columns == null)
            {
                columns = target.GetColumnsJArray();
            }
            while (target.Read())
            {
                onReadRowProcessAction?
                            .Invoke
                                (
                                    target
                                    , columns
                                    , rowIndex
                                );
                rowIndex ++;
            }
        }

        public static async Task ReadRowsAsync
                             (
                                 this DbDataReader target
                                 , JArray columns = null
                                 , Func
                                        <
                                            IDataReader
                                            , JArray        // columns
                                            , int           // row index
                                            , Task
                                        >
                                            onReadRowProcessActionAsync = null
                             )
        {
            int rowIndex = 0;
            if (columns == null)
            {
                columns = target.GetColumnsJArray();
            }
            while
                (
                    await target.ReadAsync()
                )
            {
                if (onReadRowProcessActionAsync != null)
                {
                    await
                        onReadRowProcessActionAsync
                                (
                                    target
                                    , columns
                                    , rowIndex
                                );
                }
                rowIndex++;
            }
        }

        public static IEnumerable<JToken> AsRowsJTokensEnumerable
                             (
                                 this IDataReader target
                                 , JArray columns = null
                                 , Func
                                        <
                                            //int             // resultSet index
                                            IDataReader
                                            , int           // row index
                                            , int           // column index
                                            , Type          // fieldType
                                            , string        // fieldName
                                            ,
                                                (
                                                    bool needDefaultProcess
                                                    , JProperty field   //  JObject Field 对象
                                                )
                                        >
                                            onReadRowColumnProcessFunc = null
                             )
        {
            var dbDataReader = (DbDataReader) target;
            return
                AsRowsJTokensEnumerable
                    (
                        dbDataReader
                        , columns
                        , onReadRowColumnProcessFunc
                    );
        }
        public static IEnumerable<JToken> AsRowsJTokensEnumerable
                             (
                                 this DbDataReader target
                                 , JArray columns = null
                                 , Func
                                        <
                                            //int             // resultSet index
                                            IDataReader
                                            , int           // row index
                                            , int           // column index
                                            , Type          // fieldType
                                            , string        // fieldName
                                            ,
                                                (
                                                    bool needDefaultProcess
                                                    , JProperty field   //  JObject Field 对象
                                                )
                                        >
                                            onReadRowColumnProcessFunc = null
                             )
        {
            var fieldsCount = target.FieldCount;
            int rowIndex = 0;
            while (target.Read())
            {
                JObject row = new JObject();
                for (var fieldIndex = 0; fieldIndex < fieldsCount; fieldIndex++)
                {
                    var fieldType = target.GetFieldType(fieldIndex);
                    var fieldName = string.Empty;
                    if (columns != null)
                    {
                        fieldName = columns[fieldIndex]["ColumnName"].Value<string>();
                    }
                    else
                    {
                        target.GetName(fieldIndex);
                        if (fieldName.IsNullOrEmptyOrWhiteSpace())
                        {
                            fieldName = $"Column-{fieldIndex + 1}";
                        }
                    }
                    JProperty field = null;
                    var needDefaultProcess = true;
                    if (onReadRowColumnProcessFunc != null)
                    {
                        var r = onReadRowColumnProcessFunc
                                    (
                                        target
                                        , rowIndex
                                        , fieldIndex
                                        , fieldType
                                        , fieldName
                                    );
                        needDefaultProcess = r.needDefaultProcess;
                        if (r.field != null)
                        {
                            field = r.field;
                        }
                    }
                    if (needDefaultProcess)
                    {
                        field = GetFieldJProperty
                                    (
                                        target
                                        , fieldIndex
                                        , fieldType
                                        , fieldName
                                    );
                    }
                    if (field != null)
                    {
                        row.Add(field);
                    }
                }
                rowIndex ++;
                yield
                    return
                            row;
            }
        }

        public static JProperty GetFieldJProperty
                            (
                                this IDataReader target
                                , int i
                                , Type fieldType
                                , string fieldName
                            )
        {
            JValue fieldValue = null;
            if (!target.IsDBNull(i))
            {
                if
                    (
                        fieldType == typeof(bool)
                    )
                {
                    fieldValue = new JValue(target.GetBoolean(i));
                }
                else if
                    (
                        fieldType == typeof(byte)
                    )
                {
                    fieldValue = new JValue(target.GetByte(i));
                }
                else if
                    (
                        fieldType == typeof(char)
                    )
                {
                    fieldValue = new JValue(target.GetChar(i));
                }
                else if
                    (
                        fieldType == typeof(DateTime)
                    )
                {
                    fieldValue = new JValue(target.GetDateTime(i));
                }
                else if
                    (
                        fieldType == typeof(decimal)
                    )
                {
                    fieldValue = new JValue(target.GetDecimal(i));
                }
                else if
                    (
                        fieldType == typeof(double)
                    )
                {
                    fieldValue = new JValue(target.GetDouble(i));
                }
                else if
                    (
                        fieldType == typeof(float)
                    )
                {
                    fieldValue = new JValue(target.GetFloat(i));
                }
                else if
                    (
                        fieldType == typeof(Guid)
                    )
                {
                    fieldValue = new JValue(target.GetGuid(i));
                }
                else if
                    (
                        fieldType == typeof(short)
                    )
                {
                    fieldValue = new JValue(target.GetInt16(i));
                }
                else if
                    (
                        fieldType == typeof(int)
                    )
                {
                    fieldValue = new JValue(target.GetInt32(i));
                }
                else if
                    (
                        fieldType == typeof(long)
                    )
                {
                    fieldValue = new JValue(target.GetInt64(i));
                }
                else if
                    (
                        fieldType == typeof(string)
                    )
                {
                    fieldValue = new JValue(target.GetString(i));
                }
                else
                {
                    fieldValue = new JValue(target[i]);
                }
            }
            var r = new JProperty(fieldName, fieldValue);
            return r;
        }
    }
}