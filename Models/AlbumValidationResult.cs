using ServerAPI.ViewModels;
using System.Collections.Generic;

namespace ServerAPI.Models
{
    public class AlbumValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public AlbumViewModel Album { get; set; }
    }
}
