namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResolvedTimetoRepairedTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActiveOutages", "RepairedTime", c => c.DateTime());
            AddColumn("dbo.ArchivedOutages", "RepairedTime", c => c.DateTime());
            DropColumn("dbo.ActiveOutages", "ResolvedTime");
            DropColumn("dbo.ArchivedOutages", "ResolvedTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ArchivedOutages", "ResolvedTime", c => c.DateTime());
            AddColumn("dbo.ActiveOutages", "ResolvedTime", c => c.DateTime());
            DropColumn("dbo.ArchivedOutages", "RepairedTime");
            DropColumn("dbo.ActiveOutages", "RepairedTime");
        }
    }
}
