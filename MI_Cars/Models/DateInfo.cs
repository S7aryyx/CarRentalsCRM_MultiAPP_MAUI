using System;
using System.Collections.Generic;

namespace MI_Cars.Models
{
    public class DateInfo
    {
        public DateTime Date { get; set; }
        public Rental Rental { get; set; } // null = свободно, иначе занято
    }
}
