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

        public MachineControllerService(IOpenNebulaAccessor accessor, MachineConfigurator machineConfigurator,
            IDbContextFactory factory)
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
            _DeletionTimer = new(DeletionTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(2));
            _CourseDeletionTimer = new(CourseDeletionTimerCallback, null, -TimeSpan.Zero, TimeSpan.FromMinutes(1));

            Log.Information("MachineControllerService - Machine Controller Service Started");
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
                foreach (var request in requests)
                {
                    var subcontext = _factory.GetContext();
                    Log.Verbose(
                        "MachineControllerService - Machine Deletion - {RequestMachineId}: Checking Deletion Request",
                        request.MachineID);
                    if (DateTime.UtcNow.ToUniversalTime().CompareTo(request.DeletionDate.ToUniversalTime()) <= 0)
                        return;
                    Log.Verbose(
                        "MachineControllerService - Machine Deletion - {RequestMachineId}: Machine has passed the deletion threshold",
                        request.MachineID);
                    var machine =
                        await subcontext.Machines.FirstOrDefaultAsync(m => m.MachineID == request.MachineID);
                    if (machine == null)
                    {
                        Log.Error(
                            "MachineControllerService - Machine Deletion - {RequestMachineId}: has no machine associated with it",
                            request.MachineID);
                    }
                    else
                        switch (machine.OpenNebulaID)
                        {
                            case null:
                                Log.Error("MachineControllerService - Machine Deletion - {RequestMachineId}: " +
                                          "Request has no OpenNebula ID associated with it i.e. 0", request.MachineID);
                                break;
                            case 0:
                                Log.Error("MachineControllerService - Machine Deletion - {RequestMachineId}: " +
                                          "Request has no OpenNebula ID associated with it i.e. 0", request.MachineID);
                                break;
                        }

                    if (!_accessor.PerformVirtualMachineAction(MachineActions.TERMINATE_HARD,
                            (int) machine.OpenNebulaID))
                    {
                        Log.Error(
                            "MachineControllerService - Machine Deletion - {RequestMachineId}: Error while deleting machine machine_id:{RequestMachineId} OpenNebula_id:{OpennebulaId} ",
                            machine.MachineID.ToString(), machine.OpenNebulaID.ToString());
                    }

                    Log.Information(
                        "MachineControllerService - Machine Deletion - {RequestMachineId}: Machine and associated request has been deleted",
                        request.MachineID);
                    subcontext.MachineDeletionRequests.Remove(request);
                    subcontext.Machines.Remove(machine);
                    await subcontext.SaveChangesAsync();
                }

                ;
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
                Log.Verbose("MachineControllerService - Course Deletion: Testing for courses scheduled for deletion");
                _CourseDeletionIsGoing = true;
                var context = _factory.GetContext();
                var courses = await context.Courses.Where(c => c.Active == false).ToListAsync();
                Log.Verbose("MachineControllerService - Course Deletion: Found {Count} courses scheduled for deletion",
                    courses.Count);
                foreach (var course in courses)
                {
                    //If any machines are still associated with the course then skip this course.
                    if (await context.Machines.AnyAsync(m => m.CourseID == course.CourseID))
                    {
                        Log.Verbose(
                            "MachineControllerService - Course Deletion - {CourseId}: Not all machines associated machines have been deleted. Skipping deletion for course: {CourseLongName}",
                            course.CourseID, course.CourseName);
                        return;
                    }

                    //Delete the course
                    context.Courses.Remove(course);
                    Log.Information(
                        "MachineControllerService - Course Deletion - {CourseId}: No machines associated with course. Deleting course: {CourseLongName}",
                        course.CourseID, course.CourseName);
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
                    Log.Information("MachineControllerService - Machine Creation Queuer: Machines to be" +
                                    " Scheduled for creation: {Machines}", string.Join(",", registeredMachines));
                }

                registeredMachines.ForEach(machine =>
                {
                    machine.MachineCreationStatus = CreationStatus.QUEUED_FOR_CREATION;
                    var creationResult = _accessor.CreateVirtualMachine(
                        int.Parse(
                            Environment.GetEnvironmentVariable("OpenNebulaDefaultTemplate") ?? "/ScalableTeaching"),
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

                var machines = await context.Machines
                    .Where(machine => machine.MachineCreationStatus == CreationStatus.QUEUED_FOR_CREATION)
                    .ToListAsync();

                foreach (var machine in machines)
                {
                    var subcontext = _factory.GetContext();
                    if (machine.MachineStatus?.MachineState == MachineStates.ACTIVE)
                    {
                        Log.Information(
                            "MachineControllerService - Machine Creation Configuration - {MachineId}: Machine Booted after creation",
                            machine.MachineID);
                        try
                        {
                            if (machine.MachineStatus?.MachineIp == null)
                            {
                                Log.Warning(
                                    "MachineControllerService - Machine Creation Configuration - {MachineId}: No ip assigned",
                                    machine.MachineID);
                                return;
                            }

                            if (await _machineConfigurator.ConfigureMachineWithFile(machine))
                            {
                                throw new Exception("Something went wrong while configuring machine");
                            }
                            else
                            {
                                //Keeping this despite being redundant to prevent future issues that could be introduced with changes to the error handling of misconfigured machines
                                Log.Information(
                                    "MachineControllerService - Machine Creation Configuration - {MachineId}: Machine Configured Successfully",
                                    machine.MachineID);
                            }


                            machine.MachineCreationStatus = CreationStatus.CONFIGURED;
                            subcontext.Machines.Update(machine);
                            await subcontext.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            Log.Error(e,
                                "MachineControllerService - Machine Creation Configuration - {MachineId}: Error occurred configuring machine",
                                machine.MachineID);
                        }
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
                var context = _factory.GetContext();
                var pollTime = DateTimeOffset.UtcNow;
                List<VmModel> vmModels = _accessor.GetAllVirtualMachineInfo(false, -3);
                var validMachineIDs = vmModels.AsEnumerable().Select(model => model.MachineId);
                Dictionary<int, VmModel> machineStatusMap = new();
                foreach (var id in validMachineIDs)
                {
                    machineStatusMap.Add(id, _accessor.GetVirtualMachineInformation(id));
                }

                var machines = await context.Machines
                    .Where(machine => validMachineIDs.Contains((int) machine.OpenNebulaID)).ToListAsync();
                foreach (var machine in machines)
                {
                    if ((await context.MachineStatuses.FindAsync(machine.MachineID)) == null)
                    {
                        context.MachineStatuses.Add(MachineStatus.MachineStatusFactory(machine.MachineID,
                            machineStatusMap.GetValueOrDefault((int) machine.OpenNebulaID), pollTime));
                    }
                    else
                    {
                        var status = await context.MachineStatuses.FindAsync(machine.MachineID);
                        context.MachineStatuses.Update(status.Update(MachineStatus.MachineStatusFactory(
                            machine.MachineID, machineStatusMap.GetValueOrDefault((int) machine.OpenNebulaID),
                            pollTime)));
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
