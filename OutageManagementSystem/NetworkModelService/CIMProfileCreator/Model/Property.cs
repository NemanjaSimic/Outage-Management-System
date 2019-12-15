using System;
using FTN.ESI.SIMES.CIM.Manager;

namespace FTN.ESI.SIMES.CIM.Model
{
    class Property : ProfileElement
    {
        protected string domain;
        protected Class domainAsObject;
        protected string dataType;
        protected Type dataTypeAsSimple;
        protected string range;
        protected ProfileElement rangeAsObject;
        bool isDataTypeSimple;
        protected ProfileElement dataTypeAsComplexObject;


        public Property() : base("From Derived") { }


        #region get and set
        public bool IsPropertyDataTypeSimple
        {
            get
            {
                return isDataTypeSimple;
            }
        }

        public string Domain
        {
            get
            {
                return domain;
            }
            set
            {
                domain = value;
            }
        }

        

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the ProfileElement which is cosidered for data type of this profile property.</para>
        /// <para>Given ProfileElement should be of type ProfileElementTypes.Class.</para>
        /// <para>This can be null if IsPropertyDataTypeSimple has <c>true</c> value.</para>
        /// </summary>
        public ProfileElement DataTypeAsComplexObject
        {
            get
            {
                return dataTypeAsComplexObject;
            }
            set
            {
                dataTypeAsComplexObject = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the CIMType which is the data type of this profile property.</para>        
        /// <para>This can be null if IsPropertyDataTypeSimple has <c>false</c> value.</para>
        /// </summary>
        public Type DataTypeAsSimple
        {
            get
            {
                return dataTypeAsSimple;
            }
            set
            {
                dataTypeAsSimple = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the string representation of data type for this profile property.</para>
        /// </summary>
        public string DataType
        {
            get
            {
                return dataType;
            }
            set
            {
                dataType = value;

                if (dataType != null)
                {
                    isDataTypeSimple = true;
                    string shortDT = StringManipulationManager.ExtractShortestName(dataType, StringManipulationManager.SeparatorSharp);
                    switch (shortDT.ToLower())
                    {
                        case SimpleDataTypeInteger:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Int32");
                                break;
                            }
                        case SimpleDataTypeInt:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Int32");
                                break;
                            }
                        case SimpleDataTypeFloat:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Single");
                                break;
                            }
                        case SimpleDataTypeString:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.String");
                                break;
                            }
                        case SimpleDataTypeBoolean:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Boolean");
                                break;
                            }
                        case SimpleDataTypeBool:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Boolean");
                                break;
                            }
                        case SimpleDataTypeDateTime:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.DateTime");
                                break;
                            }
                        default:
                            {
                                isDataTypeSimple = false;
                                break;
                            }
                    }
                }
            }
        }

        public Class DomainAsObject
        {
            get
            {
                return domainAsObject;
            }
            set
            {
                domainAsObject = value;
            }
        }

        public string Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
            }
        }

        public ProfileElement RangeAsObject
        {
            get
            {
                return rangeAsObject;
            }
            set
            {
                rangeAsObject = value;
            }
        }
        #endregion get and set
    }
}
