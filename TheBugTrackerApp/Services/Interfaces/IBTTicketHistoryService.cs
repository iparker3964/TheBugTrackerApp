﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTrackerApp.Models;

namespace TheBugTrackerApp.Services.Interfaces
{
    public interface IBTTicketHistoryService
    {
        Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket,string userId);
        Task AddHistoryAsync(int ticketId,string model,string userId);

        Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId,int companyId);

        Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId);
    }
}
