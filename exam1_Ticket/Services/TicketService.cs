using System.Net.Sockets;
using exam1_Ticket.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticket.Entites;


namespace exam1_Ticket.Services
{
    public class TicketService
    {
        private readonly AccelokaDbContext _db;
        public TicketService(AccelokaDbContext db)
        {
            _db = db;
        }


        public async Task<List<TicketModel>> GetAvailableTickets(
    string? categoryName,
    string? ticketCode,
    string? ticketName,
    decimal? maxPrice,
    DateTime? minDate,
    DateTime? maxDate,
    string? orderBy,
    string? orderState)
        {
            var query = _db.Tickets
                .Where(Q => Q.Quota > 0) 
                .Select(Q => new TicketModel
                {
                    TicketCode = Q.TicketCode,
                    TicketName = Q.TicketName,
                    CategoryName = Q.Category.CategoryName, 
                    TanggalEvent = Q.TanggalEvent.ToDateTime(TimeOnly.MinValue),
                    Price = Q.Price,
                    Quota = Q.Quota

                });

            // Filtering berdasarkan query parameter yang diinput
            if (!string.IsNullOrEmpty(categoryName))
                query = query.Where(Q => Q.CategoryName.Contains(categoryName));

            if (!string.IsNullOrEmpty(ticketCode))
                query = query.Where(Q => Q.TicketCode.Contains(ticketCode));

            if (!string.IsNullOrEmpty(ticketName))
                query = query.Where(Q => Q.TicketName.Contains(ticketName));

            if (maxPrice.HasValue)
                query = query.Where(Q => Q.Price <= maxPrice.Value);

            if (minDate.HasValue)
                query = query.Where(Q => Q.TanggalEvent >= minDate.Value);

            if (maxDate.HasValue)
                query = query.Where(Q => Q.TanggalEvent <= maxDate.Value);

            //  Default sorting: Order by TicketCode Ascending
            if (string.IsNullOrEmpty(orderBy)) orderBy = "TicketCode";
            if (string.IsNullOrEmpty(orderState)) orderState = "asc";

            bool isAscending = orderState.ToLower() == "asc";

            query = orderBy switch
            {
                "TicketCode" => isAscending ? query.OrderBy(Q => Q.TicketCode) : query.OrderByDescending(Q => Q.TicketCode),
                "TicketName" => isAscending ? query.OrderBy(Q => Q.TicketName) : query.OrderByDescending(Q => Q.TicketName),
                "CategoryName" => isAscending ? query.OrderBy(Q => Q.CategoryName) : query.OrderByDescending(Q => Q.CategoryName),
                "Price" => isAscending ? query.OrderBy(Q => Q.Price) : query.OrderByDescending(Q => Q.Price),
                "TanggalEvent" => isAscending ? query.OrderBy(Q => Q.TanggalEvent) : query.OrderByDescending(Q => Q.TanggalEvent),
                _ => query.OrderBy(Q => Q.TicketCode) // Default order by TicketCode ascending
            };

            return await query.ToListAsync();
        }


