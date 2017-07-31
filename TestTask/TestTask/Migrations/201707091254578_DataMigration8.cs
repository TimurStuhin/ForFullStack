namespace TestTask.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration8 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ResultModels", "MiddleScore");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ResultModels", "MiddleScore", c => c.Int(nullable: false));
        }
    }
}
