CREATE PROCEDURE [dbo].[Procedure]
	@param1 int = 0,
	@param2 int
AS
	DELETE FROM [dbo].[Consumers];
	DELETE FROM [dbo].[ConsumersHistorical];
	DELETE FROM [dbo].[Equipments];
	DELETE FROM [dbo].[EquipmentsHistorical];
	DELETE FROM [dbo].[OutageEntities];
GO
