using AutoMapper;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController(IGenericRepository<Reservation> repository, IMapper mapper) : ControllerBase
    {
        private readonly IGenericRepository<Reservation> _repository = repository;
        private readonly IMapper _mapper = mapper;

        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationDTO reservationDto)
        {
            try
            {
                var newReservation = _mapper.Map<Reservation>(reservationDto);
                await _repository.AddAsync(newReservation);
                return CreatedAtAction( "reservtion created", reservationDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating reservation: " + ex.Message);
            }
        }

        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetReservationById(int id)
        // {
        //     var reservation = await _repository.GetByIdAsync(id);
        //     return Ok(reservation);
        // }

        // [HttpGet("user/{userId}")]
        // public async Task<IActionResult> GetReservationsByUserId(int userId)
        // {
        //     var reservations = await _reservationService.GetReservationsByUserIdAsync(userId);
        //     return Ok(reservations);
        // }

        // [HttpPut("update/{id}")]
        // public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDto reservationDto)
        // {
        //     if (reservationDto == null || id != reservationDto.Id)
        //     {
        //         return BadRequest("Invalid reservation data.");
        //     }

        //     var updatedReservation = await _reservationService.UpdateReservationAsync(reservationDto);
        //     if (updatedReservation == null)
        //     {
        //         return NotFound();
        //     }
        //     return Ok(updatedReservation);
        // }

        // [HttpDelete("delete/{id}")]
        // public async Task<IActionResult> DeleteReservation(int id)
        // {
        //     var result = await _reservationService.DeleteReservationAsync(id);
        //     if (!result)
        //     {
        //         return NotFound();
        //     }
        //     return NoContent();
        // }


    }
}
