using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheBugTrackerApp.Models.ViewModels
{
    public class AssignDeveloperViewModel
    {
        public Ticket Ticket { get; set; }
        public SelectList Developers { get; set; }
        public string DeveloperId { get; set; }
    }
}
