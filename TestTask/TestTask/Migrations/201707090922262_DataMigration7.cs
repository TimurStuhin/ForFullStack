namespace TestTask.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ResultModels", "MiddleScore", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ResultModels", "MiddleScore");
        }
    }
}
