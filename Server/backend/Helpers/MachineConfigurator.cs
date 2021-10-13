using Microsoft.EntityFrameworkCore;
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
using System.Text;
using System.Threading;
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
            var assignments = await GetContext().MachineAssignments.Where(ass => ass.MachineID == machine.MachineID).ToListAsync();
            foreach (var assignment in assignments)//TODO: Expecting issues with resolution of virtual properties on the items from the database
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
            Console.WriteLine($"Connection info, ip: {machine.MachineStatus.MachineIp}, Default username: {_defaultUsername}, ");
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
                    await p.WaitForExitAsync();

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
                var ppaPrepCommand = await PerformSSHCommand(client, $"sudo apt-get install -y software-properties-common");
                Console.WriteLine($"MachineConfigurator: ppaPrepCommand.Result {ppaPrepCommand.Result}");

                //PPA
                foreach (var ppa in machine.Ppa)
                {
                    var ppaAddCommand = await PerformSSHCommand(client, $"sudo add-apt-repository -y {ppa}");
                    Console.WriteLine($"MachineConfigurator: ppaAddCommand.Result {ppaAddCommand.Result}");
                }

                //Post ppa update and upgrade
                var postPpaUpdateCommand = await PerformSSHCommand(client, $"sudo apt-get update && sudo apt-get upgrade -y");
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

        public async Task<bool> ConfigureMachineWithFile(Machine machine)//TODO: Unifinished due to unknown elements related to lack of access to images on open nebula
        {
            //Construct configuration string
            List<MachineConfigurationUser> configurationUsers = new();
            List<MachineAssignment> assignments = await GetContext().MachineAssignments.Where(assignment => assignment.MachineID == machine.MachineID).ToListAsync();
            Console.WriteLine($"Machine assignment count: {assignments.Count}");
            foreach (var assignment in assignments)//TODO: Expecting issues with resolution of virtual properties on the items from the database
            {
                Console.WriteLine($"Calculating machine assignments");
                if (assignment.GroupID == null)
                {
                    var context = GetContext();
                    if (await context.Users.FindAsync(assignment.UserUsername) == null)
                    {
                        context.Users.Add(await UserFactory.Create(assignment.UserUsername));
                    }
                    MachineConfigurationUser machineConfigurationUser = new();
                    machineConfigurationUser.Groups = machine.LinuxGroups;
                    machineConfigurationUser.Username = assignment.UserUsername;
                    machineConfigurationUser.UserPassword = assignment.OneTimePassword;

                    User user = (await context.Users.FindAsync(assignment.UserUsername));
                    machineConfigurationUser.UserPublicKey = user.UserPublicKey;
                    configurationUsers.Add(machineConfigurationUser);
                }
                else
                {
                    Console.WriteLine($"Calculating for group based assignments");
                    foreach (var groupAssignment in await GetContext().GroupAssignments.Where(ga => ga.GroupID == assignment.GroupID).ToListAsync())
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
            //var connectionInfo = new ConnectionInfo(machine.MachineStatus.MachineIp, _defaultUsername, new PrivateKeyAuthenticationMethod("admin", new PrivateKeyFile($"{SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa")));
            //Console.WriteLine("Connection info, ip: {machine.MachineStatus.MachineIp}, Default username: {_defaultUsername}, ");
            var builder = new StringBuilder();
            
            //Update Admin password to prevent lockout by ssh key mishap
            builder.AppendLine($"echo \"admin:{Environment.GetEnvironmentVariable("ADMIN_PASSWD")}\" | chpasswd");
            
            //Add groups
            foreach (var group in machine.LinuxGroups)
            {
                builder.AppendLine($"sudo groupadd {group}");
            }
            Console.WriteLine("Configuration users count {}");
            //Add users
            foreach (var user in configurationUsers)
            {
                Console.WriteLine($"Configuring user {user.Username}");
                var p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "openssl";
                p.StartInfo.Arguments = $"passwd -6 -salt {RandomString(10)} {user.UserPassword}";
                p.Start();
                await p.WaitForExitAsync(); //TODO: Its clipping the data when reading single line

                var output = (await p.StandardOutput.ReadToEndAsync()).TrimEnd('\n', '\r', ' '); //Trim any trailing non hash chars

                //Create User
                builder.AppendLine($"useradd -s \"/usr/bin/bash\" -mp {output} {user.Username.ToLower()}");

                //Add user to linux groups
                foreach (var group in user.Groups)
                {
                    builder.AppendLine($"usermod -aG {group} {user.Username.ToLower()}");
                }

                //Prep authorized_keys
                builder.AppendLine($"mkdir -p /home/{user.Username.ToLower()}/.ssh &&" +
                    $" touch /home/{user.Username.ToLower()}/.ssh/authorized_keys &&" +
                    $" chown -R {user.Username.ToLower()}:{user.Username.ToLower()} /home/{user.Username.ToLower()}/.ssh &&" +
                    $" chmod -R 755 /home/{user.Username.ToLower()}/.ssh");

                //Add key
                builder.AppendLine($"sudo sh -c 'echo \"{user.UserPublicKey}\" >> /home/{user.Username.ToLower()}/.ssh/authorized_keys'");

                //Remove password requirement from sudo
                builder.AppendLine($"sudo sh -c 'echo \"{user.Username.ToLower()} ALL=(ALL) NOPASSWD:ALL\" > /etc/sudoers.d/{user.Username.ToLower()}'");
            }

            //Update
            builder.AppendLine( $"sudo apt-get update");

            //Prep for ppa
            builder.AppendLine($"sudo apt-get install -y software-properties-common");

            //PPA
            foreach (var ppa in machine.Ppa)
            {
                builder.AppendLine($"sudo add-apt-repository -y {ppa}");
            }

            //Post PPA update
            builder.AppendLine($"sudo apt-get update");

            //Install apts
            foreach (var apt in machine.Apt)
            {
                builder.AppendLine($"sudo apt-get install -y {apt}");
            }

            //Post ppa update and upgrade
            builder.AppendLine($"sudo apt-get update && sudo apt-get upgrade -y");

            //Apt cleanup
            builder.AppendLine($"sudo apt-get autoremove -y");

            await File.WriteAllBytesAsync($"{SERVER_SCALABLE_TEACHING_PATH}/configfile/{machine.HostName}.sh", Encoding.UTF8.GetBytes(builder.ToString()));

            var p_scp = new Process();
            p_scp.StartInfo.UseShellExecute = false;
            p_scp.StartInfo.RedirectStandardOutput = true;
            p_scp.StartInfo.RedirectStandardError = true;
            p_scp.StartInfo.FileName = "scp";
            p_scp.StartInfo.Arguments = $"-i {SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa -o StrictHostKeyChecking=no -B {SERVER_SCALABLE_TEACHING_PATH}/configfile/{machine.HostName}.sh admin@{machine.MachineStatus.MachineIp}:/home/admin/configfile.sh";
            p_scp.Start();
            await p_scp.WaitForExitAsync();

            Console.WriteLine($"Did scp into {machine.HostName} {machine.MachineStatus.MachineIp}, status: Exit code: {p_scp.ExitCode}\nout: {p_scp.StandardOutput.ReadToEnd()} \n err{p_scp.StandardError.ReadToEnd()}");

            //Using task as a hack - this is an odd boy that sometimes does not return
            //TODO: Better solution
            Task.Run(async () =>
            {
                Console.WriteLine($"Starting ssh: {machine.HostName}, {machine.MachineStatus.MachineIp}");
                var p_ssh = new Process();
                p_ssh.StartInfo.UseShellExecute = false;
                p_ssh.StartInfo.RedirectStandardOutput = true;
                p_ssh.StartInfo.RedirectStandardError = true;
                p_ssh.StartInfo.FileName = "ssh";
                p_ssh.StartInfo.Arguments =
                    $"-o StrictHostKeyChecking=no -i {SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa admin@{machine.MachineStatus.MachineIp} \"sudo chmod 777 /home/admin/configfile.sh && sudo sh -c '/home/admin/configfile.sh'; sudo rm /home/admin/configfile.sh; touch /home/admin/ranConfig; exit\"";
                p_ssh.Start();
                await p_ssh.WaitForExitAsync();
                Console.WriteLine($"Finished ssh: {machine.HostName}, {machine.MachineStatus.MachineIp}");
            });

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


