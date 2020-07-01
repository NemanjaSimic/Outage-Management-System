using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Exceptions.NMS;
using OMS.Common.Cloud;

namespace OMS.Common.NmsContracts.GDA
{

    public enum DeltaOpType : byte 
	{ 
		Insert = 0, 
		Update = 1, 
		Delete = 2 
	}

	[DataContract]	
	public class Delta
	{
		#region Private Properties
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		#endregion Private Properties

		private long id;
		private List<ResourceDescription> insertOps = new List<ResourceDescription>();
		private List<ResourceDescription> deleteOps = new List<ResourceDescription>();
		private List<ResourceDescription> updateOps = new List<ResourceDescription>();
		private bool positiveIdsAllowed;		
		
		private static ModelResourcesDesc resDesc = null;

		public static ModelResourcesDesc ResourceDescs
		{
			get 
			{
				if (Delta.resDesc == null)
				{
					Delta.resDesc = new ModelResourcesDesc();
				}

				return Delta.resDesc;
			}

			set 
			{ 
				Delta.resDesc = value; 
			}
		}				
	
		public Delta()
		{
			this.positiveIdsAllowed = false;
		}

		public Delta(long id)
		{
			this.id = id;
			this.positiveIdsAllowed = false;
		}		

		public Delta(Delta toCopy)
		{
			this.id = toCopy.id;			
			foreach (ResourceDescription resDesc in toCopy.insertOps)
			{
				ResourceDescription toAdd = new ResourceDescription(resDesc);
				this.AddDeltaOperation(DeltaOpType.Insert, toAdd, true);
			}
			
			foreach (ResourceDescription resDesc in toCopy.updateOps)
			{
				ResourceDescription toAdd = new ResourceDescription(resDesc);
				this.AddDeltaOperation(DeltaOpType.Update, toAdd, true);
			}
			
			foreach (ResourceDescription resDesc in toCopy.deleteOps)
			{
				ResourceDescription toAdd = new ResourceDescription(resDesc);
				this.AddDeltaOperation(DeltaOpType.Delete, toAdd, true);
			}

			this.positiveIdsAllowed = toCopy.positiveIdsAllowed;
		}

		public Delta(long id, byte[] deltaBinary)
		{
			Delta delta = Delta.Deserialize(deltaBinary);

			this.id = id;
			this.insertOps = delta.insertOps;
			this.updateOps = delta.updateOps;
			this.deleteOps = delta.deleteOps;
			this.positiveIdsAllowed = delta.positiveIdsAllowed;
		}

		public Delta(byte[] deltaBinary)
		{
			Delta delta = Delta.Deserialize(deltaBinary);

			this.id = delta.id;
			this.insertOps = delta.insertOps;
			this.updateOps = delta.updateOps;
			this.deleteOps = delta.deleteOps;
			this.positiveIdsAllowed = delta.positiveIdsAllowed;
		}

		[DataMember]		
		public long Id
		{
			get { return id; }
			set { id = value; }
		}		

		[DataMember]		
		public List<ResourceDescription> InsertOperations
		{
			get	{ return insertOps;	}
			set { insertOps = value; }
		}

		[DataMember]		
		public List<ResourceDescription> UpdateOperations
		{
			get	{ return updateOps;	}
			set { updateOps = value; }
		}

		[DataMember]
		public List<ResourceDescription> DeleteOperations
		{
			get { return deleteOps;	}
			set { deleteOps = value; }
		}

		[DataMember]		
		public bool PositiveIdsAllowed
		{
			get { return positiveIdsAllowed; }
			set { positiveIdsAllowed = value; }
		}
		
		public long NumberOfOperations
		{
			get { return insertOps.Count + updateOps.Count + deleteOps.Count; }
		}
		
		public bool ContainsDeltaOperation(DeltaOpType type, long id)
		{
			switch (type)
			{
				case DeltaOpType.Insert:
				{
					foreach (ResourceDescription resDesc in this.InsertOperations)
					{
						if (resDesc.Id == id)
						{
							return true;
						}
					}

					break;
				}

				case DeltaOpType.Update:
				{
					foreach (ResourceDescription resDesc in this.UpdateOperations)
					{
						if (resDesc.Id == id)
						{
							return true;
						}
					}

					break;
				}

				case DeltaOpType.Delete:
				{
					foreach (ResourceDescription resDesc in this.DeleteOperations)
					{
						if (resDesc.Id == id)
						{
							return true;
						}
					}

					break;
				}
			}

			return false;
		}

