using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.WcfClient.NMS;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerImplementation.ModelAccess
{
    public class ConsumerAccess : IConsumerAccessContract
	{
		private readonly string baseLogString;

		#region Private Properties
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private ModelResourcesDesc modelResourcesDesc;
		#endregion Private Properties

		public ConsumerAccess()
		{
			modelResourcesDesc = new ModelResourcesDesc();
			InitializeEnergyConsumers().Wait();
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");
		}

		private async Task InitializeEnergyConsumers()
		{
            try
            {
				UnitOfWork dbContext = new UnitOfWork();
				int resourcesLeft;
				int numberOfResources = 10000;

				var networkModelGDAClient = NetworkModelGdaClient.CreateClient();
				int iteratorId = await networkModelGDAClient.GetExtentValues(ModelCode.ENERGYCONSUMER, modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONSUMER));

				resourcesLeft = await networkModelGDAClient.IteratorResourcesTotal(iteratorId);

				List<ResourceDescription> energyConsumers = new List<ResourceDescription>();

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = await networkModelGDAClient.IteratorNext(numberOfResources, iteratorId);
					energyConsumers.AddRange(rds);

					resourcesLeft = await networkModelGDAClient.IteratorResourcesLeft(iteratorId);
				}

				await networkModelGDAClient.IteratorClose(iteratorId);

				int i = 0;

				foreach (ResourceDescription energyConsumer in energyConsumers)
				{
					Consumer consumer = new Consumer()
					{
						ConsumerId = energyConsumer.GetProperty(ModelCode.IDOBJ_GID).AsLong(),
						ConsumerMRID = energyConsumer.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
						FirstName = $"FirstName{i}", //TODO: energyConsumer.GetProperty(ModelCode.ENERGYCONSUMER_FIRSTNAME).AsString();
						LastName = $"LastName{i}"   //TODO: energyConsumer.GetProperty(ModelCode.ENERGYCONSUMER_LASTNAME).AsString();
					};

					i++;


					if (dbContext.ConsumerRepository.Get(consumer.ConsumerId) == null)
					{
						dbContext.ConsumerRepository.Add(consumer);
					}
				}

				dbContext.Complete();
				dbContext.Dispose();
			}
            catch (Exception e)
            {
				Logger.LogError($"{baseLogString} InitializeEnergyConsumers => Exception: {e.Message}", e);
            }
		}

		#region IConsumerAccessContract
		public Task<Consumer> AddConsumer(Consumer consumer)
		{
			return Task.Run(() =>
			{
				//MODO: razmisliti o ConditonalValue<Consumer>
				Consumer consumerDb = null;

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						consumerDb = unitOfWork.ConsumerRepository.Add(consumer);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} AddConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return consumerDb;
			});
		}

		public Task<IEnumerable<Consumer>> FindConsumer(ConsumerExpression expression)
		{
			return Task.Run(() =>
			{
				IEnumerable<Consumer> consumers = new List<Consumer>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						consumers = unitOfWork.ConsumerRepository.Find(expression.Predicate);
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} FindConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return consumers;
			});
		}

		public Task<IEnumerable<Consumer>> GetAllConsumers()
		{
			return Task.Run(() =>
			{
				IEnumerable<Consumer> consumers = new List<Consumer>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						consumers = unitOfWork.ConsumerRepository.GetAll();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetAllConsumers => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return consumers;
			});
		}

		public Task<Consumer> GetConsumer(long gid)
		{
			return Task.Run(() =>
			{
				//MODO: razmisliti o ConditonalValue<Consumer>
				Consumer consumer = null;

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						consumer = unitOfWork.ConsumerRepository.Get(gid);
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return consumer;
			});
		}

		public Task RemoveAllConsumers()
		{
			return Task.Run(() =>
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.ConsumerRepository.RemoveAll();
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} RemoveAllConsumers => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task RemoveConsumer(Consumer consumer)
		{
			return Task.Run(() => 
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.ConsumerRepository.Remove(consumer);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} RemoveConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task UpdateConsumer(Consumer consumer)
		{
			return Task.Run(() =>
            {
				using(var unitOfWork = new UnitOfWork())
                {
					try
					{
						unitOfWork.ConsumerRepository.Update(consumer);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} UpdateConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
                }
			});
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => true);
		}
		#endregion IConsumerAccessContract
	}
}
