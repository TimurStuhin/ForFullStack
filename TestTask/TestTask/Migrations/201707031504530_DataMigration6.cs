namespace TestTask.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubjectModels", "SubjectId", c => c.Int(nullable: false));
            DropColumn("dbo.SubjectModels", "Subject");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SubjectModels", "Subject", c => c.String());
            DropColumn("dbo.SubjectModels", "SubjectId");
        }
    }
}
