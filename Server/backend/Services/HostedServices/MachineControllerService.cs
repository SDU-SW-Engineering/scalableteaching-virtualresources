#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScalableTeaching.Data;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using ScalableTeaching.OpenNebula;
using ScalableTeaching.OpenNebula.Models;
using Serilog;

namespace ScalableTeaching.Services.HostedServices
{
    public class MachineControllerService : IHostedService, IDisposable
    {
        private readonly IOpenNebulaAccessor _accessor;
        private readonly MachineConfigurator _machineConfigurator;
        private readonly IDbContextFactory _factory;

        private Timer _CreationQueueingTimer;
        private Timer _CreatedTimer;
        private Timer _DeletionTimer;
        private Timer _StatusTimer;
        private Timer _CourseDeletionTimer;

        
        //I am aware that i should use locks or semaphores. However i had issues and as such settled on this.
        private bool _StatusIsGoing = false;
        private bool _CreationQueueingIsGoing = false;
        private bool _CreatedIsGoing = false;
        private bool _DeletionIsGoing = false;
        private bool _CourseDeletionIsGoing = false;

        public MachineControllerService(IOpenNebulaAccessor accessor, MachineConfigurator machineConfigurator, IDbContextFactory factory)
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

            //Quick times
            _CreationQueueingTimer = new(CreationQueueingTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _CreatedTimer = new(CreatedTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _StatusTimer = new(StatusTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _DeletionTimer = new(DeletionTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromDays(1));
            _CourseDeletionTimer = new(CourseDeletionTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromHours(12));

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
            _CourseDeletionTimer?.Dispose();
        }

        /// <summary>
        /// Deletes machines scheduled for deletion if they have passed the deletion threshold
        /// </summary>
        /// <param name="state">unused parameter</param>
        private async void DeletionTimerCallback(object? state)
        {
            if (_DeletionIsGoing) return;
            try
            {
                _DeletionIsGoing = true;
                var context = _factory.GetContext();

                
                var requests = await context.MachineDeletionRequests.ToListAsync();
                foreach(var request in requests)
                {
                    var subcontext = _factory.GetContext();
                    Console.WriteLine($"Checking Deletion Request: {request.MachineID}");
                    Log.Information("Checking Deletion Request: {id}", request.MachineID);
                    if (DateTime.UtcNow.ToUniversalTime().CompareTo(request.DeletionDate.ToUniversalTime()) <= 0)
                        return;
                    Console.WriteLine($"Deletion Request: {request.MachineID} has passed the deletion threshold");
                    Log.Information("Deletion Request: {id} has passed the deletion threshold. Deletion will progress", request.MachineID);
                    var machine = 
                        await subcontext.Machines.FirstOrDefaultAsync(m => m.MachineID == request.MachineID);
                    if (machine == null)
                    {
                        Console.WriteLine($"Deletion Request: {request.MachineID} has no machine associated with it");
                        Log.Error("Deletion Request: {id} has no machine associated with it", request.MachineID);
                    }
                    else switch (machine.OpenNebulaID)
                    {
                        case null:
                            Console.WriteLine($"Deletion Requests machine: {request.MachineID} " +
                                              $"has no OpenNebula ID associated with it");
                            Log.Error("Deletion Request: {machineid}" +
                                      "has no OpenNebula ID associated with it i.e. 0", request.MachineID);
                            break;
                        case 0:
                            Console.WriteLine($"Deletion Requests machine: {request.MachineID} " +
                                              $"has no OpenNebula ID associated with it ie 0");
                            Log.Error("Deletion Request: {machineid}" +
                                      "has no OpenNebula ID associated with it i.e. 0", request.MachineID);
                            break;
                    }

                    if (!_accessor.PerformVirtualMachineAction(MachineActions.TERMINATE_HARD,
                            (int)machine.OpenNebulaID))
                    {
                        Log.Error("MachineControllerService - Error while deleting machine machine_id:{machineid} OpenNebula_id:{opennebula_id} ", machine.MachineID.ToString(), machine.OpenNebulaID.ToString());
                        Console.Error.WriteLineAsync("There was an error in terminating a machine. See log for more information.");
                    }
                    
                    Console.WriteLine($"Machine id: {request.MachineID}, and associated request has been deleted");
                    Log.Information("Machine id: {id}, and associated request has been deleted", request.MachineID);
                    subcontext.MachineDeletionRequests.Remove(request);
                    subcontext.Machines.Remove(machine);
                    await subcontext.SaveChangesAsync();
                };
                CourseDeletionTimerCallback(null);
                _DeletionIsGoing = false;
            }
            finally
            {
                _DeletionIsGoing = false;
            }
        }

        private async void CourseDeletionTimerCallback(object? state)
        {
            if (_CourseDeletionIsGoing) return;
            try
            {
                Log.Information("Testing for courses scheduled for deletion");
                _CourseDeletionIsGoing = true;
                var context = _factory.GetContext();
                var courses = await context.Courses.Where(c => c.Active == false).ToListAsync();
                Log.Information("Found {count} courses scheduled for deletion.", courses.Count);
                foreach (var course in courses)
                {
                    //If any machines are still associated with the course then skip this course.
                    if (await context.Machines.AnyAsync(m => m.CourseID == course.CourseID))
                    {
                        Log.Information($"Not all machines associated machines have been deleted. Skipping deletion for course: {course.CourseID}.");
                        Console.Out.WriteLineAsync($"Not all machines associated machines have been deleted. Skipping deletion for course: {course.CourseID}.");
                        return;
                    }
                    //Delete the course
                    context.Courses.Remove(course);
                }
                await context.SaveChangesAsync();
                _CourseDeletionIsGoing = false;
            }
            finally
            {
                _CourseDeletionIsGoing = false;
            }
        }

        /// <summary>
        /// Takes newly created machines from the database and schedules
        /// them for creation with the open nebula internal scheduler.
        /// </summary>
        private async void CreationQueueingTimerCallback(object? state)
        {
            if (_CreationQueueingIsGoing) return;
            try
            {
                _CreationQueueingIsGoing = true;
                var context = _factory.GetContext();
                var registeredMachines = await context.Machines
                    .Where(machine => machine.MachineCreationStatus == CreationStatus.REGISTERED).ToListAsync();
                if (registeredMachines.Count != 0)
                {
                    Console.WriteLine($"MachineControllerService.CreationQueueingTimerCallback:Machines to be" +
                                      $" Scheduled for creation: {String.Join(",\n", registeredMachines)}");
                    Log.Information("MachineControllerService.CreationQueueingTimerCallback:Machines to be" +
                    " Scheduled for creation: {machines}", String.Join(",\n", registeredMachines));
                }

                registeredMachines.ForEach(machine =>
                {
                    machine.MachineCreationStatus = CreationStatus.QUEUED_FOR_CREATION;
                    var creationResult = _accessor.CreateVirtualMachine(
                        int.Parse(Environment.GetEnvironmentVariable("OpenNebulaDefaultTemplate") ?? "/ScalableTeaching"),
                        machine.HostName,
                        machine.Memory,
                        machine.VCPU,
                        machine.Storage
                        );
                    machine.OpenNebulaID = creationResult.Item2;
                });
                context.Machines.UpdateRange(registeredMachines);
                await context.SaveChangesAsync();
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
        private async void CreatedTimerCallback(object? state)
        {
            if (_CreatedIsGoing) return;
            try
            {
                _CreatedIsGoing = true;
                VmDeploymentContext context = _factory.GetContext();
                var machines = await context.Machines.Where(machine => machine.MachineCreationStatus == CreationStatus.QUEUED_FOR_CREATION).ToListAsync();
                foreach (Machine machine in machines)
                {
                    if (machine.MachineStatus?.MachineState == MachineStates.ACTIVE)
                    {
                        Console.WriteLine($"MachineControllerService.CreatedTimerCallback: Machine Booted after creation: { machine.MachineID}");
                        Log.Information("MachineControllerService.CreatedTimerCallback: Machine Booted after creation: {id}", machine.MachineID);
                        try
                        {
                            if (machine.MachineStatus?.MachineIp == null)
                            {
                                Console.WriteLine($"Error configuring machine: no ip");
                                Log.Warning("Error configuring machine: {mid}. No ip assigned", machine.MachineID);
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
                        context.Machines.Update(machine);
                        await context.SaveChangesAsync();
                    }
                }
                _CreatedIsGoing = false;
            }
            finally
            {
                _CreatedIsGoing = false;
            }
        }

        private async void StatusTimerCallback(object? state)
        {
            if (_StatusIsGoing) return;
            try
            {
                _StatusIsGoing = true;
                //Console.WriteLine($"MachineControllerService.StatusTimerCallback: Callback Time: {DateTimeOffset.Now}");
                var context = _factory.GetContext();
                var pollTime = DateTimeOffset.UtcNow;
                List<VmModel> vmModels = _accessor.GetAllVirtualMachineInfo(false, -3);
                var validMachineIDs = vmModels.AsEnumerable().Select(model => model.MachineId);
                Dictionary<int, VmModel> machineStatusMap = new();
                foreach (var id in validMachineIDs)
                {
                    machineStatusMap.Add(id, _accessor.GetVirtualMachineInformation(id));
                }
                //Dictionary<int, VmModel> MachineStatusMap = vmModels.ToDictionary(machine => { return machine.MachineId; });
                var machines = await context.Machines.Where(machine => validMachineIDs.Contains((int)machine.OpenNebulaID)).ToListAsync();
                //Console.WriteLine($"MachineControllerService.StatusTimerCallback: ON IDs {String.Join(", ", ValidMachineIDs)}");
                foreach (var machine in machines)
                {
                    if ((await context.MachineStatuses.FindAsync(machine.MachineID)) == null)
                    {
                        context.MachineStatuses.Add(MachineStatus.MachineStatusFactory(machine.MachineID, machineStatusMap.GetValueOrDefault((int)machine.OpenNebulaID), pollTime));
                    }
                    else
                    {
                        var status = await context.MachineStatuses.FindAsync(machine.MachineID);
                        context.MachineStatuses.Update(status.Update(MachineStatus.MachineStatusFactory(machine.MachineID, machineStatusMap.GetValueOrDefault((int)machine.OpenNebulaID), pollTime)));
                    }
                    await context.SaveChangesAsync();
                }
                _StatusIsGoing = false;
            }
            finally
            {
                _StatusIsGoing = false;
            }
        }
    }
}
