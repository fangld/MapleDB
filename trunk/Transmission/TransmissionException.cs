using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;

namespace Transmission
{
    /// <summary>
    /// �ײ㴫������쳣
    /// </summary>
    [Serializable]
    public class TransmissionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionException"/> class.
        /// </summary>
        public TransmissionException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionException"/> class.
        /// </summary>
        /// <param name="info"><see cref="T:System.Runtime.Serialization.SerializationInfo"></see>���������й��������쳣�����л��Ķ������ݡ�</param>
        /// <param name="context"><see cref="T:System.Runtime.Serialization.StreamingContext"></see>���������й�Դ��Ŀ�����������Ϣ��</param>
        protected TransmissionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public TransmissionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TransmissionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
