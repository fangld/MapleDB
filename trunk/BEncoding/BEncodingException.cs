using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BEncoding
{
    /// <summary>
    /// B编码的异常
    /// </summary>
    [Serializable]
    public class BEncodingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BEncodingException"/> class.
        /// </summary>
        public BEncodingException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BEncodingException"/> class.
        /// </summary>
        /// <param name="info"><see cref="T:System.Runtime.Serialization.SerializationInfo"></see>，它存有有关所引发异常的序列化的对象数据。</param>
        /// <param name="context"><see cref="T:System.Runtime.Serialization.StreamingContext"></see>，它包含有关源或目标的上下文信息。</param>
        protected BEncodingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BEncodingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public BEncodingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BEncodingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BEncodingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
