using System;

namespace CIM.Model
{
    /// <summary>
    /// CIMConstants class defines various constanst common for CIM files.  
    /// <para>@author: Stanislava Selena</para>
    /// </summary>
    public static class CIMConstants
    {
        /// <summary>
        /// List of possible winding types in CIM.
        /// </summary>
        public enum WindingType
        {
            Primary = 0,
            Secondary,
            Tertiary,
            Quaternary,
            Other
        };

        /// <summary>
        /// List of possible voltage levels (used in this application).
        /// </summary>
        public enum VoltageLevel
        {
            None = 0,
            LV,
            HVMV,            
            HV          
        };

        /// <summary> cim namespace "cim" </summary>
        public const string CIMNamespace = "cim";

		/// <summary> Predefied CIM type "IdentifiedObject" </summary>
		public const string TypeNameIdentifiedObject = "IdentifiedObject";
        /// <summary> Predefied CIM type "ConnectivityNode" </summary>
        public const string TypeNameConnectivityNode = "ConnectivityNode";
        /// <summary> Predefied CIM type "Terminal" </summary>
        public const string TypeNameTerminal = "Terminal";
        /// <summary> Predefied CIM type "PowerTransformer" </summary>
        public const string TypeNamePowerTransformer = "PowerTransformer";
        /// <summary> Predefied CIM type "TransformerWinding" </summary>
        public const string TypeNameTransformerWinding = "TransformerWinding";
        /// <summary> Predefied CIM type "EquipmentContainer" </summary>
        public const string TypeNameEquipmentContainer = "EquipmentContainer";
        /// <summary> Predefied CIM type "Substation" </summary>
        public const string TypeNameSubstation = "Substation";
        /// <summary> Predefied CIM type "VoltageLevel" </summary>
        public const string TypeNameVoltageLevel = "VoltageLevel";
        /// <summary> Predefied CIM type "Bay" </summary>
        public const string TypeNameBay = "Bay";
        /// <summary> Predefied CIM type "Line" </summary>
        public const string TypeNameLine = "Line";
        /// <summary> Predefied CIM type "ConductingEquipment" </summary>
        public const string TypeNameConductingEquipment = "ConductingEquipment";
        /// <summary> Predefied CIM type "BaseVoltage" </summary>
        public const string TypeNameBaseVoltage = "BaseVoltage";

        /// <summary> Predefied expected CIM attribute for TransformerWinding: "ratedKV" </summary>
        public const string AttributeNameTransformerWindingVoltageKV = "ratedKV";
        /// <summary> Predefied expected CIM attribute for TransformerWinding: "ratedU" </summary>
        public const string AttributeNameTransformerWindingVoltageU = "ratedU";
        /// <summary> Predefied expected CIM attribute of TransformerWinding: "windingType" </summary>
        public const string AttributeNameTransformerWindingType = "windingType";               
        /// <summary> Predefied expected CIM attribute of BaseVoltage: "nominalVoltage" </summary>
        public const string AttributeNameBaseVoltageNominalVoltage = "nominalVoltage";
        /// <summary> Predefied expected CIM attribute "name" </summary>
        public const string AttributeNameIdentifiedObjectNameShort = "name";
		/// <summary> Predefied expected CIM attribute "EquipmentContainer" </summary>
		public const string AttributeNameEquipmentContainer = "EquipmentContainer";
		/// <summary> Predefied expected CIM attribute "ConnectivityNodeContainer" </summary>
		public const string AttributeNameConnectivityNodeContainer = "ConnectivityNodeContainer";
        /// <summary> Predefied expected CIM attribute "IdentifiedObject.name" </summary>
        public const string AttributeNameIdentifiedObjectName = "IdentifiedObject.name";
        /// <summary> Predefied expected CIM attribute "IdentifiedObject.mRID" </summary>
        public const string AttributeNameIdentifiedObjectMRID = "IdentifiedObject.mRID";
		/// <summary> Predefied expected CIM attribute "Terminal.sequenceNumber" </summary>
		public const string AttributeNameTerminalSequenceNumber = "Terminal.sequenceNumber";

        /// <summary> string "primary" - Predefied expected value of attribute "TransformerWinding.windingType" </summary>
        public const string TransformerWindingTypePrimary = "primary";
        /// <summary> string "secondary" - Predefied expected value of attribute "TransformerWinding.windingType" </summary>
        public const string TransformerWindingTypeSecondary = "secondary";
        /// <summary> string "tertiary" -  Predefied expected value of attribute "TransformerWinding.windingType" </summary>
        public const string TransformerWindingTypeTertiary = "tertiary";
        /// <summary> string "quaternary" -  Predefied expected value of attribute "TransformerWinding.windingType" </summary>
        public const string TransformerWindingTypeQuaternary = "quaternary";

        /// <summary> criteria for main TransformerWinding object: TransformerWinding.ratedU lower value </summary>
        public const Double VoltageRangeLowerU = 1000;
        /// <summary> criteria for main TransformerWinding object: TransformerWinding.ratedU upper value </summary>
        public const Double VoltageRangeUpperU = 35000;
        /// <summary> criteria for main TransformerWinding object: TransformerWinding.ratedKV lower value </summary>
        public const Double VoltageRangeLowerKV = 1;
        /// <summary> criteria for main TransformerWinding object: TransformerWinding.ratedKV upper value </summary>
        public const Double VoltageRangeUpperKV = 35;
    }
}
