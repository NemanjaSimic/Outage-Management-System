namespace OutageDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HistoricalDatabase : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.EquipmentsHistorical");
            DropTable("dbo.ConsumersHistorical");
        }
    }
}
