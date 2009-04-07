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
    /// ���ݿ������
    /// </summary>
    internal class DbAccess
    {
        #region Fields

        /// <summary>
        /// ���ݿ������ַ���
        /// </summary>
        private string connectionString;

        /// <summary>
        /// �ֽ�����������������
        /// </summary>
        private static readonly string[] BinaryTypeName;

        #endregion

        #region Constructors

        /// <summary>
        /// ��̬���캯��
        /// </summary>
        static DbAccess()
        {
            BinaryTypeName = new string[] {"binary", "varbinary", "image"};
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="connString">���ݿ������ַ���</param>
        public DbAccess(string connString)
        {
            connectionString = connString;
        }

        #endregion

        #region Methods

        /// <summary>
        /// ����Scalar����
        /// </summary>
        /// <param name="cmd">���ݿ�����</param>
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
        /// ����Scalar����ֵ�ڵ�
        /// </summary>
        /// <param name="obj">Scalar����ֵ</param>
        /// <returns>����Scalar����ֵ�ڵ�</returns>
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
        /// ����NonQuery����
        /// </summary>
        /// <param name="cmd">���ݿ�����</param>
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
        /// ����Reader����
        /// </summary>
        /// <param name="cmd">���ݿ�����</param>
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
        /// �������ͽڵ�
        /// </summary>
        /// <param name="rdr">���ݿ��ȡ��</param>
        /// <returns>�������ͽڵ�</returns>
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
        /// �ж��Ƿ�Ϊ�ֽ���������
        /// </summary>
        /// <param name="typeName">���жϵ�����</param>
        /// <returns>���Ϊ�ֽ���������;����true;���򷵻�false</returns>
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
