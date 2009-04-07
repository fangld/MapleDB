using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DbProxy.DbProxyTypes
{
    public class DbProxyNullValueException : DbProxyTypeException, ISerializable
    {
        public DbProxyNullValueException()
            : base("Data is Null. This method or property cannot be called on Null values.")
        {
        }

        public DbProxyNullValueException(string message)
            : base(message)
        {
        }

        public DbProxyNullValueException(string message, Exception e)
            : base(message, e)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            si.AddValue("DbProxyNullValueExceptionMessage", Message, typeof(string));
        }
    }
}
