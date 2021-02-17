using backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Data
{
    public class VmDeploymentContext : DbContext
    {
        public VmDeploymentContext(DbContextOptions<VmDeploymentContext> options) : base(options) { }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<MachineCredentail> MachineCredentails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Port> Ports { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupAssignment> GroupAssignments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Port>().HasKey(c => new { c.MachineID, c.PortNumber });
            modelBuilder.Entity<MachineCredentail>().HasKey(c => new { c.MachineID, c.UserID });
            modelBuilder.Entity<GroupAssignment>().HasKey(c => new { c.GroupID, c.UserID });
            //modelBuilder.Entity<User>().HasKey(c => new { c.UserID, c.Username });
        }

        public DbSet<backend.Models.Group> Group { get; set; }

    }
}
