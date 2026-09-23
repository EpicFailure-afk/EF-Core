using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirst {
  internal class Context : DbContext {
    // create constructor to connect to db
    public Context() : base(@"Data source= localhost\SQLEXPRESS; initial catalog = Intake46; Integrated security = true") {

    }
    
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
  }
}
