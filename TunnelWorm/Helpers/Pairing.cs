using System.Collections.Generic;

namespace TunnelWorm.Helpers
{
    public static class Pairing
    {
        public static KeyValuePair<string, string> Of(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}