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

public class LmonStore : EditorWindow
{
    const string storeVersion = "v1.2";
    static string[] scriptVersions;
    //static List<LmonStoreObject> menuItems = new List<LmonStoreObject>();
    static Dictionary<StoreCategory, List<LmonStoreObject>> menuItems = new Dictionary<StoreCategory, List<LmonStoreObject>>();
    

    [MenuItem("Lmon/Store")]
    public static void ShowWindow()
    {
        menuItems.Clear();
        for (int i = 0; i < Enum.GetNames(typeof(StoreCategory)).Length; i++)
        {
            menuItems.Add((StoreCategory)i, new List<LmonStoreObject>());
        }
        EditorWindow window = GetWindow<LmonStore>("Lmon Store");
        window.maxSize = new Vector2(400, 395);
        window.minSize = window.maxSize;

        scriptVersions = GetLemonVersions();

        SearchItems();
    }

    static int totalItems = 0;

    static void SearchItems()
    {
        totalItems = 0;
        string[] scrObject = Directory.GetFiles(Application.dataPath + "/Scripts/Lmon Store/Menu Items", "*.asset");
        for (int i = 0; i < scrObject.Length; i++)
        {
            try
            {
                string newStr = scrObject[i].Replace(Application.dataPath, "Assets").Replace('\\','/');
                LmonStoreObject newObj = (LmonStoreObject)AssetDatabase.LoadAssetAtPath(newStr, typeof(LmonStoreObject));
                menuItems[newObj.category].Add(newObj);
                totalItems++;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load: " + (scrObject[i]) + "\n"+e.Message);
            }

        }
    }

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

    private void OnGUI()
    {
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
                        AssetDatabase.ImportPackage(loadPath, true);
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
            Rect scrollRect = new Rect(displayBox.x, displayBox.y + yOffset, displayBox.width-15, displayBox.height - yOffset);
            
            viewPoint = GUI.BeginScrollView(scrollRect, viewPoint, new Rect(0, 0, scrollRect.width - 100, (30 * totalItems)), false, true,null, new GUIStyle(GUI.skin.verticalScrollbar));
            for (int i = 0; i < Enum.GetNames(typeof(StoreCategory)).Length; i++)
            {
                List<LmonStoreObject> foundList = menuItems[(StoreCategory)i];
                for (int x = 0; x < foundList.Count; x++)
                {
                    if (DisplayLmonAsset(foundList[x], new Rect(0,0,scrollRect.width,scrollRect.height), 0, ref heightIndex,viewPoint))
                    {
                        if (foundList[x].DownloadType == DownloadType.Lmon)
                        {
                            downloading = true;
                            DownloadPackage(foundList[x].AssetName);
                        } else if(foundList[x].DownloadType == DownloadType.GitHub)
                        {
                            downloading = true;
                            string versionNumber = GetLatestGitVersion(foundList[x].gitHubLink);
                            string outputString = "";
                            for (int n = 0; n < foundList[x].linkSegments.Length; n++)
                            {
                                if (i < foundList[x].linkSegments.Length - 1)
                                {
                                    outputString += foundList[x].linkSegments[n] + versionNumber;
                                }
                                else
                                {
                                    outputString += foundList[x].linkSegments[n];
                                }
                            }
                            string downloadString = string.Format("{0}/{1}/{2}", foundList[x].gitHubLink.Replace("latest", "download"), versionNumber,outputString);
                            downloading = true;
                            DownloadPackage(downloadString, foundList[x].AssetName);
                        } else if (foundList[x].DownloadType == DownloadType.Direct)
                        {
                            downloading = true;
                            DownloadPackage(foundList[x].directLink, foundList[x].AssetName);
                        }
                    }
                }
            }
        } else
        {
            List<LmonStoreObject> foundList = menuItems[(StoreCategory)storeSelection-1];
            viewPoint = GUI.BeginScrollView(displayBox, viewPoint, new Rect(0, 0, displayBox.x, (30 * foundList.Count)),false,true);
            for (int x = 0; x < foundList.Count; x++)
            {
                if (DisplayLmonAsset(foundList[x], r, yOffset, ref heightIndex,viewPoint))
                {
                    downloading = true;
                    DownloadPackage(foundList[x].AssetName);
                }
            }
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
        webRequest.downloadHandler = new DownloadHandlerFile(path);
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
        webRequest.downloadHandler = new DownloadHandlerFile(path);
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

    static bool DisplayLmonAsset(LmonStoreObject target, Rect r, float yOffset, ref int index, Vector2 scrollView)
    {
        bool disable = true;

        if (target.findDisableScript)
        {
            string[] dynBone = AssetDatabase.FindAssets(target.disableScript);

            if (dynBone.Length > 0)
            {
                for (int i = 0; i < dynBone.Length; i++)
                {
                    if (AssetDatabase.GUIDToAssetPath(dynBone[i]) == "Assets/DynamicBone/Scripts/" + target.disableScript + ".cs")
                    {
                        disable = false;
                        break;
                    }
                }
            }

            EditorGUI.BeginDisabledGroup(disable);
        }

        Texture targetTexture = (Texture)target.targetImage;

        GUI.DrawTexture(new Rect(10, ((30 * index)) + yOffset , 25, 25), targetTexture);
        if (GUI.Button(new Rect(40, (30 * index) + yOffset , r.width - 60, 25), "Download "+target.displayText))
        {
            return true;

        }
        index++;

        if (target.findDisableScript)
        {
            EditorGUI.EndDisabledGroup();

            if (disable)
            {
                EditorGUI.HelpBox(new Rect(40, ((30 * index)) + yOffset , r.width - 60, 25), "Install: " + target.disableScript, MessageType.Error);
            }
            else
            {
                EditorGUI.HelpBox(new Rect(40, ((30 * index)) + yOffset , r.width - 60, 25), target.disableScript + " Installed", MessageType.Info);
            }
            index++;
        }
        return false;
    }
}
#endif