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
    public class BTLookupService : IBTLookupService
    {
        private readonly ApplicationDbContext _context;
        public BTLookupService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("************* ERROR *************");
                Console.WriteLine("Error getting project priorities");
                Console.WriteLine(ex.Message);
                Console.WriteLine("********************************");

                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("************* ERROR *************");
                Console.WriteLine("Error getting ticket priorities");
                Console.WriteLine(ex.Message);
                Console.WriteLine("********************************");

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("************* ERROR *************");
                Console.WriteLine("Error getting ticket statues");
                Console.WriteLine(ex.Message);
                Console.WriteLine("********************************");

                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("************* ERROR *************");
                Console.WriteLine("Error getting ticket types");
                Console.WriteLine(ex.Message);
                Console.WriteLine("********************************");

                throw;
            }
        }
    }
}
