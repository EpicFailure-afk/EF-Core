using System;
using System.Collections.Generic;
using System.Data.Entity;
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

      // Day-1

      #region EF in Action 

      /*

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

      //foreach (var dept in query) {
      //  Console.WriteLine(dept.Name);
      //}

      // --------------------Day-2--------------------
      // Navigation properties
      // - group of properties that link classes to each other 

      //var dept_1 = context.Departments.First();
      var dept_1 = context.Departments.Find(3);
      // - Find(3) --> return the object or record of ID 3

      foreach (var emp in dept_1.Employees) {
        Console.WriteLine(emp.Name);
      }

      */

      #endregion

      Console.WriteLine("---------------------------------------------");

      // how to perform update, insert and delete 
      #region update
      /

      // update
      var dept_2 = context.Departments.First();
      dept_2.Name = "Intake 42";
      context.SaveChanges();
      
      /*
      
      #endregion

      // know it exists
      //context.Entry(dept_2).State = System.Data.Entity.EntityState.Added;

      #region read only data
      var query_4 =
        (from d in context.Departments
         where d.ID > 10
         select d.ID).AsNoTracking();
      // context will not track them
      // means any change will affect the data in mem only  
      #endregion

      /*  diff between Find and Single 
      var dept = context.Departments.Find(3);
      var dept = context.Departments.Single(d => d.ID == 3);
      */

      #region insert

      /*
      var dept_4 = new Department { Name = "INT 46" };
      dept_4.Employees = new List<Employee> {
        new Employee {Name = "A1", BirthDate = DateTime.Now}
      };
      context.Departments.Add(dept_4);
      // after Add(), the state of entry will be "Added"
      // context will find that there are 2 objects of Employees also added so it will be added  
      context.SaveChanges();
      */

      // Another way
      
      /*
      var dept_5 = new Department { Name = "askjdhg" };

      var emp = new Employee {
        Name = "jhsdgf",
        BirthDate = DateTime.Now,
        Department = dept_5 
      };

      context.Employees.Add(emp);
      context.SaveChanges();
      */
      
      #endregion

      #region delete
      var dept_6 = context.Departments.Find(60);
      //context.Departments.Remove(dept_6);
      //context.SaveChanges();

      #endregion


    }
  }
}
