using System;

namespace Outage.Common
{

    public enum DMSType : short
    {
        MASK_TYPE = unchecked((short)0xFFFF),

        BASEVOLTAGE                         = 0x0001,
        TERMINAL                            = 0x0002,
        CONNECTIVITYNODE                    = 0x0003,
        POWERTRANSFORMER                    = 0x0004,
        ENERGYSOURCE                        = 0x0005,
        ENERGYCONSUMER                      = 0x0006,
        TRANSFORMERWINDING                  = 0x0007,
        FUSE                                = 0x0008,
        DISCONNECTOR                        = 0x0009,
        BREAKER                             = 0x000a,
        LOADBREAKSWITCH                     = 0x000b,
        ACLINESEGMENT                       = 0x000c,
        DISCRETE                            = 0x000d,
        ANALOG                              = 0x000e,
        SYNCHRONOUSMACHINE                  = 0x000f

    }
    
    [Flags]
    public enum ModelCode : long
    {
        IDOBJ								    = 0x1000000000000000,
		IDOBJ_GID							    = 0x1000000000000104,
		IDOBJ_DESCRIPTION					    = 0x1000000000000207,
		IDOBJ_MRID							    = 0x1000000000000307,
		IDOBJ_NAME							    = 0x1000000000000407,

        PSR									    = 0x1100000000000000,

        BASEVOLTAGE                             = 0x1200000000010000,
        BASEVOLTAGE_NOMINALVOLTAGE              = 0x1200000000010105,
        BASEVOLTAGE_CONDUCTINGEQUIPMENTS        = 0x1200000000010219,


        TERMINAL                                = 0x1300000000020000,
        TERMINAL_CONDUCTINGEQUIPMENT            = 0x1300000000020109,
        TERMINAL_CONNECTIVITYNODE               = 0x1300000000020209,
        TERMINAL_MEASUREMENTS                   = 0x1300000000020319,

        CONNECTIVITYNODE                        = 0x1400000000030000,
        CONNECTIVITYNODE_TERMINALS              = 0x1400000000030119,

        EQUIPMENT                               = 0x1110000000000000,

        POWERTRANSFORMER                        = 0x1111000000040000,
        POWERTRANSFORMER_TRANSFORMERWINDINGS    = 0x1111000000040119,

        CONDUCTINGEQUIPMENT                     = 0x1112000000000000,
        CONDUCTINGEQUIPMENT_BASEVOLTAGE         = 0x1112000000000109,
        CONDUCTINGEQUIPMENT_TERMINALS           = 0x1112000000000219,
        CONDUCTINGEQUIPMENT_ISREMOTE            = 0x1112000000000301,

        ENERGYSOURCE                            = 0x1112100000050000,

        ENERGYCONSUMER                          = 0x1112200000060000,
        ENERGYCONSUMER_FIRSTNAME                = 0x1112200000060107,
        ENERGYCONSUMER_LASTNAME                 = 0x1112200000060207,

        TRANSFORMERWINDING                      = 0x1112300000070000,
        TRANSFORMERWINDING_POWERTRANSFORMER     = 0x1112300000070109,

        SWITCH                                  = 0x1112400000000000,

        FUSE                                    = 0x1112410000080000,

        DISCONNECTOR                            = 0x1112420000090000,

        PROTECTEDSWITCH                         = 0x1112430000000000,

        BREAKER                                 = 0x11124310000a0000,
        BREAKER_NORECLOSING                     = 0x11124310000a0101,

        LOADBREAKSWITCH                         = 0x11124320000b0000,

        CONDUCTOR                               = 0x1112500000000000,

        ACLINESEGMENT                           = 0x11125100000c0000,

        SYNCHRONOUSMACHINE                      = 0x11126000000f0000,
        SYNCHRONOUSMACHINE_CAPACITY             = 0x11126000000f0105,
        SYNCHRONOUSMACHINE_CURRENTREGIME        = 0x11126000000f0205,


        MEASUREMENT = 0x1500000000000000,
        MEASUREMENT_ADDRESS                     = 0x1500000000000107,
        MEASUREMENT_ISINPUT                     = 0x1500000000000201,
        MEASUREMENT_TERMINAL                    = 0x1500000000000309,

        DISCRETE                                = 0x15100000000d0000,
        DISCRETE_CURRENTOPEN                    = 0x15100000000d0101,
        DISCRETE_MAXVALUE                       = 0x15100000000d0203,
        DISCRETE_MEASUREMENTTYPE                = 0x15100000000d030a,
        DISCRETE_MINVALUE                       = 0x15100000000d0403,
        DISCRETE_NORMALVALUE                    = 0x15100000000d0503,

        ANALOG                                  = 0x15200000000e0000,
        ANALOG_CURRENTVALUE                     = 0x15200000000e0105,
        ANALOG_MAXVALUE                         = 0x15200000000e0205,
        ANALOG_MINVALUE                         = 0x15200000000e0305,
        ANALOG_NORMALVALUE                      = 0x15200000000e0405,
        ANALOG_SIGNALTYPE                       = 0x15200000000e050a,
        ANALOG_DEVIATION                        = 0x15200000000e0605,
        ANALOG_SCALINGFACTOR                    = 0x15200000000e0705,
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


