using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MI_Cars.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public int RentalId { get; set; }
        public string Path { get; set; }       // путь к файлу
        public string PhotoType { get; set; }  // "issue" или "return"
    }
}
