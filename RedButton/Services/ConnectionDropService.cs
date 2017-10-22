using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Principal;

namespace RedButton.Services
{
    public class ConnectionDropService
    {
        private readonly string _connectionBroker;

        public ConnectionDropService()
        {
            _connectionBroker = ConfigurationManager.AppSettings["rdcb-domain"];
        }

        public List<string> GetActiveAccounts() // List<string> accounts
        {
            var list = new List<string> {WindowsIdentity.GetCurrent().Name};
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            using(var pipeline = runspace.CreatePipeline())
            {
                var scriptBody = $"Get-RDUserSession -ConnectionBroker \"{_connectionBroker}\"";
                pipeline.Commands.AddScript(scriptBody);
                list.AddRange(pipeline.Invoke().Select(obj => obj.ToString()));
                list.Add($"Error: {string.Join(",", pipeline.Error.ReadToEnd())}");
            }
            runspace.Close();
            return list;
        }

        public void DropConnection(List<string> accounts)
        {
            using (var script = PowerShell.Create())
            {
                var accountVariable = $"$accounts=@(\"{string.Join("\",\"", accounts)}\")";
                var getUserCommand = $"Get-RDUserSession -ConnectionBroker \"{_connectionBroker}\"";
                const string filterCommand = "Where-Object{$accounts -contains $_.userName}";
                const string deleteCommand = "ForEach-Object {Disconnect-RDUser -HostServer $_.HostServer -UnifiedSessionID $_.UnifiedSessionId -Force}";
                var scriptBody = $"{accountVariable};{getUserCommand} | {filterCommand} | {deleteCommand}";
                script.AddScript(scriptBody);
                script.Invoke();
            }
        }

        public string GetCommand(List<string> accounts)
        {
            var accountVariable = $"$accounts=@(\"{string.Join("\",\"", accounts)}\")";
            var getUserCommand = $"Get-RDUserSession -ConnectionBroker \"{_connectionBroker}\"";
            const string filterCommand = "Where-Object{$accounts -contains $_.userName}";
            return $"{accountVariable};{getUserCommand} | {filterCommand}";
        }
    }
}