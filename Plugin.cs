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
    [BepInPlugin("ngbatz.ngbatzsubboard", "NgbatzSubBoard", "1.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        private ConfigEntry<string> ChannelID;
        private ConfigEntry<bool> TikTok;
        private ConfigEntry<string> TikTokHandle;

        private TextMeshPro txt;
        private Renderer profilePicRenderer;

        void Start()
        {
            ChannelID = Config.Bind("General", "ChannelID", "UCm-PH9-cRJj6PcPeqoo_Xbw", "YouTube Channel ID.");
            TikTok = Config.Bind("General", "TikTok", false, "Enable TikTok mode.");
            TikTokHandle = Config.Bind("General", "TikTokHandle", "realgorillatagvr", "TikTok handle (without @).");

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
            profilePicRenderer = r.transform.Find("Board/ProfilePic")?.GetComponent<Renderer>();

            if (!txt) 
            { 
                Logger.LogInfo("Assetbundle most likely failed to load."); 
                return; 
            }

            UpdateSubCount();
            StartCoroutine(UpdateCount());
        }

        async void UpdateSubCount()
        {
            using HttpClient http = new HttpClient();

            if (!TikTok.Value)
            {
                string json = await http.GetStringAsync($"https://api.socialcounts.org/youtube-live-subscriber-count/{ChannelID.Value}");
                JObject data = JObject.Parse(json);

                string subCount = data["est_sub"]?.ToString() ?? "???";
                string viewsCount = data["table"]?[0]?["count"]?.ToString() ?? "???";
                string channelName = data["channel"]?["title"]?.ToString() ?? ChannelID.Value;
                string pfpUrl = data["channel"]?["thumbnail"]?.ToString();

                txt.text = $"{channelName}\n\n<size=50%>SUBSCRIBERS: {subCount}\nVIEWS: {viewsCount}</size>";

                if (!string.IsNullOrEmpty(pfpUrl))
                    await SetProfilePic(pfpUrl);
            }
            else
            {
                string json = await http.GetStringAsync($"https://mixerno.space/api/tiktok-user-counter/user/{TikTokHandle.Value}");
                JObject data = JObject.Parse(json);

                string followersCount = data["counts"]?[0]?["count"]?.ToString() ?? "???";
                string username = data["userInfo"]?["uniqueId"]?.ToString() ?? TikTokHandle.Value;
                string pfpUrl = data["userInfo"]?["avatarLarger"]?.ToString();

                txt.text = $"{username}\n\n<size=50%>FOLLOWERS: {followersCount}</size>";

                if (!string.IsNullOrEmpty(pfpUrl))
                    await SetProfilePic(pfpUrl);
            }
        }

        private IEnumerator UpdateCount()
        {
            while (true)
            {
                yield return new WaitForSeconds(300);
                UpdateSubCount();
            }
        }

        async System.Threading.Tasks.Task SetProfilePic(string url)
        {
            try
            {
                using HttpClient http = new HttpClient();
                byte[] imgData = await http.GetByteArrayAsync(url);

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(imgData);

                if (profilePicRenderer)
                    profilePicRenderer.material.mainTexture = tex;
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Failed to load profile picture: {ex.Message}");
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
