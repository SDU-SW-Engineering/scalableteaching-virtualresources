using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Models;
using ScalableTeaching.Services;
using Serilog;

namespace ScalableTeaching.Helpers;

public class MachineConfigurator
{
    private const string VM_SCALABLE_TEACHING_PATH = "/home/admin/ScalableTeaching";
    private const string SERVER_SCALABLE_TEACHING_PATH = "/ScalableTeaching";
    private readonly IDbContextFactory _dbContextFactory;
    private readonly string _defaultUsername;

    public MachineConfigurator(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _defaultUsername = Environment.GetEnvironmentVariable("VM_DEFAULT_USERNAME");
    }

    //Commented out due to issues with ssh libraries
    // public async Task<bool> ConfigureMachine(Machine machine)
    // {
    //     //Construct configuration string
    //     List<MachineConfigurationUser> configurationUsers = new();
    //     var assignments = await GetContext().MachineAssignments.Where(ass => ass.MachineID == machine.MachineID).ToListAsync();
    //     foreach (var assignment in assignments)//TODO: Expecting issues with resolution of virtual properties on the items from the database
    //     {
    //         if (assignment.GroupID == null)
    //         {
    //
    //             if (await GetContext().Users.FindAsync(assignment.UserUsername) == null)
    //             {
    //                 GetContext().Users.Add(await UserFactory.Create(assignment.UserUsername));
    //             }
    //             MachineConfigurationUser machineConfigurationUser = new();
    //             machineConfigurationUser.Groups = machine.LinuxGroups;
    //             machineConfigurationUser.Username = assignment.UserUsername;
    //             machineConfigurationUser.UserPassword = assignment.OneTimePassword;
    //
    //
    //
    //             User user = (await GetContext().Users.FindAsync(assignment.UserUsername));
    //             machineConfigurationUser.UserPublicKey = user.UserPublicKey;
    //             configurationUsers.Add(machineConfigurationUser);
    //         }
    //         else
    //         {
    //             foreach (var groupAssignment in GetContext().GroupAssignments.Where(ga => ga.GroupID == assignment.GroupID))
    //             {
    //                 MachineConfigurationUser machineConfigurationUser = new();
    //                 machineConfigurationUser.Groups = machine.LinuxGroups;
    //                 machineConfigurationUser.Username = groupAssignment.User.Username;
    //                 machineConfigurationUser.UserPassword = assignment.OneTimePassword;
    //                 machineConfigurationUser.UserPublicKey = groupAssignment.User.UserPublicKey;
    //                 configurationUsers.Add(machineConfigurationUser);
    //             }
    //         }
    //     }
    //     var connectionInfo = new ConnectionInfo(machine.MachineStatus.MachineIp, _defaultUsername, new PrivateKeyAuthenticationMethod("admin", new PrivateKeyFile($"{SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa")));
    //     Console.WriteLine($"Connection info, ip: {machine.MachineStatus.MachineIp}, Default username: {_defaultUsername}, ");
    //     using (var client = new SshClient(connectionInfo))
    //     {
    //         MemoryStream stdin = new();
    //         MemoryStream stdout = new();
    //         MemoryStream extout = new();
    //         client.Connect();
    //
    //         //Add groups
    //         foreach (var group in machine.LinuxGroups)
    //         {
    //             var command = client.CreateCommand($"sudo groupadd {group}");
    //             var result = command.BeginExecute();
    //             await Task.Run(() => result.AsyncWaitHandle.WaitOne());
    //         }
    //         //Add users
    //         foreach (MachineConfigurationUser user in configurationUsers)
    //         {
    //             //Define file that the password hash is stored in temporarily
    //             var passwordHashFile = $"/tmp/{StringHelper.RandomString(16)}";
    //             
    //             //Generate password hash
    //             var p = new Process();
    //             p.StartInfo.UseShellExecute = false;
    //             p.StartInfo.RedirectStandardOutput = false;
    //             p.StartInfo.FileName = "openssl";
    //             p.StartInfo.Arguments = $"passwd -6 -salt '{StringHelper.RandomString(16)}' '{user.UserPassword}' > {passwordHashFile}";
    //             p.Start();
    //             await p.WaitForExitAsync();
    //             
    //             //Read password hash
    //             string output = (await File.ReadAllTextAsync(passwordHashFile)).TrimEnd();
    //             Console.WriteLine($"Add user: {user.Username} hashed pw: {output}");
    //             
    //             //Cleanup password hash file
    //             File.Delete(passwordHashFile);
    //
    //             //Create User
    //             var useraddCommand = await PerformSSHCommand(client, $"sudo useradd -s \"/usr/bin/bash\" -mp {output} {user.Username.ToLower()}");
    //             Console.WriteLine($"MachineConfigurator: useraddCommand text: {useraddCommand.CommandText} Result: {useraddCommand.Result}");
    //
    //             //Add user to linux groups
    //             var usermodCommand = await PerformSSHCommand(client, $"sudo usermod -aG {string.Join(",", user.Groups)} {user.Username.ToLower()}");
    //             Log.Information("MachineConfigurator: usermodCommand text: {UsermodCommandCommandText} .Result {UsermodCommandResult}", usermodCommand.CommandText, usermodCommand.Result);
    //             
    //             //Prep authorized_keys
    //             var AuthorizedKeysCommand = await PerformSSHCommand(client, $"sudo mkdir -p /home/{user.Username.ToLower()}/.ssh &&" +
    //                 $" sudo touch /home/{user.Username.ToLower()}/.ssh/authorized_keys &&" +
    //                 $" sudo chown -R {user.Username.ToLower()}:{user.Username.ToLower()} /home/{user.Username.ToLower()}/.ssh &&" +
    //                 $" sudo chmod -R 755 /home/{user.Username.ToLower()}/.ssh");
    //             Console.WriteLine($"MachineConfigurator: AuthorizedKeysCommand text: {AuthorizedKeysCommand.CommandText} .Result {AuthorizedKeysCommand.Result}");
    //
    //             //Add key
    //             var addKeyCommand = client.CreateCommand($"sudo sh -c 'echo \"{user.UserPublicKey}\" >> /home/{user.Username.ToLower()}/.ssh/authorized_keys'");
    //             var addKeyResult = addKeyCommand.BeginExecute();
    //             await Task.Run(() => addKeyResult.AsyncWaitHandle.WaitOne());
    //             Console.WriteLine($"MachineConfigurator: addKeyCommand text {addKeyCommand.CommandText} .Result {addKeyCommand.Result}");
    //
    //             //Remove password requirement from sudo
    //             var nopasswdCommand = await PerformSSHCommand(client, $"sudo sh -c 'echo \"{user.Username.ToLower()} ALL=(ALL) NOPASSWD:ALL\" > /etc/sudoers.d/{user.Username.ToLower()}'");
    //             Console.WriteLine($"MachineConfigurator: nopasswdCommand text {nopasswdCommand.CommandText} .Result {nopasswdCommand.Result}");
    //         }
    //
    //         //Update
    //         var aptUpdateUpgradeCommand = await PerformSSHCommand(client, $"sudo apt-get update");
    //         Console.WriteLine($"MachineConfigurator: aptUpdateUpgradeCommand.Result {aptUpdateUpgradeCommand.Result}");
    //
    //         //Prep for ppa
    //         var ppaPrepCommand = await PerformSSHCommand(client, $"sudo apt-get install -y software-properties-common");
    //         Console.WriteLine($"MachineConfigurator: ppaPrepCommand.Result {ppaPrepCommand.Result}");
    //
    //         //PPA
    //         foreach (var ppa in machine.Ppa)
    //         {
    //             var ppaAddCommand = await PerformSSHCommand(client, $"sudo add-apt-repository -y {ppa}");
    //             Console.WriteLine($"MachineConfigurator: ppaAddCommand.Result {ppaAddCommand.Result}");
    //         }
    //
    //         //Post ppa update and upgrade
    //         var postPpaUpdateCommand = await PerformSSHCommand(client, $"sudo apt-get update && sudo apt-get upgrade -y");
    //         Console.WriteLine($"MachineConfigurator: postPpaUpdateCommand.Result {postPpaUpdateCommand.Result}");
    //
    //         //Install apts
    //         foreach (var apt in machine.Apt)
    //         {
    //             var aptInstallCommand = await PerformSSHCommand(client, $"sudo apt-get install -y {apt}");
    //             Console.WriteLine($"MachineConfigurator: aptInstallCommand.Result {aptInstallCommand.Result}");
    //         }
    //
    //         //Apt cleanup
    //         SshCommand aptCleanupCommand = await PerformSSHCommand(client, $"sudo apt-get autoremove -y");
    //         Console.WriteLine($"MachineConfigurator: aptCleanupCommand.Result {aptCleanupCommand.Result}");
    //
    //         client.Disconnect();
    //     }
    //     return true; //TODO: Implement error handeling for configuration 
    // }

