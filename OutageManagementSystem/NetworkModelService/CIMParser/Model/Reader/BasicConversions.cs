using System;

namespace CIM.Model
{
    class BasicConversions
    {
        public static bool StrToInt(string str, out int num)
        {
            return Int32.TryParse(str, out num);
        }
    }
}
