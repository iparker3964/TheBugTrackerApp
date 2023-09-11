using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTrackerApp.Data;
using TheBugTrackerApp.Models;
using TheBugTrackerApp.Services.Interfaces;

namespace TheBugTrackerApp.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;
        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            //New ticket has been added
            if (oldTicket == null && newTicket != null)
            {
                TicketHistory history = new TicketHistory(){
                    TicketId = newTicket.Id,
                    Property = "",
                    OldValue = "",
                    NewValue = "",
                    Description = "New Ticket Created",
                    Created = DateTimeOffset.Now,
                    UserId = userId
                };

                try
                {
                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"****ERROR**** Error creating new ticket history --> {ex.Message}");
                    throw;
                }
            }
            else
            {
                //Check ticket title
                if (!oldTicket.Title.Equals(newTicket.Title))
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        Description = $"New ticket title: {newTicket.Title}",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTimeOffset.Now,
                        Property = "Title",
                        UserId = userId
                    };

                    await _context.TicketHistories.AddAsync(history);
                }
                //Check ticket description
                if (!oldTicket.Description.Equals(newTicket.Description))
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTimeOffset.Now,
                        Description = $"New ticket description: {newTicket.Description}",
                        UserId = userId
                    };

                    await _context.TicketHistories.AddAsync(history);

                }
                //Check ticket priority
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Priority",
                        OldValue = oldTicket.TicketPriority.Name,
                        NewValue = newTicket.TicketPriority.Name,
                        Description = $"New ticket priority: {newTicket.TicketPriority.Name}",
                        Created = DateTimeOffset.Now,
                        UserId = userId
                    };

                    await _context.TicketHistories.AddAsync(history);
                }
                //Check ticket status
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Status",
                        OldValue = oldTicket.TicketStatus.Name,
                        NewValue = newTicket.TicketStatus.Name,
                        Description = $"New ticket status: {newTicket.TicketStatus.Name}",
                        Created = DateTimeOffset.Now,
                        UserId = userId
                    };

                    await _context.TicketHistories.AddAsync(history);
                }
                //Check ticket type
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Type",
                        OldValue = oldTicket.TicketType.Name,
                        NewValue = newTicket.TicketType.Name,
                        Created = DateTimeOffset.Now,
                        Description = $"New ticket type: {newTicket.TicketType.Name}",
                        UserId = userId
                    };

                    await _context.TicketHistories.AddAsync(history);
                }
                //Check ticket developer
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newTicket.DeveloperUser?.FullName,
                        Description = $"New ticket developer: {newTicket.DeveloperUser.FullName}",
                        Created = DateTimeOffset.Now,
                        UserId = userId
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"**** ERROR **** Error adding ticket history --> {ex.Message}");
                    throw;
                }
            }
        }

        public async Task AddHistoryAsync(int ticketId, string model, string userId)
        {
            try
            {
                Ticket ticket = await _context.Tickets.FindAsync(ticketId);
                string description = model.ToLower().Replace("ticket", "");
                description = $"New {description} add to ticket: {ticket.Title}";

                TicketHistory history = new()
                {
                    TicketId = ticket.Id,
                    Property = model,
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = description
                };

                await _context.TicketHistories.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("***********************");
                Console.WriteLine("Error adding history");
                Console.WriteLine(ex.Message);
                Console.WriteLine("**********************");
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            try
            {
                List<Project> projects = (await _context.Companies
                                                 .Include(c => c.Projects)
                                                    .ThenInclude(p => p.Tickets)
                                                        .ThenInclude(t => t.History)
                                                            .ThenInclude(h => h.User)
                                                 .FirstOrDefaultAsync(c => c.Id == companyId)).Projects.ToList();
                List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();

                List<TicketHistory> ticketHistory = tickets.SelectMany(t => t.History).ToList();

                return ticketHistory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error getting company tickets history --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            try
            {
                List<TicketHistory> ticketHistory = new();

                Project project = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.History)
                                                                .ThenInclude(h => h.User)
                                                         .FirstOrDefaultAsync(p => p.Id == projectId);

                ticketHistory = project.Tickets.SelectMany(t => t.History).ToList();

                return ticketHistory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error getting project tickets histories --> {ex.Message}");
                throw;
            }
        }
    
    }
}
