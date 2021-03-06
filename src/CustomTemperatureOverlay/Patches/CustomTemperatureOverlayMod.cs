﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;
using Harmony;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomTemperatureOverlay.Patches
{
    public static class CustomTemperatureOverlayMod
    {
        public const string ModName = "CustomTemperatureOverlay";
        public const string configFileName = "Config.json";

        [HarmonyPatch(typeof(SimDebugView))]
        [HarmonyPatch("OnPrefabInit")]
        public static class SimDebugView_OnPrefabInit
        {
            public static void Postfix()
            {
                Common.ModState.Initialize(ModName, null);
                ConfigHelper<SimDebugView.ColorThreshold[]>.Watch(configFileName, UpdateThresholds);
                if (ConfigHelper<SimDebugView.ColorThreshold[]>.TryLoad(configFileName, out var newThresholds))
                {
                    UpdateThresholds(newThresholds);
                }
                else
                {
                    UpdateThresholds(State.DefaultThresholds);
                }
            }

            private static void UpdateThresholds(SimDebugView.ColorThreshold[] newThresholds)
            {
                int newThresholdsLength = newThresholds.Length;
                int requiredThresholdsLength = SimDebugView.Instance.temperatureThresholds.Length;
                object[] logObject = new object[requiredThresholdsLength];
                newThresholds = newThresholds.OrderBy(t => t.value).ToArray();

                for (int i = 0; i < requiredThresholdsLength; i++)
                {
                    SimDebugView.Instance.temperatureThresholds[i] = i < newThresholdsLength
                        ? newThresholds[i]
                        : newThresholds[newThresholdsLength - 1];

                    var threshold = SimDebugView.Instance.temperatureThresholds[i];

                    logObject[i] = new
                    {
                        color = new
                        {
                            threshold.color.r,
                            threshold.color.g,
                            threshold.color.b,
                            threshold.color.a
                        },
                        threshold.value
                    };
                }

                Common.Logger.Log("Config loaded: " + Environment.NewLine + JsonConvert.SerializeObject(logObject));
            }
        }
    }
}
