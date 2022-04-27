using Microsoft.EntityFrameworkCore;
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

            Console.WriteLine("Machine Controller Service Started");
            return Task.CompletedTask;
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
        private async void DeletionTimerCallback(object state)
        {
            Console.WriteLine("Entering deletion debugging");
            if (_DeletionIsGoing) return;
            Console.WriteLine("deletion debugging: Trying to go through workflow");
            try
            {
                _DeletionIsGoing = true;
                var context = GetContext();
                
                (await context.MachineDeletionRequests.ToListAsync()).ForEach(async request =>
                {
                    var subcontext = GetContext();
                    Console.WriteLine($"Checking Deletion Request: {request.MachineID}");
                    if (request.DeletionDate.ToUniversalTime() >= DateTime.UtcNow) return;
                    Console.WriteLine($"Deletion Request: {request.MachineID} has passed the deletion threshold");
                    var machine = await subcontext.Machines.FirstOrDefaultAsync(m => m.MachineID == request.MachineID);
                    if (machine == null)
                    {
                        Console.WriteLine($"Deletion Request: {request.MachineID} has no machine associated with it");
                    }
                    else switch (machine.OpenNebulaID)
                    {
                        case null:
                            Console.WriteLine( $"Deletion Request: {request.MachineID} has no OpenNebula ID associated with it");
                            break;
                        case 0:
                            Console.WriteLine($"Deletion Request: {request.MachineID} has no OpenNebula ID associated with it ie 0");
                            break;
                    }

                    if (!_accessor.PerformVirtualMachineAction(MachineActions.TERMINATE_HARD,
                            (int)machine.OpenNebulaID)) return;
                    Console.WriteLine($"Deletion Request: {request.MachineID} has been deleted");
                    subcontext.MachineDeletionRequests.Remove(request);
                    subcontext.Machines.Remove(machine);
                    await subcontext.SaveChangesAsync();
                });
                _DeletionIsGoing = false;
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
                var RegisteredMachines = await _context.Machines
                    .Where(machine => machine.MachineCreationStatus == CreationStatus.REGISTERED).ToListAsync();
                if(RegisteredMachines.Count != 0) 
                    Console.WriteLine($"MachineControllerService.CreationQueueingTimerCallback:Machines to be" +
                        $" Scheduled for creation: {String.Join(",\n", RegisteredMachines)}");
                RegisteredMachines.ForEach(machine =>
                {
                    machine.MachineCreationStatus = CreationStatus.QUEUED_FOR_CREATION;
                    var CreationResult = _accessor.CreateVirtualMachine(
                        int.Parse(Environment.GetEnvironmentVariable("OpenNebulaDefaultTemplate")),
                        machine.HostName,
                        machine.Memory,
                        machine.VCPU,
                        machine.Storage
                        );
                    machine.OpenNebulaID = CreationResult.Item2;
                });
                _context.Machines.UpdateRange(RegisteredMachines);
                await _context.SaveChangesAsync();
                _CreationQueueingIsGoing = false;
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
                var machines = await _context.Machines.Where(machine => machine.MachineCreationStatus == CreationStatus.QUEUED_FOR_CREATION).ToListAsync();
                foreach (Machine machine in machines)
                {
                    if (machine.MachineStatus?.MachineState == MachineStates.ACTIVE)
                    {
                        Console.WriteLine($"MachineControllerService.CreatedTimerCallback: Machine Booted after creation: { machine.MachineID}");
                        try
                        {
                            if (machine.MachineStatus?.MachineIp == null)
                            {
                                Console.WriteLine($"Error configuring machine: no ip");
                                continue;
                            }
                            await _machineConfigurator.ConfigureMachineWithFile(machine);//TODO: Return to using ssh based configuration
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
                        await _context.SaveChangesAsync();
                    }
                }
                _CreatedIsGoing = false;
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
                //Console.WriteLine($"MachineControllerService.StatusTimerCallback: Callback Time: {DateTimeOffset.Now}");
                var _context = GetContext();
                var PollTime = DateTimeOffset.UtcNow;
                List<VmModel> vmModels = _accessor.GetAllVirtualMachineInfo(false, -3);
                var ValidMachineIDs = vmModels.AsEnumerable().Select(model => model.MachineId);
                Dictionary<int, VmModel> MachineStatusMap = new();
                foreach (var id in ValidMachineIDs)
                {
                    MachineStatusMap.Add(id, _accessor.GetVirtualMachineInformation(id));
                }
                //Dictionary<int, VmModel> MachineStatusMap = vmModels.ToDictionary(machine => { return machine.MachineId; });
                var machines = await _context.Machines.Where(machine => ValidMachineIDs.Contains((int)machine.OpenNebulaID)).ToListAsync();
                //Console.WriteLine($"MachineControllerService.StatusTimerCallback: ON IDs {String.Join(", ", ValidMachineIDs)}");
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
                _StatusIsGoing = false;
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
