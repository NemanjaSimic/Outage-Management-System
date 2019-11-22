using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace FTN.Common
{
	[DataContract]		
	public class DeltaDB
	{
		private long id;
		private byte[] direct;

		public DeltaDB()
		{
		}

		public DeltaDB(long id, byte[] deltaDBBinary)
		{
			DeltaDB deltaDB = DeltaDB.Deserialize(deltaDBBinary);

			this.id = id;
			this.direct = deltaDB.direct;
		}

		public DeltaDB(byte[] deltaDBBinary)
		{
			DeltaDB deltaDB = DeltaDB.Deserialize(deltaDBBinary);

			this.id = deltaDB.id;
			this.direct = deltaDB.direct;
		}
		
		public DeltaDB(long id)
		{
			this.id = id;			
		}

		[DataMember]		
		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		[DataMember]		
		public byte[] Direct
		{
			get { return direct; }
			set { direct = value; }
		}
		

		public byte[] Serialize()
		{
			// to do: Fix this
			return new byte[0];
			////using (Serializer serializer = serializerFactory.CreateSerializer())
			////{
			////    serializer.Serialize(this);

			////    MemoryStream ms = new MemoryStream();
			////    serializer.GetSerializedData(ms);
			////    ms.Close();
			////    return ms.ToArray();
			////}
		}

		public static DeltaDB Deserialize(byte[] deltaDBBinary)
		{
			// to do: Fix this
			return new DeltaDB();

			////using (Deserializer deserializer = deserializerFactory.CreateDeserializer(new ArraySegment<byte>(deltaDBBinary, 0, deltaDBBinary.Length)))
			////{
			////    return (DeltaDB)deserializer.Deserialize();
			////}
		}
	}
}