		public ResourceDescription GetDeltaOperation(DeltaOpType type, long id)
		{
			switch (type)
			{
				case DeltaOpType.Insert:
				{
					foreach (ResourceDescription resDesc in this.InsertOperations)
					{
						if (resDesc.Id == id)
						{
							return resDesc;
						}
					}

					break;
				}

				case DeltaOpType.Update:
				{
					foreach (ResourceDescription resDesc in this.UpdateOperations)
					{
						if (resDesc.Id == id)
						{
							return resDesc;
						}
					}

					break;
				}

				case DeltaOpType.Delete:
				{
					foreach (ResourceDescription resDesc in this.DeleteOperations)
					{
						if (resDesc.Id == id)
						{
							return resDesc;
						}
					}

					break;
				}
			}

			string message = string.Format($"There is no {type.ToString()} delta operation with GID: 0x{id:X16}.");
			Logger.LogError(message);
			throw new Exception(message);
		}

		public void AddDeltaOperation(DeltaOpType type, ResourceDescription rd, bool addAtEnd)
		{
			List<ResourceDescription> operations = null;
			switch (type)
			{
				case DeltaOpType.Insert:
					operations = insertOps;
					break;
				case DeltaOpType.Update:
					operations = updateOps;
					break;
				case DeltaOpType.Delete:
					operations = deleteOps;
					break;
			}

			if (addAtEnd)
			{
				operations.Add(rd);
			}
			else
			{
				operations.Insert(0, rd);
			}
		}

		public void FixNegativeToPositiveIds(ref Dictionary<short, int> typesCounters, ref Dictionary<long, long> globaldChanges)
		{
			string message = String.Format("Fixing negative to positive IDs for delta with ID: {0} started.", GetCompositeId(id));
			Logger.LogDebug(message);

			if (globaldChanges == null)
			{
				globaldChanges = new Dictionary<long, long>();
			}

			// fix ids in insert operations - generate positive ids
			foreach (ResourceDescription rd in insertOps)
			{
				long gidOld = rd.Id;
				int idOld = ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id);
				if (idOld < 0)
				{
					if (!globaldChanges.ContainsKey(gidOld))
					{
						short type = ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id);

						if (type <= 0)
						{
							string errorMessage = $"FixNegativeToPositiveIds => Invalid DMS type found in insert delta operation ID: 0x{rd.Id:X16}.";
							Logger.LogError(errorMessage);
							throw new ModelException(ErrorCode.InvalidDelta, gidOld, 0, errorMessage);
						}

						int idNew = typesCounters[type] + 1;
						typesCounters[type] = idNew;						

						long gidNew = ChangeEntityIdInGlobalId(gidOld, idNew);
						gidNew = IncorporateSystemIdToValue(gidNew, 0);

						globaldChanges[gidOld] = gidNew;
						rd.Id = gidNew;
					}
					else
					{
						message = $"FixNegativeToPositiveIds => Failed to fix negative to positive IDs in insert delta operations for delta with ID: {GetCompositeId(id)} because negative resource ID: 0x{gidOld:X16} already exists in previous insert delta operations.";
						Logger.LogError(message);

						string exceptionMessage = $"Invalid delta. Negative resource ID: 0x{gidOld:X16} already exists in previous insert delta operations.";
						throw new Exception(exceptionMessage);
					}
				}
				else if (!this.positiveIdsAllowed)
				{
					message = $"Failed to fix negative to positive IDs in insert delta operations for delta with ID: {GetCompositeId(id)} because resource ID: 0x{gidOld:X16} must not be positive.";
					Logger.LogError(message);

					string exceptionMessage = $"Invalid insert delta operation. Resource ID: 0x{gidOld:X16} must not be positive.";
					throw new Exception(exceptionMessage);
				}
			}

