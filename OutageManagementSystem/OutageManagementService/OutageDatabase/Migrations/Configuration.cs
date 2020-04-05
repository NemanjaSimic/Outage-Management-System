namespace OutageDatabase.Migrations
{
	using OMSCommon.OutageDatabaseModel;
	using Outage.Common;
	using OutageDatabase.Repository;
	using System;
	using System.Collections.Generic;
	using System.Data.Entity.Migrations;

	internal sealed class Configuration : DbMigrationsConfiguration<OutageContext>
	{
		private ILogger logger;

		private ILogger Logger
		{
			get { return logger ?? (logger = LoggerWrapper.Instance); }
		}

		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		protected override void Seed(OutageContext outageContext)
		{
			//base.Seed(outageContext);

			UnitOfWork dbContext = new UnitOfWork(outageContext);

			//dbContext.OutageRepository.RemoveAll();
			//dbContext.ConsumerRepository.RemoveAll();
			//dbContext.EquipmentRepository.RemoveAll();

			//ovaj ceo IF zakomentarisati ako hocete da odradite update-database vise od 1 puta
			if (dbContext.ConsumerRepository.GetAll() != null)
			{
				Equipment br_1 = new Equipment() { EquipmentId = 0x0000000A00000001, EquipmentMRID = "BR_1" };
				Equipment br_2 = new Equipment() { EquipmentId = 0x0000000A00000009, EquipmentMRID = "BR_2" };
				Equipment br_4 = new Equipment() { EquipmentId = 0x0000000A0000000F, EquipmentMRID = "BR_4" };
				Equipment br_5 = new Equipment() { EquipmentId = 0x0000000A00000010, EquipmentMRID = "BR_5" };
				Equipment br_7 = new Equipment() { EquipmentId = 0x0000000A0000000E, EquipmentMRID = "BR_7" };
				Equipment br_8 = new Equipment() { EquipmentId = 0x0000000A00000013, EquipmentMRID = "BR_8" };
				Equipment rc_1 = new Equipment() { EquipmentId = 0x0000000A00000011, EquipmentMRID = "RC_1" };

				OutageEntity archivedOutage1 = new OutageEntity()
				{
					OutageId = 2,
					OutageState = OutageState.ARCHIVED,
					OutageElementGid = 0x0000000C0000001A,
					ReportTime = new DateTime(2019, 10, 5, 13, 35, 16),
					IsolatedTime = new DateTime(2019, 10, 5, 13, 43, 23),
					RepairedTime = new DateTime(2019, 10, 5, 13, 57, 42),
					ArchivedTime = new DateTime(2019, 10, 5, 14, 03, 7),
					DefaultIsolationPoints = new List<Equipment>(2)
					{
						br_2,
						rc_1
					},
					OptimumIsolationPoints = new List<Equipment>(2)
					{
						br_7,
						br_8
					},
					AffectedConsumers = new List<Consumer>(7)
					{
						dbContext.ConsumerRepository.Get(0x0000000600000008),
						dbContext.ConsumerRepository.Get(0x0000000600000009),
						dbContext.ConsumerRepository.Get(0x000000060000000A),
						dbContext.ConsumerRepository.Get(0x000000060000000B),
						dbContext.ConsumerRepository.Get(0x000000060000000D),
						dbContext.ConsumerRepository.Get(0x000000060000000E),
						dbContext.ConsumerRepository.Get(0x000000060000000F)
					},
					IsResolveConditionValidated = true
				};

				OutageEntity archivedOutage2 = new OutageEntity()
				{
					OutageId = 3,
					OutageState = OutageState.ARCHIVED,
					OutageElementGid = 0x0000000C0000001A,
					ReportTime = new DateTime(2019, 11, 12, 10, 25, 33),
					IsolatedTime = new DateTime(2019, 11, 12, 10, 37, 12),
					RepairedTime = new DateTime(2019, 11, 12, 10, 44, 36),
					ArchivedTime = new DateTime(2019, 11, 12, 10, 47, 2),
					DefaultIsolationPoints = new List<Equipment>(2)
					{
						br_2,
						rc_1
					},
					OptimumIsolationPoints = new List<Equipment>(2)
					{
						br_7,
						br_8
					},
					AffectedConsumers = new List<Consumer>(7)
					{
						dbContext.ConsumerRepository.Get(0x0000000600000008),
						dbContext.ConsumerRepository.Get(0x0000000600000009),
						dbContext.ConsumerRepository.Get(0x000000060000000A),
						dbContext.ConsumerRepository.Get(0x000000060000000B),
						dbContext.ConsumerRepository.Get(0x000000060000000D),
						dbContext.ConsumerRepository.Get(0x000000060000000E),
						dbContext.ConsumerRepository.Get(0x000000060000000F)
					},
					IsResolveConditionValidated = true
				};

				OutageEntity archivedOutage3 = new OutageEntity()
				{
					OutageId = 4,
					OutageState = OutageState.ARCHIVED,
					OutageElementGid = 0x0000000C0000001A,
					ReportTime = new DateTime(2019, 12, 25, 9, 11, 35),
					IsolatedTime = new DateTime(2019, 12, 25, 10, 19, 28),
					RepairedTime = new DateTime(2019, 12, 25, 10, 24, 37),
					ArchivedTime = new DateTime(2019, 12, 25, 10, 30, 11),
					DefaultIsolationPoints = new List<Equipment>(2)
					{
						br_2,
						rc_1
					},
					OptimumIsolationPoints = new List<Equipment>(2)
					{
						br_7,
						br_8
					},
					AffectedConsumers = new List<Consumer>(7)
					{
						dbContext.ConsumerRepository.Get(0x0000000600000008),
						dbContext.ConsumerRepository.Get(0x0000000600000009),
						dbContext.ConsumerRepository.Get(0x000000060000000A),
						dbContext.ConsumerRepository.Get(0x000000060000000B),
						dbContext.ConsumerRepository.Get(0x000000060000000D),
						dbContext.ConsumerRepository.Get(0x000000060000000E),
						dbContext.ConsumerRepository.Get(0x000000060000000F)
					},
					IsResolveConditionValidated = true
				};

				OutageEntity archivedOutage4 = new OutageEntity()
				{
					OutageId = 5,
					OutageState = OutageState.ARCHIVED,
					OutageElementGid = 0x0000000C0000000D,
					ReportTime = new DateTime(2019, 12, 30, 13, 51, 12),
					IsolatedTime = new DateTime(2019, 12, 30, 14, 27, 28),
					RepairedTime = new DateTime(2019, 12, 30, 14, 42, 52),
					ArchivedTime = new DateTime(2019, 12, 30, 14, 51, 34),
					DefaultIsolationPoints = new List<Equipment>(2)
					{
						br_1,
						rc_1
					},
					OptimumIsolationPoints = new List<Equipment>(2)
					{
						br_4,
						br_5
					},
					AffectedConsumers = new List<Consumer>(9)
					{
						dbContext.ConsumerRepository.Get(0x000000060000002A),
						dbContext.ConsumerRepository.Get(0x000000060000002B),
						dbContext.ConsumerRepository.Get(0x000000060000002C),
						dbContext.ConsumerRepository.Get(0x0000000600000002),
						dbContext.ConsumerRepository.Get(0x0000000600000003),
						dbContext.ConsumerRepository.Get(0x0000000600000004),
						dbContext.ConsumerRepository.Get(0x0000000600000005),
						dbContext.ConsumerRepository.Get(0x0000000600000006),
						dbContext.ConsumerRepository.Get(0x0000000600000007)
					},
					IsResolveConditionValidated = true
				};

				OutageEntity archivedOutage5 = new OutageEntity()
				{
					OutageId = 6,
					OutageState = OutageState.ARCHIVED,
					OutageElementGid = 0x0000000C0000000D,
					ReportTime = new DateTime(2020, 1, 14, 17, 22, 41),
					IsolatedTime = new DateTime(2020, 1, 14, 17, 39, 20),
					RepairedTime = new DateTime(2020, 1, 14, 17, 56, 19),
					ArchivedTime = new DateTime(2020, 1, 14, 18, 12, 7),
					DefaultIsolationPoints = new List<Equipment>(2)
					{
						br_1,
						rc_1
					},
					OptimumIsolationPoints = new List<Equipment>(2)
					{
						br_4,
						br_5
					},
					AffectedConsumers = new List<Consumer>(9)
					{
						dbContext.ConsumerRepository.Get(0x000000060000002A),
						dbContext.ConsumerRepository.Get(0x000000060000002B),
						dbContext.ConsumerRepository.Get(0x000000060000002C),
						dbContext.ConsumerRepository.Get(0x0000000600000002),
						dbContext.ConsumerRepository.Get(0x0000000600000003),
						dbContext.ConsumerRepository.Get(0x0000000600000004),
						dbContext.ConsumerRepository.Get(0x0000000600000005),
						dbContext.ConsumerRepository.Get(0x0000000600000006),
						dbContext.ConsumerRepository.Get(0x0000000600000007)
					},
					IsResolveConditionValidated = true
				};

				dbContext.OutageRepository.Add(archivedOutage1);
				dbContext.OutageRepository.Add(archivedOutage2);
				dbContext.OutageRepository.Add(archivedOutage3);
				dbContext.OutageRepository.Add(archivedOutage4);
				dbContext.OutageRepository.Add(archivedOutage5);
				try
				{
					dbContext.Complete();
				}
				catch(Exception e)
				{
					string message = "OutageDatabase - Configuration.cs::Seed method => exception on Complete()";
					//Logger.LogError(message, e);
					Logger.LogError($"{message}\n Message: {e.Message}");

					Console.WriteLine($"{message}, Message: {e.Message})");
				}

				//outage1
				List<ConsumerHistorical> consumerHistoricalOut1 = new List<ConsumerHistorical>(7)
				{
					new ConsumerHistorical() { ConsumerId = 0x0000000600000008, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000009, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000A, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000B, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000D, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000E, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000F, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut1);

				List<EquipmentHistorical> equipmentHistoricalOut1 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_2.EquipmentId, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 35, 16), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut1);

				List<ConsumerHistorical> consumerHistoricalOut2 = new List<ConsumerHistorical>(3)
				{
					new ConsumerHistorical() { ConsumerId = 0x000000060000000D, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 43, 23), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000E, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 43, 23), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000F, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 43, 23), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut2);

				List<EquipmentHistorical> equipmentHistoricalOut2 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_7.EquipmentId, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 43, 23), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_8.EquipmentId, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 43, 23), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_2.EquipmentId, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 43, 23), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 13, 43, 23), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut2);

				List<ConsumerHistorical> consumerHistoricalOut3 = new List<ConsumerHistorical>(4)
				{
					new ConsumerHistorical() { ConsumerId = 0x0000000600000008, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 14, 03, 7), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000009, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 14, 03, 7), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000A, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 14, 03, 7), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000B, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 14, 03, 7), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut3);

				List<EquipmentHistorical> equipmentHistoricalOut3 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_7.EquipmentId, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 14, 03, 7), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = br_8.EquipmentId, OutageId = archivedOutage1.OutageId, OperationTime = new DateTime(2019, 10, 5, 14, 03, 7), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut3);

				//outage2
				List<ConsumerHistorical> consumerHistoricalOut4 = new List<ConsumerHistorical>(7)
				{
					new ConsumerHistorical() { ConsumerId = 0x0000000600000008, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000009, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000A, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000B, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000D, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000E, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000F, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut4);

				List<EquipmentHistorical> equipmentHistoricalOut4 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_2.EquipmentId, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 25, 33), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut4);

				List<ConsumerHistorical> consumerHistoricalOut5 = new List<ConsumerHistorical>(3)
				{
					new ConsumerHistorical() { ConsumerId = 0x000000060000000D, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 37, 12), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000E, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 37, 12), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000F, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 37, 12), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut5);

				List<EquipmentHistorical> equipmentHistoricalOut5 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_7.EquipmentId, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 37, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_8.EquipmentId, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 37, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_2.EquipmentId, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 37, 12), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 37, 12), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut5);

				List<ConsumerHistorical> consumerHistoricalOut6 = new List<ConsumerHistorical>(4)
				{
					new ConsumerHistorical() { ConsumerId = 0x0000000600000008, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 47, 2), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000009, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 47, 2), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000A, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 47, 2), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000B, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 47, 2), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut6);

				List<EquipmentHistorical> equipmentHistoricalOut6 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_7.EquipmentId, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 47, 2), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = br_8.EquipmentId, OutageId = archivedOutage2.OutageId, OperationTime = new DateTime(2019, 11, 12, 10, 47, 2), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut6);

				//outage3
				List<ConsumerHistorical> consumerHistoricalOut7 = new List<ConsumerHistorical>(7)
				{
					new ConsumerHistorical() { ConsumerId = 0x0000000600000008, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000009, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000A, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000B, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000D, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000E, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000F, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut7);

				List<EquipmentHistorical> equipmentHistoricalOut7 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_2.EquipmentId, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 9, 11, 35), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut7);

				List<ConsumerHistorical> consumerHistoricalOut8 = new List<ConsumerHistorical>(3)
				{
					new ConsumerHistorical() { ConsumerId = 0x000000060000000D, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 19, 28), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000E, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 19, 28), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000F, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 19, 28), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut8);

				List<EquipmentHistorical> equipmentHistoricalOut8 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_7.EquipmentId, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 19, 28), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_8.EquipmentId, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 19, 28), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_2.EquipmentId, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 19, 28), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 19, 28), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut8);

				List<ConsumerHistorical> consumerHistoricalOut9 = new List<ConsumerHistorical>(4)
				{
					new ConsumerHistorical() { ConsumerId = 0x0000000600000008, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 30, 11), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000009, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 30, 11), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000A, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 30, 11), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000000B, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 30, 11), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut9);

				List<EquipmentHistorical> equipmentHistoricalOut9 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_7.EquipmentId, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 30, 11), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = br_8.EquipmentId, OutageId = archivedOutage3.OutageId, OperationTime = new DateTime(2019, 12, 25, 10, 30, 11), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut9);

				//outage4
				List<ConsumerHistorical> consumerHistoricalOut10 = new List<ConsumerHistorical>(9)
				{
					new ConsumerHistorical() { ConsumerId = 0x000000060000002A, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000002B, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000002C, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000002, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000003, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000004, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000005, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000006, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000007, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut10);

				List<EquipmentHistorical> equipmentHistoricalOut10 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_1.EquipmentId, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 13, 51, 12), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut10);

				List<ConsumerHistorical> consumerHistoricalOut11 = new List<ConsumerHistorical>(5)
				{
					new ConsumerHistorical() { ConsumerId = 0x0000000600000003, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000004, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000005, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000006, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000007, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut11);

				List<EquipmentHistorical> equipmentHistoricalOut11 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_4.EquipmentId, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_5.EquipmentId, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_1.EquipmentId, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 27, 28), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut11);

				List<ConsumerHistorical> consumerHistoricalOut12 = new List<ConsumerHistorical>(4)
				{
					new ConsumerHistorical() { ConsumerId = 0x000000060000002A, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 51, 34), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000002B, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 51, 34), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000002C, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 51, 34), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000002, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 51, 34), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut12);

				List<EquipmentHistorical> equipmentHistoricalOut12 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_4.EquipmentId, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 51, 34), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = br_5.EquipmentId, OutageId = archivedOutage4.OutageId, OperationTime = new DateTime(2019, 12, 30, 14, 51, 34), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut12);

				//outage5
				List<ConsumerHistorical> consumerHistoricalOut13 = new List<ConsumerHistorical>(9)
				{
					new ConsumerHistorical() { ConsumerId = 0x000000060000002A, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000002B, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x000000060000002C, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000002, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000003, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000004, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000005, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000006, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000007, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut13);

				List<EquipmentHistorical> equipmentHistoricalOut13 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_1.EquipmentId, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 22, 41), DatabaseOperation = DatabaseOperation.INSERT}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut13);

				List<ConsumerHistorical> consumerHistoricalOut14 = new List<ConsumerHistorical>(5)
				{
					new ConsumerHistorical() { ConsumerId = 0x0000000600000003, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000004, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000005, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000006, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000007, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut14);

				List<EquipmentHistorical> equipmentHistoricalOut14 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_4.EquipmentId, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_5.EquipmentId, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.INSERT},
					new EquipmentHistorical() { EquipmentId  = br_1.EquipmentId, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = rc_1.EquipmentId, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 17, 39, 20), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut14);

				List<ConsumerHistorical> consumerHistoricalOut15 = new List<ConsumerHistorical>(4)
				{
					new ConsumerHistorical() { ConsumerId = 0x000000060000002A, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 18, 12, 7), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000002B, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 18, 12, 7), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x000000060000002C, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 18, 12, 7), DatabaseOperation = DatabaseOperation.DELETE},
					new ConsumerHistorical() { ConsumerId = 0x0000000600000002, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 18, 12, 7), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricalOut15);

				List<EquipmentHistorical> equipmentHistoricalOut15 = new List<EquipmentHistorical>(2)
				{
					new EquipmentHistorical() { EquipmentId  = br_4.EquipmentId, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 18, 12, 7), DatabaseOperation = DatabaseOperation.DELETE},
					new EquipmentHistorical() { EquipmentId  = br_5.EquipmentId, OutageId = archivedOutage5.OutageId, OperationTime = new DateTime(2020, 1, 14, 18, 12, 7), DatabaseOperation = DatabaseOperation.DELETE}
				};
				dbContext.EquipmentHistoricalRepository.AddRange(equipmentHistoricalOut15);
			}

			try
			{
				dbContext.Complete();
			}
			catch (Exception e)
			{
				string message = "OutageDatabase - Configuration.cs::Seed method => exception on Complete()";
				//Logger.LogError(message, e);
				Logger.LogError($"{message}\n Message: {e.Message}");

				Console.WriteLine($"{message}, Message: {e.Message})");
			}
			finally
			{
				//dbContext.Dispose();
				//exception thrown if dispose is called...
			}
		}
	}
}
