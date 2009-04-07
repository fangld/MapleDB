using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using BEncoding;

namespace DbProxy.Server
{
    internal class NodeDecoder
    {
        #region Methods

        /// <summary>
        /// 创建存储过程的命令
        /// </summary>
        /// <param name="node">待解析的节点</param>
        /// <returns>返回存储过程的命令</returns>
        internal static DbCommand CreateProcCommand(DictNode node)
        {
            DbCommand result = new SqlCommand();
            string cmdText = ((BytesNode)node["text"]).StringText;
            result.CommandText = cmdText;
            result.CommandType = CommandType.StoredProcedure;

            if (node.ContainsKey("parms"))
            {
                ListNode parmsNode = ((ListNode)node["parms"]);

                foreach (BEncodedNode parmNode in parmsNode)
                {
                    DictNode dictNode = (DictNode)parmNode;
                    result.Parameters.Add(CreateParm(dictNode));
                }
            }
            return result;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="parmNode">待解析的节点</param>
        /// <returns>返回创建的参数</returns>
        private static DbParameter CreateParm(DictNode parmNode)
        {
            SqlParameter parm = new SqlParameter();
            parm.ParameterName = ((BytesNode)parmNode["name"]).StringText;
            parm.DbType = (DbType)((IntNode)parmNode["type"]).Value;
            if (parmNode.ContainsKey("size"))
            {
                parm.Size = ((IntNode)parmNode["size"]).Value;
            }

            if (parmNode.ContainsKey("direction"))
            {
                ParameterDirection direction = (ParameterDirection)((IntNode)parmNode["direction"]).Value;
                switch (direction)
                {
                    case ParameterDirection.Input:
                    case ParameterDirection.InputOutput:
                        parm.Direction = direction;
                        parm.Value = GetObject(parm.DbType, (BytesNode)parmNode["value"]);
                        break;
                    case ParameterDirection.Output:
                    case ParameterDirection.ReturnValue:
                        parm.Direction = direction;
                        break;
                }
            }
            else
            {
                parm.Value = GetObject(parm.DbType, (BytesNode)parmNode["value"]);
            }

            return parm;
        }

        /// <summary>
        /// 获取参数的数值
        /// </summary>
        /// <param name="type">参数的类型</param>
        /// <param name="valueNode">待解析的节点</param>
        /// <returns>返回参数的数值</returns>
        private static object GetObject(DbType type, BytesNode valueNode)
        {
            string valueStr = valueNode.StringText;
            if (valueStr == null)
            {
                return DBNull.Value;
            }
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return valueStr;
                case DbType.Byte:
                    return byte.Parse(valueStr);
                case DbType.SByte:
                    return sbyte.Parse(valueStr);
                case DbType.UInt16:
                    return ushort.Parse(valueStr);
                case DbType.Int16:
                    return short.Parse(valueStr);
                case DbType.UInt32:
                    return uint.Parse(valueStr);
                case DbType.Int32:
                    return int.Parse(valueStr);
                case DbType.UInt64:
                    return ulong.Parse(valueStr);
                case DbType.Int64:
                    return long.Parse(valueStr);
                case DbType.Single:
                    return float.Parse(valueStr);
                case DbType.Double:
                    return double.Parse(valueStr);
                case DbType.Decimal:
                    return decimal.Parse(valueStr);
                case DbType.DateTime:
                    return DateTime.Parse(valueStr);
                case DbType.Binary:
                    return valueNode.ByteArray;
                default:
                    throw new NotSupportedException(string.Format("不支持该类型数据:{0}", type));
            }
        }

        /// <summary>
        /// 创建SQL文本的命令
        /// </summary>
        /// <param name="node">待解析的节点</param>
        /// <returns>返回SQL文本的命令</returns>
        internal static DbCommand CreateTextCommand(DictNode node)
        {
            DbCommand result = new SqlCommand();
            string cmdText = ((BytesNode)node["text"]).StringText;
            result.CommandText = cmdText;
            result.CommandType = CommandType.Text;
            return result;
        }

        #endregion
    }
}
