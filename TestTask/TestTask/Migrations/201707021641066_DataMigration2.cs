namespace TestTask.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubjectModels", "Type", c => c.String());
            DropColumn("dbo.SubjectModels", "CountStudent");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SubjectModels", "CountStudent", c => c.Int(nullable: false));
            DropColumn("dbo.SubjectModels", "Type");
        }
    }
}
