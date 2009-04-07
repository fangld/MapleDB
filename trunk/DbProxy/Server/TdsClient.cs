using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net.Sockets;
using BEncoding;
using Packets;

namespace DbProxy.Server
{
    /// <summary>
    /// 底层连接的客户端
    /// </summary>
    public class TdsClient
    {
        #region Fields

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private string connectionString;

        /// <summary>
        /// 该客户端是否关闭
        /// </summary>
        private bool isClose;

        /// <summary>
        /// 客户端是否已经打开
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// 传输包处理器
        /// </summary>
        private IPacketHandler handler;

        /// <summary>
        /// 数据包监测器
        /// </summary>
        private IPacketMonitor monitor;

        /// <summary>
        /// 数据库访问器
        /// </summary>
        private DbAccess dbAccess;

        #endregion

        #region Properties

        /// <summary>
        /// 获取客户端是否已经打开
        /// </summary>
        public bool IsOpen
        {
            get { return isOpen; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="handler">数据包处理器</param>
        /// <param name="monitor">数据包监测器</param>
        /// <param name="connectionString">数据连接字符串</param>
        public TdsClient(IPacketHandler handler, IPacketMonitor monitor, string connectionString)
        {
            this.handler = handler;
            this.connectionString = connectionString;
            dbAccess = new DbAccess(connectionString);
            isOpen = false;
            this.monitor = monitor;
            monitor.Register(handler);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 交互函数
        /// </summary>
        /// <param name="stateInfo"></param>
        internal void Exchange(object stateInfo)
        {
            try
            {
                isClose = false;
                do
                {
                    byte[] rcvMsg = handler.ReceiveSealedPacket();
                    DictNode responseNode = HandlePacket(rcvMsg);
                    byte[] sndMsg = BEncode.ByteArrayEncode(responseNode);
                    handler.SendSealedPacket(sndMsg);
                    if (isClose)
                    {
                        return;
                    }
                } while (true);
            }

            catch(TdsException te)
            {
                Console.WriteLine("Tds Exception:{0}", te.Message);
            }

            catch (PacketException pe)
            {
                Console.WriteLine("Packet Exception:{0}", pe.Message);
            }

            catch (IOException ioe)
            {
                Console.WriteLine("IO Exception:{0}", ioe.Message);
            }

            catch (SocketException se)
            {
                Console.WriteLine("Socket Exception:{0}", se.Message);
            }

            catch (BEncodingException bee)
            {
                Console.WriteLine("BEncoding Exception:{0}", bee.Message);
            }

            finally
            {
                monitor.Unregister(handler);
                handler.Close();
            }
        }

        /// <summary>
        /// 处理接收的数据传输包
        /// </summary>
        /// <param name="bytes">接收的数据传输包</param>
        /// <returns>返回发送的数据传输包</returns>
        internal DictNode HandlePacket(byte[] bytes)
        {
            DictNode node = (DictNode)BEncode.Decode(bytes);

            //处理关闭信息情况
            long action = ((IntNode)node["action"]).Value;
            if (action == 2)
            {
                isClose = true;
                return CreateCloseNode();
            }

            CommandType type = (CommandType) ((IntNode) node["type"]).Value;


            //建立对应的command
            DbCommand cmd;
            switch (type)
            {
                case CommandType.Text:
                    cmd = NodeDecoder.CreateTextCommand(node);
                    break;
                case CommandType.StoredProcedure:
                    cmd = NodeDecoder.CreateProcCommand(node);
                    break;
                default:
                    throw new ArgumentException(string.Format("不包含该SQL语句类型:{0}!", type));
            }

            //进行数据库访问
            DictNode result;
            try
            {
                switch (action)
                {
                    case 3:
                        result = dbAccess.HandleNonQuery(cmd);
                        break;
                    case 4:
                        result = dbAccess.HandleReader(cmd);
                        break;
                    case 5:
                        result = dbAccess.HandleScalar(cmd);
                        break;
                    default:
                        throw new ArgumentException(string.Format("不能够执行该数据库操作代码:{0}!", action));
                }
                cmd.Dispose();
            }
            catch (Exception e)
            {
                result = CreateExceptionNode(e);
            }

            return result;
        }

        /// <summary>
        /// 创建异常数据
        /// </summary>
        /// <param name="e">异常数据</param>
        /// <returns>返回异常数据节点</returns>
        static DictNode CreateExceptionNode(Exception e)
        {
            DictNode result = new DictNode();
            result.Add("code", 4);
            result.Add("message", string.Format("{0}: {1}", e.GetType(), e.Message));
            return result;
        }

        /// <summary>
        /// 创建关闭回应包
        /// </summary>
        /// <returns></returns>
        static DictNode CreateCloseNode()
        {
            DictNode result = new DictNode();
            result.Add("code", 1);
            return result;
        }

        #endregion
    }
}
