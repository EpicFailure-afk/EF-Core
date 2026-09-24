using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirst {
  // Data Annotation 
  [Table("Department", Schema ="HR")]
  internal class Department {
    public int ID { get; set; }
    public string Name { get; set; }

    [Required]
    public string Location { get; set; }

    public virtual ICollection<Employee> Employees { get; set; }
    
    // add relation 1-many between Department and Project
    public virtual ICollection<Project> Projects { get; set; }
  }
}
