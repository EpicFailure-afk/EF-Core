using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace EF_core_1 {
  internal class Program {
    static void Main(string[] args) {
      Context context = new Context();

      #region query expression
      /*
      var query =
        from d in context.Departments
        where d.ID < 4
        select d;
      */
      #endregion

      #region eager loading
      var query =
        context.Departments
        .Include(d => d.Projects)
        .Include(d => d.Employees) // eager loading 
        .ThenInclude(e => e.Attendances);
      // one object from Department has collection of Employees and collection of Projects
      // one object from Employee has collection of Attendance we will use (ThenInclude())
      #endregion


      foreach (var dept in query) {
        foreach (var emp in dept.Employees) {
          Console.WriteLine(emp.Name);
        }
        Console.WriteLine(dept.Name);
      }
    }
  }
}
