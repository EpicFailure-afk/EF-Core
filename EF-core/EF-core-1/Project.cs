using System;
using System.Collections.Generic;
using System.Text;

namespace EF_core_1 {
  internal class Project {
    public int ID{ get; set; }
    public string Name { get; set; }

    public int DepartmentID { get; set; }

    //nav-prop
    public Department department { get; set; }
  }
}
