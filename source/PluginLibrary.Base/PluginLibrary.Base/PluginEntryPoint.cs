using System.Collections.Generic;
/* 
 * All of these items are optional
 * The assembly dll will be loaded anyway, even if it has no entry point but in that case it will most likely do nothing
 */

namespace PluginLibrary.Base    // Namespace in which PluginEntryPoint resides must be same as dll name
{
    public static class PluginEntryPoint    // Entry class
    {
        public static readonly Dictionary<string, string> info = new Dictionary<string, string>() { // Dictionary contains basic info
            {"name","PluginBase"},
            {"about","Base for a plugin that does nothing"},
            {"version","1"},
        };

        public static void Init() {         // This function will be called when plugin is loaded
            // Do stuff
        }
    }
}
