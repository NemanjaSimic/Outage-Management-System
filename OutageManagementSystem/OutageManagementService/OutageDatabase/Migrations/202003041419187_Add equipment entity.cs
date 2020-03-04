namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addequipmententity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Equipments",
                c => new
                    {
                        EquipmentId = c.Long(nullable: false),
                        EquipmentMRID = c.String(),
                        ArchivedOutage_OutageId = c.Long(),
                        ArchivedOutage_OutageId1 = c.Long(),
                        ActiveOutage_OutageId = c.Long(),
                        ActiveOutage_OutageId1 = c.Long(),
                    })
                .PrimaryKey(t => t.EquipmentId)
                .ForeignKey("dbo.ArchivedOutages", t => t.ArchivedOutage_OutageId)
                .ForeignKey("dbo.ArchivedOutages", t => t.ArchivedOutage_OutageId1)
                .ForeignKey("dbo.ActiveOutages", t => t.ActiveOutage_OutageId)
                .ForeignKey("dbo.ActiveOutages", t => t.ActiveOutage_OutageId1)
                .Index(t => t.ArchivedOutage_OutageId)
                .Index(t => t.ArchivedOutage_OutageId1)
                .Index(t => t.ActiveOutage_OutageId)
                .Index(t => t.ActiveOutage_OutageId1);
            
            AddColumn("dbo.ActiveOutages", "Equipment_EquipmentId", c => c.Long());
            AddColumn("dbo.ArchivedOutages", "Equipment_EquipmentId", c => c.Long());
            CreateIndex("dbo.ActiveOutages", "Equipment_EquipmentId");
            CreateIndex("dbo.ArchivedOutages", "Equipment_EquipmentId");
            AddForeignKey("dbo.ActiveOutages", "Equipment_EquipmentId", "dbo.Equipments", "EquipmentId");
            AddForeignKey("dbo.ArchivedOutages", "Equipment_EquipmentId", "dbo.Equipments", "EquipmentId");
            DropColumn("dbo.ActiveOutages", "DefaultIsolationPoints");
            DropColumn("dbo.ActiveOutages", "OptimumIsolationPoints");
            DropColumn("dbo.ArchivedOutages", "DefaultIsolationPoints");
            DropColumn("dbo.ArchivedOutages", "OptimumIsolationPoints");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ArchivedOutages", "OptimumIsolationPoints", c => c.String());
            AddColumn("dbo.ArchivedOutages", "DefaultIsolationPoints", c => c.String());
            AddColumn("dbo.ActiveOutages", "OptimumIsolationPoints", c => c.String());
            AddColumn("dbo.ActiveOutages", "DefaultIsolationPoints", c => c.String());
            DropForeignKey("dbo.Equipments", "ActiveOutage_OutageId1", "dbo.ActiveOutages");
            DropForeignKey("dbo.Equipments", "ActiveOutage_OutageId", "dbo.ActiveOutages");
            DropForeignKey("dbo.Equipments", "ArchivedOutage_OutageId1", "dbo.ArchivedOutages");
            DropForeignKey("dbo.Equipments", "ArchivedOutage_OutageId", "dbo.ArchivedOutages");
            DropForeignKey("dbo.ArchivedOutages", "Equipment_EquipmentId", "dbo.Equipments");
            DropForeignKey("dbo.ActiveOutages", "Equipment_EquipmentId", "dbo.Equipments");
            DropIndex("dbo.Equipments", new[] { "ActiveOutage_OutageId1" });
            DropIndex("dbo.Equipments", new[] { "ActiveOutage_OutageId" });
            DropIndex("dbo.Equipments", new[] { "ArchivedOutage_OutageId1" });
            DropIndex("dbo.Equipments", new[] { "ArchivedOutage_OutageId" });
            DropIndex("dbo.ArchivedOutages", new[] { "Equipment_EquipmentId" });
            DropIndex("dbo.ActiveOutages", new[] { "Equipment_EquipmentId" });
            DropColumn("dbo.ArchivedOutages", "Equipment_EquipmentId");
            DropColumn("dbo.ActiveOutages", "Equipment_EquipmentId");
            DropTable("dbo.Equipments");
        }
    }
}
