namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Listpropertiestostring : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActiveOutages", "DefaultIsolationPoints", c => c.String());
            AddColumn("dbo.ActiveOutages", "OptimumIsolationPoints", c => c.String());
            AddColumn("dbo.ArchivedOutages", "DefaultIsolationPoints", c => c.String());
            AddColumn("dbo.ArchivedOutages", "OptimumIsolationPoints", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ArchivedOutages", "OptimumIsolationPoints");
            DropColumn("dbo.ArchivedOutages", "DefaultIsolationPoints");
            DropColumn("dbo.ActiveOutages", "OptimumIsolationPoints");
            DropColumn("dbo.ActiveOutages", "DefaultIsolationPoints");
        }
    }
}
