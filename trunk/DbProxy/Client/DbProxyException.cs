﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;

namespace DbProxy.Client
{
    /// <summary>
    /// 数据访问层的异常
    /// </summary>
    [Serializable]
    public class DbProxyException : DbException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbProxyException"/> class.
        /// </summary>
        public DbProxyException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbProxyException"/> class.
        /// </summary>
        /// <param name="info"><see cref="T:System.Runtime.Serialization.SerializationInfo"></see>，它存有有关所引发异常的序列化的对象数据。</param>
        /// <param name="context"><see cref="T:System.Runtime.Serialization.StreamingContext"></see>，它包含有关源或目标的上下文信息。</param>
        protected DbProxyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbProxyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DbProxyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbProxyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DbProxyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
