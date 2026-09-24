namespace CodeFirst.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Department_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("HR.Department", t => t.Department_ID)
                .Index(t => t.Department_ID);
            
            AddColumn("dbo.Employees", "DepartmentID", c => c.Int(nullable: false));
            CreateIndex("dbo.Employees", "DepartmentID");
            AddForeignKey("dbo.Employees", "DepartmentID", "HR.Department", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Projects", "Department_ID", "HR.Department");
            DropForeignKey("dbo.Employees", "DepartmentID", "HR.Department");
            DropIndex("dbo.Projects", new[] { "Department_ID" });
            DropIndex("dbo.Employees", new[] { "DepartmentID" });
            DropColumn("dbo.Employees", "DepartmentID");
            DropTable("dbo.Projects");
        }
    }
}
