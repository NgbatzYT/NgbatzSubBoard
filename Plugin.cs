using System.IO; // 🐀🐀🐀🐀🐀🐀🐀
using System.Net.Http;// 🐀🐀🐀🐀🐀🐀🐀
using System.Reflection;// 🐀🐀🐀🐀🐀🐀🐀
using BepInEx;// 🐀🐀🐀🐀🐀🐀🐀
using HarmonyLib;// 🐀🐀🐀🐀🐀🐀🐀
using Newtonsoft.Json.Linq;// 🐀🐀🐀🐀🐀🐀🐀
using TMPro;// 🐀🐀🐀🐀🐀🐀🐀
using UnityEngine;// 🐀🐀🐀🐀🐀🐀🐀
using BepInEx.Configuration;// 🐀🐀🐀🐀🐀🐀🐀
// 🐀🐀🐀🐀🐀🐀🐀
namespace NgbatzSubBoard// 🐀🐀🐀🐀🐀🐀🐀
{// 🐀🐀🐀🐀🐀🐀🐀
    [BepInPlugin("ngbatz.ngbatzsubboard", "NgbatzSubBoard", "1.0.0")]// 🐀🐀🐀🐀🐀🐀🐀
    public class Plugin : BaseUnityPlugin// 🐀🐀🐀🐀🐀🐀🐀
    {// 🐀🐀🐀🐀🐀🐀🐀
        private ConfigEntry<string> ChannelID;
        TextMeshPro txt;

        void Start()
        {
            ChannelID = Config.Bind("General", "ChannelID", "UCm-PH9-cRJj6PcPeqoo_Xbw", "This is the channel ID that the live sub count will use.");
            
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
            if(!txt) { Logger.LogInfo("SSSSSSSSSSSSSSSSSSSSSSSSSSSAAAADFJUHDGBVYHUJINHBGYHUNBVGhehrckysegrfkuyscegfkcgfkuyerggkgygefkcgrkyrgkarg");
                return;
            }
            UpdateSubCount();
        }

        async void UpdateSubCount()
        {
            HttpClient pp = new HttpClient();
            string ppp = await pp.GetStringAsync($"https://api.socialcounts.org/youtube-live-subscriber-count/{ChannelID.Value}");
            var ligma = JObject.Parse(ppp);

            txt.text = $"SUBSCRIBE\n\n<size=65%>SUB COUNT: {ligma["est_sub"]}\n\nVIEW COUNT: {ligma["table"]?[0]?["count"]}</size>";
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

