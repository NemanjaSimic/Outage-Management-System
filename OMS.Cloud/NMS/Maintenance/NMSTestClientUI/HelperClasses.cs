using OMS.Common.Cloud;
using OMS.Common.NmsContracts.GDA;
using System;
using System.Collections.Generic;
using System.Text;

namespace FTN.Services.NetworkModelService.TestClientUI
{
    public class GlobalIdentifierViewModel
    {
        public long GID { get; set; }

        public string Type { get; set; }
    }

    public class ClassTypeViewModel
    {
        public ModelCode ClassType { get; set; }
    }

    public class DmsTypeViewModel
    {
        public DMSType DmsType { get; set; }
    }

    public class PropertyViewModel
    {
        public ModelCode Property { get; set; }
    }

    public static class RelationalPropertiesHelper
    {
        private static readonly Dictionary<ModelCode, ModelCode> relations = new Dictionary<ModelCode, ModelCode>
        {
            { ModelCode.CONDUCTINGEQUIPMENT_TERMINALS,          ModelCode.TERMINAL              },
            { ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE,        ModelCode.BASEVOLTAGE           },

            { ModelCode.TERMINAL_CONDUCTINGEQUIPMENT,           ModelCode.CONDUCTINGEQUIPMENT   },
            { ModelCode.TERMINAL_CONNECTIVITYNODE,              ModelCode.CONNECTIVITYNODE      },
            { ModelCode.TERMINAL_MEASUREMENTS,                  ModelCode.MEASUREMENT           },
                
            { ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENTS,       ModelCode.CONDUCTINGEQUIPMENT   },

            { ModelCode.MEASUREMENT_TERMINAL,                   ModelCode.TERMINAL              },

            { ModelCode.CONNECTIVITYNODE_TERMINALS,             ModelCode.TERMINAL              },

            { ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS,   ModelCode.TRANSFORMERWINDING    },

            { ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER,    ModelCode.POWERTRANSFORMER      },
        };
 
        public static Dictionary<ModelCode, ModelCode> Relations { get { return relations; } }
    }

    public static class StringAppender
    {
        public static void AppendReferenceVector(StringBuilder sb, Property property)
        {
            sb.Append($"\t{property.Id}: {Environment.NewLine}");
            foreach (long gid in property.AsReferences())
            {
                sb.Append($"\t\tGid: 0x{gid:X16}{ Environment.NewLine}");
            }
        }

        public static void AppendReference(StringBuilder sb, Property property)
        {
            sb.Append($"\t{property.Id}: 0x{property.AsReference():X16}{Environment.NewLine}");
        }

        public static void AppendString(StringBuilder sb, Property property)
        {
            sb.Append($"\t{property.Id}: {property.AsString()}{Environment.NewLine}");
        }

        public static void AppendFloat(StringBuilder sb, Property property)
        {
            sb.Append($"\t{property.Id}: {property.AsFloat()}{Environment.NewLine}");
        }

        public static void AppendLong(StringBuilder sb, Property property)
        {
            sb.Append($"\t{property.Id}: 0x{property.AsLong():X16}{Environment.NewLine}");
        }
    }
}
