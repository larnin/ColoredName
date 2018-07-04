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

namespace CustomDeathMessages
{
    public class Entry : IPlugin
    {
        public string IPCIdentifier { get { return "ColoredName"; } set { } }

        public void Initialize(IManager manager)
        {
            var harmony = HarmonyInstance.Create("com.Larnin.ColoredName");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        
    }
}