			// change reference ids in insert operations
			foreach (ResourceDescription rd in insertOps)
			{
				foreach (Property p in rd.Properties)
				{
					if (p.Type == PropertyType.Reference)
					{
						long gidOld = p.AsReference();
						int idOld = ModelCodeHelper.ExtractEntityIdFromGlobalId(gidOld);
						if (idOld < 0)
						{
							if (globaldChanges.ContainsKey(gidOld))
							{
								p.SetValue(globaldChanges[gidOld]);
							}
							else
							{
								message = $"Failed to fix negative to positive IDs in insert delta operations for delta with ID: {GetCompositeId(id)} because negative reference (property code: {p.Id}, value: 0x{gidOld:X16}) does not exist in insert delta operations. Resource ID: 0x{rd.Id:X16}.";
								Logger.LogError(message);

								string exceptionMessage = $"Invalid insert delta operation. Negative reference (property code: {p.Id}, value: 0x{gidOld:X16}) does not exist in insert delta operations. Resource ID: 0x{rd.Id:X16}.";
								throw new Exception(exceptionMessage);
							}
						}
					}
					else if (p.Type == PropertyType.ReferenceVector)
					{
						bool changed = false;

						List<long> gidsRef = p.AsReferences();
						for (int i = 0; i < gidsRef.Count; i++)
						{
							long gidOldRef = gidsRef[i];
							int idOldRef = ModelCodeHelper.ExtractEntityIdFromGlobalId(gidOldRef);
							if (idOldRef < 0)
							{
								if (globaldChanges.ContainsKey(gidOldRef))
								{
									gidsRef[i] = globaldChanges[gidOldRef];
									changed = true;
								}
								else
								{
									message = $"Failed to fix negative to positive IDs in insert delta operations for delta with ID: {GetCompositeId(id)} because negative reference (property code: {p.Id}, value: 0x{gidOldRef:X16}) does not exist in insert delta operations. Resource ID: 0x{rd.Id:X16}.";
									Logger.LogError(message);

									string exceptionMessage = $"Invalid insert delta operation. Negative reference (property code: {p.Id}, value: 0x{gidOldRef:X16}) does not exist in insert delta operations. Resource ID: 0x{rd.Id:X16}.";
									throw new Exception(exceptionMessage);
								}
							}
						}

						if (changed)
						{
							p.SetValue(gidsRef);
						}
					}
				}
			}

			// change ids and reference ids in update operations
			foreach (ResourceDescription rd in updateOps)
			{
				long gidOld = rd.Id;
				int idOld = ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id);
				if (idOld < 0)
				{
					if (globaldChanges.ContainsKey(gidOld))
					{
						rd.Id = globaldChanges[gidOld];
					}
					else
					{
						message = $"Failed to fix negative to positive IDs in update delta operations for delta with ID {GetCompositeId(id)} because negative resource ID: 0x{gidOld:X16} does not exists in insert delta operations.";
						Logger.LogError(message);

						string exceptionMessage = $"Invalid update delta operation. Negative resource ID: 0x{gidOld:X16} does not exists in insert delta operations.";
						throw new Exception(exceptionMessage);
					}
				}

