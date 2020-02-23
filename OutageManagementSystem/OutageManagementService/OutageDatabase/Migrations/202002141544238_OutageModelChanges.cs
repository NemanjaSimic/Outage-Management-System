namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OutageModelChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActiveOutages", "OutageElementGid", c => c.Long(nullable: false));
            AddColumn("dbo.ActiveOutages", "CreatedTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.ActiveOutages", "IsolatedTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.ActiveOutages", "ResolvedTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.ActiveOutages", "OutageState", c => c.Short(nullable: false));
            DropColumn("dbo.ActiveOutages", "ElementGid");
            DropColumn("dbo.ActiveOutages", "ReportTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActiveOutages", "ReportTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.ActiveOutages", "ElementGid", c => c.Long(nullable: false));
            DropColumn("dbo.ActiveOutages", "OutageState");
            DropColumn("dbo.ActiveOutages", "ResolvedTime");
            DropColumn("dbo.ActiveOutages", "IsolatedTime");
            DropColumn("dbo.ActiveOutages", "CreatedTime");
            DropColumn("dbo.ActiveOutages", "OutageElementGid");
        }
    }
}
