using Exiled.API.Features;
using KeyBindingServiceMeow.API.Event.EventArgs;
using KeyBindingServiceMeow.KeyHandlers;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using DongShanAPI.Hint;
using UnityEngine;

namespace KeyBindingServiceMeow.ClientSetupHelper
{
    //Not implemented yet
    internal class Verificator
    {
        private static List<Verificator> setupHelpers = new List<Verificator>();

        private Player player;
        private bool isSetup = false;
        private DateTime timeCreated;

        private KeyCode setupKey => Config.instance.SetupKey;

        public Verificator(Player player)
        {
            this.player = player;
            this.timeCreated = DateTime.Now;

            EventKeyHandler.instance.RegisterKey(setupKey, Verify);
            BroadCast(Config.instance.MessageBeforeSetup);

            if(Config.instance.TimeOut == -1)
                Timing.CallDelayed((float)Config.instance.TimeOut + 0.05f, CheckTimeOut);

            setupHelpers.Add(this);
        }

        public static void Create(Player player)
        {
            if (!Config.instance.UsePlayerVerification)
                return;

            if (setupHelpers.Any(x => x.player == player))
                throw new Exception("A setup helper is already created for " + player.Nickname);

            new Verificator(player);

            Log.Debug("SetupHelper Created for " + player.Nickname);
        }

        public static void Destruct(Player player)
        {
            Verificator instance = Get(player);

            setupHelpers.Remove(instance);

            Log.Debug("SetupHelper Destructed for " + player.Nickname);
        }

        public static bool IsSetup(Player player)
        {
            return Get(player).isSetup;
        }

        public static bool IsTimeout(Player player)
        {
            var helper = Get(player);

            if (helper.isSetup)
                return false;

            return Get(player).timeCreated + TimeSpan.FromSeconds(Config.instance.TimeOut) > DateTime.Now;
        }

        public static Verificator Get(Player player)
        {
            return setupHelpers.Find(x => x.player == player);
        }

        public void Verify(KeyPressedEventArg ev)
        {
            EventKeyHandler.instance.UnregisterKey(setupKey, Verify);

            this.isSetup = true;
            BroadCast(Config.instance.MessageAfterSetup);

            API.Event.Events.InvokeServiceVerified(new ServiceVerifiedEventArg(this.player));
        }

        private void CheckTimeOut()
        {
            if (isSetup)
                return;

            BroadCast(Config.instance.ScreenMessageWhenTimeOut);
            player.SendConsoleMessage(Config.instance.ConsoleMessageWhenTimeOut, "green");

            API.Event.Events.InvokeServiceVerificationTimeout(new ServiceVerificationTimeoutEventArg(this.player));
        }

        private void BroadCast(string message)
        {
            string str = message
                .Replace("{SetupKey}", setupKey.ToString());

            if (Config.instance.UseHintServiceMeow)
            {
                player.RueiHint(400, str, true, Config.instance.MessageTime);
            }
            else
            {
                player.RueiHint(400, str, true, Config.instance.MessageTime);
            }
        }
    }
}
