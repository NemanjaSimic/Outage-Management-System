namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fields_in_model : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActiveOutages", "ElementGid", c => c.Long(nullable: false));
            AddColumn("dbo.ActiveOutages", "ReportTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.ActiveOutages", "CreatedTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActiveOutages", "CreatedTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.ActiveOutages", "ReportTime");
            DropColumn("dbo.ActiveOutages", "ElementGid");
        }
    }
}
