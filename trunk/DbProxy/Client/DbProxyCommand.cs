using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Text;
using System.Threading;
using BEncoding;
using DbProxy.Common;

namespace DbProxy.Client
{
    public class DbProxyCommand : DbCommand, ICloneable
    {
        #region Fields

        private bool disposed;
        private string commandText;
        private int commandTimeout;
        private CommandType commandType;
        private DbTransaction dbTransaction;
        private bool designTimeVisible;
        private DbProxyConnection connection;
        private UpdateRowSource updatedRowSource;
        private DbProxyParameterCollection parameters;
        private CommandBehavior behavior;

        private const int ProcNum = (int)CommandType.StoredProcedure;
        private const int TextNum = (int)CommandType.Text;

        private const int InputNum = (int)ParameterDirection.Input;
        private const int OutputNum = (int)ParameterDirection.Output;
        private const int InputOutputNum = (int)ParameterDirection.InputOutput;
        private const int ReturnValueNum = (int)ParameterDirection.ReturnValue;

        #endregion

        #region Properties

        public override string CommandText
        {
            get { return commandText == null ? string.Empty : commandText; }
            set { commandText = value; }
        }

        public override int CommandTimeout
        {
            get { return commandTimeout; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The property value assigned is less than 0.");
                commandTimeout = value;
            }
        }

        public override CommandType CommandType
        {
            get { return commandType; }
            set
            {
                if (value == CommandType.TableDirect)
                    throw new ArgumentOutOfRangeException("CommandType.TableDirect is not supported " +
                        "by the Db Proxy Data Provider.");

                ExceptionHelper.CheckEnumValue(typeof(CommandType), value);
                commandType = value;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return dbTransaction; }
            set { dbTransaction = value; }
        }

        public override bool DesignTimeVisible
        {
            get { return designTimeVisible; }
            set { designTimeVisible = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return updatedRowSource; }
            set
            {
                ExceptionHelper.CheckEnumValue(typeof(UpdateRowSource), value);
                updatedRowSource = value;
            }
        }

        internal CommandBehavior CommandBehavior
        {
            get { return behavior; }
        }

