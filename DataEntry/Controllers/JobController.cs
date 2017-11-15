using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DataEntry.Controllers
{
    [Route("api/[controller]")]
    public class JobController : Controller
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        //[Authorize]
        [HttpGet("[action]")]
        public IEnumerable<Job> Jobs(int startDateIndex)
        {
            var user = User;
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Job
            {
                Id = Guid.NewGuid().ToString(),
                Name = Summaries[index],
                DateModified = DateTime.Now.AddDays(index + startDateIndex).ToString("d"),
                DateCreated = DateTime.Now.AddDays(index + startDateIndex).ToString("d"),
                CreatedBy = "test"
            });
        }

        [Authorize]
        [HttpGet("{jobId}")]
        public Job Get(string jobId)
        {
            var rng = new Random();
            return new Job
            {
                Id = jobId,
                Name = Summaries[rng.Next(1, 5)],
                DateModified = DateTime.UtcNow.ToString("d"),
                DateCreated = DateTime.UtcNow.ToString("d"),
                CreatedBy = "test"
            };
        }

        [Authorize]
        [HttpPost]
        public Job Post(Job Job)
        {
            Job.Id = Guid.NewGuid().ToString();
            Job.DateModified = DateTime.UtcNow.ToString("d");
            Job.DateCreated = DateTime.UtcNow.ToString("d");
            Job.CreatedBy = "user";
            return Job;
        }

        [Authorize]
        [HttpPut("{jobId}")]
        public Job Put(string jobId, Job Job)
        {
            Job.DateModified = DateTime.UtcNow.ToString("d");
            return Job;
        }

        [Authorize]
        [HttpDelete("{jobId}")]
        public StatusCodeResult Delete(string jobId)
        {
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
