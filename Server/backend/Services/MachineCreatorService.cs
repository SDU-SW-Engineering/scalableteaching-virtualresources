using Microsoft.Extensions.Hosting;
using ScalableTeaching.Data;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using ScalableTeaching.OpenNebula;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScalableTeaching.Services
{
    public class MachineCreatorService : IHostedService, IDisposable
    {
        private readonly VmDeploymentContext _context;
        private readonly IOpenNebulaAccessor _accessor;

        private readonly Queue<Machine> creationQueue;
        private readonly Queue<Machine> configurationQueue;


        private Timer _CreationTimer;
        private Timer _ConfigurationTimer;

        public MachineCreatorService(VmDeploymentContext context, IOpenNebulaAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _CreationTimer = new(CreationTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(2));
            _ConfigurationTimer = new(ConfigurationTimerCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(11));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            _CreationTimer?.Dispose();
        }

        private async void CreationTimerCallback(object state)
        {
            var RegisteredMachines = _context.Machines.Where(machine => machine.MachineCreationStatus == CreationStatus.REGISTERED).ToList();
            RegisteredMachines.ForEach(machine =>
            {
                machine.MachineCreationStatus = CreationStatus.QUEUED_FOR_CREATION;
            });
            _context.Machines.UpdateRange(RegisteredMachines);
            await _context.SaveChangesAsync();
            RegisteredMachines.ForEach(async machine => {
                _context.Machines.Update(machine);
                var CreationResult = _accessor.CreateVirtualMachine(int.Parse(Environment.GetEnvironmentVariable("OpenNebulaDefaultTemplate")),machine.HostName);
                await _context.SaveChangesAsync();
                creationQueue.Enqueue(machine);
            });
            return;
        }

        private void ConfigurationTimerCallback(object state)
        {

        }

    }
}