        public new DbProxyConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return connection; }
            set
            {
                if (!(value == null || value is DbProxyConnection))
                    throw new InvalidCastException("The value was not a valid DbProxyConnection.");
                Connection = (DbProxyConnection)value; ;
            }
        }

        #endregion

        #region Constructors

        public DbProxyCommand()
            : this(string.Empty, null)
        {

        }

        public DbProxyCommand(string cmdText)
            : this(cmdText, null)
        {
        }

        public DbProxyCommand(string cmdText, DbProxyConnection connection)
        {
            commandText = cmdText;
            this.connection = connection;
            commandType = CommandType.Text;
            updatedRowSource = UpdateRowSource.Both;
            parameters = new DbProxyParameterCollection(this);
            behavior = CommandBehavior.Default;
        }

        #endregion

        #region Methods

        internal void CloseDataReader(bool moreResults)
        {
            Connection.DataReader = null;

            if ((behavior & CommandBehavior.CloseConnection) != 0)
                Connection.Close();

            // Reset the behavior
            behavior = CommandBehavior.Default;
        }

        public override void Cancel()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override DbParameter CreateDbParameter()
        {
            return new DbProxyParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader();
        }

        public override int ExecuteNonQuery()
        {
            DictNode contentNode = new DictNode();
            contentNode.Add("action", 3);
            byte[] sendMsg = BuildMessage(contentNode);
            connection.SendPacket(sendMsg);
            byte[] rcvMsg = connection.ReceivePacket();
            return DecodeNonQuery(rcvMsg);
        }

        public override object ExecuteScalar()
        {
            DictNode contentNode = new DictNode();
            contentNode.Add("action", 5);
            byte[] sendMsg = BuildMessage(contentNode);
            connection.SendPacket(sendMsg);
            byte[] rcvMsg = connection.ReceivePacket();
            return DecodeScalar(rcvMsg);
        }

        public override void Prepare()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public new DbProxyDataReader ExecuteReader()
        {
            DbProxyDataReader result = new DbProxyDataReader(this);

            DictNode contentNode = new DictNode();
            contentNode.Add("action", 4);
            byte[] sendMsg = BuildMessage(contentNode);
            connection.SendPacket(sendMsg);
            byte[] rcvMsg = connection.ReceivePacket();
            DecodeReader(rcvMsg, result.SchemaTable);
            return result;
        }

        static Dictionary<string, Type> types = new Dictionary<string, Type>();

        static DbProxyCommand()
        {
            types.Add("varchar", typeof(string));
            types.Add("char", typeof(string));

            types.Add("tinyint", typeof(byte));
            types.Add("smallint", typeof(short));
            types.Add("int", typeof(int));
            types.Add("bigint", typeof(long));

            types.Add("datetime", typeof(DateTime));
            types.Add("binary", typeof(byte[]));
            types.Add("varbinary", typeof(byte[]));
            types.Add("image", typeof(byte[]));

            types.Add("real", typeof(float));
            types.Add("float", typeof(double));
            types.Add("decimal", typeof(decimal));
        }

        private void DecodeReader(byte[] content, DataTable table)
        {
            DictNode rootNode = (DictNode)BEncode.Decode(content);
            IntNode codeNode = (IntNode)rootNode["code"];
            switch (codeNode.Value)
            {
                case 1:
                case 3:
                    throw new DbProxyException("数据集收到的数据包出错!");
                case 4:
                    BytesNode messageNode = (BytesNode)rootNode["message"];
                    throw new DbProxyException(messageNode.StringText);
                case 5:
                    throw new DbProxyException("校验出错!");
            }

            //解码类型表
            ListNode typelistNode = (ListNode)rootNode["typelist"];
            table.BeginInit();
            table.BeginLoadData();
            foreach (BEncodedNode node in typelistNode)
            {
                BytesNode nameNode = (BytesNode)(((DictNode)node)["name"]);
                BytesNode typeNode = (BytesNode)(((DictNode)node)["type"]);
                DataColumn column = new DataColumn(nameNode.StringText);
                column.DataType = types[typeNode.StringText];
                table.Columns.Add(column);
            }

            //解码数据集
            ListNode itemsNode = (ListNode)rootNode["items"];
            for (int i = 0; i < itemsNode.Count; i++)
            {
                ListNode itemNode = (ListNode)itemsNode[i];
                DataRow row = table.NewRow();
                for (int j = 0; j < itemNode.Count; j++)
                {
                    Type type = table.Columns[j].DataType;
                    string valueStr = ((BytesNode)itemNode[j]).StringText;
                    if (valueStr == null)
                    {
                        row[j] = DBNull.Value;
                    }

                    else if (type == typeof(string))
                    {
                        row[j] = valueStr;
                    }

                    else if (type == typeof(byte))
                    {
                        row[j] = byte.Parse(valueStr);
                    }

                    else if (type == typeof(sbyte))
                    {
                        row[j] = sbyte.Parse(valueStr);
                    }

                    else if (type == typeof(ushort))
                    {
                        row[j] = ushort.Parse(valueStr);
                    }

                    else if (type == typeof(short))
                    {
                        row[j] = short.Parse(valueStr);
                    }

                    else if (type == typeof(uint))
                    {
                        row[j] = uint.Parse(valueStr);
                    }

                    else if (type == typeof(int))
                    {
                        row[j] = int.Parse(valueStr);
                    }

                    else if (type == typeof(ulong))
                    {
                        row[j] = ulong.Parse(valueStr);
                    }

                    else if (type == typeof(long))
                    {
                        row[j] = long.Parse(valueStr);
                    }

                    else if (type == typeof(byte[]))
                    {
                        row[j] = ((BytesNode)itemNode[j]).ByteArray;
                    }

                    else if (type == typeof(DateTime))
                    {
                        row[j] = DateTime.Parse(valueStr);
                    }

                    else if (type == typeof(float))
                    {
                        row[j] = float.Parse(valueStr);
                    }

                    else if (type == typeof(double))
                    {
                        row[j] = double.Parse(valueStr);
                    }

                    else if (type == typeof(decimal))
                    {
                        row[j] = decimal.Parse(valueStr);
                    }
                }
                table.Rows.Add(row);
            }
            if (rootNode.ContainsKey("parmlist"))
            {
                HandleParmsNode((rootNode)["parmlist"]);
            }
            table.EndLoadData();
            table.EndInit();
        }

        private int DecodeNonQuery(byte[] content)
        {
            DictNode rootNode = (DictNode)BEncode.Decode(content);
            IntNode codeNode = (IntNode)rootNode["code"];

            switch (codeNode.Value)
            {
                case 1:
                    IntNode lineNode = (IntNode)rootNode["linenum"];
                    if (rootNode.ContainsKey("parmlist"))
                    {
                        HandleParmsNode((rootNode)["parmlist"]);
                    }
                    return lineNode.Value;
                case 2:
                case 3:
                    throw new DbProxyException("数据集收到的数据包出错!");
                case 4:
                    BytesNode messageNode = (BytesNode)rootNode["message"];
                    throw new DbProxyException(messageNode.StringText);
                case 5:
                    throw new DbProxyException("校验出错!");
                default:
                    throw new DbProxyException(string.Format("无效消息代码:{0}!", codeNode.Value));
            }
        }

        private object DecodeScalar(byte[] content)
        {
            DictNode rootNode = (DictNode)BEncode.Decode(content);
            IntNode codeNode = (IntNode)rootNode["code"];

            switch (codeNode.Value)
            {
                case 1:
                case 2:
                    throw new DbProxyException("数据集收到的数据包出错!");
                case 3:
                    DictNode itemNode = (DictNode)rootNode["item"];
                    string typeName = ((BytesNode)itemNode["type"]).StringText;
                    string value = ((BytesNode)itemNode["value"]).StringText;
                    if (rootNode.ContainsKey("parmlist"))
                    {
                        HandleParmsNode((rootNode)["parmlist"]);
                    }
                    return NodeEncoder.GetObject(typeName, value);
                case 4:
                    BytesNode messageNode = (BytesNode)rootNode["message"];
                    throw new DbProxyException(messageNode.StringText);
                case 5:
                    throw new DbProxyException("校验出错!");
                default:
                    throw new DbProxyException(string.Format("无效消息代码:{0}!", codeNode.Value));
            }
        }

        private void HandleParmsNode(BEncodedNode parmlistNode)
        {
            ListNode parmsNode = (ListNode)parmlistNode;
            foreach (BEncodedNode node in parmsNode)
            {
                DictNode parmNode = (DictNode)node;
                string name = ((BytesNode)parmNode["name"]).StringText;
                string typeName = ((BytesNode)parmNode["type"]).StringText;
                parameters[name].Value = NodeEncoder.GetObject(typeName, (BytesNode)parmNode["value"]);
            }
        }

        private byte[] BuildMessage(DictNode contentNode)
        {
            switch (commandType)
            {
                case CommandType.Text:
                    contentNode.Add("type", TextNum);
                    return BuildTextMessage(contentNode);
                case CommandType.StoredProcedure:
                    contentNode.Add("type", ProcNum);
                    return BuildProcMessage(contentNode);
            }
            throw new DbProxyException("建立传输数据出错!");
        }

        /// <summary>
        /// 生成存储过程发送的字节流
        /// </summary>
        /// <returns>返回发送的字节流</returns>
        private byte[] BuildProcMessage(DictNode contentNode)
        {
            contentNode.Add("text", commandText);

            //建立参数列表
            if (parameters.Count != 0)
            {
                ListNode parmsNode = new ListNode();
                foreach (DbProxyParameter para in parameters)
                {
                    DictNode parmNode = new DictNode();
                    parmNode.Add("name", para.ParameterName);
                    parmNode.Add("type", (int)para.DbType);

                    switch (para.Direction)
                    {
                        case ParameterDirection.Input:
                            parmNode.Add("value", NodeEncoder.GetValueNode(para));
                            break;
                        case ParameterDirection.Output:
                            parmNode.Add("direction", OutputNum);
                            break;
                        case ParameterDirection.InputOutput:
                            parmNode.Add("direction", InputOutputNum);
                            parmNode.Add("value", NodeEncoder.GetValueNode(para));
                            break;
                        case ParameterDirection.ReturnValue:
                            parmNode.Add("direction", ReturnValueNum);
                            break;
                    }


                    if (para.Size != 0)
                    {
                        parmNode.Add("size", para.Size);
                    }

                    parmsNode.Add(parmNode);
                }

                contentNode.Add("parms", parmsNode);
            }

            return BEncode.ByteArrayEncode(contentNode);
        }

        /// <summary>
        /// 生成SQL文本发送的字节流
        /// </summary>
        /// <returns>返回发送的字节流</returns>
        private byte[] BuildTextMessage(DictNode contentNode)
        {
            contentNode.Add("text", commandText);
            return BEncode.ByteArrayEncode(contentNode);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                parameters.Clear();
            }
            base.Dispose(disposing);
            disposed = true;
        }

        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            return new DbProxyCommand(commandText, connection);
        }

        #endregion
    }
}