namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addconsumertype : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Consumers", "Type", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Consumers", "Type");
        }
    }
}
