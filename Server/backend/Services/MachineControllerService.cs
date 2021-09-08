using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScalableTeaching.Data;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using ScalableTeaching.OpenNebula;
using ScalableTeaching.OpenNebula.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScalableTeaching.Services
{
    public class MachineControllerService : IHostedService, IDisposable
    {
        private readonly IOpenNebulaAccessor _accessor;
        private readonly MachineConfigurator _machineConfigurator;
        private readonly IServiceScopeFactory _factory;

        private readonly List<Guid> _creationQueue = new();
        private readonly List<(Machine, DateTimeOffset)> _deletionQueue = new();


        private Timer _CreationQueueingTimer;
        private Timer _CreatedTimer;
        private Timer _DeletionTimer;
        private Timer _StatusTimer;

        public MachineControllerService(IOpenNebulaAccessor accessor, MachineConfigurator machineConfigurator, IServiceScopeFactory factory)
        {
            _factory = factory;
            _accessor = accessor;
            _machineConfigurator = machineConfigurator;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _CreationQueueingTimer = new(CreationQueueingTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(2));
            _CreatedTimer = new(CreatedTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(5));
            _StatusTimer = new(StatusTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _DeletionTimer = new(DeletionTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        public DateTimeOffset ScheduleForDeletion(Machine machine)
        {
            var deletionTime = DateTimeOffset.UtcNow.AddDays(30);
            _deletionQueue.Add((machine, deletionTime));
            return deletionTime;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            _CreationQueueingTimer?.Dispose();
            _CreatedTimer?.Dispose();
            _DeletionTimer?.Dispose();
        }

        private void DeletionTimerCallback(object state)
        {
            var _context = GetContext();
            _deletionQueue.ForEach(machine =>
            {
                Console.WriteLine($"MachineControllerService.DeletionTimerCallback:Machines Scheduled for creation: {String.Join(", ", _creationQueue)}");
                if (machine.Item2.UtcDateTime < DateTime.UtcNow)
                {
                    if (_accessor.PerformVirtualMachineAction(MachineActions.TERMINATE_HARD, (int)machine.Item1.OpenNebulaID))
                    {
                        _context.Machines.Remove(machine.Item1);
                        _context.SaveChangesAsync();
                    }
                }
            });
        }

        private async void CreationQueueingTimerCallback(object state)
        {   
            var _context = GetContext();
            var RegisteredMachines = _context.Machines.Where(machine => machine.MachineCreationStatus == CreationStatus.REGISTERED).ToList();
            Console.WriteLine($"MachineControllerService.CreationQueueingTimerCallback:Machines to be Scheduled for creation: {String.Join(",\n", RegisteredMachines)}");
            RegisteredMachines.ForEach(machine =>
            {
                machine.MachineCreationStatus = CreationStatus.QUEUED_FOR_CREATION;
                var CreationResult = _accessor.CreateVirtualMachine(int.Parse(Environment.GetEnvironmentVariable("OpenNebulaDefaultTemplate")), machine.HostName);
                machine.OpenNebulaID = CreationResult.Item2;
                _creationQueue.Add(machine.MachineID);
            });
            _context.Machines.UpdateRange(RegisteredMachines);
            await _context.SaveChangesAsync();
            return;
        }

        private async void CreatedTimerCallback(object state)
        {
            var _context = GetContext();
            var PollTime = DateTimeOffset.UtcNow;
            var machines = _context.Machines.Where(machine => _creationQueue.Contains(machine.MachineID));
            foreach (Machine machine in machines)
            {
                Console.WriteLine($"MachineControllerService.CreatedTimerCallback:Machines Scheduled for creation: {String.Join(", ", _creationQueue)}");
                if (machine.MachineStatus.MachineState == MachineStates.ACTIVE)
                {
                    Console.WriteLine($"MachineControllerService.CreatedTimerCallback: Machine Booted after creation: { machine.MachineID}");
                    await _machineConfigurator.ConfigureMachine(machine);
                    machine.MachineCreationStatus = CreationStatus.CONFIGURED;
                    _context.Machines.Update(machine);
                    _creationQueue.Remove(machine.MachineID);
                }
                await _context.SaveChangesAsync();
            }
        }

        private async void StatusTimerCallback(object state)
        {
            Console.WriteLine($"MachineControllerService.StatusTimerCallback: Callback Time: {DateTimeOffset.Now}");
            var _context = GetContext();
            var PollTime = DateTimeOffset.UtcNow;

            Dictionary<int, VmModel> MachineStatusMap = _accessor.GetAllVirtualMachineInfo(false, -2).ToDictionary(machine => { return machine.MachineId; });
            var ValidMachineIDs = MachineStatusMap.Keys.ToList();
            var machines = _context.Machines.Where(machine => ValidMachineIDs.Contains((int)machine.OpenNebulaID)).ToList();
            Console.WriteLine($"MachineControllerService.StatusTimerCallback: ON IDs {String.Join(", ", ValidMachineIDs)}");
            foreach(var machine in machines)
            {
                _context.MachineStatuses.Update(MachineStatus.MachineStatusFactory(machine.MachineID, MachineStatusMap.GetValueOrDefault((int)machine.OpenNebulaID),PollTime));
            }
            await _context.SaveChangesAsync();
        }

        private VmDeploymentContext GetContext()
        {
            return _factory.CreateScope().ServiceProvider.GetRequiredService<VmDeploymentContext>();
        }
    }
}
