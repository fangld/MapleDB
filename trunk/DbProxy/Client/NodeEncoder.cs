using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using BEncoding;

namespace DbProxy.Client
{
    internal static class NodeEncoder
    {
        internal static object GetObject(string typeName, BytesNode valueNode)
        {
            if (typeName == "System.Byte[]")
            {
                return valueNode.ByteArray;
            }

            if (typeName == "System.DBNull")
            {
                return DBNull.Value;
            }

            string value = valueNode.StringText;

            switch (typeName)
            {
                case "System.String":
                    return value;
                case "System.Byte":
                    return byte.Parse(value);
                case "System.SByte":
                    return sbyte.Parse(value);
                case "System.UInt16":
                    return ushort.Parse(value);
                case "System.Int16":
                    return short.Parse(value);
                case "System.UInt32":
                    return uint.Parse(value);
                case "System.Int32":
                    return int.Parse(value);
                case "System.UInt64":
                    return ulong.Parse(value);
                case "System.Int64":
                    return long.Parse(value);
                case "System.DateTime":
                    return DateTime.Parse(value);
                case "System.Single":
                    return float.Parse(value);
                case "System.Double":
                    return double.Parse(value);
                case "System.Decimal":
                    return decimal.Parse(value);
                case "System.Object":
                    return value;
                default:
                    throw new NotSupportedException(string.Format("不支持该类型:{0}的数据", typeName));
            }
        }

        internal static BEncodedNode GetValueNode(DbParameter para)
        {
            //如果参数值为空，返回空字节串节点
            if (para.Value == DBNull.Value)
                return BytesNode.NullBytesNode;

            switch (para.DbType)
            {
                case DbType.Byte:
                    return para.Value is Enum ? ((byte)para.Value).ToString() : para.Value.ToString();
                case DbType.SByte:
                    return para.Value is Enum ? ((sbyte)para.Value).ToString() : para.Value.ToString();
                case DbType.UInt16:
                    return para.Value is Enum ? ((ushort)para.Value).ToString() : para.Value.ToString();
                case DbType.Int16:
                    return para.Value is Enum ? ((short)para.Value).ToString() : para.Value.ToString();
                case DbType.UInt32:
                    return para.Value is Enum ? ((uint)para.Value).ToString() : para.Value.ToString();
                case DbType.Int32:
                    return para.Value is Enum ? ((int)para.Value).ToString() : para.Value.ToString();
                case DbType.UInt64:
                    return para.Value is Enum ? ((ulong)para.Value).ToString() : para.Value.ToString();
                case DbType.Int64:
                    return para.Value is Enum ? ((long)para.Value).ToString() : para.Value.ToString();
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Single:
                case DbType.Double:
                case DbType.Decimal:
                case DbType.DateTime:
                    return para.Value.ToString();
                case DbType.Binary:
                    return (byte[])para.Value;
                default:
                    throw new NotSupportedException(string.Format("不支持该类型{0}!", para.DbType));
            }
        }
    }
}
