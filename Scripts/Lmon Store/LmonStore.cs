using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using System.Net;
using System;

public enum StoreCategory
{
    Misc, Avatar, World
}

public enum DownloadType
{
    Direct, GitHub, Lmon
}

public class LmonStore : EditorWindow
{
    const string storeVersion = "v1.2";
    static string[] scriptVersions;

#if MENUITEM
    static Dictionary<StoreCategory, List<LmonStoreMenuItem>> menuItems = new Dictionary<StoreCategory, List<LmonStoreMenuItem>>();
#endif

    static LmonStore staticWindow;

    [MenuItem("Lmon/Store")]
    public static void ShowWindow()
    {
#if MENUITEM
        menuItems.Clear();
        for (int i = 0; i < Enum.GetNames(typeof(StoreCategory)).Length; i++)
        {
            menuItems.Add((StoreCategory)i, new List<LmonStoreMenuItem>());
        }
#endif
        LmonStore window = GetWindow<LmonStore>("Lmon Store");
        window.maxSize = new Vector2(400, 395);
        window.minSize = window.maxSize;

        scriptVersions = GetLemonVersions();
#if MENUITEM
        SearchItems();
#endif

        staticWindow = window;
    }

    static int totalItems = 0;
#if MENUITEM
    static void SearchItems()
    {
        totalItems = 0;
        string[] scrObject = Directory.GetFiles(Application.dataPath + "/Scripts/Lmon Store/Menu Items", "*.asset");
        for (int i = 0; i < scrObject.Length; i++)
        {
            try
            {
                string newStr = scrObject[i].Replace(Application.dataPath, "Assets").Replace('\\','/');
                LmonStoreMenuItem newMenuItem = (LmonStoreMenuItem)AssetDatabase.LoadAssetAtPath(newStr, typeof(LmonStoreMenuItem));
                menuItems[(StoreCategory)newMenuItem.category].Add(newMenuItem);
                totalItems++;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load: " + (scrObject[i]) + "\n"+e.Message);
            }

        }
    }
#endif
    string file;

    int sdkType = 0;
    string[] allSdk = { "SDK2", "SDK3 Avatar", "SDK3 World" };
    bool downloading = false;
    bool import = false;
    UnityWebRequest webRequest;
    string loadPath = "";

    int storeSelection = 0;

    Vector2 viewPoint = Vector2.zero;


    const string sdk3World = "https://vrchat.com/download/sdk3-worlds";
    const string sdk3Avatar = "https://vrchat.com/download/sdk3-avatars";
    const string sdk2 = "https://vrchat.com/download/sdk2";
    const string udonSharpSdK = "https://github.com/MerlinVR/UdonSharp/releases/latest";

    bool forceInstall = false;



