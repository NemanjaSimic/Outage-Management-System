namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Commonfieldsandnullableproperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ArchivedOutages", "OutageElementGid", c => c.Long(nullable: false));
            AddColumn("dbo.ArchivedOutages", "IsolatedTime", c => c.DateTime());
            AddColumn("dbo.ArchivedOutages", "ResolvedTime", c => c.DateTime());
            AddColumn("dbo.ArchivedOutages", "OutageState", c => c.Short(nullable: false));
            AlterColumn("dbo.ActiveOutages", "IsolatedTime", c => c.DateTime());
            AlterColumn("dbo.ActiveOutages", "ResolvedTime", c => c.DateTime());
            DropColumn("dbo.ActiveOutages", "ElementGid");
            DropColumn("dbo.ArchivedOutages", "ElementGid");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ArchivedOutages", "ElementGid", c => c.Long(nullable: false));
            AddColumn("dbo.ActiveOutages", "ElementGid", c => c.Long(nullable: false));
            AlterColumn("dbo.ActiveOutages", "ResolvedTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ActiveOutages", "IsolatedTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.ArchivedOutages", "OutageState");
            DropColumn("dbo.ArchivedOutages", "ResolvedTime");
            DropColumn("dbo.ArchivedOutages", "IsolatedTime");
            DropColumn("dbo.ArchivedOutages", "OutageElementGid");
        }
    }
}
