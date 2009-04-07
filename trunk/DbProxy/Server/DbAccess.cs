using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using BEncoding;

namespace DbProxy.Server
{
    /// <summary>
    /// 数据库访问器
    /// </summary>
    internal class DbAccess
    {
        #region Fields

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private string connectionString;

        /// <summary>
        /// 字节数组类型名称数组
        /// </summary>
        private static readonly string[] BinaryTypeName;

        #endregion

        #region Constructors

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static DbAccess()
        {
            BinaryTypeName = new string[] {"binary", "varbinary", "image"};
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接字符串</param>
        public DbAccess(string connString)
        {
            connectionString = connString;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 处理Scalar函数
        /// </summary>
        /// <param name="cmd">数据库命令</param>
        public DictNode HandleScalar(DbCommand cmd)
        {
            object obj;
            using (DbConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                cmd.Connection = conn;
                obj = cmd.ExecuteScalar();
            }

            DictNode result = new DictNode();
            result.Add("code", 3);
            BEncodedNode itemNode = CreateItemNode(obj);
            result.Add("item", itemNode);

            ListNode parmlistNode = CreateParmListNode(cmd);
            if (parmlistNode.Count != 0)
            {
                result.Add("parmlist", parmlistNode);
            }
            return result;
        }

        /// <summary>
        /// 创建Scalar返回值节点
        /// </summary>
        /// <param name="obj">Scalar返回值</param>
        /// <returns>返回Scalar返回值节点</returns>
        private BEncodedNode CreateItemNode(object obj)
        {
            string typeName = obj != null ? obj.GetType().ToString() : typeof(object).ToString();
            BytesNode valueNode = obj != null
                                      ? typeName != "System.Byte[]" ? new BytesNode(obj.ToString()) : (byte[])obj
                                      : BytesNode.NullBytesNode;
            DictNode result = new DictNode();
            result.Add("type", typeName);
            result.Add("value", valueNode);
            return result;
        }

        /// <summary>
        /// 处理NonQuery函数
        /// </summary>
        /// <param name="cmd">数据库命令</param>
        public DictNode HandleNonQuery(DbCommand cmd)
        {
            int lineNum;
            using (DbConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                cmd.Connection = conn;
                lineNum = cmd.ExecuteNonQuery();
            }
            DictNode result = new DictNode();
            result.Add("code", 1);
            result.Add("linenum", lineNum);

            ListNode parmlistNode = CreateParmListNode(cmd);
            if (parmlistNode.Count != 0)
            {
                result.Add("parmlist", parmlistNode);
            }

            return result;
        }

        /// <summary>
        /// 处理Reader函数
        /// </summary>
        /// <param name="cmd">数据库命令</param>
        public DictNode HandleReader(DbCommand cmd)
        {
            DictNode result = new DictNode();
            using (DbConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                cmd.Connection = conn;
                using (DbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {

                    ListNode typeListNode = CreateTypeListNode(rdr);
                    result.Add("typelist", typeListNode);

                    ListNode itemsNode = new ListNode();
                    while (rdr.Read())
                    {
                        ListNode itemNode = new ListNode();
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            if (rdr.IsDBNull(i))
                            {
                                itemNode.Add(BytesNode.NullBytesNode);
                            }
                            else
                            {
                                if (IsBinaryType(rdr.GetDataTypeName(i)))
                                {
                                    itemNode.Add((byte[])rdr.GetValue(i));
                                }
                                else
                                {
                                    itemNode.Add(rdr.GetValue(i).ToString());
                                }
                            }
                        }
                        itemsNode.Add(itemNode);
                    }
                    result.Add("items", itemsNode);
                }
            }
            result.Add("code", 2);

            ListNode parmlistNode = CreateParmListNode(cmd);
            if (parmlistNode.Count != 0)
            {
                result.Add("parmlist", parmlistNode);
            }
            return result;
        }

        /// <summary>
        /// 创建类型节点
        /// </summary>
        /// <param name="rdr">数据库读取器</param>
        /// <returns>返回类型节点</returns>
        private ListNode CreateTypeListNode(IDataRecord rdr)
        {
            ListNode result = new ListNode();

            for (int i = 0; i < rdr.FieldCount; i++)
            {
                DictNode columnNode = new DictNode();
                columnNode.Add("name", rdr.GetName(i));
                columnNode.Add("type", rdr.GetDataTypeName(i));
                result.Add(columnNode);
            }
            return result;
        }

        /// <summary>
        /// 判断是否为字节数组类型
        /// </summary>
        /// <param name="typeName">待判断的类型</param>
        /// <returns>如果为字节数组类型;返回true;否则返回false</returns>
        private bool IsBinaryType(string typeName)
        {
            for (int i = 0; i < BinaryTypeName.Length; i++)
            {
                if (typeName.Equals(BinaryTypeName[i]))
                    return true;
            }
            return false;
        }

        private ListNode CreateParmListNode(DbCommand cmd)
        {
            ListNode result = new ListNode();

            foreach (DbParameter para in cmd.Parameters)
            {
                switch (para.Direction)
                {
                    case ParameterDirection.InputOutput:
                    case ParameterDirection.Output:
                    case ParameterDirection.ReturnValue:
                        DictNode parmNode = new DictNode();
                        parmNode.Add("name", para.ParameterName);
                        parmNode.Add("type", para.Value.GetType().ToString());
                        if (para.Value == DBNull.Value)
                        {
                            parmNode.Add("value", BytesNode.NullBytesNode);
                        }
                        else if (para.DbType == DbType.Binary)
                        {
                            parmNode.Add("value", (byte[])para.Value);
                        }
                        else
                        {
                            parmNode.Add("value", para.Value.ToString());
                        }
                        result.Add(parmNode);
                        break;
                }
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
            return result;
        }

        #endregion
    }
}
