using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyncCloud.Models
{
    public class FileReferenceModel
    {
        public Guid UserId { get; set; }
        public string FileId { get; set; }
        public string UserName { get; set; }
        public string FileName { get; set; }
        public ulong FileSize { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}