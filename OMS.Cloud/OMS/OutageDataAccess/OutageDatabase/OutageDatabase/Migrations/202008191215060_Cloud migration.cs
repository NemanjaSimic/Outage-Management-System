namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cloudmigration : DbMigration
    {
        public override void Up()
        {
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
                "dbo.ConsumersHistorical",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ConsumerId = c.Long(nullable: false),
                        OutageId = c.Long(),
                        OperationTime = c.DateTime(nullable: false),
                        DatabaseOperation = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EquipmentsHistorical",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        EquipmentId = c.Long(nullable: false),
                        OutageId = c.Long(),
                        OperationTime = c.DateTime(nullable: false),
                        DatabaseOperation = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
            
        }
        
        public override void Down()
        {
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
            DropTable("dbo.EquipmentsHistorical");
            DropTable("dbo.ConsumersHistorical");
            DropTable("dbo.Equipments");
            DropTable("dbo.OutageEntities");
            DropTable("dbo.Consumers");
        }
    }
}
