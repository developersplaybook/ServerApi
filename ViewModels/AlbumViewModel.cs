using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class AlbumViewModel
    {
        public int AlbumID { get; set; }
        public string Caption { get; set; }
        public bool IsPublic { get; set; }
        public int PhotoCount { get; set; }
    }
}

