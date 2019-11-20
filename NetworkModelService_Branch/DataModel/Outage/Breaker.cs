//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataModel.Outage
{
    using System;
    using DataModel.Outage;
    
    
    /// A mechanical switching device capable of making, carrying, and breaking currents under normal circuit conditions and also making, carrying for a specified time, and breaking currents under specified abnormal circuit conditions e.g.  those of short circuit.
    public class Breaker : ProtectedSwitch {
        
        /// Specifies that manual reclosing after fault is not permitted on this breaker. Such breakers must not have a reclosing relay either.
        private System.Boolean? cim_noReclosing;
        
        private const bool isNoReclosingMandatory = true;
        
        private const string _noReclosingPrefix = "tdms";
        
        public virtual bool NoReclosing {
            get {
                return this.cim_noReclosing.GetValueOrDefault();
            }
            set {
                this.cim_noReclosing = value;
            }
        }
        
        public virtual bool NoReclosingHasValue {
            get {
                return this.cim_noReclosing != null;
            }
        }
        
        public static bool IsNoReclosingMandatory {
            get {
                return isNoReclosingMandatory;
            }
        }
        
        public static string NoReclosingPrefix {
            get {
                return _noReclosingPrefix;
            }
        }
    }
}
