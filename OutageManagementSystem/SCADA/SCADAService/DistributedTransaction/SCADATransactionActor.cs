﻿using Outage.DistributedTransactionActor;
using System;

namespace Outage.SCADA.SCADAService.DistributedTransaction
{
    public class SCADATransactionActor : TransactionActor
    {
        protected static SCADAModel scadaModel = null;

        public static SCADAModel SCADAModel
        {
            set
            {
                scadaModel = value;
            }
        }

        public override bool Prepare()
        {
            bool success = false;

            try
            {
                success = true;
                success = scadaModel.Prepare();
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception catched in Prepare method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                logger.LogInfo("Preparation on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                logger.LogInfo("Preparation on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public override void Commit()
        {
            try
            {
                scadaModel.Commit();
                ModbusSimulatorHandler.RestartSimulator();
                logger.LogInfo("Commit on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                logger.LogFatal($"Exception catched in Commit method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                logger.LogInfo("Commit on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override void Rollback()
        {
            try
            {
                scadaModel.Rollback();
                ModbusSimulatorHandler.RestartSimulator();
                logger.LogInfo("Rollback on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                logger.LogFatal($"Exception catched in Rollback method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                logger.LogInfo("Rollback on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }
    }
}