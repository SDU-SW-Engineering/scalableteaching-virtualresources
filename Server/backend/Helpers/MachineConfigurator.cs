using Newtonsoft.Json.Linq;
using Renci.SshNet;
using ScalableTeaching.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalableTeaching.Helpers
{
    public class MachineConfigurator
    {
        private string _defaultUsername;
        private const string VM_SCALABLE_TEACHING_PATH = "/home/admin/ScalableTeaching";
        private const string SERVER_SCALABLE_TEACHING_PATH = "/ScalableTeaching";
        public MachineConfigurator()
        {
            _defaultUsername = Environment.GetEnvironmentVariable("VM_DEFAULT_USERNAME");
        }
        public async Task<bool> ConfigureMachine(Machine machine)//TODO: Unifinished due to unknown elements related to lack of access to images on open nebula
        {
            

            var SystemConfiguratorStream = new MemoryStream(File.ReadAllBytes($"{SERVER_SCALABLE_TEACHING_PATH}/SystemConfigurator"));
            var ConfigurationJson = new JObject();
            //Construct configuration string
            List<MachineConfigurationUser> configurationUsers = new();
            foreach (var assignment in machine.MachineAssignments)//TODO: Expecting issues with resolution of virtual properties on the items from the database
            {
                if(assignment.GroupID == null)
                {
                    configurationUsers.Add(new MachineConfigurationUser() 
                    { 
                        Groups = machine.LinuxGroups,
                        Username = assignment.UserUsername,
                        UserPassword = assignment.OneTimePassword,
                        UserPublicKey = SSHKeyHelper.GetSSHPublicKey(SSHKeyHelper.ParseKeyFromPem(assignment.User.UserPrivateKey), assignment.UserUsername)  
                    });
                }
                else
                {
                    foreach(var groupAssignment in assignment.Group.GroupAssignments)
                    {
                        configurationUsers.Add(new MachineConfigurationUser()
                        {
                            Groups = machine.LinuxGroups,
                            Username = groupAssignment.User.Username,
                            UserPassword = assignment.OneTimePassword,
                            UserPublicKey = SSHKeyHelper.GetSSHPublicKey(SSHKeyHelper.ParseKeyFromPem(groupAssignment.User.UserPrivateKey), groupAssignment.User.Username)
                        });
                    }
                }
            }
            ConfigurationJson.Add("hostname", machine.HostName);
            ConfigurationJson.Add("groups", JToken.FromObject(machine.LinuxGroups));
            ConfigurationJson.Add("aptPPA", JToken.FromObject(machine.Ppa));
            ConfigurationJson.Add("aptPackages", JToken.FromObject(machine.Apt));
            ConfigurationJson.Add("users", JToken.FromObject(configurationUsers));

            //Connect and configure
            var connectionInfo = new ConnectionInfo(machine.HostName, _defaultUsername, new PrivateKeyAuthenticationMethod("admin", new PrivateKeyFile("/home/admin/.ssh/id_rsa")));

            //Upload required files
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                client.DeleteDirectory(VM_SCALABLE_TEACHING_PATH);
                client.CreateDirectory(VM_SCALABLE_TEACHING_PATH);
                client.UploadFile(SystemConfiguratorStream, $"{VM_SCALABLE_TEACHING_PATH}/SystemConfigurator");

                var writestream = client.OpenWrite($"{VM_SCALABLE_TEACHING_PATH}/Config.json");
                await writestream.WriteAsync(Encoding.UTF8.GetBytes(ConfigurationJson.ToString()));
            }
            //Excecute things
            using (var client = new SshClient(connectionInfo))
            {
                MemoryStream stdin = new();
                MemoryStream stdout = new();
                MemoryStream extout = new();

                client.Connect();
                var command = client.CreateCommand($"{VM_SCALABLE_TEACHING_PATH}/SystemConfigurator $> {VM_SCALABLE_TEACHING_PATH}/ConfigurationOutput.log");
                var result = command.BeginExecute();

                await Task.Run(()=>result.AsyncWaitHandle.WaitOne());
            }
            return true; //TODO: Implement error handeling for configuration 
        }
        private class MachineConfigurationUser
        {
            public string Username { get; set; }
            public string UserPassword { get; set; }
            public string UserPublicKey { get; set; }
            public List<string> Groups { get; set; }
        }
    }
    
}


