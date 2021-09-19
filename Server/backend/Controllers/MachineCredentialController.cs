using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Data;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalableTeaching.Controllers
{
    [Authorize(Policy = "UserLevel")]
    [Route("api/[controller]")]
    [ApiController]
    public class MachineCredentialController : ControllerBase
    {
        private readonly VmDeploymentContext _context;
        private readonly SshConfigBuilder _configBuilder;

        public MachineCredentialController(VmDeploymentContext context, SshConfigBuilder configBuilder)
        {
            _context = context;
            _configBuilder = configBuilder;
        }

        /// <summary>
        /// Builds a ssh config string for a single machine 
        /// </summary>
        /// <param name="id">Id of the machine for which the string should be created</param>
        /// <returns>String representation of the config for the specific host</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMachineCredential(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Must be a valid guid");
            var _machine = await _context.Machines.FindAsync(id);
            if (_machine == null) return BadRequest("Machine does not exist");

            if (_machine.UserUsername != GetUsername())// Machine not owned by requesting user
            {
                var _assignments = _machine.MachineAssignments.Where(
                    assignment => assignment.UserUsername == GetUsername());
                if (_assignments.Any())
                    return Ok(new { credentialString = 
                        await _configBuilder.
                        GetMachineCredentialStringAsync(_machine, _assignments.First().UserUsername) });
                else
                    return BadRequest("You are not assigned to this machine");
            }
            else // Machine is owned by requesting user
            {
                return Ok(new { credentialString =
                    await _configBuilder.GetMachineCredentialStringAsync(_machine, GetUsername())});
            }
        }

        /// <summary>
        /// Builds a complete ssh config string 
        /// </summary>
        /// <returns>String representing a ssh config files contents</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllMachineCredentials()
        {
            return Ok(new { credentialString = await CompleteMachineCredentials()});
        }

        /// <summary>
        /// Builds a complete ssh config file based on machine credentials for a machine
        /// </summary>
        /// <returns></returns>
        private async Task<string> CompleteMachineCredentials()
        {
            //Find all owned machines
            List<Machine> machines = await _context.Machines
                .Where(machine => machine.UserUsername == GetUsername()).ToListAsync();
            //Find all directly assigned machines
            var machineAssignments = await _context.MachineAssignments
                .Where(assignment => assignment.UserUsername == GetUsername()).ToListAsync();
            foreach(var assignment in machineAssignments)
            {
                machines.Add(_context.Machines.Find(assignment.MachineID));
            }
            //Find all group assigned machines
            var groupAssignments = await _context.GroupAssignments
                .Where(assignment => assignment.UserUsername == GetUsername()).ToListAsync();
            List<Group> groups = groupAssignments.Select(assignment => assignment.Group).ToList();
            foreach (var group in groups)
            {
                var assignments = await _context.MachineAssignments
                    .Where(assignment => assignment.GroupID == group.GroupID).ToListAsync();
                var groupAssignedMachines = new List<Machine>();
                assignments.ForEach(assignment =>
                {
                    machines.Add(assignment.Machine);
                });
            }
            //Construct string
            var builder = new StringBuilder();
            foreach(Machine machine in machines)
            {
                builder.Append(await _configBuilder
                    .GetMachineCredentialStringAsync(machine, GetUsername())).Append(Environment.NewLine);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Serves the user an updated config file for their ssh configuration 
        /// </summary>
        /// <returns>ssh configuration file as octet-stream</returns>
        [HttpGet("file/sshconfig")]
        public async Task<FileStreamResult> DownloadSshConfigFile()
        {
            return (await SshConfigFile()).ToFileStreamResult();
        }//TODO: Test

        /// <summary>
        /// Rest get endpoint returning the usercredentials and ssh configuration in a zip file
        /// </summary>
        /// <returns>Zipfile Stream</returns>
        [HttpGet("file/zip")]
        public async Task<FileStreamResult> DownloadSshZip()
        {
            return (await GetConfigAndKeysAsZip()).ToFileStreamResult();
        }

        /// <summary>
        /// Builds the complete credentials set for the user that made the request
        /// </summary>
        /// <returns>InMemoryFile with the filename "config" and the
        /// contents matching a ssh config file for the specific user</returns>
        private async Task<InMemoryFile> SshConfigFile()
        {
            return new InMemoryFile("config", Encoding.UTF8.GetBytes(await CompleteMachineCredentials()));
        }

        /// <summary>
        /// Takes the complete config file, public and private key of the user and turns them into files and zips them
        /// </summary>
        /// <returns>Credentials in a zip file</returns>
        private async Task<InMemoryFile> GetConfigAndKeysAsZip()
        {
            var userKey =
                SSHKeyHelper.ParseKeyFromPem((await _context.Users.FirstAsync(user => user.Username == GetUsername()))
                    .UserPrivateKey);
            var files = new List<InMemoryFile>
            {
                await SshConfigFile(),
                new InMemoryFile("id_rsa_scalable_" + GetUsername() + ".pub",
                    Encoding.UTF8.GetBytes(SSHKeyHelper.GetSSHPublicKey(userKey, GetUsername()))),
                new InMemoryFile("id_rsa_scalable_" + GetUsername(),
                Encoding.UTF8.GetBytes(_context.Users.Find(GetUsername()).UserPrivateKey))
            };
            return await ZipBuilder.BuildZip(files, GetUsername() + "_ScalableTeachingUserCredentials.zip");
        }
                                                                                                                       



        private async Task<bool> IsAssignedToMachine(string username, Machine machine)
        {
            return await Task<bool>.Run(() =>
            {
                if (machine == null) return false;
                if (username.Length == 0 || username == null) return false;
                if (machine.UserUsername == username) return true;
                if (machine.MachineAssignments
                .Where(assignement => assignement.UserUsername == username).Any()) return true;
                return false;
            });

        }

        /// <summary>
        /// Gives the value of the username claim for the current httpcontext
        /// </summary>
        /// <returns>Username of logged in user</returns>
        private string GetUsername()
        {
            return HttpContext.User.Claims.First(claim => claim.Type == "username").Value.ToLower();
        }
    }
}
