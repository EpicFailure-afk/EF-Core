using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EF_core_1
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
            // var query =
            //     (context.Departments.AsSingleQuery()
            //         .Include(d => d.Projects)
            //         .Include(d => d.Employees) // eager loading
            //         .ThenInclude(e => e.Attendances)).ToList();

            // one object from Department has collection of Employees and collection of Projects
            // one object from Employee has collection of Attendance we will use (ThenInclude())
            // AsSplitQuery() and ToList, prevent from multi join among 4 tables
            #endregion

            var query_2 = context.Departments.ToList();
            // that will get the Departments with out any collections related to it
            //how to access Employees?
            // by explicit loading:

            #region ExplicitLoading

            /*
                foreach (var dept in query_2)
                {
                    #region ExplicitLoading

                    context.Entry(dept).Collection(c => c.Employees).Load();
                    context.Entry(dept).Reference(o => o.Branch).Load();

                    // where condition with Explicit loading
                    var emps = context.Entry(dept)
                        .Collection(c => c.Employees)
                        .Query()
                        .Where(e => e.ID < 10);

                    #endregion

                    foreach (var emp in dept.Employees)
                    {
                        Console.WriteLine(emp.Name);
                    }

                    Console.WriteLine(dept.Name);
                }
            */
            #endregion

            #region Client Vs. Sever Evaluation
            /*
                var query_3 =
                    (from d in context.Departments
                     select string.Join(':', "Dept", d.Name)).ToList();
            */
            #endregion

            #region using EF.Functions: database functions
            var query_4 =
                (from d in context.Departments
                 where d.Name.Contains("d")  // in db --> where d.name like '%d%'
                 select d);

            // use like here
            var query_5 =
                (from d in context.Departments
                 where EF.Functions.Like(d.Name, "%d%")
                 select d);

            #endregion

            #region  NoTracking(readOnly)
            var query_6 =
                // context.Employees.AsNoTracking()
                context.Employees.AsNoTrackingWithIdentityResolution()
                    .Include(e => e.Department);

            #endregion

            #region Global query filter
            // if i have a property at Employee is called Deleted that says that emp is deleted or not
            // everytime i should check manually if that record is deleted or not
            // if i forget this will give me logical errors
            var query_7 =
                from e in context.Employees
                where e.ID > 10
                select e;
            #endregion

            #region ShadowProp
            // access deleted prop

            /*
                var dept = context.Departments.First();
                context.Entry(dept).Property("Deleted").CurrentValue = true;
                context.SaveChanges();
            */

            // get all deleted depts
            var query_8 =
                from d in context.Departments
                where EF.Property<bool>(d, "Deleted") == true
                select d;
            #endregion

        }
    }
}