    private void OnGUI()
    {
        if (staticWindow == null)
        {
            ShowWindow();
        }
        EditorGUI.BeginDisabledGroup(downloading);
        Rect r = new Rect(10, 10, position.width - 20, 380);
        if (downloading)
        {
            EditorGUILayout.HelpBox("Downloading", MessageType.Warning);
            r.y += 100;
            if (webRequest.isDone)
            {
                downloading = false;
            }

        }
        else
        {
            if (import)
            {
                if (webRequest != null)
                {
                    if (webRequest.isDone)
                    {
                        EditorGUILayout.HelpBox("Importing", MessageType.Warning);
                        r.y += 100;
                        AssetDatabase.ImportPackage(loadPath, !forceInstall);
                        forceInstall = false;
                        import = false;
                    }
                }
            }
        }
        Color defaultColor = GUI.backgroundColor;
        Rect displayBox = new Rect(r.x - 5, r.y - 5, r.width + 10, r.height + 10);
        GUI.backgroundColor = defaultColor * 0.75f;



        GUI.Box(displayBox, "");
        GUI.backgroundColor = defaultColor;
        GUILayout.BeginArea(r);
        float yOffset = 0;
        string currentStoreVersion = ExtractVersion("LmonStore");
        if (currentStoreVersion != storeVersion)
        {
            EditorGUILayout.HelpBox(string.Format("Version Mismatch ({0}), download {1}", storeVersion, currentStoreVersion), MessageType.Warning);
            if (GUILayout.Button("Download Latest Version"))
            {
                downloading = true;
                DownloadPackage("LmonStore");
            }
            yOffset += 60;
        }
        else
        {
#if !MENUITEM
            downloading = true;
            forceInstall = true;
            DownloadPackage("LmonStoreMenuItem");
#else
            if (LmonStoreMenuItem.version != ExtractVersion("LmonStoreMenuItem"))
            {
                forceInstall = true;
                downloading = true;
                DownloadPackage("LmonStoreMenuItem");
            }
#endif
        }

        EditorGUI.BeginDisabledGroup(downloading);

        sdkType = EditorGUILayout.Popup("SDK Type", sdkType, allSdk);

        if (sdkType == 2)
        {
#if VRC_SDK_VRCSDK3
            EditorGUI.BeginDisabledGroup(false);
#else
            EditorGUI.BeginDisabledGroup(true);
#endif

            if (GUILayout.Button("Download UdonSharp"))
            {
                downloading = true;
                DownloadPackage(string.Format("https://github.com/MerlinVR/UdonSharp/releases/download/{0}/UdonSharp_{0}.unitypackage", GetLatestGitVersion(udonSharpSdK)), "UdonSharp");
            }
            EditorGUI.EndDisabledGroup();
            yOffset += 20;
        }

        if (GUILayout.Button("Download SDK"))
        {
            if (downloading == false)
            {
                if (sdkType == 0)
                {
                    downloading = true;
                    DownloadPackage(sdk2, "SDK_2");
                }
                else if (sdkType == 1)
                {
                    downloading = true;
                    DownloadPackage(sdk3Avatar, "SDK3_Avatar");
                }
                else if (sdkType == 2)
                {
                    downloading = true;
                    DownloadPackage(sdk3World, "SDK3_World");
                }
            }
        }
        yOffset += 40;

        List<string> allCategories = new List<string>();

        allCategories.Add("All");

        for (int i = 0; i < Enum.GetNames(typeof(StoreCategory)).Length; i++)
        {
            allCategories.Add(Enum.GetNames(typeof(StoreCategory))[i]);
        }

        storeSelection = EditorGUILayout.Popup(storeSelection, allCategories.ToArray());
        yOffset += 20;

        int heightIndex = 0;

        if (storeSelection == 0)
        {
            Rect scrollRect = new Rect(displayBox.x, displayBox.y + yOffset, displayBox.width - 15, displayBox.height - yOffset);

            viewPoint = GUI.BeginScrollView(scrollRect, viewPoint, new Rect(0, 0, scrollRect.width - 100, (30 * totalItems) + 50), false, true, null, new GUIStyle(GUI.skin.verticalScrollbar));
#if MENUITEM
            for (int i = 0; i < Enum.GetNames(typeof(StoreCategory)).Length; i++)
            {
#if !UDON
                if ((StoreCategory)i == StoreCategory.World)
                {
                    continue;
                }
#endif
                List<LmonStoreMenuItem> foundList = menuItems[(StoreCategory)i];
                for (int x = 0; x < foundList.Count; x++)
                {
                    if (LmonStoreMenuItemEditor.DisplayLmonAsset(foundList[x], new Rect(0, 0, scrollRect.width, scrollRect.height), 0, ref heightIndex, viewPoint))
                    {
                        if (foundList[x].DownloadType == (int)DownloadType.Lmon)
                        {
                            downloading = true;
                            DownloadPackage(foundList[x].AssetName);
                        }
                        else if (foundList[x].DownloadType == (int)DownloadType.GitHub)
                        {
                            downloading = true;
                            string versionNumber = GetLatestGitVersion(foundList[x].gitHubLink);
                            string outputString = "";
                            for (int n = 0; n < foundList[x].linkSegments.Length - 1; n++)
                            {
                                outputString += foundList[x].linkSegments[n] + versionNumber;
                            }

                            outputString += foundList[x].linkSegments[foundList[x].linkSegments.Length - 1];
                            string downloadString = string.Format("{0}/{1}/{2}", foundList[x].gitHubLink.Replace("latest", "download"), versionNumber, outputString);
                            downloading = true;
                            DownloadPackage(downloadString, foundList[x].AssetName);
                        }
                        else if (foundList[x].DownloadType == (int)DownloadType.Direct)
                        {
                            downloading = true;
                            DownloadPackage(foundList[x].directLink, foundList[x].AssetName);
                        }
                    }
                }
            }
#endif
        }
        else
        {
#if MENUITEM
            bool display = true;

            List<LmonStoreMenuItem> foundList = menuItems[(StoreCategory)storeSelection - 1];
            viewPoint = GUI.BeginScrollView(displayBox, viewPoint, new Rect(0, 0, displayBox.width, (30 * foundList.Count)), false, true);

            if ((StoreCategory)storeSelection - 1 == StoreCategory.World)
            {
#if !UDON
                display = false;
                EditorGUILayout.HelpBox("Missing Udon", MessageType.Error);
#endif
            }

            if (display)
            {
                for (int x = 0; x < foundList.Count; x++)
                {
                    if (LmonStoreMenuItemEditor.DisplayLmonAsset(foundList[x], r, yOffset, ref heightIndex, viewPoint))
                    {
                        downloading = true;
                        DownloadPackage(foundList[x].AssetName);
                    }
                }
            }
#endif
        }

        GUI.EndScrollView();

        GUILayout.EndArea();
        EditorGUI.EndDisabledGroup();
    }

