using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SheduleOffersControllers : ControllerBase
    {
       // this controller is for using HangFire Libaray 
       // will send offers to all users in db weekly or monthly 
       // passing cron expression to the HangFire to send mails  



    }
}
