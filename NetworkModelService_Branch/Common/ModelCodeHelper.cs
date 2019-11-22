using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Common
{
	public class ModelCodeHelper
	{
		
		/// <summary>
		/// Extracts system id from global id of an entity
		/// </summary>
		/// <param name="globalId"></param>
		/// <returns></returns>
		public static short ExtractSystemIdFromGlobalId(long globalId)
		{
			unchecked
			{
				globalId >>= 48;
				globalId &= 0x000000000000ffff;
				return (short)globalId;
			}
		}

		/// <summary>
		/// Extracts type global id of an entity
		/// </summary>
		/// <param name="globalId"></param>
		/// <returns></returns>
		public static short ExtractTypeFromGlobalId(long globalId)
		{
			unchecked
			{
				globalId >>= 32;
				globalId &= 0x000000000000ffff;
				return (short)globalId;
			}
		}
	
		/// <summary>
		/// Extracts entity id from global id of an entity
		/// </summary>
		/// <param name="globalId"></param>
		/// <returns></returns>
		public static int ExtractEntityIdFromGlobalId(long globalId)
		{
			unchecked
			{
				globalId &= 0x00000000ffffffff;
				return (int)globalId;
			}
		}
		
		/// <summary>
		/// Creates global id for entity from system id, type and entity id (fragment).
		/// </summary>
		/// <param name="systemId"></param>
		/// <param name="type"></param>
		/// <param name="entityId"></param>
		/// <returns>Global id of the entity.</returns>
		public static long CreateGlobalId(short systemId, short type, int entityId)
		{
			unchecked
			{
				ulong globalId = 0x0000000000000000;
				ulong temp = (uint)entityId;
				globalId |= temp;

				temp = (ushort)type;
				temp <<= 32;
				globalId |= temp;

				temp = (ushort)systemId;
				temp <<= 48;
				globalId |= temp;

				return (long)globalId;
			}
		}	      

		public static DMSType GetTypeFromModelCode(ModelCode code)
		{
			return (DMSType)((long)((long)code & (long)ModelCodeMask.MASK_TYPE) >> 16);
		}	

		public static bool GetModelCodeFromString(string strModelCode, out ModelCode modelCode)
		{
			return Enum.TryParse(strModelCode, true, out modelCode);
		}

		public static bool GetDMSTypeFromString(string strDmsType, out DMSType dmsType)
		{
			return Enum.TryParse(strDmsType, true, out dmsType);
		}				
    }
}