    void DownloadPackage(string fileName)
    {
        import = true;

        webRequest = new UnityWebRequest(string.Format("https://github.com/LmonUnluck/VRCLemonStore/releases/download/{0}/{1}.unitypackage", ExtractVersion(fileName), fileName), UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath, fileName + ".unitypackage");
        webRequest.downloadHandler = new DownloadHandlerFile(path.Replace("\\", "/"));
        webRequest.SendWebRequest();
        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(webRequest.error);
            downloading = false;
        }
        else
        {
            loadPath = path;
            downloading = false;
        }
    }

    void DownloadPackage(string fileName, string outputName)
    {
        import = true;
        webRequest = new UnityWebRequest(fileName, UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath, outputName + ".unitypackage");
        webRequest.downloadHandler = new DownloadHandlerFile(path.Replace("\\", "/"));
        webRequest.SendWebRequest();
        while (webRequest.responseCode == -1)
        {
            //do something, or nothing while blocking
        }
        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(webRequest.error);
            downloading = false;
        }
        else
        {
            loadPath = path;
            downloading = false;
        }
    }

    string GetLatestGitVersion(string url)
    {
        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
        webRequest.AllowAutoRedirect = false;
        webRequest.Timeout = 10000;
        HttpWebResponse webResponse;
        using (webResponse = (HttpWebResponse)webRequest.GetResponse())
        {
            if ((int)webResponse.StatusCode >= 300 && (int)webResponse.StatusCode <= 399)
            {
                string uriString = webResponse.Headers["Location"];
                Uri newUri = new Uri(uriString);
                return newUri.Segments[newUri.Segments.Length - 1];
            }
        }
        return null;
    }

    string ExtractVersion(string scriptName)
    {
        if (scriptVersions != null)
        {
            for (int i = 0; i < scriptVersions.Length; i++)
            {
                string[] versionType = scriptVersions[i].Split('=');

                if (versionType[0] == scriptName)
                {
                    return versionType[1];
                }
            }
        }
        else
        {
            scriptVersions = GetLemonVersions();

            return ExtractVersion(scriptName);
        }

        return null;
    }

    static string[] GetLemonVersions()
    {
        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/LmonUnluck/VRCLemonStore/main/script_versions.txt");
        webRequest.AllowAutoRedirect = false;
        webRequest.Timeout = 10000;
        HttpWebResponse webResponse;
        string responseText = null;
        using (webResponse = (HttpWebResponse)webRequest.GetResponse())
        {
            if ((int)webResponse.StatusCode == 200)
            {
                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new System.IO.StreamReader(webResponse.GetResponseStream(), encoding))
                {
                    responseText = reader.ReadToEnd();
                }
            }
        }

        webRequest.Abort();

        if (responseText != null)
        {
            string[] lines = responseText.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            return lines;


        }

        return null;
    }
}
#endif