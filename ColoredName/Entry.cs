using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Spectrum.API.Configuration;
using System.IO;
using Harmony;
using System.Reflection;
using Events.Car;
using Events.LocalClient;
using Events;
using Events.Stunt;
using Events.Local;
using Events.ClientToAllClients;

namespace CustomDeathMessages
{
    public class Configs
    {
        public bool colorizeName;
        public bool colorizeMessage;

        public Configs()
        {
            var settings = new Settings("ColoredName");
            
            var entries = new Dictionary<string, bool>
            {
                {"ColorizeName", true },
                {"ColorizeMessage", false }
            };

            foreach (var s in entries)
                if (!settings.ContainsKey(s.Key))
                    settings.Add(s.Key, s.Value);

            settings.Save();

            colorizeName = (bool)settings["ColorizeName"];
            colorizeMessage = (bool)settings["ColorizeMessage"];
        }
    }

    public class Entry : IPlugin
    {
        static System.Random rnd = new System.Random();
        static Configs configs;

        public void Initialize(IManager manager, string ipcIdentifier)
        {
            configs = new Configs();
            var harmony = HarmonyInstance.Create("com.Larnin.ColoredName");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        class FormatFlags
        {
            public FormatFlags(System.Random r)
            {
                bold = r.Next(4) < 1;
                sup = r.Next(8) < 1;
                sub = r.Next(8) < 1;
                underlined = r.Next(4) < 1;
                italic = r.Next(4) < 1;
                strike = r.Next(4) < 1;
                if (!sup && !sub)
                    supSub = r.Next(4) < 1;
            }

            public bool bold;
            public bool sup;
            public bool sub;
            public bool underlined;
            public bool italic;
            public bool strike;
            public bool supSub;
        }

        [HarmonyPatch(typeof(ClientPlayerInfo), "GetChatName")]
        internal class ClientPlayerInfoGetChatName
        {
            static bool Prefix(ClientPlayerInfo __instance, ref string result, string originalColorHex)
            {
                var name = Entry.colorizeText(__instance.Username_);

                result =  string.Concat(new string[]
                {
                    name,
                    "[",
                    originalColorHex,
                    "]"
                });

                return false;
            }
        }

        [HarmonyPatch(typeof(ClientLogic), "OnEventSubmitChatMessage")]
        internal class ClientLogicOnEventSubmitChatMessage
        {
            static bool Prefix(ClientLogic __instance, ChatSubmitMessage.Data data)
            {
                var chatName = __instance.GetType().GetField("GetClientChatName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as string;
                if (configs.colorizeName)
                    chatName = colorizeText(chatName);

                if(data.message_.StartsWith("!") || data.message_.StartsWith("%") || !configs.colorizeMessage)
                    Message.SendMessage(chatName + ": " + data.message_);
                else Message.SendMessage(chatName + ": " + colorizeText(data.message_));

                return false;
            }
        }

        public static string colorizeText(string name)
        {
            FormatFlags flags = new FormatFlags(rnd);

            float size = 0.06f * (float)Math.Sqrt(name.Length);

            float t1 = (float)rnd.NextDouble();
            float t2 = t1 + rnd.Next(2) >= 1 ? size : -size;
            string newName = "";
            if (flags.bold) newName += "[b]";
            if (flags.sup) newName += "[sup]";
            if (flags.sub) newName += "[sub]";
            if (flags.italic) newName += "[i]";
            if (flags.strike) newName += "[s]";
            if (flags.underlined) newName += "[u]";
            for (int i = 0; i < name.Length; i++)
            {
                float value = ((t2 - t1) / name.Length) * i + t1;
                while (value < 0) value++;
                while (value > 1) value--;
                if (flags.supSub) newName += i % 2 == 0 ? "[sub]" : "[sup]";
                newName += "[" + ColorEx.ColorToHexNGUI(new ColorHSB(value, 1.0f, 1.0f, 1f).ToColor()) + "]" + name[i] + "[-]";
                if (flags.supSub) newName += i % 2 == 0 ? "[/sub]" : "[/sup]";
            }
            if (flags.underlined) newName += "[/u]";
            if (flags.strike) newName += "[/s]";
            if (flags.italic) newName += "[/i]";
            if (flags.sub) newName += "[/sub]";
            if (flags.sup) newName += "[/sup]";
            if (flags.bold) newName += "[/b]";

            return newName;
        }
    }
}
