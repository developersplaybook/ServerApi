using System;

namespace ServerAPI.ViewModels
{
    [Serializable]
    public class PhotoViewModel
    {
        public int PhotoID { get; set; }
        public int AlbumID { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string AlbumCaption { get; set; } = string.Empty;
    }
}
