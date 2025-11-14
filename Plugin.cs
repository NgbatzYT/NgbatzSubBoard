using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace NgbatzSubBoard
{
    [BepInPlugin("ngbatz.ngbatzsubboard", "NgbatzSubBoard", "1.1.2")]
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
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NgbatzSubBoard.sub");
            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            stream.Close();

            var r = Instantiate(bundle.LoadAsset<GameObject>("NgSubBoard"));

            r.transform.position = new Vector3(-62.733f, 12.5845f, -83.5582f);
            r.transform.rotation = Quaternion.Euler(0f, 3f, 0f);

            txt = r.transform.Find("Board/SubText").GetComponent<TextMeshPro>();

            InvokeRepeating(nameof(UpdateSubCount), 0, 300);
        }

        async void UpdateSubCount()
        {
            if (!TikTok.Value)
            {
                HttpClient pp = new HttpClient();
                string ppp = await pp.GetStringAsync($"https://api.socialcounts.org/youtube-live-subscriber-count/{ChannelID.Value}");
                var ligma = JObject.Parse(ppp);

                string subCount = ligma["counters"]?["estimation"]?["subscriberCount"].ToString() ?? "Failed to get";
                string viewsCount = ligma["counters"]?["estimation"]?["viewCount"]?.ToString() ?? "Failed to get";

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
    }
}


