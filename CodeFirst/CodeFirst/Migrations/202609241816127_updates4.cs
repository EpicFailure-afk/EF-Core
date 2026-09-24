namespace CodeFirst.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates4 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WorksFors",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Hours = c.Int(nullable: false),
                        Employee_ID = c.Int(),
                        Project_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Employees", t => t.Employee_ID)
                .ForeignKey("dbo.Projects", t => t.Project_ID)
                .Index(t => t.Employee_ID)
                .Index(t => t.Project_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WorksFors", "Project_ID", "dbo.Projects");
            DropForeignKey("dbo.WorksFors", "Employee_ID", "dbo.Employees");
            DropIndex("dbo.WorksFors", new[] { "Project_ID" });
            DropIndex("dbo.WorksFors", new[] { "Employee_ID" });
            DropTable("dbo.WorksFors");
        }
    }
}
