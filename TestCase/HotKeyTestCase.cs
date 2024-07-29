﻿using Exiled.API.Features;
using KeyBindingServiceMeow.API.Event.EventArgs;
using KeyBindingServiceMeow.API.Features.HotKey;
using KeyBindingServiceMeow.KeyApplications.HotKeys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KeyBindingServiceMeow.TestCase
{
    internal static class HotKeyTestCase
    {
        public static void OnEnabled()
        {
            API.Event.Events.KeyServiceReady += RegisterKey;
        }

        public static void OnDisabled()
        {
            API.Event.Events.KeyServiceReady -= RegisterKey;
        }

        public static void RegisterKey(KeyServiceReadyEventArg ev)
        {
            List<HotKey> hotKeys = new List<HotKey>()
            {
                new HotKey(KeyCode.F1, "TestCase-PrintLog", "Print", "Print a log on the console"),
                new HotKey(KeyCode.F2, "TestCase-KillPlayer", "Suicide", "Kill the player who pressed this hotkey", "Test Case 1"),
                new HotKey(KeyCode.F2, "TestCase-RevivePlayer", "Revive", "Revive the player who pressed this hotkey", "Test Case 1"),
                new HotKey(KeyCode.F3, "TestCase-SendMessage", "SendMessage", "Let server send a message to you", "Test Case 2")
            };

            hotKeys[0].KeyPressed += PrintLog;
            hotKeys[1].KeyPressed += KillPlayer;
            hotKeys[2].KeyPressed += RevivePlayer;
            hotKeys[3].KeyPressed += SendMessage;

            foreach (var key in hotKeys)
            {
                HotKeyManager.Get(ev.Player).RegisterKey(key);
            }
            
        }

        public static void PrintLog(HotKeyPressedEventArg ev)
        {
            Log.Info("HotKeyTestCase: " + ev.hotkey.description);
        }

        public static void KillPlayer(HotKeyPressedEventArg ev)
        {
            var player = Player.Get(ev.player.Nickname); // Ensure you retrieve a correct instance of 'Player'

            if (player != null && player.IsAlive)
                player.Kill(DamageTypes.None);
        }

        public static void RevivePlayer(HotKeyPressedEventArg ev)
        {
            var player = Player.Get(ev.player.Nickname);

            if (player != null && !player.IsAlive)
                player.SetRole(RoleType.NtfScientist); // Use SetRole instead of nonexistent Set method on RoleType class.
        }

        public static void SendMessage(HotKeyPressedEventArg ev)
        {
            ev.player.SendConsoleMessage("This is a test message", "#7CB342");
        }
    }
}
