using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BEncoding.Properties;

namespace BEncoding
{
    /// <summary>
    /// Handler列表类
    /// </summary>
    public class ListNode : BEncodedNode, IList<BEncodedNode>
    {
        #region Fields

        /// <summary>
        /// Handler列表
        /// </summary>
        private readonly IList<BEncodedNode> items;

        #endregion

        #region Exception strings

        private static readonly string ExListNodeLengthError =
            Resources.ResourceManager.GetString("ListNodeLengthError");

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数,定义元素类型为列表类型
        /// </summary>
        public ListNode()
        {
            items = new List<BEncodedNode>();
        }

        /// <summary>
        /// 构造函数,定义元素类型为列表类型
        /// </summary>
        /// <param name="listNode">BEncodedNode列表</param>
        public ListNode(IList<BEncodedNode> listNode)
        {
            items = listNode;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 添加B编码节点
        /// </summary>
        /// <param name="item">待添加的节点</param>
        public void Add(BEncodedNode item)
        {
            items.Add(item);
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;

            //跳过字符'{'
            position++;

            try
            {
                //当遇到'}'(ASCII码为125),解析结束
                while (source[position] != 125)
                {
                    BEncodedNode node = BEncode.Decode(source, ref position);

                    //当遇到'}'(ASCII码为93125解析结束
                    if (node == null)
                        break;

                    //列表添加handler
                    items.Add(node);
                }
            }

            //当捕捉IndexOutOfRangeException,抛出BEncodingException
            catch (IndexOutOfRangeException)
            {
                throw new BEncodingException(ExListNodeLengthError);
            }

            //跳过字符'e'
            position++;

            //返回所解析的数组长度
            return position - start;
        }

        /// <summary>
        /// Handler列表类的解码函数
        /// </summary>
        /// <param name="stream">待解码的内存写入流</param>
        public override void Encode(Stream stream)
        {
            //向内存流写入'{'(ASCII码为123)
            stream.WriteByte(123);

            //对于每一个Handler进行编码
            foreach (BEncodedNode item in items)
            {
                item.Encode(stream);
            }

            //向内存流写入'}'(ASCII码为125)
            stream.WriteByte(125);
        }

        #endregion

        #region IList<BEncodedNode> Members

        public int IndexOf(BEncodedNode item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, BEncodedNode item)
        {
            items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public BEncodedNode this[int index]
        {
            get
            {
                return items[index];
            }
            set
            {
                items[index] = value;
            }
        }

        #endregion

        #region ICollection<BEncodedNode> Members

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(BEncodedNode item)
        {
            return items.Contains(item);
        }

        public void CopyTo(BEncodedNode[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return items.Count; }
        }

        public bool IsReadOnly
        {
            get { return items.IsReadOnly; }
        }

        public bool Remove(BEncodedNode item)
        {
            return items.Remove(item);
        }

        #endregion

        #region IEnumerable<BEncodedNode> Members

        public IEnumerator<BEncodedNode> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion
    }
}
