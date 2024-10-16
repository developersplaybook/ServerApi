using System;

namespace ServerAPI.Models
{
    [Serializable]
	public class PhotoSlim
	{
        public int PhotoID { get; set; }
		public int AlbumID { get; set; }
        public string Caption { get; set; } = string.Empty;
    }
}
