using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataEntry.Dao;
using Microsoft.EntityFrameworkCore;

namespace DataEntry.Controllers
{
    [Route("api/[controller]")]
    public class JobController : Controller
    {
        private readonly DataEntryDBContext _context;
        const int ITEMS_PER_PAGE = 10;

        public JobController(DataEntryDBContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrator,User")]
        [HttpGet("[action]")]
        public IEnumerable<JobDto> Jobs(int startDateIndex)
        {
            var itemsToSkip = ITEMS_PER_PAGE * startDateIndex;
            return _context.Jobs
                .Include(jobs => jobs.CreatedBy)
                .Skip(itemsToSkip).Take(ITEMS_PER_PAGE).ToList();
        }

        [Authorize(Roles = "Administrator,User")]
        [HttpGet("{jobId}")]
        public JobDto Get(Guid jobId)
        {
            return _context.Jobs
                .Include(jobs => jobs.CreatedBy)
                .FirstOrDefault(x => x.Id == jobId);
        }

        [Authorize(Roles = "Administrator,User")]
        [HttpPost]
        public JobDto Post(JobDto job)
        {
            var userClaims = User.Claims.ToList();
            var nameClaim = userClaims.FirstOrDefault(x => x.Type == "name");
            var user = _context.Users.FirstOrDefault(x => x.UserName == nameClaim.Value);

            job.Id = Guid.NewGuid();
            job.DateModified = DateTime.UtcNow;
            job.DateCreated = DateTime.UtcNow;
            job.CreatedBy = user;

            _context.Jobs.Add(job);
            _context.SaveChanges();

            return job;
        }

        [Authorize(Roles = "Administrator,User")]
        [HttpPut("{jobId}")]
        public JobDto Put(Guid jobId, JobDto job)
        {
            var currentJob = _context.Jobs.FirstOrDefault(x => x.Id == jobId);
            if (currentJob == null)
                throw new Exception("Job not found");

            currentJob.DateModified = DateTime.UtcNow;
            currentJob.Name = job.Name;

            _context.Jobs.Update(currentJob);
            _context.SaveChanges();

            return currentJob;
        }

        [Authorize(Roles = "Administrator,User")]
        [HttpDelete("{jobId}")]
        public StatusCodeResult Delete(Guid jobId)
        {
            var job = _context.Jobs.FirstOrDefault(x => x.Id == jobId);
            if (job == null)
                return BadRequest();

            _context.Jobs.Remove(job);
            _context.SaveChanges();

            return Ok();
        }

        public class Job
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string DateCreated { get; set; }
            public string DateModified { get; set; }
            public string CreatedBy { get; set; }
        }
    }
}
