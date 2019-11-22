using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSCommon
{
    public enum DMSType : short
    {
        MASK_TYPE = unchecked((short)0xFFFF),


    }
    
    [Flags]
    public enum ModelCode : long
    {
        IDOBJ								= 0x1000000000000000,
		IDOBJ_GID							= 0x1000000000000104,
		IDOBJ_DESCRIPTION					= 0x1000000000000207,
		IDOBJ_MRID							= 0x1000000000000307,
		IDOBJ_NAME							= 0x1000000000000407,

        PSR									= 0x1100000000000000,

        BASEVOLTAGE                         = 0x1200000000010000,
        BASEVOLTAGE_NOMINALVOLTAGE          = 0x1200000000010105,
        BASEVOLTAGE_CONDUCTINGEQUIPMENTS    = 0x1200000000010219,


        TERMINAL                            = 0x1300000000020000,
        TERMINAL_CONDUCTINGEQUIPMENTS       = 0x1300000000020119,
        TERMINAL_CONNECTIVITYNODE           = 0x1300000000020209,
        TERMINAL_MEASUREMENTS               = 0x1300000000020319,

        CONNECTIVITYNODE                    = 0x1400000000030000,
        CONNECTIVITYNODE_TERMINALS          = 0x1400000000030109, //DA LI JE APSTRAKTNA ILI NE (PO SLICI NIJE, NA HTMLU JESTE)

        EQUIPMENT                           = 0x1110000000000000,

        POWERTRANSFORMER                    = 0x1111000000030000,
        POWERTRANSFORMER_TRANSFORMERWINDINGS= 0x1111000000030119, //TODO: TRANSFORMEREND???

        CONDUCTINGEQUIPMENT                 = 0x1112000000000000,
        CONDUCTINGEQUIPMENT_BASEVOLTAGE     = 0x1112000000000109,
        CONDUCTINGEQUIPMENT_TERMINALS       = 0x1112000000000219,

        ENERGYSOURCE                        = 0x1112100000040000,

        ENERGYCONSUMER                      = 0x1112200000050000,

        TRANSFORMERWINDING                  = 0x1112300000060000,  //TODO: TRANSFORMEREND??? 
        TRANSFORMERWINDING_POWERTRANSFORMER = 0x1112300000060119,  //TODO: TRANSFORMEREND???

        SWITCH                              = 0x111240000000000,

        FUSE                                = 0x111241000070000,

        DISCONNECTOR                        = 0x111242000080000,

        PROTECTEDSWITCH                     = 0x111243000000000,

        BREAKER                             = 0x111243100090000,
        BREAKER_NORECLOSING                 = 0x111243100090101,

        LOADBREAKSWITCH                     = 0x1112432000a0000,

        CONDUCTOR                           = 0x111250000000000,

        ACLINESEGMENT                       = 0x1112510000b0000,

        MEASUREMENT                         = 0x150000000000000,
        MEASUREMENT_ADDRESS                 = 0x150000000000107,
        MEASUREMENT_ISINPUT                 = 0x150000000000201,
        MEASUREMENT_TERMINAL                = 0x150000000000309,

        DISCRETE                            = 0x1510000000c0000,
        DISCRETE_CURRENTOPEN                = 0x1510000000c0101,
        DISCRETE_MAXVALUE                   = 0x1510000000c0203,
        DISCRETE_MEASUREMENTTYPE            = 0x1510000000c030a,
        DISCRETE_MINVALUE                   = 0x1510000000c0403,
        DISCRETE_NORMALVALUE                = 0x1510000000c0503,

        ANALOG                              = 0x1520000000d0000,
        ANALOG_CURRENTVALUE                 = 0x1520000000d0105,
        ANALOG_MAXVALUE                     = 0x1520000000d0205,
        ANALOG_MINVALUE                     = 0x1520000000d0305,
        ANALOG_NORMALVALUE                  = 0x1520000000d0405,
        ANALOG_SIGNALTYPE                   = 0x1520000000d050a,

    }

    [Flags]
    public enum ModelCodeMask : long
    {
        MASK_TYPE = 0x00000000ffff0000,
        MASK_ATTRIBUTE_INDEX = 0x000000000000ff00,
        MASK_ATTRIBUTE_TYPE = 0x00000000000000ff,

        MASK_INHERITANCE_ONLY = unchecked((long)0xffffffff00000000),
        MASK_FIRSTNBL = unchecked((long)0xf000000000000000),
        MASK_DELFROMNBL8 = unchecked((long)0xfffffff000000000),
    }
}
