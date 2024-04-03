using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ContentBoinging
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static AudioClip _boing;
        private static ManualLogSource _logger;
        
        private void Awake()
        {
            // Plugin startup logic
            _logger = Logger;
            Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ContentBoinging.boing");
            var bundle = AssetBundle.LoadFromStream(stream);
            _boing = bundle.LoadAsset<AudioClip>("boing");
            Log(_boing);
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
            Log(harmony.GetPatchedMethods().Count());
        }

        internal static void Log(object msg) => _logger.LogInfo(msg);

        internal class Patches
        {
            [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.RPCA_Jump))]
            [HarmonyPostfix]
            public static void BoingJump(Player ___player)
            {
                BoingPlayer(___player);
            }

            [HarmonyPatch(typeof(Player), nameof(Player.RPCA_TakeDamageAndAddForce))]
            [HarmonyPostfix]
            public static void BoingTakeDamageAndAddForce(Player __instance)
            {
                BoingPlayer(__instance);
            }

            private static void BoingPlayer(Player player)
            {
                SFX_Instance sfx = Instantiate(player.sfx_0_Impact);
                sfx.clips = new[] { _boing };
                SFX_Player.instance.PlaySFX(sfx, player.refs.headPos.position);
            }
        }
    }
}
