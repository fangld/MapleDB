using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;

namespace DbProxyService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            //serviceProcessInstaller.Username = "Administrator";
            //serviceProcessInstaller.Password = "gznt";
        }
    }
}