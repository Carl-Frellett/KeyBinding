using CommandSystem;
using Exiled.API.Features;
using Exiled.Events;
using KeyBindingServiceMeow.API;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace KeyBindingServiceMeow.BindingManager
{
    /// <summary>
    /// This command receives the keys from client
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class BindingReceiverCommand : ICommand
    {
        public string Command { get; } = "UseKeyBind";

        public string[] Aliases { get; } = new string[1] { "U^" };//This might reduce the bandwidth required....I guess

        public string Description { get; } = "Used for KeyBindingMeow. Do not use this command if you're unsure what this is.";

        public bool SanitizeResponse { get; } = true;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = new List<string>(arguments);

            //Check args
            if (args.Count != 1)
            {
                Log.Error("Incorrect number of arguments passed to BindingReceiverCommand.");

                response = "An internal error had occured while handling the binding key";
                return false;
            }

            // 确保 sender 是 PlayerCommandSender 类型
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
            KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), args[0]);

            KeyBindingManager.Notify(player, keyCode);

            response = string.Empty;
            return true;
        }
    }
}
