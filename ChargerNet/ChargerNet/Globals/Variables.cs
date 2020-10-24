using ChargerNet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;

namespace ChargerNet.Globals
{
    public class Variables
    {
        public const string GOOGLE_MAPS_ANDROID_API_KEY = "AIzaSyCTo1fecP6ftTtPLkvF7xtyfJqCmraWkdI";
        public const string DatabaseName = "ChargerNetDB.db";
        public const string UAH = "₴";
        public class CurrentUser
        {
            public const string FileName = ".userinfo";
            public static string FullPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FileName);
            public string Phone { get; set; }
            public string Name { get; set; }
            public User User { get; set; }

            public void Save()
            {
                var text = JsonConvert.SerializeObject(this);
                File.WriteAllText(FullPath, text);
            }

            private CurrentUser() { }


            private static CurrentUser _current;
            public static CurrentUser Current
            {
                get
                {
                    if (_current == null)
                    {
                        object obj = null;
                        if (File.Exists(FullPath))
                        {
                            var text = File.ReadAllText(FullPath);
                            obj = JsonConvert.DeserializeObject(text);
                        }
                        _current = new CurrentUser();
                        try
                        {
                            if (obj != null)
                            {
                                var phone = ((JObject)obj).Value<string>(nameof(Phone));
                                if (!string.IsNullOrWhiteSpace(phone))
                                {
                                    _current.Phone = phone;
                                }
                                var name = ((JObject)obj).Value<string>(nameof(Name));
                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    _current.Name = name;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    return _current;
                }
            }
        }
    }
}
