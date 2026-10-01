using System;
using System.Collections.Generic;
using System.Text;

namespace EF_core_1 {
  internal class Employee {
    public int ID { get; set; }
    public string Name { get; set; }

    public int DepartmentID { get; set; } // prop to link between Employee and Department 
    // navigation prop
    public Department Department { get; set; }
    public ICollection<Attendance> Attendances { get; set; }
  }
}
