using System.Linq.Expressions;
using exam1_Ticket.Model;
using exam1_Ticket.Services;
using Microsoft.AspNetCore.Mvc;
using Ticket.Entites;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace exam1_Ticket.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _service;
        public TicketController(TicketService service)
        {
            _service = service;
        }


        // GET: api/<GetAvailableTickets>
        [HttpGet("get-available-ticket")]
        public async Task<IActionResult> GetAvailableTickets(
            [FromQuery] string? categoryName,
            [FromQuery] string? ticketCode,
            [FromQuery] string? ticketName,
            [FromQuery] decimal? maxPrice,
            [FromQuery] DateTime? minDate,
            [FromQuery] DateTime? maxDate,
            [FromQuery] string? orderBy,
            [FromQuery] string? orderState)
        {
            try
            {
                var data = await _service.GetAvailableTickets(categoryName, ticketCode, ticketName, maxPrice, minDate, maxDate, orderBy, orderState);

                if (data == null || !data.Any())
                {
                    return NotFound(new ProblemDetails
                    {
                        Type = "https://example.com/probs/no-tickets",
                        Title = "No Available Tickets",
                        Status = 404,
                        Detail = "No tickets match the given criteria.",
                        Instance = HttpContext.Request.Path
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Available tickets retrieved successfully.",
                    
                    ticket = data,
                    totalTicket = data.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Type = "https://example.com/probs/server-error",
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                });
            }
        }


        [HttpPost("book-ticket")]
        public async Task<IActionResult> BookTicket([FromBody] List<BookedTiketRequest> requests)
        {
            try
            {
                var bookedTickets = new List<object>();

                foreach (var request in requests)
                {
                    var data = await _service.BookTicket(request.TicketCode, request.Quantity);
                    bookedTickets.Add(data);
                }

                return Ok(new
                {
                    success = true,
                    message = "Tickets booked successfully.",
                    bookedTickets
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Type = "https://example.com/probs/server-error",
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                });
            }
        }

        



        [HttpGet("get-booked-ticket/{BookedTicketId}")]
        public async Task<IActionResult> GetBookedTicket(Guid BookedTicketId)
        {
            var bookedTicket = await _service.GetBookedTicket(BookedTicketId);

            if (bookedTicket == null)

            {
                return NotFound(new ProblemDetails
                {
                    Title = "Booked Ticket Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"No booked ticket found with ID {BookedTicketId}",
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(new
            {
                success = true,
                message = "Booked ticket details retrieved successfully",
                data = bookedTicket
            });
        }


        [HttpDelete("revoke-ticket/{bookedTicketId}/{kodeTicket}/{Quantity}")]
        public async Task<IActionResult> RevokeTicket(Guid bookedTicketId, string kodeTicket, int Quantity)
        {
            var result = await _service.RemoveTicket(bookedTicketId, kodeTicket, Quantity);

            if (result is ProblemDetails problemDetails)
            {
                return StatusCode(problemDetails.Status ?? 500, problemDetails);
            }

            return Ok(result);
        }


        ////Update
        //        [HttpPut("api/v1/edit-booked-ticket/{BookedTicketId}")]
        //public async Task<IActionResult> EditBookedTicket([FromRoute] Guid BookedTicketId, [FromBody] EditBookedTicketRequest request)
        //{
        //    // Assign BookedTicketId dari URL ke request
        //    request.BookedTicketId = BookedTicketId;

        //    var result = await _service.EditBookedTicket(request);

        //    // Cek status dari result
        //    var status = (int)result.GetType().GetProperty("status").GetValue(result);
        //    if (status == 404)
        //    {
        //        return NotFound(result);
        //    }
        //    else if (status == 400)
        //    {
        //        return BadRequest(result);
        //    }

        //    return Ok(result);
        //}
        [HttpPut("api/v1/edit-booked-ticket/{BookedTicketId}")]
        public async Task<IActionResult> EditBookedTicket([FromRoute] Guid BookedTicketId, [FromBody] EditBookedTicketRequest request)
        {
            // Kirim BookedTicketId dari URL ke service
            var result = await _service.EditBookedTicket(BookedTicketId, request);

            // Cek status dari result
            var status = (int)result.GetType().GetProperty("status").GetValue(result);
            if (status == 404)
            {
                return NotFound(result);
            }
            else if (status == 400)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }




    }
}
