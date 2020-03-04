namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActiveOutages",
                c => new
                    {
                        OutageId = c.Long(nullable: false, identity: true),
                        ElementGid = c.Long(nullable: false),
                        ReportTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OutageId);
            
            CreateTable(
                "dbo.Consumers",
                c => new
                    {
                        ConsumerId = c.Long(nullable: false),
                        ConsumerMRID = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                    })
                .PrimaryKey(t => t.ConsumerId);
            
            CreateTable(
                "dbo.ArchivedOutages",
                c => new
                    {
                        OutageId = c.Long(nullable: false),
                        ElementGid = c.Long(nullable: false),
                        ReportTime = c.DateTime(nullable: false),
                        ArchiveTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OutageId);
            
            CreateTable(
                "dbo.ConsumerActiveOutages",
                c => new
                    {
                        Consumer_ConsumerId = c.Long(nullable: false),
                        ActiveOutage_OutageId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Consumer_ConsumerId, t.ActiveOutage_OutageId })
                .ForeignKey("dbo.Consumers", t => t.Consumer_ConsumerId, cascadeDelete: true)
                .ForeignKey("dbo.ActiveOutages", t => t.ActiveOutage_OutageId, cascadeDelete: true)
                .Index(t => t.Consumer_ConsumerId)
                .Index(t => t.ActiveOutage_OutageId);
            
            CreateTable(
                "dbo.ArchivedOutageConsumers",
                c => new
                    {
                        ArchivedOutage_OutageId = c.Long(nullable: false),
                        Consumer_ConsumerId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.ArchivedOutage_OutageId, t.Consumer_ConsumerId })
                .ForeignKey("dbo.ArchivedOutages", t => t.ArchivedOutage_OutageId, cascadeDelete: true)
                .ForeignKey("dbo.Consumers", t => t.Consumer_ConsumerId, cascadeDelete: true)
                .Index(t => t.ArchivedOutage_OutageId)
                .Index(t => t.Consumer_ConsumerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ArchivedOutageConsumers", "Consumer_ConsumerId", "dbo.Consumers");
            DropForeignKey("dbo.ArchivedOutageConsumers", "ArchivedOutage_OutageId", "dbo.ArchivedOutages");
            DropForeignKey("dbo.ConsumerActiveOutages", "ActiveOutage_OutageId", "dbo.ActiveOutages");
            DropForeignKey("dbo.ConsumerActiveOutages", "Consumer_ConsumerId", "dbo.Consumers");
            DropIndex("dbo.ArchivedOutageConsumers", new[] { "Consumer_ConsumerId" });
            DropIndex("dbo.ArchivedOutageConsumers", new[] { "ArchivedOutage_OutageId" });
            DropIndex("dbo.ConsumerActiveOutages", new[] { "ActiveOutage_OutageId" });
            DropIndex("dbo.ConsumerActiveOutages", new[] { "Consumer_ConsumerId" });
            DropTable("dbo.ArchivedOutageConsumers");
            DropTable("dbo.ConsumerActiveOutages");
            DropTable("dbo.ArchivedOutages");
            DropTable("dbo.Consumers");
            DropTable("dbo.ActiveOutages");
        }
    }
}
