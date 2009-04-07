using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using Transmission;

namespace DbProxy.Monitor
{
    /// <summary>
    /// Db Proxy¼àÊÓÆ÷
    /// </summary>
    public class DbProxyMonitor
    {
        #region Methods

        private DoubleChannelClient client;

        private int serverPort;

        #endregion

        #region Properties

        #endregion

        #region Constructors

        public DbProxyMonitor(int serverPort)
        {
            this.serverPort = serverPort;
        }

        #endregion

        #region Methods

        public void Start()
        {
            client.Connect("localhost", serverPort);
        }

        public void Stop()
        {
            
        }

        #endregion
    }
}
