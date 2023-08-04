using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheBugTrackerApp.Services.Interfaces;

namespace TheBugTrackerApp.Services
{
    public class BTFileService : IBTFileService
    {
        private readonly string[] suffixes = { "Bytes","KB","MB","GB","TB","PB"};
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            try
            {
                string file = Convert.ToBase64String(fileData);
                string test = string.Format($"data:{extension};base64,{file}");
                return string.Format($"data:{extension};base64,{file}");
               
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error converting byte array to file {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    byte[] byteFile = memoryStream.ToArray();

                    memoryStream.Close();

                    return byteFile;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error converting file to byte array {ex.Message}");
                throw;
            }
        }

        public string FormatFileSize(long bytes)
        {
            decimal fileSize = bytes;
            int count = 0;

            while (Math.Round(fileSize / 1024) >= 1)
            {
                fileSize /= 1024;
                count++;
            }
            
            return string.Format("{0:n1}{1}", fileSize, suffixes[count]);
        }

        public string GetFileIcon(string file)
        {
            string fileImage = "default";

            if (!string.IsNullOrWhiteSpace(file))
            {
                fileImage = Path.GetExtension(file).Replace(".", "");
                return $"/img/png/{fileImage}.png";
            }

            return fileImage;
        }
    }
}
