using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TheBugTrackerApp.Extensions;

namespace TheBugTrackerApp.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        [DisplayName("Ticket")]
        public int TicketId { get; set; }
        [DisplayName("File date")]
        public DateTimeOffset Created { get; set; }
        [DisplayName("Team Member")]
        public string UserId { get; set; }
        [DisplayName("File Description")]
        public string Description { get; set; }
        //IFormFile allows for manipulating a file from the interface
        [NotMapped]
        [DataType(DataType.Upload)]
        [DisplayName("Select a file")]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf",".csv" })]
        public IFormFile FormFile { get; set; }
        [DisplayName("File Name")]
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        [DisplayName("File Extension")]
        public string FileContentType { get; set; }

        //Navigation properties
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}
