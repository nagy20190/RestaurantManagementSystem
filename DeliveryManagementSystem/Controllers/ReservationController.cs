using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        [HttpPost("{id}/reservations")]
        public async Task<ActionResult<Reservation>> MakeReservation(int id, [FromBody] CreateReservationDTO reservationDto)
        {
            // Make a reservation at restaurant
            throw new NotImplementedException();
        }

    }
}
