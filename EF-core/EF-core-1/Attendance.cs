using System;
using System.Collections.Generic;
using System.Text;

namespace EF_core_1
{
    internal class Attendance
    {
        public int EmployeeID { get; set; }
        public DateTime Date { get; set; }
        // i wanna do both of them Primary key
        // this will be done through Fluent API at ModelBuilder

        // nav-prop
        public virtual Employee Employee { get; set; }
    }
}
