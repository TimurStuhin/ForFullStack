namespace TestTask.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration5 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ResultModels", "UserId", c => c.String());
            AlterColumn("dbo.SubjectModels", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SubjectModels", "UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.ResultModels", "UserId", c => c.Int(nullable: false));
        }
    }
}
