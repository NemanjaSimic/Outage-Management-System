using SCADA_Common;
using ModBus.Connection;
using ModBus.FunctionParameters;
using ModBus.ModbusFuntions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Acquisitor
{
    public class Acquisition
    {

        private FunctionExecutor commandExecutor;

        public Acquisition(FunctionExecutor fe)
        {
            commandExecutor = fe;
        }

        public void Acquire()
        {
            try
            {
                

                    ushort adresa1 = 00040;
                    ushort kvantitet1 = 1;
                    ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters
                    (6, (byte)ModbusFunctionCode.READ_COILS, adresa1, kvantitet1);

                    ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                    this.commandExecutor.EnqueueCommand(fn);
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}