				foreach (Property p in rd.Properties)
				{
					if (p.Type == PropertyType.Reference)
					{
						long gidOldRef = p.AsReference();
						int idOldRef = ModelCodeHelper.ExtractEntityIdFromGlobalId(gidOldRef);
						if (idOldRef < 0)
						{
							if (globaldChanges.ContainsKey(gidOldRef))
							{
								p.SetValue(globaldChanges[gidOldRef]);
							}
							else
							{
								message = $"Failed to fix negative to positive IDs in update delta operations for delta with ID: {GetCompositeId(id)} because negative reference (property code: {p.Id}, value: 0x{gidOldRef:X16}) does not exist in insert delta operations. Resource ID: 0x{rd.Id:X16}.";
								Logger.LogError(message);

								string exceptionMessage = $"Invalid update delta operation. Negative reference (property code: {p.Id}, value: 0x{gidOldRef:X16}) does not exist in insert delta operations. Resource ID: 0x{rd.Id:X16}.";
								throw new Exception(exceptionMessage);
							}
						}
					}
					else if (p.Type == PropertyType.ReferenceVector)
					{
						bool changed = false;

						List<long> gidsRef = p.AsReferences();
						for (int i = 0; i < gidsRef.Count; i++)
						{
							long gidOldRef = gidsRef[i];
							int idOldRef = ModelCodeHelper.ExtractEntityIdFromGlobalId(gidOldRef);
							if (idOldRef < 0)
							{
								if (globaldChanges.ContainsKey(gidOldRef))
								{
									gidsRef[i] = globaldChanges[gidOldRef];
									changed = true;
								}
								else
								{
									message = $"Failed to fix negative to positive IDs in update delta operations for delta with ID: {GetCompositeId(id)} because negative reference (property code: {p.Id}, value: 0x{gidOldRef:X16}) does not exist in insert delta operations. Resource ID: 0x{rd.Id:X16}.";									
									Logger.LogError(message);

									string exceptionMessage = $"Invalid update delta operation. Negative reference (property code: {p.Id}, value: 0x{gidOldRef:X16}) does not exist in insert delta operations. Resource ID: 0x{rd.Id:X16}.";
									throw new Exception(exceptionMessage);
								}
							}
						}

						if (changed)
						{
							p.SetValue(gidsRef);
						}
					}
				}
			}

			// change ids in delete operations
			foreach (ResourceDescription rd in deleteOps)
			{
				long gidOld = rd.Id;
				int idOld = ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id);
				if (idOld < 0)
				{
					if (globaldChanges.ContainsKey(gidOld))
					{
						rd.Id = globaldChanges[gidOld];
					}
					else
					{
						message = $"Failed to fix negative to positive IDs in delete delta operations for delta with ID: {GetCompositeId(id)} because negative resource ID: 0x{gidOld:X16} does not exists in insert delta operations.";
						Logger.LogError(message);						

						string exceptionMessage = $"Invalid delete delta operation. Negative resource ID: 0x{gidOld:X16} does not exists in insert delta operations.";
						throw new Exception(message);
					}
				}
			}

			message = $"Fixing negative to positive IDs for delta with ID: {GetCompositeId(id)} completed successfully.";
			Logger.LogDebug(message);
		}

		public void SortOperations()
		{
			string message = $"Sorting delta operations for delta with ID: {GetCompositeId(id)}.";
			Logger.LogDebug(message);

			List<ResourceDescription> insertOpsOrdered = new List<ResourceDescription>();
			List<ResourceDescription> deleteOpsOrdered = new List<ResourceDescription>();
			int insertOpsOrderedNo = 0;
			int deleteOpsOrderedNo = 0;
			int indexOp = 0;

			// pass through all given types
			for (int indexType = 0; indexType < Delta.ResourceDescs.TypeIdsInInsertOrder.Count; indexType++)
			{
				DMSType type = ModelResourcesDesc.GetTypeFromModelCode(Delta.ResourceDescs.TypeIdsInInsertOrder[indexType]);

				// pass through all insert operations
				// move operations with current type to list of ordered insert operations
				indexOp = 0;
				for (indexOp = 0; indexOp < insertOps.Count; indexOp++)
				{
					if (insertOps[indexOp] != null && type == (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(insertOps[indexOp].Id))
					{
						// add at the end of list of ordered insert operations
						insertOpsOrdered.Add(insertOps[indexOp]);
						insertOps[indexOp] = null;
						insertOpsOrderedNo++;

						// remove from the old non sorted list - ATTENTION: this turns to be VERY SLOW operation in case delta is BIG
						////insertOps.RemoveAt(indexOp);
					}
				}

                // pass through all delete operations
                // move operations with current type to list of ordered delete operations
                indexOp = 0;
                for (indexOp = 0; indexOp < deleteOps.Count; indexOp++)
                {
                    if (deleteOps[indexOp] != null && type == (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(deleteOps[indexOp].Id))
                    {
                        // add at the end of list of ordered delete operations
                        deleteOpsOrdered.Add(deleteOps[indexOp]);
                        deleteOps[indexOp] = null;
                        deleteOpsOrderedNo++;

                        // remove from the old non sorted list - ATTENTION: this turns to be VERY SLOW operation in case delta is BIG
                        ////deleteOps.RemoveAt(indexOp);
                    }
                }
            }

            // check if there are insert operations not covered by given data model types
            if (insertOps.Count != insertOpsOrderedNo)
			{
				// find type that is not specified in given list of types
				short typeNotDefined = 0;
				for (indexOp = 0; indexOp < insertOps.Count; indexOp++)
				{
					if (insertOps[indexOp] != null)
					{
						typeNotDefined = (short)ModelCodeHelper.ExtractTypeFromGlobalId(insertOps[indexOp].Id);
					}
				}

				message = $"Failed to sort delta operations because there are some insert operations (count = {insertOps.Count - insertOpsOrderedNo}) whose type (e.g {typeNotDefined}) is not specified in the given list of types.";
				Logger.LogError(message);

				string exceptionMessage = $"Invalid delta. Some insert operations (count = {insertOps.Count - insertOpsOrderedNo}) whose type (e.g {typeNotDefined}) is not correct.";
				throw new ModelException(ErrorCode.InvalidDelta, exceptionMessage);
			}

			// remember ordered insert operations
			insertOps = insertOpsOrdered;

			// check if there are delete operations not covered by given data model types
			if(deleteOps.Count != deleteOpsOrderedNo)
			{
				// find type that is not specified in given list of types
				short typeNotDefined = 0;
				for (indexOp = 0; indexOp < deleteOps.Count; indexOp++)
				{
					if (deleteOps[indexOp] != null)
					{
						typeNotDefined = ModelCodeHelper.ExtractTypeFromGlobalId(deleteOps[indexOp].Id);
					}
				}
				message = $"Failed to sort delta operations because there are some delete operations (count = {deleteOps.Count}) which type (e.g. {typeNotDefined}) is not specified in given list of types.";
				Logger.LogError(message);

				string exceptionMessage = $"Invalid delta. Some delete operations (count = {deleteOps.Count}) which type (e.g. {typeNotDefined}) is not correct.";
				throw new ModelException(ErrorCode.InvalidDelta, exceptionMessage);
			}

			// remember ordered delete operations
			deleteOpsOrdered.Reverse();
			deleteOps = deleteOpsOrdered;

			message = $"Sorting delta operations for delta with ID: {GetCompositeId(id)} completed successfully.";
			Logger.LogDebug(message);
		}
     
		/// <summary>
		/// Reverses order of delta operations for each type of operation (insert, update, delete)
		/// </summary>
		public void ReverseOrderOfOperations()
		{
			this.insertOps.Reverse();
			this.updateOps.Reverse();
			this.deleteOps.Reverse();
		}		
	
		public byte[] Serialize()
		{			
			MemoryStream compresionStream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(compresionStream, this);
			byte[] result = compresionStream.ToArray();
			compresionStream.Close();

			return result;			
		}

		public static Delta Deserialize(byte[] deltaBinary)
		{
			if (deltaBinary == null || deltaBinary.Length == 0)
			{
				return new Delta();
			}
						
			MemoryStream compresionStream = new MemoryStream(deltaBinary);
			BinaryFormatter formatter = new BinaryFormatter();
			Delta delta = (Delta)formatter.Deserialize(compresionStream);						
			compresionStream.Close();

			return delta;
		}

		public void ExportToXml(XmlTextWriter xmlWriter, EnumDescs enumDescs)
		{
			xmlWriter.WriteStartElement("delta");
			xmlWriter.WriteStartAttribute("id");
			xmlWriter.WriteValue(this.Id);
			xmlWriter.WriteStartElement("operations");

			if (this.InsertOperations.Count > 0)
			{
				xmlWriter.WriteStartElement("operation");
				xmlWriter.WriteStartAttribute("type");
				xmlWriter.WriteValue("insert");
				xmlWriter.WriteStartElement("ResourceDescriptions");
				xmlWriter.WriteStartAttribute("count");
				xmlWriter.WriteValue(this.InsertOperations.Count);
				for (int i = 0; i < this.InsertOperations.Count; i++)
				{
					this.InsertOperations[i].ExportToXml(xmlWriter, enumDescs);
				}

				xmlWriter.WriteEndElement(); // operation
				xmlWriter.WriteEndElement(); // ResourceDescriptions
			}
			
			if (this.UpdateOperations.Count > 0)
			{
				xmlWriter.WriteStartElement("operation");
				xmlWriter.WriteStartAttribute("type");
				xmlWriter.WriteValue("update");
				xmlWriter.WriteStartElement("ResourceDescriptions");
				xmlWriter.WriteStartAttribute("count");
				xmlWriter.WriteValue(this.UpdateOperations.Count);
				for (int i = 0; i < this.UpdateOperations.Count; i++)
				{
					this.UpdateOperations[i].ExportToXml(xmlWriter, enumDescs);
				}

				xmlWriter.WriteEndElement(); // operation
				xmlWriter.WriteEndElement(); // ResourceDescriptions
			}				

			if (this.DeleteOperations.Count > 0)
			{
				xmlWriter.WriteStartElement("operation");
				xmlWriter.WriteStartAttribute("type");
				xmlWriter.WriteValue("delete");
				xmlWriter.WriteStartElement("ResourceDescriptions");
				xmlWriter.WriteStartAttribute("count");
				xmlWriter.WriteValue(this.DeleteOperations.Count);
				for (int i = 0; i < this.DeleteOperations.Count; i++)
				{
					this.DeleteOperations[i].ExportToXml(xmlWriter, enumDescs);
				}

				xmlWriter.WriteEndElement(); // operation
				xmlWriter.WriteEndElement(); // ResourceDescriptions
			}

			xmlWriter.WriteEndElement(); // operations
			xmlWriter.WriteEndElement(); // delta
		}	

		public bool RemoveResourceDescription(long id, DeltaOpType opType)
		{
			bool removed = false;
			
			switch (opType)
			{
				case DeltaOpType.Insert:
					for (int i = 0; i < this.insertOps.Count; i++)
					{
						if (this.insertOps[i].Id == id)
						{
							this.insertOps.RemoveAt(i--);
							removed = true;
						}
					}

					break;
				case DeltaOpType.Update:
					for (int i = 0; i < this.updateOps.Count; i++)
					{
						if (this.updateOps[i].Id == id)
						{
							this.updateOps.RemoveAt(i--);
							removed = true;
						}
					}

					break;
				case DeltaOpType.Delete:
					for (int i = 0; i < this.deleteOps.Count; i++)
					{
						if (this.deleteOps[i].Id == id)
						{
							this.deleteOps.RemoveAt(i--);
							removed = true;
						}
					}

					break;
			}

			return removed;
		}

		public void ClearDeltaOperations()
		{
			insertOps.Clear();
			deleteOps.Clear();
			updateOps.Clear();
		}		

		private string GetCompositeId(long valueWithSystemId)
		{
			string systemId = (Math.Abs(valueWithSystemId) >> 48).ToString();
			string valueWithoutSystemId = (Math.Abs(valueWithSystemId) & 0x0000FFFFFFFFFFFF).ToString();

			string dash = valueWithSystemId < 0 ? "-" : "";
			return $"{dash}{systemId}.{valueWithoutSystemId}";
		}

		private long IncorporateSystemIdToValue(long valueWithoutSystemId, short systemId)
		{
			unchecked
			{
				ulong valueWithSystemId = 0x0000000000000000;
				ulong temp = (ulong)(valueWithoutSystemId & 0x0000ffffffffffff);
				valueWithSystemId |= temp;

				temp = (ulong)systemId;
				temp <<= 48;
				valueWithSystemId |= temp;

				return (long)valueWithSystemId;
			}
		}

		public long ChangeEntityIdInGlobalId(long gidOld, int idNew)
		{
			unchecked
			{
				long gidNew = idNew;
				gidOld &= (long)0xffffffff00000000;
				gidNew &= (long)0x00000000ffffffff;
				gidNew |= gidOld;
				return gidNew;
			}
		}
	}
}
