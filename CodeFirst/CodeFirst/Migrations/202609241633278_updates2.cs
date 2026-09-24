namespace CodeFirst.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates2 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Departments", newName: "Department");
            MoveTable(name: "dbo.Department", newSchema: "HR");
            RenameColumn(table: "dbo.Employees", name: "Name", newName: "FullName");
            AddColumn("HR.Department", "Location", c => c.String(nullable: false, defaultValue: ""));
            AddColumn("dbo.Employees", "Birthdate", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.Employees", "FullName", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Employees", "FullName", c => c.String());
            DropColumn("dbo.Employees", "Birthdate");
            DropColumn("HR.Department", "Location");
            RenameColumn(table: "dbo.Employees", name: "FullName", newName: "Name");
            MoveTable(name: "HR.Department", newSchema: "dbo");
            RenameTable(name: "dbo.Department", newName: "Departments");
        }
    }
}
