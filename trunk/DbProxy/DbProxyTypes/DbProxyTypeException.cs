using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;

namespace DbProxy.DbProxyTypes
{
    public class DbProxyTypeException : DbException, ISerializable
    {
        public DbProxyTypeException()
            : base("A sql exception has occured.")
        {
        }

        public DbProxyTypeException(string message)
            : base(message)
        {
        }

        public DbProxyTypeException(string message, Exception e)
            : base(message, e)
        {
        }

        protected DbProxyTypeException(SerializationInfo si, StreamingContext sc)
            : base(si.GetString("DbProxyTypeExceptionMessage"))
        {
        }

        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            si.AddValue("DbProxyTypeExceptionMessage", Message, typeof(string));
        }
    }
}
