using System.IO;
using System.Reflection;


namespace TypPostriku
{
    public class AppRuntimeData
    {
        public string startupPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public string settingFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"Data");
        public string appSettingFile = "config.json";
        public string appName = Assembly.GetEntryAssembly().GetName().FullName.Split(',')[0];
        public Dictionary<string, string> AppClientSettings = new Dictionary<string, string>();
    }
}
