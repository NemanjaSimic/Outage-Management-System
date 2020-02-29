namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PropertyIsResolveConditionValidated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActiveOutages", "IsResolveConditionValidated", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActiveOutages", "IsResolveConditionValidated");
        }
    }
}
