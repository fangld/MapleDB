using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using DbProxy;
using DbProxy.Server;

namespace DbProxyService
{
    public partial class ServerService : ServiceBase
    {
        private const string dbConnString = "Data Source=localhost;Initial Catalog=TestDbProxy;Persist Security Info=True;User ID=sa;Password=gznt";

        private const int timeout = 10000;

        private DbProxyServer server;

        public ServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            server = new DbProxyServer(dbConnString, timeout);
            server.Start();
        }

        protected override void OnStop()
        {
            server.Stop();
        }
    }
}
