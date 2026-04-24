using AutoMapper;
using DeliveryManagementSystem.BLL.Healpers;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController(IGenericRepository<Reservation> repository, IMapper mapper, JWTReader jWTReader) : ControllerBase
    {
        private readonly IGenericRepository<Reservation> _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly JWTReader _jwtReader = jWTReader;


        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationDTO reservationDto)
        {
            var newReservation = _mapper.Map<Reservation>(reservationDto);
            await _repository.AddAsync(newReservation);
            return CreatedAtAction(nameof(ReservationByID), new { id = newReservation.ID }, reservationDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ReservationByID(int id)
        {
            var reservation = await _repository.GetByIdAsync(id);
            var userId = await _jwtReader.GetCurrentUserId();
            if (reservation.UserID != userId)
            {
                return Forbid("You are not authorized to view this reservation.");
            }
            var reservationDto = _mapper.Map<ReservationDTO>(reservation);
            return Ok(reservationDto);
        }

        //- `GET /user/{userId/}` — Get reservations by user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReservationByUser(int userId)
        {
            var reservations = await _repository.FindByCondition(r => r.UserID == userId).ToListAsync();
            var dtos = _mapper.Map<List<ReservationDTO>>(reservations);
            return Ok(dtos);
        }

        //- `PUT /update/{id}` — Update reservation
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDTO reservationDto)
        {
            var existingReservation = await _repository.GetByIdAsync(id);
            _mapper.Map(reservationDto, existingReservation);
            await _repository.UpdateAsync(existingReservation);
            return Ok("Reservation updated successfully");
        }


        //- `DELETE /delete/{id}` — Cancel / delete reservation
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var existingReservation = await _repository.GetByIdAsync(id);
            await _repository.DeleteAsync(existingReservation);
            return Ok("Reservation deleted successfully");
        }


        //- `GET / availability` — Check table availability by restaurant + date
        [HttpGet("availability")]
        public async Task<IActionResult> CheckAvailability(int restaurantId, DateTime date)
        {
            var reservations = await _repository.FindByCondition(r => r.RestaurantID == restaurantId && r.ReservationDate.Date == date.Date).ToListAsync();
            var dtos = _mapper.Map<List<ReservationDTO>>(reservations);
            return Ok(dtos);
        }




    }
}