    public async Task<bool> ConfigureMachineWithFile(Machine machine)
    {
        //Extract information about the users that should be configured on the machine
        List<MachineConfigurationUser> configurationUsers = new();
        var assignments = await _dbContextFactory.GetContext().MachineAssignments
            .Where(assignment => assignment.MachineID == machine.MachineID).ToListAsync();
        Log.Information("Configure Machine:{{{MachineId}}} -" +
                        " Machine assignment count: {AssignmentsCount}", machine.MachineID, assignments.Count);
        foreach (var assignment in assignments)
        {
            Log.Information("Configure Machine:{{{MachineId}}} -" +
                            " Calculating machine assignments", machine.MachineID);
            if (assignment.GroupID == null)
            {
                var context = _dbContextFactory.GetContext();

                if (await context.Users.FindAsync(assignment.UserUsername) == null)
                    context.Users.Add(await UserFactory.Create(assignment.UserUsername));

                await context.SaveChangesAsync();

                MachineConfigurationUser machineConfigurationUser = new()
                {
                    Groups = machine.LinuxGroups,
                    Username = assignment.UserUsername,
                    UserPassword = assignment.OneTimePassword
                };

                var user = await context.Users.FindAsync(assignment.UserUsername);
                machineConfigurationUser.UserPublicKey = user.UserPublicKey;

                configurationUsers.Add(machineConfigurationUser);
            }
            else
            {
                Log.Information("Configure Machine:{{{MachineId}}} -" +
                                " Calculating for group based assignments", machine.MachineID);
                var usersAssignedToGroup = await _dbContextFactory.GetContext().GroupAssignments
                    .Where(ga => ga.GroupID == assignment.GroupID).ToListAsync();
                foreach (var groupAssignment in usersAssignedToGroup)
                {
                    MachineConfigurationUser machineConfigurationUser = new()
                    {
                        Groups = machine.LinuxGroups,
                        Username = groupAssignment.User.Username,
                        UserPassword = assignment.OneTimePassword,
                        UserPublicKey = groupAssignment.User.UserPublicKey
                    };
                    configurationUsers.Add(machineConfigurationUser);
                }
            }
        }

        var builder = new StringBuilder();

        //Update Admin password to prevent lockout by ssh key mishap
        builder.AppendLine($"echo \"admin:{Environment.GetEnvironmentVariable("ADMIN_PASSWD")}\" | sudo chpasswd");

        //Add groups
        foreach (var group in machine.LinuxGroups) builder.AppendLine($"sudo groupadd {group}");

        Log.Verbose("Configure Machine:{{{MachineId}}} - Configuration users count {ConfigurationUsersCount}",
            machine.MachineID, configurationUsers.Count);

        //Add users
        foreach (var user in configurationUsers)
        {
            Log.Verbose("Configure Machine:{{{MachineId}}} -" +
                        " Configuring user {UserUsername}", machine.MachineID, user.Username);

            //User password hash save file
            Log.Verbose("Configure Machine:{{{MachineId}}} - Generating password hash for user: {UserUsername}",
                machine.MachineID, user.Username);
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "openssl";
            p.StartInfo.Arguments =
                $"passwd -6 -salt {StringHelper.RandomString(16)} '{user.UserPassword}'";
            p.Start();
            await p.WaitForExitAsync();
            var userPasswordHash = (await p.StandardOutput.ReadToEndAsync()).TrimEnd();

            Log.Verbose("Configure Machine:{{{MachineId}}} - Generated hash is {GeneratedHash}", machine.MachineID,
                userPasswordHash);

            //Create User with password set
            builder.AppendLine($"useradd -s \"/usr/bin/bash\" -m -p '{userPasswordHash}' " +
                               $"{(user.Groups.Count > 0 ? "-G" + string.Join(",", user.Groups) : "")}" + // Conditionally add the groups 
                               $" {user.Username.ToLower()}");

            //Prep authorized_keys
            builder.AppendLine(
                $"mkdir -p /home/{user.Username.ToLower()}/.ssh &&" +
                $" touch /home/{user.Username.ToLower()}/.ssh/authorized_keys &&" +
                $" chown -R {user.Username.ToLower()}:{user.Username.ToLower()} /home/{user.Username.ToLower()}/.ssh &&" +
                $" chmod -R 700 /home/{user.Username.ToLower()}/.ssh");

            //Add key
            builder.AppendLine(
                $"sudo sh -c 'echo \"{user.UserPublicKey}\" >> /home/{user.Username.ToLower()}/.ssh/authorized_keys'");

            //Remove password requirement from sudo
            builder.AppendLine(
                $"sudo sh -c 'echo \"{user.Username.ToLower()} " +
                $"ALL=(ALL) NOPASSWD:ALL\" > /etc/sudoers.d/{user.Username.ToLower()}'");

            // //Update to correct password 
            // builder.AppendLine($"sudo sh -c 'yes {user.UserPassword} | passwd {user.Username.ToLower()}'");
        }

        //Update
        builder.AppendLine("sudo apt-get update");

        //Prep for ppa
        builder.AppendLine("sudo apt-get install -y software-properties-common");

        //If any ppa have been specified
        if (machine.Ppa.Count > 0)
        {
            //PPA
            foreach (var ppa in machine.Ppa) builder.AppendLine($"sudo add-apt-repository -y {ppa}");

            //Post PPA update
            builder.AppendLine("sudo apt-get update");
        }

        //Install apts
        //Multiple commands are used to ensure that
        //even if any single package does not exists, the others are still installed.
        foreach (var apt in machine.Apt) builder.AppendLine($"sudo apt-get install -y {apt}");

        //Post ppa update and upgrade
        builder.AppendLine("sudo apt-get update && sudo apt-get upgrade -y");

        //Apt cleanup extrenious packages
        builder.AppendLine("sudo apt-get autoremove -y");

        //Save the config file
        await File.WriteAllBytesAsync($"{SERVER_SCALABLE_TEACHING_PATH}/configfile/{machine.HostName}.sh",
            Encoding.UTF8.GetBytes(builder.ToString()));

        //Send the config file to the server
        var p_scp = new Process();
        p_scp.StartInfo.UseShellExecute = false;
        p_scp.StartInfo.RedirectStandardOutput = true;
        p_scp.StartInfo.RedirectStandardError = true;
        p_scp.StartInfo.FileName = "scp";
        p_scp.StartInfo.Arguments = $"-i {SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa -o StrictHostKeyChecking=no " +
                                    $"-B {SERVER_SCALABLE_TEACHING_PATH}/configfile/{machine.HostName}.sh " +
                                    $"admin@{machine.MachineStatus.MachineIp}:/home/admin/configfile.sh";
        p_scp.Start();
        await p_scp.WaitForExitAsync();

        Log.Verbose(
            "Configure Machine:{{{MachineId}}} - Starting ssh: Did scp into {MachineHostName} {MachineStatusMachineIp}, status:\n" +
            "Exit code: {ExitCode} \n" +
            "stdout: {Stdout} \n" +
            "stderr: {Stderr}",
            machine.MachineID,
            machine.HostName,
            machine.MachineStatus.MachineIp,
            p_scp.ExitCode,
            p_scp.StandardOutput.ReadToEnd(),
            p_scp.StandardError.ReadToEnd());

        //Run the command
        Log.Verbose("Configure Machine:{{{MachineId}}} - Starting ssh: {MachineHostName}, {MachineStatusMachineIp}",
            machine.MachineID, machine.HostName, machine.MachineStatus.MachineIp);
        var randomDetectionString = StringHelper.RandomString(10);

        var p_ssh = new Process();
        p_ssh.StartInfo.UseShellExecute = false;
        p_ssh.StartInfo.RedirectStandardOutput = true;
        p_ssh.StartInfo.RedirectStandardError = true;
        p_ssh.StartInfo.FileName = "ssh";
        p_ssh.StartInfo.Arguments =
            $"-o StrictHostKeyChecking=no -i {SERVER_SCALABLE_TEACHING_PATH}/.ssh/id_rsa" +
            $" admin@{machine.MachineStatus.MachineIp} \"sudo chmod 777 /home/admin/configfile.sh;" +
            " sudo sh -c '/home/admin/configfile.sh';" +
//            " sudo rm /home/admin/configfile.sh;" +
            $" touch /home/admin/ranConfig; echo {randomDetectionString}; exit\"";
        p_ssh.Start();

        var time = DateTime.Now;
        while (time + TimeSpan.FromMinutes(5) >= DateTime.Now)
        {
            var output = await p_ssh.StandardOutput.ReadLineAsync();
            var err = await p_ssh.StandardError.ReadLineAsync();

            if (err != null)
                Log.Verbose("Configure Machine:{{{MachineId}}} - RunSSHProcessError - {err}",
                    machine.MachineID, err);

            if (p_scp.HasExited)
                Log.Warning("Configure Machine:{{{MachineId}}} - RunSSHProcess - Process terminated unexpectedly",
                    machine.MachineID);

            if (output != null)
            {
                Log.Verbose("Configure Machine:{{{MachineId}}} - RunSSHProcessOutput - {output}", machine.MachineID,
                    output);
                if (output.Contains(randomDetectionString))
                {
                    p_ssh.Kill();
                    Log.Information(
                        "Configure Machine:{{{MachineId}}} - Finished ssh: {MachineHostName}, {MachineStatusMachineIp}",
                        machine.MachineID,
                        machine.HostName,
                        machine.MachineStatus.MachineIp);
                    return true;
                }
            }
        }

        Log.Warning("Configure Machine:{{{MachineId}}} - RunSSHProcess - Process timed out and was terminated",
            machine.MachineID);
        return false;
    }

    //Commented out due to issues with ssh libraries
    // private static async Task<SshCommand> PerformSSHCommand(SshClient client, string command)
    // {
    //     var _command = client.CreateCommand(command);
    //     var _commandResult = _command.BeginExecute();
    //     await Task.Run(() => _commandResult.AsyncWaitHandle.WaitOne());
    //     return _command;
    // }

    private class MachineConfigurationUser
    {
        public string Username { get; set; }
        public string UserPassword { get; set; }
        public string UserPublicKey { get; set; }
        public List<string> Groups { get; set; }
    }
}
