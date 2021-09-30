using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using ScalableTeaching.Data;
using ScalableTeaching.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            //Construct configuration string
            List<MachineConfigurationUser> configurationUsers = new();
            foreach (var assignment in machine.MachineAssignments)//TODO: Expecting issues with resolution of virtual properties on the items from the database
            {
                if (assignment.GroupID == null)
                {

                    if (await GetContext().Users.FindAsync(assignment.UserUsername) == null)
                    {
                        GetContext().Users.Add(await UserFactory.Create(assignment.UserUsername));
                    }
                    MachineConfigurationUser machineConfigurationUser = new();
                    machineConfigurationUser.Groups = machine.LinuxGroups;
                    machineConfigurationUser.Username = assignment.UserUsername;
                    machineConfigurationUser.UserPassword = assignment.OneTimePassword;



                    User user = (await GetContext().Users.FindAsync(assignment.UserUsername));
                    machineConfigurationUser.UserPublicKey = user.UserPublicKey;
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
                        machineConfigurationUser.UserPublicKey = groupAssignment.User.UserPublicKey;
                        configurationUsers.Add(machineConfigurationUser);
                    }
                }
            }
            var connectionInfo = new ConnectionInfo(machine.MachineStatus.MachineIp, _defaultUsername, new PrivateKeyAuthenticationMethod("admin", new PrivateKeyFile($"{SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa")));
            using (var client = new SshClient(connectionInfo))
            {
                MemoryStream stdin = new();
                MemoryStream stdout = new();
                MemoryStream extout = new();
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

                    string output = (await p.StandardOutput.ReadToEndAsync()).Trim();
                    Console.WriteLine($"Add user: {user.Username} hashed pw: {output}");


                    //Create User
                    var useraddCommand = await PerformSSHCommand(client, $"sudo useradd -s \"/usr/bin/bash\" -mp {output} {user.Username.ToLower()}");
                    Console.WriteLine($"MachineConfigurator: useraddCommand text: {useraddCommand.CommandText} Result: {useraddCommand.Result}");

                    //Add user to linux groups
                    foreach (var group in user.Groups)
                    {
                        var usermodCommand = await PerformSSHCommand(client, $"sudo usermod -aG {group} {user.Username.ToLower()}");
                        Console.WriteLine($"MachineConfigurator: usermodCommand text: {usermodCommand.CommandText} .Result {usermodCommand.Result}");
                    }

                    //Prep authorized_keys
                    var AuthorizedKeysCommand = await PerformSSHCommand(client, $"sudo mkdir -p /home/{user.Username.ToLower()}/.ssh &&" +
                        $" sudo touch /home/{user.Username.ToLower()}/.ssh/authorized_keys &&" +
                        $" sudo chown -R {user.Username.ToLower()}:{user.Username.ToLower()} /home/{user.Username.ToLower()}/.ssh &&" +
                        $" sudo chmod -R 755 /home/{user.Username.ToLower()}/.ssh");
                    Console.WriteLine($"MachineConfigurator: AuthorizedKeysCommand text: {AuthorizedKeysCommand.CommandText} .Result {AuthorizedKeysCommand.Result}");

                    //Add key
                    var addKeyCommand = client.CreateCommand($"sudo sh -c 'echo \"{user.UserPublicKey}\" >> /home/{user.Username.ToLower()}/.ssh/authorized_keys'");
                    var addKeyResult = addKeyCommand.BeginExecute();
                    await Task.Run(() => addKeyResult.AsyncWaitHandle.WaitOne());
                    Console.WriteLine($"MachineConfigurator: addKeyCommand text {addKeyCommand.CommandText} .Result {addKeyCommand.Result}");

                    //Remove password requirement from sudo
                    var nopasswdCommand = await PerformSSHCommand(client, $"sudo sh -c 'echo \"{user.Username.ToLower()} ALL=(ALL) NOPASSWD:ALL\" > /etc/sudoers.d/{user.Username.ToLower()}'");
                    Console.WriteLine($"MachineConfigurator: nopasswdCommand text {nopasswdCommand.CommandText} .Result {nopasswdCommand.Result}");
                }

                //Update
                var aptUpdateUpgradeCommand = await PerformSSHCommand(client, $"sudo apt-get update");
                Console.WriteLine($"MachineConfigurator: aptUpdateUpgradeCommand.Result {aptUpdateUpgradeCommand.Result}");

                //Prep for ppa
                var ppaPrepCommand = await PerformSSHCommand (client, $"sudo apt-get install -y software-properties-common");
                Console.WriteLine($"MachineConfigurator: ppaPrepCommand.Result {ppaPrepCommand.Result}");

                //PPA
                foreach (var ppa in machine.Ppa)
                {
                    var ppaAddCommand = await PerformSSHCommand (client, $"sudo add-apt-repository -y {ppa}");
                    Console.WriteLine($"MachineConfigurator: ppaAddCommand.Result {ppaAddCommand.Result}");
                }

                //Post ppa update and upgrade
                var postPpaUpdateCommand = await PerformSSHCommand (client, $"sudo apt-get update && sudo apt-get upgrade -y");
                Console.WriteLine($"MachineConfigurator: postPpaUpdateCommand.Result {postPpaUpdateCommand.Result}");

                //Install apts
                foreach (var apt in machine.Apt)
                {
                    var aptInstallCommand = await PerformSSHCommand(client, $"sudo apt-get install -y {apt}");
                    Console.WriteLine($"MachineConfigurator: aptInstallCommand.Result {aptInstallCommand.Result}");
                }

                //Apt cleanup
                SshCommand aptCleanupCommand = await PerformSSHCommand(client, $"sudo apt-get autoremove -y");
                Console.WriteLine($"MachineConfigurator: aptCleanupCommand.Result {aptCleanupCommand.Result}");

                client.Disconnect();
            }
            return true; //TODO: Implement error handeling for configuration 
        }

        private static async Task<SshCommand> PerformSSHCommand(SshClient client, string command)
        {
            var _command = client.CreateCommand(command);
            var _commandResult = _command.BeginExecute();
            await Task.Run(() => _commandResult.AsyncWaitHandle.WaitOne());
            return _command;
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


