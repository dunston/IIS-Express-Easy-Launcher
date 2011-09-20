using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using NUnit.Framework;

namespace LaunchIISExpress.Tests
{
    [TestFixture]
    public class Registry
    {

        [Test]
        public void SetupRegistry()
        {
            
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsImpersonationContext context = identity.Impersonate();
            RegistryKey regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Directory\shell", RegistryKeyPermissionCheck.ReadWriteSubTree);
            Assert.NotNull(regkey, "RegKey Was null");
            
            RegistryKey contextKey = regkey.CreateSubKey("LaunchIISExpressExpress");
            Assert.NotNull(contextKey, "ContextKey was not created");

            contextKey.SetValue("", "Launch IIS Express", RegistryValueKind.String);
            contextKey.Flush();
            Assert.NotNull(contextKey.GetValue(""), "ContextKey(Default) was not created");

            RegistryKey commandKey = contextKey.CreateSubKey("command");
            commandKey.Flush();
            Assert.NotNull(commandKey, "commandKey was not created");

            commandKey.SetValue("", "Test Path", RegistryValueKind.String);
            commandKey.Flush();
            Assert.NotNull(contextKey.GetValue(""), "commandKey(Default) was not created");

            context.Undo();
        }

        [Test]
        public void RemoveRegistry()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsImpersonationContext context = identity.Impersonate();
            RegistryKey regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Directory\shell",RegistryKeyPermissionCheck.ReadWriteSubTree);
            Assert.NotNull(regkey, "RegKey Was null");

            regkey.DeleteSubKeyTree("LaunchIISExpressExpress");
            regkey.Flush();
            Thread.Sleep(1000);
            RegistryKey launchIISExpressExpressKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Directory\shell\LaunchIISExpressExpress");
            Assert.IsNull(launchIISExpressExpressKey, "launchIISExpressExpressKey should be null if its removed");
            
            context.Undo();
        }
    }
}
