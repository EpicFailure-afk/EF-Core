using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirst {
  internal class Program {
    static void Main(string[] args) {
      Context context = new Context();

      /*
      context.Departments.Add(new Department {
        Name = "SD"
      });

      context.SaveChanges();
      */

      foreach (var item in context.Departments) {
        Console.WriteLine(item.Name);
      }

    }
  }
}
