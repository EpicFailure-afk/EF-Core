using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EF_core_1 {
  [Table("Branch")] // change the name of the table
  internal class Branch {
    public int ID { get; set; }

    [Required]  // data annotation 
    public string Name { get; set; }

    // nav-props
    public ICollection<Department> Departments { get; set; }
  }
}
