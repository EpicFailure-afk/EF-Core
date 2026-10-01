using System;
using System.Collections.Generic;
using System.Text;

namespace EF_core_1 {
  internal class Department {
    public int ID { get; set; }
    public string Name { get; set; }

    public int BranchID { get; set; } // prop to link Branch with Department

    // navigation prop
    public ICollection<Employee> Employees { get; set; }
    public ICollection<Project> Projects { get; set; }

    public Branch Branch { get; set; }
  }
}
