using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using ScalableTeaching.Controllers;
using ScalableTeaching.Data;
using ScalableTeaching.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                if (assignment.GroupID == null)
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
                    foreach (var groupAssignment in GetContext().GroupAssignments.Where(ga => ga.GroupID == assignment.GroupID))
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
            //ConfigurationJson.Add("hostname", machine.HostName);
            //ConfigurationJson.Add("groups", JToken.FromObject(machine.LinuxGroups));
            //ConfigurationJson.Add("aptPPA", JToken.FromObject(machine.Ppa));
            //ConfigurationJson.Add("aptPackages", JToken.FromObject(machine.Apt));
            //ConfigurationJson.Add("users", JToken.FromObject(configurationUsers));

            //Connect and configure
            var connectionInfo = new ConnectionInfo(machine.MachineStatus.MachineIp, _defaultUsername, new PrivateKeyAuthenticationMethod("admin", new PrivateKeyFile($"{SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa")));

            ////Upload required files
            //using (var client = new SftpClient(connectionInfo))
            //{
            //    client.Connect();
            //    if (!client.ListDirectory("/home/admin/").Where(file => file.FullName == VM_SCALABLE_TEACHING_PATH).Any())
            //    {
            //        client.CreateDirectory(VM_SCALABLE_TEACHING_PATH);
            //    }
            //    if(!client.ListDirectory(VM_SCALABLE_TEACHING_PATH).Where(file => file.Name == "SystemConfigurator").Any())
            //    {
            //        client.UploadFile(SystemConfiguratorStream, $"{VM_SCALABLE_TEACHING_PATH}/SystemConfigurator");
            //    }
            //    var ConfigBytes = Encoding.UTF8.GetBytes(ConfigurationJson.ToString());
            //    var writestream = client.OpenWrite($"{VM_SCALABLE_TEACHING_PATH}/Config.json");
            //    await writestream.WriteAsync(ConfigBytes);
            //    writestream.Close();

            //}
            //Excecute things
            using (var client = new SshClient(connectionInfo))
            {
                MemoryStream stdin = new();
                MemoryStream stdout = new();
                MemoryStream extout = new();

                //ConfigurationJson.Add("users", JToken.FromObject(configurationUsers));
                client.Connect();

                //Add groups
                foreach (var group in machine.LinuxGroups)
                {
                    var command = client.CreateCommand($"sudo groupadd {group}");
                    var result = command.BeginExecute();
                    await Task.Run(() => result.AsyncWaitHandle.WaitOne());
                }
                //Add users
                foreach (MachineConfigurationUser user in configurationUsers)
                {
                    var p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "openssl";
                    p.StartInfo.Arguments = $"passwd -6 -salt {RandomString(10)} {user.UserPassword}";
                    p.Start();
                    await p.WaitForExitAsync(); //TODO: Its clipping the data when reading single line

                    string output = p.StandardOutput.ReadLine();
                    Console.WriteLine($"Add user: {user.Username} hashed pw: {output}");

                    //Create User
                    var useraddCommand = client.CreateCommand($"sudo useradd -mp {output} {user.Username.ToLower()}");
                    var useraddResult = useraddCommand.BeginExecute();
                    await Task.Run(() => useraddResult.AsyncWaitHandle.WaitOne());
                    Console.WriteLine($"MachineConfigurator: useraddCommand text: {useraddCommand.CommandText} Result: {useraddCommand.Result}");

                    //Add user to linux groups
                    foreach (var group in user.Groups)
                    {
                        var usermodCommand = client.CreateCommand($"sudo usermod -aG {group} {user.Username.ToLower()}");
                        var usermodResult = usermodCommand.BeginExecute();
                        await Task.Run(() => usermodResult.AsyncWaitHandle.WaitOne());
                        Console.WriteLine($"MachineConfigurator: usermodCommand text: {usermodCommand.CommandText} .Result {usermodCommand.Result}");
                    }

                    //Prep authorized_keys
                    var AuthorizedKeysCommand = 
                        client.CreateCommand($"sudo mkdir -p /home/{user.Username.ToLower()}/.ssh ;" +
                        $" sudo touch /home/{user.Username.ToLower()}/.ssh/authorized_keys ;" +
                        $" sudo chown -R {user.Username.ToLower()}:{user.Username.ToLower()} /home/{user.Username.ToLower()}/.ssh ;" +
                        $" sudo chmod -R 600 /home/{user.Username.ToLower()}/.ssh");
                    var AuthorizedKeysResult = AuthorizedKeysCommand.BeginExecute();
                    await Task.Run(() => AuthorizedKeysResult.AsyncWaitHandle.WaitOne());
                    Console.WriteLine($"MachineConfigurator: AuthorizedKeysCommand text: {AuthorizedKeysCommand.CommandText} .Result {AuthorizedKeysCommand.Result}");

                    //Add key
                    var addKeyCommand = client.CreateCommand($"sudo sh -c 'echo \"{user.UserPublicKey}\" >> /home/{user.Username.ToLower()}/.ssh/authorized_keys'");
                    var addKeyResult = addKeyCommand.BeginExecute();
                    await Task.Run(() => addKeyResult.AsyncWaitHandle.WaitOne());
                    Console.WriteLine($"MachineConfigurator: addKeyCommand text {addKeyCommand.CommandText} .Result {addKeyCommand.Result}");

                }

                //Update Upgrade
                var aptUpdateUpgradeCommand = client.CreateCommand($"sudo apt-get update");
                var aptUpdateUpgradeResult = aptUpdateUpgradeCommand.BeginExecute();
                await Task.Run(() => aptUpdateUpgradeResult.AsyncWaitHandle.WaitOne());
                Console.WriteLine($"MachineConfigurator: aptUpdateUpgradeCommand.Result {aptUpdateUpgradeCommand.Result}");

                //Prep for ppa
                var ppaPrepCommand = client.CreateCommand($"sudo apt-get install -y software-properties-common");
                var ppaPrepResult = ppaPrepCommand.BeginExecute();
                await Task.Run(() => ppaPrepResult.AsyncWaitHandle.WaitOne());
                Console.WriteLine($"MachineConfigurator: ppaPrepCommand.Result {ppaPrepCommand.Result}");

                //PPA
                foreach (var ppa in machine.Ppa)
                {
                    var ppaAddCommand = client.CreateCommand($"sudo add-apt-repository -y {ppa}");
                    var ppaAddResult = ppaAddCommand.BeginExecute();
                    await Task.Run(() => ppaAddResult.AsyncWaitHandle.WaitOne());
                    Console.WriteLine($"MachineConfigurator: ppaAddCommand.Result {ppaAddCommand.Result}");
                }

                //Post ppa update and upgrade
                var postPpaUpdateCommand = client.CreateCommand($"sudo apt-get update && sudo apt-get upgrade -y");
                var postPpaUpdateResult = postPpaUpdateCommand.BeginExecute();
                await Task.Run(() => postPpaUpdateResult.AsyncWaitHandle.WaitOne());
                Console.WriteLine($"MachineConfigurator: postPpaUpdateCommand.Result {postPpaUpdateCommand.Result}");

                //Install apts
                foreach (var apt in machine.Apt)
                {
                    var aptInstallCommand = client.CreateCommand($"sudo apt-get install -y {apt}");
                    var aptInstallResult = aptInstallCommand.BeginExecute();
                    await Task.Run(() => aptInstallResult.AsyncWaitHandle.WaitOne());
                    Console.WriteLine($"MachineConfigurator: aptInstallCommand.Result {aptInstallCommand.Result}");
                }

                //Apt cleanup
                var aptCleanupCommand = client.CreateCommand($"sudo apt-get autoremove -y");
                var aptCleanupResult = aptCleanupCommand.BeginExecute();
                await Task.Run(() => aptCleanupResult.AsyncWaitHandle.WaitOne());
                Console.WriteLine($"MachineConfigurator: aptCleanupCommand.Result {aptCleanupCommand.Result}");

                client.Disconnect();
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
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}


