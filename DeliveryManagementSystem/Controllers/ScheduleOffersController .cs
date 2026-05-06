using DeliveryManagementSystem.BLL.Services;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleOffersController(IGenericRepository<User> userRepository, EmailServices emailServices) : ControllerBase
    {
        // this controller is for using HangFire Libaray 
        private readonly IGenericRepository<User> _userRepository = userRepository;
        private readonly EmailServices _emailServices = emailServices;

        [HttpPost]
        public async Task<IActionResult> SendScheduleOffers()
        {
            try
            {
                var users = await _userRepository
                .GetAll()
                .Select(u => u.Id)
                .ToListAsync();
                foreach (var userId in users)
                {
                    var jobId = userId.ToString(); // Use user ID as the job identifier

                    RecurringJob.AddOrUpdate(jobId,
                        () => SendOfferJob(userId), Cron.Weekly);
                }
                return Ok(new { Registered = users.Count });
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server error {ex.Message}, {ex.InnerException?.Message}");
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task SendOfferJob(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            string offerMessage =
              $"Dear {user.UserName}, we have a special offer for you! " +
              $"Get 20% off on your next order. Use code OFFER20 at checkout.";

            await _emailServices.SendEmail(
           subject: "Special Offer Just for You!",
           toEmail: user.Email,
           userName: user.UserName,
           message: offerMessage);

            Console.WriteLine($"Offer sent to {user.UserName} mailto:({user.Email})");
        }


        //DELETE /unregister-offer-jobs/{userId}A user unsubscribes or is deleted —you must be able to remove their job
        [HttpDelete("unregister-offer-jobs/{userId}")]
        public async Task<IActionResult> UnregisterOfferJob(int userId)
        {
            try
            {
                var jobId = userId.ToString();
                RecurringJob.RemoveIfExists(jobId);
                return Ok(new { Message = "Offer job unregistered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server error {ex.Message}, {ex.InnerException?.Message}");
            }
        }

        //DELETE /unregister-all-offer-jobsReset/cleanup all scheduled offer jobs at once
        [HttpDelete("unregister-all-offer-jobs")]
        public async Task<IActionResult> UnregisterAllOfferJobs()
        {
            try
            {
                var jobs = JobStorage.Current.GetConnection().GetRecurringJobs();
                if (jobs.Count == 0)
                {
                    return Ok(new { Message = "No offer jobs found to unregister." });
                }
                RecurringJob.RemoveIfExists("*"); // Remove all jobs
                return Ok(new { Message = "All offer jobs unregistered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server error {ex.Message}, {ex.InnerException?.Message}");
            }
        }


        [HttpPost("trigger-offer/{userId}")]
        public async Task<IActionResult> TriggerOffer(int userId)
        {
            try
            {
                BackgroundJob.Enqueue(() => SendOfferJob(userId));
                return Ok(new { Message = "Offer triggered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server error {ex.Message}, {ex.InnerException?.Message}");
            }
        }

        //GET /jobsList all registered offer jobs and their next run time (admin visibility)
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs()
        {
            try
            {
                var jobs = JobStorage.Current.GetConnection().GetRecurringJobs();
                var jobList = jobs.Select(j => new
                {
                    JobId = j.Id,
                    NextExecution = j.NextExecution
                }).ToList();
                return Ok(jobList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server error {ex.Message}, {ex.InnerException?.Message}");
            }
        }


        //POST /register-offer-jobs/{userId}Register a job for a single user (e.g. new signup) instead of 
        [HttpPost("register-offer-jobs/{userId}")]
        public async Task<IActionResult> RegisterOfferJob(int userId)
        {
            try
            {
                var jobId = userId.ToString(); // Use user ID as the job identifier
                RecurringJob.AddOrUpdate(jobId,
                    () => SendOfferJob(userId), Cron.Weekly);
                return Ok(new { Message = "Offer job registered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server error {ex.Message}, {ex.InnerException?.Message}");
            }
        }



    }
}
