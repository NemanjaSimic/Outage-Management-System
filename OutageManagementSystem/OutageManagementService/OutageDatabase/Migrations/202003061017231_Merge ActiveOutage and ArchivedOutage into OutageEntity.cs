namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MergeActiveOutageandArchivedOutageintoOutageEntity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ConsumerActiveOutages", "Consumer_ConsumerId", "dbo.Consumers");
            DropForeignKey("dbo.ConsumerActiveOutages", "ActiveOutage_OutageId", "dbo.ActiveOutages");
            DropForeignKey("dbo.ArchivedOutageConsumers", "ArchivedOutage_OutageId", "dbo.ArchivedOutages");
            DropForeignKey("dbo.ArchivedOutageConsumers", "Consumer_ConsumerId", "dbo.Consumers");
            DropIndex("dbo.ConsumerActiveOutages", new[] { "Consumer_ConsumerId" });
            DropIndex("dbo.ConsumerActiveOutages", new[] { "ActiveOutage_OutageId" });
            DropIndex("dbo.ArchivedOutageConsumers", new[] { "ArchivedOutage_OutageId" });
            DropIndex("dbo.ArchivedOutageConsumers", new[] { "Consumer_ConsumerId" });
            CreateTable(
                "dbo.OutageEntities",
                c => new
                    {
                        OutageId = c.Long(nullable: false, identity: true),
                        OutageElementGid = c.Long(nullable: false),
                        OutageState = c.Short(nullable: false),
                        ReportTime = c.DateTime(nullable: false),
                        IsolatedTime = c.DateTime(),
                        RepairedTime = c.DateTime(),
                        ArchivedTime = c.DateTime(),
                        IsResolveConditionValidated = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.OutageId);
            
            CreateTable(
                "dbo.Equipments",
                c => new
                    {
                        EquipmentId = c.Long(nullable: false),
                        EquipmentMRID = c.String(),
                    })
                .PrimaryKey(t => t.EquipmentId);
            
            CreateTable(
                "dbo.OutageConsumers",
                c => new
                    {
                        OutageRefId = c.Long(nullable: false),
                        ConsumerRefId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.OutageRefId, t.ConsumerRefId })
                .ForeignKey("dbo.OutageEntities", t => t.OutageRefId, cascadeDelete: true)
                .ForeignKey("dbo.Consumers", t => t.ConsumerRefId, cascadeDelete: true)
                .Index(t => t.OutageRefId)
                .Index(t => t.ConsumerRefId);
            
            CreateTable(
                "dbo.OutageDefaultEquipments",
                c => new
                    {
                        OutageRefId = c.Long(nullable: false),
                        DefaultEquipmentRefId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.OutageRefId, t.DefaultEquipmentRefId })
                .ForeignKey("dbo.OutageEntities", t => t.OutageRefId, cascadeDelete: true)
                .ForeignKey("dbo.Equipments", t => t.DefaultEquipmentRefId, cascadeDelete: true)
                .Index(t => t.OutageRefId)
                .Index(t => t.DefaultEquipmentRefId);
            
            CreateTable(
                "dbo.OutageOptimumEquipments",
                c => new
                    {
                        OutageRefId = c.Long(nullable: false),
                        OptimumEquipmentRefId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.OutageRefId, t.OptimumEquipmentRefId })
                .ForeignKey("dbo.OutageEntities", t => t.OutageRefId, cascadeDelete: true)
                .ForeignKey("dbo.Equipments", t => t.OptimumEquipmentRefId, cascadeDelete: true)
                .Index(t => t.OutageRefId)
                .Index(t => t.OptimumEquipmentRefId);
            
            DropTable("dbo.ActiveOutages");
            DropTable("dbo.ArchivedOutages");
            DropTable("dbo.ConsumerActiveOutages");
            DropTable("dbo.ArchivedOutageConsumers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ArchivedOutageConsumers",
                c => new
                    {
                        ArchivedOutage_OutageId = c.Long(nullable: false),
                        Consumer_ConsumerId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.ArchivedOutage_OutageId, t.Consumer_ConsumerId });
            
            CreateTable(
                "dbo.ConsumerActiveOutages",
                c => new
                    {
                        Consumer_ConsumerId = c.Long(nullable: false),
                        ActiveOutage_OutageId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Consumer_ConsumerId, t.ActiveOutage_OutageId });
            
            CreateTable(
                "dbo.ArchivedOutages",
                c => new
                    {
                        OutageId = c.Long(nullable: false),
                        ArchiveTime = c.DateTime(nullable: false),
                        ReportTime = c.DateTime(nullable: false),
                        IsolatedTime = c.DateTime(),
                        RepairedTime = c.DateTime(),
                        OutageElementGid = c.Long(nullable: false),
                        DefaultIsolationPoints = c.String(),
                        OptimumIsolationPoints = c.String(),
                    })
                .PrimaryKey(t => t.OutageId);
            
            CreateTable(
                "dbo.ActiveOutages",
                c => new
                    {
                        OutageId = c.Long(nullable: false, identity: true),
                        OutageState = c.Short(nullable: false),
                        IsResolveConditionValidated = c.Boolean(nullable: false),
                        ReportTime = c.DateTime(nullable: false),
                        IsolatedTime = c.DateTime(),
                        RepairedTime = c.DateTime(),
                        OutageElementGid = c.Long(nullable: false),
                        DefaultIsolationPoints = c.String(),
                        OptimumIsolationPoints = c.String(),
                    })
                .PrimaryKey(t => t.OutageId);
            
            DropForeignKey("dbo.OutageOptimumEquipments", "OptimumEquipmentRefId", "dbo.Equipments");
            DropForeignKey("dbo.OutageOptimumEquipments", "OutageRefId", "dbo.OutageEntities");
            DropForeignKey("dbo.OutageDefaultEquipments", "DefaultEquipmentRefId", "dbo.Equipments");
            DropForeignKey("dbo.OutageDefaultEquipments", "OutageRefId", "dbo.OutageEntities");
            DropForeignKey("dbo.OutageConsumers", "ConsumerRefId", "dbo.Consumers");
            DropForeignKey("dbo.OutageConsumers", "OutageRefId", "dbo.OutageEntities");
            DropIndex("dbo.OutageOptimumEquipments", new[] { "OptimumEquipmentRefId" });
            DropIndex("dbo.OutageOptimumEquipments", new[] { "OutageRefId" });
            DropIndex("dbo.OutageDefaultEquipments", new[] { "DefaultEquipmentRefId" });
            DropIndex("dbo.OutageDefaultEquipments", new[] { "OutageRefId" });
            DropIndex("dbo.OutageConsumers", new[] { "ConsumerRefId" });
            DropIndex("dbo.OutageConsumers", new[] { "OutageRefId" });
            DropTable("dbo.OutageOptimumEquipments");
            DropTable("dbo.OutageDefaultEquipments");
            DropTable("dbo.OutageConsumers");
            DropTable("dbo.Equipments");
            DropTable("dbo.OutageEntities");
            CreateIndex("dbo.ArchivedOutageConsumers", "Consumer_ConsumerId");
            CreateIndex("dbo.ArchivedOutageConsumers", "ArchivedOutage_OutageId");
            CreateIndex("dbo.ConsumerActiveOutages", "ActiveOutage_OutageId");
            CreateIndex("dbo.ConsumerActiveOutages", "Consumer_ConsumerId");
            AddForeignKey("dbo.ArchivedOutageConsumers", "Consumer_ConsumerId", "dbo.Consumers", "ConsumerId", cascadeDelete: true);
            AddForeignKey("dbo.ArchivedOutageConsumers", "ArchivedOutage_OutageId", "dbo.ArchivedOutages", "OutageId", cascadeDelete: true);
            AddForeignKey("dbo.ConsumerActiveOutages", "ActiveOutage_OutageId", "dbo.ActiveOutages", "OutageId", cascadeDelete: true);
            AddForeignKey("dbo.ConsumerActiveOutages", "Consumer_ConsumerId", "dbo.Consumers", "ConsumerId", cascadeDelete: true);
        }
    }
}
