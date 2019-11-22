using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FTN.Common
{
	
	public enum ErrorCode
	{
		Unknown						= 0x0000,
		InvalidModel				= 0x0001,
		InvalidModelChange			= 0x0002,
		InvalidModelQuery			= 0x0003,
		InvalidDelta				= 0x0004,
		EntityNotFound				= 0x0005,
		TypeContainerNotFound		= 0x0006,
		BlockNotFound				= 0x0007,
		InvalidClassId				= 0x0008,
		InvalidTypeId				= 0x0009,
		InvalidPropertyId			= 0x000a,
		DuplicateEntity				= 0x000b,
		InvalidReference			= 0x000c,
		BadPartitioning				= 0x000d,
		InvalidVersion				= 0x0010,
		InvalidDeltaCounter			= 0x0011,
		BadEntity					= 0x0012,
		SaveCaseExists				= 0x0013,
		ObsoleteIterator			= 0x0014,
		InvalidContextId			= 0x0015,
		BlockNotLoaded				= 0x0016,
		// CSS exceptions
		NoDBConnection				= 0x0020,
		NoDataAffected				= 0x0021,
		ActionNotAllowed			= 0x0022,
		ObjectNotFound				= 0x0023,
		ChangesetLocked				= 0x0024,
		ReferentialIntegrity		= 0x0025,
		CorruptedData				= 0x0026,
		ObjectAlreadyExists			= 0x0027,
		ReverseDependencyExists		= 0x0028,
		EqualPairMembers			= 0x0029,
		ExtractLocked				= 0x002a,
	}
}
