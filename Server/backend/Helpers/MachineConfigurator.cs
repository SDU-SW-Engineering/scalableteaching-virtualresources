using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using ScalableTeaching.Controllers;
using ScalableTeaching.Data;
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
        private readonly IServiceScopeFactory _factory;
        private const string VM_SCALABLE_TEACHING_PATH = "/home/admin/ScalableTeaching";
        private const string SERVER_SCALABLE_TEACHING_PATH = "/ScalableTeaching";
        public MachineConfigurator(IServiceScopeFactory factory)
        {
            _factory = factory;
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

                    if (await GetContext().Users.FindAsync(assignment.UserUsername) == null)
                    {
                        GetContext().Users.Add(AuthController.NewUser(assignment.UserUsername));
                    }
                    MachineConfigurationUser machineConfigurationUser = new();
                    machineConfigurationUser.Groups = machine.LinuxGroups;
                    machineConfigurationUser.Username = assignment.UserUsername;
                    machineConfigurationUser.UserPassword = assignment.OneTimePassword;



                    User user = (await GetContext().Users.FindAsync(assignment.UserUsername));
                    System.Security.Cryptography.RSA privateKey = SSHKeyHelper.ParseKeyFromPem(user.UserPrivateKey);
                    machineConfigurationUser.UserPublicKey = SSHKeyHelper.GetSSHPublicKey(privateKey, assignment.UserUsername);
                    configurationUsers.Add(machineConfigurationUser);
                }
                else
                {
                    foreach(var groupAssignment in GetContext().GroupAssignments.Where(ga => ga.GroupID == assignment.GroupID))
                    {
                        MachineConfigurationUser machineConfigurationUser = new();
                        machineConfigurationUser.Groups = machine.LinuxGroups;
                        machineConfigurationUser.Username = groupAssignment.User.Username;
                        machineConfigurationUser.UserPassword = assignment.OneTimePassword;
                        machineConfigurationUser.UserPublicKey = SSHKeyHelper.GetSSHPublicKey(SSHKeyHelper.ParseKeyFromPem(groupAssignment.User.UserPrivateKey), groupAssignment.User.Username);
                        configurationUsers.Add(machineConfigurationUser);
                    }
                }
            }
            ConfigurationJson.Add("hostname", machine.HostName);
            ConfigurationJson.Add("groups", JToken.FromObject(machine.LinuxGroups));
            ConfigurationJson.Add("aptPPA", JToken.FromObject(machine.Ppa));
            ConfigurationJson.Add("aptPackages", JToken.FromObject(machine.Apt));
            ConfigurationJson.Add("users", JToken.FromObject(configurationUsers));

            //Connect and configure
            var connectionInfo = new ConnectionInfo(machine.MachineStatus.MachineIp, _defaultUsername, new PrivateKeyAuthenticationMethod("admin", new PrivateKeyFile($"{SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa")));

            //Upload required files
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                if (!client.ListDirectory("/home/admin/").Where(file => file.FullName == VM_SCALABLE_TEACHING_PATH).Any())
                {
                    client.CreateDirectory(VM_SCALABLE_TEACHING_PATH);
                }
                if(!client.ListDirectory(VM_SCALABLE_TEACHING_PATH).Where(file => file.Name == "SystemConfigurator").Any())
                {
                    client.UploadFile(SystemConfiguratorStream, $"{VM_SCALABLE_TEACHING_PATH}/SystemConfigurator");
                }
                var ConfigBytes = Encoding.UTF8.GetBytes(ConfigurationJson.ToString());
                var writestream = client.OpenWrite($"{VM_SCALABLE_TEACHING_PATH}/Config.json");
                await writestream.WriteAsync(ConfigBytes);
                writestream.Close();
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
        private VmDeploymentContext GetContext()
        {
            return _factory.CreateScope().ServiceProvider.GetRequiredService<VmDeploymentContext>();
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