        public async Task<object> BookTicket(string ticketCode, int quantity)
        {
            // Cari tiket berdasarkan kode dan kuota
            var ticket = await _db.Tickets
                .Where(t => t.TicketCode == ticketCode)
                .Include(t => t.Category)
                .Select(t => new
                {
                    t.TicketId,
                    t.TicketCode,
                    t.TicketName,
                    t.Price,
                    CategoryName = t.Category.CategoryName,
                    t.Quota
                })
                .FirstOrDefaultAsync();

            // Jika tiket tidak ditemukan
            if (ticket == null)
            {
                return new ProblemDetails
                {
                    Title = "Kode tiket tidak terdaftar",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Kode tiket yang Anda masukkan tidak ditemukan dalam database.",
                    Instance = $"/api/tickets/book/{ticketCode}"
                };
            }

            // Jika kuota tidak mencukupi
            if (ticket.Quota < quantity)
            {
                return new ProblemDetails
                {
                    Title = "Kuota tidak mencukupi",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = $"Kuota tiket untuk {ticket.TicketCode} tidak mencukupi. Sisa kuota: {ticket.Quota}.",
                    Instance = $"/api/tickets/book/{ticketCode}"
                };
            }

            try
            {
                // Buat objek booking
                var bookedTiket = new BookedTiket
                {
                    BookedTicketId = Guid.NewGuid(),
                    TicketId = ticket.TicketId,
                    Quantity = quantity,
                    TanggalBooking = DateTime.UtcNow
                };

                // Update kuota tiket
                await _db.Tickets
                    .Where(t => t.TicketId == ticket.TicketId)
                    .ExecuteUpdateAsync(t =>
                        t.SetProperty(tk => tk.Quota, tk => tk.Quota - quantity));

                // Simpan booked tiket
                await _db.BookedTikets.AddAsync(bookedTiket);
                await _db.SaveChangesAsync();

                // Ambil daftar tiket yang baru saja dipesan
                var bookedTickets = await _db.BookedTikets
                    .Include(bt => bt.Ticket)
                    .ThenInclude(t => t.Category)
                    .Where(bt => bt.BookedTicketId == bookedTiket.BookedTicketId)
                    .GroupBy(bt => bt.Ticket.Category.CategoryName)
                    .Select(g => new
                    {
                        categoryName = g.Key,
                        summaryPrice = Convert.ToInt32(g.Sum(bt => bt.Ticket.Price * bt.Quantity)),
                        tickets = g.Select(bt => new
                        {
                            ticketCode = bt.Ticket.TicketCode,
                            ticketName = bt.Ticket.TicketName,
                            price = bt.Ticket.Price * bt.Quantity
                        }).ToList()
                    })
                    .ToListAsync();

                int priceSummary = bookedTickets.Sum(c => c.summaryPrice);

                return new
                {
                    priceSummary,
                    ticketsPerCategories = bookedTickets
                };
            }
            catch (DbUpdateException dbEx)
            {
                return new ProblemDetails
                {
                    Title = "Database Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = dbEx.InnerException?.Message ?? dbEx.Message,
                    Instance = $"/api/tickets/book/{ticketCode}"
                };
            }
            catch (Exception ex)
            {
                return new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = ex.Message,
                    Instance = $"/api/tickets/book/{ticketCode}"
                };
            }
        }


        public async Task<object> GetBookedTicket(Guid bookedTicketId)
        {
            // Cek apakah ID ada di BookedTikets
            var isBookedTicketId = await _db.BookedTikets
                .AnyAsync(bt => bt.BookedTicketId == bookedTicketId);

            

            // Validasi: Hanya terima jika ID ada di BookedTikets
            if (!isBookedTicketId)
            {
                return null;
            }

            // Ambil data booked ticket jika ID valid
            var bookedTickets = await _db.BookedTikets
                .Include(bt => bt.Ticket)
                .ThenInclude(t => t.Category)
                .Where(bt => bt.Ticket.Quota > 0 && bt.BookedTicketId == bookedTicketId)
                .GroupBy(bt => bt.Ticket.Category.CategoryName)
                .Select(g => new
                {
                    qtyPerCategory = g.Sum(bt => bt.Quantity),
                    categoryName = g.Key,
                    tickets = g.Select(bt => new
                    {
                        ticketCode = bt.Ticket.TicketCode,
                        ticketName = bt.Ticket.TicketName,
                        eventDate = bt.Ticket.TanggalEvent.ToString("dd-MM-yyyy")
                    }).ToList()
                })
                .ToListAsync();

            return bookedTickets;
        }


        public async Task<object> RemoveTicket(Guid bookedTicketId, string kodeTicket, int Quantity)
        {
            // Cek apakah bookedTicketId terdaftar di database
            var bookedTicket = await _db.BookedTikets
                .Include(bt => bt.Ticket)
                .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(bt => bt.BookedTicketId == bookedTicketId);

            if (bookedTicket == null)
            {
                return new ProblemDetails
                {
                    
                    Title = "Booked Ticket Not Found",
                    Status = 404,
                    Detail = "Booked Ticket ID tidak terdaftar di database.",
                    Instance = $"/api/tickets/{bookedTicketId}"
                };
            }

            // Cek apakah kode tiket sesuai dengan yang terdaftar di database
            var ticket = bookedTicket.Ticket;
            if (ticket.TicketCode != kodeTicket)
            {
                return new ProblemDetails
                {
                   
                    Title = "Invalid Ticket Code",
                    Status = 400,
                    Detail = "Kode Ticket tidak terdaftar atau tidak sesuai.",
                    Instance = $"/api/tickets/{bookedTicketId}"
                };
            }

            // Cek apakah Quantity tidak melebihi jumlah tiket yang sudah di booking
            if (Quantity > bookedTicket.Quantity)
            {
                return new ProblemDetails
                {
                    
                    Title = "Quantity Exceeded",
                    Status = 400,
                    Detail = "Quantity tidak boleh melebihi jumlah tiket yang sudah di booking.",
                    Instance = $"/api/tickets/{bookedTicketId}"
                };
            }

            // Proses pengurangan tiket
            bookedTicket.Quantity -= Quantity;
            if (bookedTicket.Quantity <= 0)
            {
                _db.BookedTikets.Remove(bookedTicket);
            }
            else
            {
                _db.BookedTikets.Update(bookedTicket);
            }

            await _db.SaveChangesAsync();

            // Cek sisa tiket dan hapus jika kosong
            var ticketremaining = await _db.BookedTikets
                .Where(Q => Q.BookedTicketId == bookedTicketId)
                .ToListAsync();
            if (!ticketremaining.Any())
            {
                _db.BookedTikets.RemoveRange(ticketremaining);
                await _db.SaveChangesAsync();
            }

            // Return data sukses
            return new
            {
                success = true,
                message = "Ticket revoked successfully",
                data = new
                {
                    ticketCode = ticket.TicketCode,
                    ticketName = ticket.TicketName,
                    categoryName = ticket.Category?.CategoryName ?? "Unknown",
                    Quantity = bookedTicket.Quantity
                }
            };
        }


