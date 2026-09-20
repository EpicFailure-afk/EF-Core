using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day1 {
  internal class Program {
    static void Main(string[] args) {
      CompanyEntities context = new CompanyEntities();
      //this preoccess connected mode 

      //context.Database.Log = Log => Console.WriteLine(Log);
      // - shows logs at the output
      // - in thyat case it shows the db query that will go to database

      // query operator 
      var query = context.Departments.Where(c => c.ID == 2);
      // Where here is not the Where which was in LINQ
      // it's not work on IEnumerable 
      // it Implements IQueryable --> contains group of lambda expressions that will be converted to some sql queries
      // Where here generates the sql query that will go to database
      // Where will be executed at server side (database)

      var id = int.Parse("2");
      // query expression
      var query_3 =
        from dept in context.Departments
        where dept.ID == id
        select dept.Name;

      var query_2 = context.Departments.ToList().Where(c => c.ID == 2);
      // Where here Implements IEnumerable 
      // this fetchs all data to memory then perform the filteration 
      // performansce leak 
      // Where here will be executed at client side (visual studio)

      foreach (var dept in query) {
        Console.WriteLine(dept.Name);
      }
    }
  }
}
