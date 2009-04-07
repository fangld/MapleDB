using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;

namespace Packets
{
    /// <summary>
    /// 底层传输包的异常
    /// </summary>
    [Serializable]
    public class PacketException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketException"/> class.
        /// </summary>
        public PacketException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketException"/> class.
        /// </summary>
        /// <param name="info"><see cref="T:System.Runtime.Serialization.SerializationInfo"></see>，它存有有关所引发异常的序列化的对象数据。</param>
        /// <param name="context"><see cref="T:System.Runtime.Serialization.StreamingContext"></see>，它包含有关源或目标的上下文信息。</param>
        protected PacketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PacketException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PacketException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
