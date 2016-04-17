using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Gruppenfoto.App
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;


        public static string BackendUrl
        {
            get { return AppSettings.GetValueOrDefault("BackendUrl", "http://gruppenfoto.sachsenhofer.com"); }
            set { AppSettings.AddOrUpdateValue("BackendUrl", value); }
        }


        public static string EventId
        {
            get { return AppSettings.GetValueOrDefault("EventId", "test"); }
            set { AppSettings.AddOrUpdateValue("EventId", value); }
        }


        public static string[] UploadQueue
        {
            get
            {
                var json = AppSettings.GetValueOrDefault("UploadQueue", "{}");
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(json);
                }
                catch
                {
                    return new string[0];
                }
            }
            set
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                AppSettings.AddOrUpdateValue("UploadQueue", json);
            }
        }
    }
}
