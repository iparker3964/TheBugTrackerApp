using Microsoft.AspNetCore.Identity.UI.Services;
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
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IBTRolesService _roleService;
        public BTNotificationService(ApplicationDbContext context, IEmailSender emailSender, IBTRolesService roleService)
        {
            _context = context;
            _emailSender = emailSender;
            _roleService = roleService;
        }
        public async Task AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error adding notification --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications.Include(n => n.Recipient)
                                                                         .Include(n => n.Sender)
                                                                         .Include(n => n.Ticket)
                                                                            .ThenInclude(t => t.Project)
                                                                         .Where(n => n.RecipientId == userId).ToListAsync();
                return notifications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error getting received notifications --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications.Include(n => n.Sender)
                                                                               .Include(n => n.Recipient)
                                                                               .Include(n => n.Ticket)
                                                                                   .ThenInclude(t => t.Project)
                                                                               .Where(n => n.SenderId == userId).ToListAsync();
                return notifications;           
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error getting sent notifications --> {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

            if (user != null)
            {
                try
                {
                    string email = user.Email;
                    string message = notification.Message;

                    await _emailSender.SendEmailAsync(email, emailSubject, message);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"****ERROR*** Error sending notification --> {ex.Message}");
                    throw;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
        {
            try
            {
                List<BTUser> users = await _roleService.GetUsersInRoleAsync(role, companyId);

                foreach (BTUser user in users)
                {
                    notification.RecipientId = user.Id;

                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error sending email notifications by role -->{ex.Message}");
                throw;
            }
        }

        public async Task SendMembersEmailNotificationsAsync(Notification notification, List<BTUser> members)
        {
            try
            {
                foreach (BTUser member in members)
                {
                    notification.RecipientId = member.Id;
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send members email notifications --> {ex.Message}");
                throw;
            }
        }
    }
}
