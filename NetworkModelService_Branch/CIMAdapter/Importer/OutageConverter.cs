using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.CIMAdapter.Importer
{
    public static class OutageConverter
    {
        #region Populate ResourceDescription
        public static void PopulateIdentifiedObjectProperties(Outage.IdentifiedObject cimIdentifiedObject, ResourceDescription rd)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                if (cimIdentifiedObject.MRIDHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.IDOBJ_MRID, cimIdentifiedObject.MRID));
                }
                if (cimIdentifiedObject.NameHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.IDOBJ_NAME, cimIdentifiedObject.Name));
                }
                if (cimIdentifiedObject.DescriptionHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.IDOBJ_DESCRIPTION, cimIdentifiedObject.Description));
                }
            }
        }

        public static void PopulatePowerSystemResourceProperties(Outage.PowerSystemResource cimPowerSystemResource, ResourceDescription rd)
        {
            if ((cimPowerSystemResource != null) && (rd != null))
            {
                OutageConverter.PopulateIdentifiedObjectProperties(cimPowerSystemResource, rd);
            }
        }

        public static void PopulateEquipmentProperties(Outage.Equipment cimEquipment, ResourceDescription rd)
        {
            if ((cimEquipment != null) && (rd != null))
            {
                OutageConverter.PopulatePowerSystemResourceProperties(cimEquipment, rd);
            }
        }

        public static void PopulateConductingEquipmentProperties(Outage.ConductingEquipment cimConductingEquipment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConductingEquipment != null) && (rd != null))
            {
                OutageConverter.PopulateEquipmentProperties(cimConductingEquipment, rd);

                if (cimConductingEquipment.BaseVoltageHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimConductingEquipment.BaseVoltage.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert").Append(cimConductingEquipment.GetType().ToString()).Append(" rdfID = \"").Append(cimConductingEquipment.ID);
                        report.Report.Append("\" - Failed to set reference to BaseVoltage: rdfID\"").Append(cimConductingEquipment.BaseVoltage.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE, gid));
                }
            }
        }

        public static void PopulatePowerTransformerProperties(Outage.PowerTransformer cimPowerTransformer, ResourceDescription rd)
        {
            if ((cimPowerTransformer != null) && (rd != null))
            {
                OutageConverter.PopulateEquipmentProperties(cimPowerTransformer, rd);
            }
        }

        public static void PopulateEnergySourceProperties(Outage.EnergySource cimEnergySource, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEnergySource != null) && (rd != null))
            {
                OutageConverter.PopulateConductingEquipmentProperties(cimEnergySource, rd, importHelper, report);
            }
        }

        public static void PopulateEnergyConsumerProperties(Outage.EnergyConsumer cimEnergyConsumer, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEnergyConsumer != null) && (rd != null))
            {
                OutageConverter.PopulateConductingEquipmentProperties(cimEnergyConsumer, rd, importHelper, report);
            }
        }

        public static void PopulateTransformerWindingProperties(Outage.TransformerWinding cimTransformerWinding, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimTransformerWinding != null) && (rd != null))
            {
                OutageConverter.PopulateConductingEquipmentProperties(cimTransformerWinding, rd, importHelper, report);

                if (cimTransformerWinding.PowerTransformerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimTransformerWinding.PowerTransformer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert").Append(cimTransformerWinding.GetType().ToString()).Append(" rdfID = \"").Append(cimTransformerWinding.ID);
                        report.Report.Append("\" - Failed to set reference to PowerTransformer: rdfID\"").Append(cimTransformerWinding.PowerTransformer.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER, gid));
                }

                
            }
        }

        public static void PopulateSwitchProperties(Outage.Switch cimSwitch, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimSwitch != null) && (rd != null))
            {
                OutageConverter.PopulateConductingEquipmentProperties(cimSwitch, rd, importHelper, report);
                
            }
        }

        public static void PopulateFuseProperties(Outage.Fuse cimFuse, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimFuse != null) && (rd != null))
            {
                OutageConverter.PopulateSwitchProperties(cimFuse, rd, importHelper, report);
            }
        }

        public static void PopulateDisconnectorProperties(Outage.Disconnector cimDisconnector, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimDisconnector != null) && (rd != null))
            {
                OutageConverter.PopulateSwitchProperties(cimDisconnector, rd, importHelper, report);
            }
        }

        public static void PopulateConductorProperties(Outage.Conductor cimConductor, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConductor != null) && (rd != null))
            {
                OutageConverter.PopulateConductingEquipmentProperties(cimConductor, rd, importHelper, report);
            }
        }

        public static void PopulateACLineSegmentProperties(Outage.ACLineSegment cimACLineSegment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimACLineSegment != null) && (rd != null))
            {
                OutageConverter.PopulateConductorProperties(cimACLineSegment, rd, importHelper, report);
            }
        }

        public static void PopulateProtectedSwitchProperties(Outage.ProtectedSwitch cimProtectedSwitch, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimProtectedSwitch != null) && (rd != null))
            {
                OutageConverter.PopulateSwitchProperties(cimProtectedSwitch, rd, importHelper, report);
            }
        }

        public static void PopulateBreakerProperties(Outage.Breaker cimBreaker, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimBreaker != null) && (rd != null))
            {
                OutageConverter.PopulateProtectedSwitchProperties(cimBreaker, rd, importHelper, report);

                if (cimBreaker.NoReclosingHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.BREAKER_NORECLOSING, cimBreaker.NoReclosing));
                }
            }
        }

        public static void PopulateLoadBreakSwitchProperties(Outage.LoadBreakSwitch cimLoadBreakSwitch, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimLoadBreakSwitch != null) && (rd != null))
            {
                OutageConverter.PopulateProtectedSwitchProperties(cimLoadBreakSwitch, rd, importHelper, report);
            }
        }

        public static void PopulateBaseVoltageProperties(Outage.BaseVoltage cimBaseVoltage, ResourceDescription rd)
        {
            if ((cimBaseVoltage != null) && (rd != null))
            {
                OutageConverter.PopulateIdentifiedObjectProperties(cimBaseVoltage, rd);

                if (cimBaseVoltage.NominalVoltageHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE, cimBaseVoltage.NominalVoltage));
                }
            }
        }

        public static void PopulateTerminalProperties(Outage.Terminal cimTerminal, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimTerminal != null) && (rd != null))
            {
                OutageConverter.PopulateIdentifiedObjectProperties(cimTerminal, rd);

                //TODO: connducting equipment i conn node, kada se doda u profil.
            }
        }

        public static void PopulateConnectivityNodeProperties(Outage.ConnectivityNode cimConnectivityNode, ResourceDescription rd)
        {
            if ((cimConnectivityNode != null) && (rd != null))
            {
                OutageConverter.PopulateIdentifiedObjectProperties(cimConnectivityNode, rd);
            }
        }

        public static void PopulateMeasurementProperties(Outage.Measurement cimMeasurement, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimMeasurement != null) && (rd != null))
            {
                OutageConverter.PopulateIdentifiedObjectProperties(cimMeasurement, rd);

                if (cimMeasurement.AddressHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_ADDRESS, cimMeasurement.Address));
                }

                if (cimMeasurement.IsInputHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_ISINPUT, cimMeasurement.IsInput));
                }

                if (cimMeasurement.TerminalHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimMeasurement.Terminal.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimMeasurement.GetType().ToString()).Append(" rdfID = \"").Append(cimMeasurement.ID);
                        report.Report.Append("\" - Failed to set reference to Terminal: rdfID \"").Append(cimMeasurement.Terminal.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_TERMINAL, gid));
                }
            }
        }

        public static void PopulateDiscreteProperties(Outage.Discrete cimDiscrete, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimDiscrete != null) && (rd != null))
            {
                OutageConverter.PopulateMeasurementProperties(cimDiscrete, rd, importHelper, report);

                if (cimDiscrete.CurrentOpenHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_CURRENTOPEN, cimDiscrete.CurrentOpen));
                }

                if (cimDiscrete.MaxValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MAXVALUE, cimDiscrete.MaxValue));
                }

                if (cimDiscrete.MeasurementTypeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MEASUREMENTTYPE, (short)GetDiscreteMeasuremetType(cimDiscrete.MeasurementType)));
                }

                if (cimDiscrete.MinValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MINVALUE, cimDiscrete.MinValue));
                }

                if (cimDiscrete.NormalValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_NORMALVALUE, cimDiscrete.NormalValue));
                }
            }
        }

        public static void PopulateAnalogProperties(Outage.Analog cimAnalog, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimAnalog != null) && (rd != null))
            {
                OutageConverter.PopulateMeasurementProperties(cimAnalog, rd, importHelper, report);

                if (cimAnalog.CurrentValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_CURRENTVALUE, cimAnalog.CurrentValue));
                }

                if (cimAnalog.MaxValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_MAXVALUE, cimAnalog.MaxValue));
                }

                if (cimAnalog.MeasurementTypeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_SIGNALTYPE, (short)GetAnalogMeasurementType(cimAnalog.MeasurementType)));
                }

                if (cimAnalog.MinValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_MINVALUE, cimAnalog.MinValue));
                }

                if (cimAnalog.NormalValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_NORMALVALUE, cimAnalog.NormalValue));
                }
            }
        }
        #endregion

        #region Enums convert

        //TODO
        public static Outage.Common.DiscreteMeasurementType GetDiscreteMeasuremetType(Outage.DiscreteMeasurementType measurementType)
        {
            return 0;
        }
        //TODO
        public static Outage.Common.AnalogMeasurementType GetAnalogMeasurementType(Outage.AnalogMeasurementType measurementType)
        {
            return 0;
        }
        #endregion
    }
}
