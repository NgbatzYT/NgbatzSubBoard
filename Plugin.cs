using System.IO;
using System.Net.Http;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections;

namespace NgbatzSubBoard
{
    [BepInPlugin("ngbatz.ngbatzsubboard", "NgbatzSubBoard", "1.1.1")]
    public class Plugin : BaseUnityPlugin
    {
        private ConfigEntry<string> ChannelID;
        private ConfigEntry<bool> TikTok;
        private ConfigEntry<string> TikTokHandle;
        TextMeshPro txt;

        void Start()
        {
            ChannelID = Config.Bind("General", "ChannelID", "UCm-PH9-cRJj6PcPeqoo_Xbw", "This is the channel ID that the live sub count will use.");
            TikTok = Config.Bind("General", "TikTok", false, "This is if you want to have the TikTok mode.");
            TikTokHandle = Config.Bind("General", "TikTokHandle", "realgorillatagvr", "This is your TikTok handle without the @");
            
            var harmony = Harmony.CreateAndPatchAll(GetType().Assembly, "ngbatz.ngbatzsubboard");
            GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        }

        void OnGameInitialized()
        {
            var e = LoadAssetBundle("NgbatzSubBoard.sub");
            
            var r = Instantiate(e.LoadAsset<GameObject>("NgSubBoard"));
            
            r.transform.position = new Vector3(-62.733f, 12.5845f, -83.5582f);
            r.transform.rotation = Quaternion.Euler(0f, 3f, 0f);
            
            txt = r.transform.Find("Board/SubText").GetComponent<TextMeshPro>();

            if(!txt) { Logger.LogInfo("Assetbundle most likely failed to load."); return; }
            UpdateSubCount();
            StartCoroutine(UpdateCount());
        }

        async void UpdateSubCount()
        {
            if (!TikTok.Value)
            {
                HttpClient pp = new HttpClient();
                string ppp = await pp.GetStringAsync($"https://api.socialcounts.org/youtube-live-subscriber-count/{ChannelID.Value}");
                var ligma = JObject.Parse(ppp);

                string subCount = ligma["est_sub"].ToString() ?? "Failed to get";
                string viewsCount = ligma["table"]?[0]?["count"]?.ToString() ?? "Failed to get";

                txt.text = $"SUBSCRIBE\n\n<size=50%>SUBSCRIBERS: {subCount}\n\nVIEWS: {viewsCount}</size>";
            }
            else
            {
                using HttpClient pp = new HttpClient();
                string ppp = await pp.GetStringAsync($"https://mixerno.space/api/tiktok-user-counter/user/{TikTokHandle.Value}");

                JObject ligma = JObject.Parse(ppp);
                string followersCount = ligma["counts"]?[0]?["count"]?.ToString() ?? "Failed to get";

                txt.text = $"FOLLOW\n\n<size=50%>FOLLOWERS: {followersCount}";
            }
        }

        private IEnumerator UpdateCount()
        {
            while (true) { 
                yield return new WaitForSeconds(300);
            
                UpdateSubCount();
            }
        }

        public AssetBundle LoadAssetBundle(string path)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            stream.Close();
            return bundle;
        }

    }
}
