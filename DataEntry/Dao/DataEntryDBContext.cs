using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataEntry.Dao
{
    public class DataEntryDBContext : IdentityDbContext<ApplicationUser>
    {
        public DataEntryDBContext(DbContextOptions<DataEntryDBContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<JobDto> Jobs { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {
    }

    [Table("Jobs")]
    public class JobDto
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public ApplicationUser CreatedBy { get; set; }
    }
}
