using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTrackerApp.Data;
using TheBugTrackerApp.Models;
using TheBugTrackerApp.Models.Enums;
using TheBugTrackerApp.Services.Interfaces;

namespace TheBugTrackerApp.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _roleService;
        private readonly IBTProjectService _projectService;
        public BTTicketService(ApplicationDbContext context, IBTRolesService roleService, IBTProjectService projectService)
        {
            _context = context;
            _roleService = roleService;
            _projectService = projectService;
        }
        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"***ERROR**** Error adding new ticket async --> {ex.Message}");

                throw;
            }
 
        }
        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task AddTicketCommentAsync(TicketComment ticketComment)
        {
            try
            {
                await _context.AddAsync(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("***************************");
                Console.WriteLine("Error adding ticket comment");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***************************");
                throw;
            }

        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;

                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error archiving ticket --> {ex.Message}");
                throw;
            }
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            try
            {
                Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket != null)
                {
                    ticket.DeveloperUserId = userId;
                    ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;

                    await _context.SaveChangesAsync();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****Error**** Error assigning user to ticket --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
           
            try
            {
                List<Ticket> tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                        .SelectMany(p => p.Tickets)
                                                        .Include(t => t.Project)
                                                        .Include(t => t.TicketType)
                                                        .Include(t => t.TicketPriority)
                                                        .Include(t => t.TicketStatus)
                                                        .Include(t => t.OwnerUser)
                                                        .Include(t => t.DeveloperUser)
                                                        .Include(t => t.Comments)
                                                        .Include(t => t.Attachments)
                                                        .Include(t => t.Notifications)
                                                        .Include(t => t.History).ToListAsync();
                return tickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting all tickets by company --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                        .SelectMany(p => p.Tickets)
                                                            .Include(t => t.Attachments)
                                                            .Include(t => t.Comments)
                                                            .Include(t => t.DeveloperUser)
                                                            .Include(t => t.OwnerUser)
                                                            .Include(t => t.TicketPriority)
                                                            .Include(t => t.TicketStatus)
                                                            .Include(t => t.TicketType)
                                                            .Include(t => t.Project)
                                                         .Where(t => t.TicketPriorityId == priorityId).ToListAsync();

                return tickets;                                     
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting all tickets by priority --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                        .SelectMany(p => p.Tickets)
                                                            .Include(t => t.Attachments)
                                                            .Include(t => t.Comments)
                                                            .Include(t => t.DeveloperUser)
                                                            .Include(t => t.OwnerUser)
                                                            .Include(t => t.TicketPriority)
                                                            .Include(t => t.TicketStatus)
                                                            .Include(t => t.TicketType)
                                                            .Include(t => t.Project)
                                                        .Where(t => t.TicketStatusId == statusId).ToListAsync();
                return tickets;
            }catch(Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting all tickets by status --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            int typeId = (await LookupTicketTypeIdAsync(typeName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                        .SelectMany(p => p.Tickets)
                                                            .Include(t => t.Attachments)
                                                            .Include(t => t.Comments)
                                                            .Include(t => t.DeveloperUser)
                                                            .Include(t => t.OwnerUser)
                                                            .Include(t => t.TicketPriority)
                                                            .Include(t => t.TicketStatus)
                                                            .Include(t => t.TicketType)
                                                            .Include(t => t.Project)
                                                        .Where(t => t.TicketTypeId == typeId).ToListAsync();
                return tickets;
            }
            catch(Exception ex)
            { 
                Console.WriteLine($"**** ERROR **** Error getting all tickets by type --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.Archived == true).ToList();

                return tickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting archived tickets --> {ex.Message}");
                throw;
            }
        }
        
        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName,int companyId,int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName)).Where(t => t.ProjectId == projectId).ToList();

                return tickets;
            }
            catch (Exception ex)
            {
               Console.WriteLine($"**** ERROR **** Error getting project tickets by priority --> {ex.Message}");
               throw;
            }

        }
        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {

            try
            {
                List<Ticket> tickets = new();

                tickets = (await GetTicketsByRoleAsync(role, userId, companyId)).Where(t => t.ProjectId == projectId).ToList();

                return tickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting project tickets by role --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByStatusAsync(companyId, statusName)).Where(t => t.ProjectId == projectId).ToList();

                return tickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting project tickets by status --> {ex.Message}");
                throw;
            }

           
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByTypeAsync(companyId,typeName)).Where(t => t.ProjectId == projectId).ToList();

                return tickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting project tickets by type --> {ex.Message}");
                throw;
            }
        }
        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await _context.Tickets
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.OwnerUser)
                                                .Include(t => t.Project)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Comments)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.History)
                                                .FirstOrDefaultAsync(t => t.Id == ticketId);

                return ticket;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting ticket by id --> {ex.Message}");
                throw;
            }
        }
        
        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            BTUser developer = new();

            try
            {
                Ticket ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);

                if (ticket?.DeveloperUserId != null)
                {
                    developer = ticket.DeveloperUser;
                }

                return developer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting ticket developer --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                if (role == Roles.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);
                }
                else if (role == Roles.Developer.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.DeveloperUserId == userId).ToList();
                }
                else if (role == Roles.Submitter.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.OwnerUserId == userId).ToList();
                }
                else if (role == Roles.ProjectManager.ToString())
                {
                    tickets = (await GetTicketsByUserIdAsync(userId, companyId));
                }

                return tickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting tickets by role --> {ex.Message}");
                throw;
            }

        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket> tickets = new();

            try
            {
                if (await _roleService.IsUserInRoleAsync(user,Roles.Admin.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId)).SelectMany(p => p.Tickets).ToList();
                }
                else if (await _roleService.IsUserInRoleAsync(user,Roles.Developer.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId)).SelectMany(p => p.Tickets).Where(t => t.DeveloperUserId == userId).ToList();
                }
                else if (await _roleService.IsUserInRoleAsync(user,Roles.Submitter.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId)).SelectMany(p => p.Tickets).Where(t => t.OwnerUserId == userId).ToList();
                }
                else if (await _roleService.IsUserInRoleAsync(user,Roles.ProjectManager.ToString()))
                {
                    tickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(p => p.Tickets).ToList();
                }

                return tickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting tickets by user id --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => string.IsNullOrEmpty(t.DeveloperUserId)).ToList();

                return tickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting project tickets by type --> {ex.Message}");
                throw;
            }
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                TicketPriority priority = await _context.TicketPriorities.FirstOrDefaultAsync(t => t.Name.Equals(priorityName));

                return priority?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error looking up ticket priority id --> {ex.Message}");
                throw;
            }
            
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus status = await _context.TicketStatuses.FirstOrDefaultAsync(s => s.Name.Equals(statusName));

                return status?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error looking up ticket status id --> {ex.Message}");
                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                TicketType type = await _context.TicketTypes.FirstOrDefaultAsync(t => t.Name.Equals(typeName));

                return type?.Id;    
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error looking up ticket type id --> {ex.Message}");
                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error looking up ticket type id --> {ex.Message}");
                throw;
            }
           
        }
    }
}
