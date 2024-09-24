using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Models
{
    [Serializable]
	public class Photo
	{
        [Key]
        public int PhotoID { get; set; }
		public int AlbumID { get; set; }
        public string Caption { get; set; }
        public byte[] BytesOriginal { get; set; }
        public byte[] BytesFull { get; set; }
        public byte[] BytesPoster { get; set; }
        public byte[] BytesThumb { get; set; }
    }
}
