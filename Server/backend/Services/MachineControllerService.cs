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

        private bool _StatusIsGoing = false;
        private bool _CreationQueueingIsGoing = false;
        private bool _CreatedIsGoing = false;
        private bool _DeletionIsGoing = false;

        public MachineControllerService(IOpenNebulaAccessor accessor, MachineConfigurator machineConfigurator, IServiceScopeFactory factory)
        {
            _factory = factory;
            _accessor = accessor;
            _machineConfigurator = machineConfigurator;
        }
        public Task StartAsync(CancellationToken cancellationToken) //TODO: Return Timings to primes
        {

            //Reasonable times
            //_CreationQueueingTimer = new(CreationQueueingTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(3));
            //_CreatedTimer = new(CreatedTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(5));
            //_StatusTimer = new(StatusTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(2));
            //_DeletionTimer = new(DeletionTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromDays(7));

            //Quick times
            _CreationQueueingTimer = new(CreationQueueingTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromSeconds(23));
            _CreatedTimer = new(CreatedTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromSeconds(29));
            _StatusTimer = new(StatusTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromSeconds(11));
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
            _StatusTimer?.Dispose();
        }

        /// <summary>
        /// Deletes machines scheduled for deletion if they have passed the deletion threshold
        /// </summary>
        /// <param name="state">unused parameter</param>
        private void DeletionTimerCallback(object state)
        {
            if (_DeletionIsGoing) return;
            try
            {
                _DeletionIsGoing = true;
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
            finally
            {
                _DeletionIsGoing = false;
            }
        }

        /// <summary>
        /// Takes newly created machines from the database and schedules them for creation with the open nebula internal scheduler.
        /// </summary>
        /// <param name="state"></param>
        private async void CreationQueueingTimerCallback(object state)
        {
            if (_CreationQueueingIsGoing) return;
            try
            {
                _CreationQueueingIsGoing = true;
                var _context = GetContext();
                var RegisteredMachines = _context.Machines.Where(machine => machine.MachineCreationStatus == CreationStatus.REGISTERED).ToList();
                Console.WriteLine($"MachineControllerService.CreationQueueingTimerCallback:Machines to be Scheduled for creation: {String.Join(",\n", RegisteredMachines)}");
                RegisteredMachines.ForEach(machine =>
                {
                    machine.MachineCreationStatus = CreationStatus.QUEUED_FOR_CREATION;
                    var CreationResult = _accessor.CreateVirtualMachine(int.Parse(Environment.GetEnvironmentVariable("OpenNebulaDefaultTemplate")), machine.HostName);
                    machine.OpenNebulaID = CreationResult.Item2;
                });
                _context.Machines.UpdateRange(RegisteredMachines);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _CreationQueueingIsGoing = false;
            }
            return;
        }

        /// <summary>
        /// Takes machines that have the creation status of <see cref="CreationStatus.QUEUED_FOR_CREATION"/> and status of active and configures them and sets them to configured
        /// </summary>
        /// <param name="state"></param>
        private async void CreatedTimerCallback(object state)
        {
            if (_CreatedIsGoing) return;
            try
            {
                _CreatedIsGoing = true;
                VmDeploymentContext _context = GetContext();
                var PollTime = DateTimeOffset.UtcNow;
                var machines = _context.Machines.Where(machine => machine.MachineCreationStatus == CreationStatus.QUEUED_FOR_CREATION).ToList();
                foreach (Machine machine in machines)
                {
                    Console.WriteLine($"MachineControllerService.CreatedTimerCallback:Machines Scheduled for creation: {String.Join(", ", _creationQueue)}");
                    if (machine.MachineStatus?.MachineState == MachineStates.ACTIVE)
                    {
                        Console.WriteLine($"MachineControllerService.CreatedTimerCallback: Machine Booted after creation: { machine.MachineID}");
                        try
                        {
                            await _machineConfigurator.ConfigureMachine(machine);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error occurred configuring machine: {machine.HostName}, {machine.MachineID}");
                            Console.WriteLine($"Error: {e.Message}");
                            Console.WriteLine(e.StackTrace);
                            continue;
                        }
                        machine.MachineCreationStatus = CreationStatus.CONFIGURED;
                        _context.Machines.Update(machine);
                        _creationQueue.Remove(machine.MachineID);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            finally
            {
                _CreatedIsGoing = false;
            }
        }

        private async void StatusTimerCallback(object state)
        {
            if (_StatusIsGoing) return;
            try
            {
                _StatusIsGoing = true;
                Console.WriteLine($"MachineControllerService.StatusTimerCallback: Callback Time: {DateTimeOffset.Now}");
                var _context = GetContext();
                var PollTime = DateTimeOffset.UtcNow;
                List<VmModel> vmModels = _accessor.GetAllVirtualMachineInfo(false, -3);
                Dictionary<int, VmModel> MachineStatusMap = vmModels.ToDictionary(machine => { return machine.MachineId; });
                var ValidMachineIDs = MachineStatusMap.Keys.ToList();
                var machines = _context.Machines.Where(machine => ValidMachineIDs.Contains((int)machine.OpenNebulaID)).ToList();
                Console.WriteLine($"MachineControllerService.StatusTimerCallback: ON IDs {String.Join(", ", ValidMachineIDs)}");
                foreach (var machine in machines)
                {
                    if ((await _context.MachineStatuses.FindAsync(machine.MachineID)) == null)
                    {
                        _context.MachineStatuses.Add(MachineStatus.MachineStatusFactory(machine.MachineID, MachineStatusMap.GetValueOrDefault((int)machine.OpenNebulaID), PollTime));
                    }
                    else
                    {
                        var status = await _context.MachineStatuses.FindAsync(machine.MachineID);
                        _context.MachineStatuses.Update(status.Update(MachineStatus.MachineStatusFactory(machine.MachineID, MachineStatusMap.GetValueOrDefault((int)machine.OpenNebulaID), PollTime)));
                    }
                    await _context.SaveChangesAsync();
                }
            }
            finally
            {
                _StatusIsGoing = false;
            }
        }
        private VmDeploymentContext GetContext()
        {
            return _factory.CreateScope().ServiceProvider.GetRequiredService<VmDeploymentContext>();
        }
    }
}
