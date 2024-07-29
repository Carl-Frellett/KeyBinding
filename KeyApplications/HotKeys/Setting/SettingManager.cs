using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;
using KeyBindingServiceMeow.API.Features.HotKey;

namespace KeyBindingServiceMeow.KeyApplications.HotKeys.Setting
{
    internal class SettingManager
    {
        public static SettingManager instance { get; private set;}

        public readonly Dictionary<string, List<HotKeySetting>> settings = new Dictionary<string, List<HotKeySetting>>();

        private string baseDirectory;

        public static void Initialize()
        {
            instance = new SettingManager();

            string directory = Config.instance.HotKeySettingDirectory;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            instance.baseDirectory = directory;
        }

        public static void Destruct()
        {
            instance = null;
        }

        private string GetFilePath(string Nickname)
        {
            return Path.Combine(baseDirectory, $"{Nickname}.json");
        }

        private void SaveSettings(string Nickname)
        {
            var filePath = GetFilePath(Nickname);

            if (!settings.ContainsKey(Nickname))
            {
                LoadSettings(Nickname); //Initialize the setting if no setting found
            }

            string json = JsonConvert.SerializeObject(settings[Nickname], Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Try load the setting from the file, if no setting found, initialize the setting
        /// </summary>
        /// <param name="Nickname">The user id of the player</param>
        /// <returns></returns>
        private List<HotKeySetting> LoadSettings(string Nickname)
        {
            string filePath = GetFilePath(Nickname);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                settings[Nickname] = JsonConvert.DeserializeObject<List<HotKeySetting>>(json);
            }
            else
            {
                settings[Nickname] = new List<HotKeySetting>();//Initialize the setting if no setting found
                SaveSettings(Nickname);
            }

            return settings[Nickname];
        }

        public IReadOnlyList<HotKeySetting> GetSettings(string Nickname)
        {
            if (!settings.ContainsKey(Nickname))
            {
                LoadSettings(Nickname);
            }

            return settings[Nickname].AsReadOnly();
        }

        public void ChangeSettings(string Nickname, HotKeySetting config)
        {
            if (!settings.ContainsKey(Nickname))
            {
                LoadSettings(Nickname);
            }

            settings[Nickname].RemoveAll(x => x.id == config.id);
            settings[Nickname].Add(config);

            SaveSettings(Nickname);
        }
    }
}
