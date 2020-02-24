namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modelrefactor : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ArchivedOutages", "OutageState");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ArchivedOutages", "OutageState", c => c.Short(nullable: false));
        }
    }
}
