using BepInEx;
using BepInEx.Logging;
using Reptile;
using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace EmailApi
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(CommonAPIGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class EmailApiPlugin : BaseUnityPlugin
    {
        public const string CharacterAPIGuid = "com.Viliger.CharacterAPI";
        private const string CommonAPIGUID = "CommonAPI";

        public EmailApiPlugin Instance;

        private static ManualLogSource DebugLog = BepInEx.Logging.Logger.CreateLogSource($"EmailApi Plugin");

        private void Awake()
        {
            Logger.LogMessage($"{PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} starting...");

            if (true)
            {
                Harmony harmony = new Harmony(PluginInfo.PLUGIN_NAME);
                harmony.PatchAll();
            }

            Instance = this;

            Logger.LogMessage($"Init Email Save");
            EmailManager.Initialize(this);

        }

        public void DelayEmailNotification(string messageID, bool save, float delay) 
        {
            StartCoroutine(DelayEmailRoutine(messageID, save, delay));
        }

        IEnumerator DelayEmailRoutine(string messageID, bool save,  float delay) {

            DebugLog.LogMessage("Start Email Wait");
            yield return new WaitForSeconds(delay);
            DebugLog.LogMessage("Push Email");
            EmailManager.EmailNotification(messageID, save);
        }

    }

 
}
