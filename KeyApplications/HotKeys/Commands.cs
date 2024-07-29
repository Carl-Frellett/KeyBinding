using UnityEngine;
using CommandSystem;
using Exiled.API.Features;

using KeyBindingServiceMeow.API.Features.HotKey;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteAdmin;

namespace KeyBindingServiceMeow.KeyApplications.HotKeys
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ChangeHotKeyCommand : ICommand
    {
        public string Command { get; } = "ChangeHotKey";

        public string[] Aliases { get; } = new string[] { "CHK" };

        public string Description { get; } = "Change the hotkey binded to the server\n Usage: CHK [HotKeyID] [NewBindingKey]";

        public bool SanitizeResponse { get; } = true;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();

            //Check args
            if(args.Count != 2)
            {
                response = "Invalid arguments. Usage: CHK [HotKeyID] [NewBindingKey]";
                return false;
            }

            if (!(sender is PlayerCommandSender playerCommandSender))
            {
                response = "<color=red>错误：此命令只能由玩家执行</color>";
                return false;
            }
            Player player = Player.Get(playerCommandSender.Nickname);

            if (player == null)
            {
                response = "<color=red>错误：未找到此玩家</color>";
                return false;
            }
            var manager = HotKeyManager.Get(player);

            var rawID = args[0];
            var rawNewKey = args[1];

            if (!manager.TryGetKey(rawID, out HotKey hotKey))
            {
                response = "HotKey ID not found";
                return false;
            }

            if (!Enum.TryParse(rawNewKey, true, out KeyCode newKey))
            {
                response = "Invalid binding key";
                return false;
            }

            //Handle args
            manager.SetKey(new HotKeySetting(hotKey.id, newKey));

            response = $"Successfully set {hotKey.id} to {newKey}";
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class ResetHotKeyCommand : ICommand
    {
        public string Command { get; } = "ResetHotKey";

        public string[] Aliases { get; } = new string[] { "RHK" };

        public string Description { get; } = "Reset the hotkey binded to the server\n Usage: RHK [HotKeyID] or RHK all(to reset all keys)";

        public bool SanitizeResponse { get; } = true;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();

            //Check args
            if (args.Count != 1)
            {
                response = "Invalid arguments. Usage: RHK [HotKeyID] or RHK all (to reset all keys)";
                return false;
            }

            //Handle args
            if (!(sender is PlayerCommandSender playerCommandSender))
            {
                response = "<color=red>错误：此命令只能由玩家执行</color>";
                return false;
            }
            Player player = Player.Get(playerCommandSender.Nickname);

            if (player == null)
            {
                response = "<color=red>错误：未找到此玩家</color>";
                return false;
            }
            var manager = HotKeyManager.Get(player);            

            //Handle "all" argument
            if (args[0].ToLower() == "all")
            {
                manager.ResetAllKeys();
                response = "Successfully reset all keys";
                return true;
            }

            //Handle normal argument
            var rawID = args[0];

            //Check again
            if (!manager.TryGetKey(rawID, out HotKey hotKey))
            {
                response = "HotKey ID not found";
                return false;
            }

            manager.ResetKey(hotKey.id);

            response = string.Empty;
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class ShowHotKeysCommand : ICommand
    {
        public string Command { get; } = "ShowHotKeys";

        public string[] Aliases { get; } = new string[] { "SHK" };

        public string Description { get; } = "Show all the hotkeys binded to the server";

        public bool SanitizeResponse { get; } = true;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // 确认命令发送方是否为一个游戏内玩家
            if (!(sender is PlayerCommandSender playerCommandSender))
            {
                response = "<color=red>错误：此命令只能由玩家执行</color>";
                return false;
            }

            // 根据 Player ID 或用户名获取 Player 实例
            var playerId = playerCommandSender.Nickname;

            // 通过 UserID 获取指定玩家对象
            var player = Player.Get(playerId);

            if (player == null)
            {
                response = "<color=red>未找到玩家</color>";
                return false;
            }

            HotKeyManager manager = HotKeyManager.Get(player);

            List<HotKey> HotKeyList = new List<HotKey>(manager.GetKeys());

            //Categorize and make sure non-categorized hotkeys shows first
            HotKeyList.Sort((x, y) =>
            {
                var xc = x.category;
                var yc = y.category;

                if (string.IsNullOrEmpty(xc) && !string.IsNullOrEmpty(yc))
                    return -1;

                if (!string.IsNullOrEmpty(xc) && string.IsNullOrEmpty(yc))
                    return 1;

                return string.Compare(xc, yc, StringComparison.Ordinal);
            });

            if (HotKeyList.Count == 0)
            {
                response = "No hotkeys found";
                return true;
            }

            List<string> content = new List<string>();
            content.Add("\n=====================Hot Keys=====================");

            string lastCategory = string.Empty;

            foreach (HotKey hotKey in HotKeyList)
            {
                if(hotKey.category != lastCategory)
                {
                    lastCategory = hotKey.category;
                    content.Add($"\n---------------------{lastCategory}---------------------");
                }

                content.Add($"-{hotKey.name}");
                content.Add($"      Key: {hotKey.currentKey}");
                content.Add($"      ID: {hotKey.id}");

                if(!string.IsNullOrEmpty(hotKey.description))
                    content.Add($"      Description: {hotKey.description}");
            }

            response = string.Join("\n", content);
            return true;
        }
    }
}