        //update
        public async Task<object> EditBookedTicket(Guid bookedTicketId, EditBookedTicketRequest request)
        {
            // Validasi: Cek apakah BookedTicketId ada di database
            var bookedTicket = await _db.BookedTikets
                .Include(bt => bt.Ticket)
                .ThenInclude(t => t.Category)
                .Where(bt => bt.BookedTicketId == bookedTicketId) 
                .ToListAsync();

            if (bookedTicket == null || !bookedTicket.Any())
            {
                return new
                {
                    status = 404,
                    title = "Not Found",
                    detail = "Booked Ticket Id tidak terdaftar."
                };
            }

            // Validasi: Periksa setiap tiket dalam request
            foreach (var ticketRequest in request.Tickets)
            {
                var ticket = await _db.Tickets
                    .FirstOrDefaultAsync(t => t.TicketCode == ticketRequest.TicketCode);

                if (ticket == null)
                {
                    return new
                    {
                        status = 404,
                        title = "Not Found",
                        detail = $"Kode tiket '{ticketRequest.TicketCode}' tidak terdaftar."
                    };
                }

                if (ticketRequest.Quantity > ticket.Quota)
                {
                    return new
                    {
                        status = 400,
                        title = "Bad Request",
                        detail = $"Quantity untuk tiket '{ticketRequest.TicketCode}' melebihi sisa quota ({ticket.Quota})."
                    };
                }
                if (ticketRequest.Quantity == 0)
                {
                    return new
                    {
                        status = 400,
                        title = "Bad Request",
                        detail = $"Quantity Tidak Boleh 0"
                    };
                }
            }

            // Update quantity tiket yang sudah di-booking
            foreach (var ticketRequest in request.Tickets)
            {
                var bookedTicketToUpdate = bookedTicket
                    .FirstOrDefault(bt => bt.Ticket.TicketCode == ticketRequest.TicketCode);

                if (bookedTicketToUpdate != null)
                {
                   
                    // Hitung selisih quantity lama dan baru
                    int selisihQuantity = ticketRequest.Quantity - bookedTicketToUpdate.Quantity;

                    // Update quantity di booked tiket
                    bookedTicketToUpdate.Quantity = ticketRequest.Quantity;
                    _db.BookedTikets.Update(bookedTicketToUpdate);

                    // Update quota di tabel tiket
                    var ticket = await _db.Tickets
                        .FirstOrDefaultAsync(t => t.TicketCode == ticketRequest.TicketCode);

                    if (ticket != null)
                    {
                        // Sesuaikan sisa quota berdasarkan selisih quantity
                        ticket.Quota -= selisihQuantity;
                        _db.Tickets.Update(ticket);
                    }
                }
            }

            await _db.SaveChangesAsync();

            // Ambil data untuk response success
            var updatedTickets = await _db.BookedTikets
                .Include(bt => bt.Ticket)
                .ThenInclude(t => t.Category)
                .Where(bt => bt.BookedTicketId == bookedTicketId)
                .Select(bt => new
                {
                    TicketCode = bt.Ticket.TicketCode,
                    TicketName = bt.Ticket.TicketName,
                    CategoryName = bt.Ticket.Category.CategoryName,
                    Quantity = bt.Quantity,
                    SisaQuota = bt.Ticket.Quota
                })
                .ToListAsync();

            return new
            {
                status = 200,
                title = "Success",
                detail = "Quantity tiket berhasil di-update.",
                data = updatedTickets
            };
        }
    }
}
