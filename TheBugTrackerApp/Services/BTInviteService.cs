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
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;
        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            Invite invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

            if (invite == null)
            {
                return false;
            }
            try
            {
                invite.IsValid = false;
                invite.InviteeId = userId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error accepting invite --> {ex.Message}");
                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                await _context.Invites.AddAsync(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error adding new invite --> {ex.Message}");
                throw;
            }
            
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                bool result = await _context.Invites.Where(i => i.CompanyId == companyId)
                                              .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error finding any invite --> {ex.Message}");
                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite invite = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                             .Include(i => i.Project)
                                                             .Include(i => i.Company)
                                                             .Include(i => i.Invitor)
                                                       .FirstOrDefaultAsync(i => i.Id == inviteId);
                return invite;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error getting invite --> {ex.Message}");
                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                Invite invite = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                            .Include(i => i.Project)
                                                            .Include(i => i.Invitor)
                                                            .Include(i => i.Company)
                                                      .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
                return invite;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error getting invite --> {ex.Message}");
                throw;
            }
           
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            if (token == null)
            {
                return false;
            }
            bool result = false;

            Invite invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

            if (invite != null)
            {
                DateTime inviteDate = invite.InviteDate.DateTime;
                //check if invite is within 7 days
                bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

                if (validDate)
                {
                    result = invite.IsValid;
                }
            }

            return result;
        }
    }
}
