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
public class LmonStore : EditorWindow
{
    const string storeVersion = "v1.0";
    static string[] scriptVersions;

    [MenuItem("Lmon/Store")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<LmonStore>("Lmon Store");
        window.maxSize = new Vector2(400, 395);
        window.minSize = window.maxSize;

        scriptVersions = GetLemonVersions();
    }



    string file;

    int sdkType = 0;
    string[] allSdk = { "SDK2", "SDK3 Avatar", "SDK3 World" };
    bool downloading = false;
    bool import = false;
    UnityWebRequest webRequest;
    string loadPath = "";
    bool sdk = false;


    const string sdk3World = "https://vrchat.com/download/sdk3-worlds";
    const string sdk3Avatar = "https://vrchat.com/download/sdk3-avatars";
    const string sdk2 = "https://vrchat.com/download/sdk2";
    const string udonSharpSdK = "https://github.com/MerlinVR/UdonSharp/releases/latest";

    private void OnGUI()
    {
        Rect r = new Rect(10, 10, position.width - 20, 380);
        if (downloading)
        {
            if (webRequest.isDone)
            {
                downloading = false;
            }
            r.y += 100;
            EditorGUILayout.HelpBox("Downloading", MessageType.Warning);
        }
        else
        {
            if (import)
            {
                if (webRequest != null)
                {
                    if (webRequest.isDone)
                    {
                        AssetDatabase.ImportPackage(loadPath, sdk);
                        import = false;
                        sdk = false;
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
            yOffset += 50;
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
                sdk = true;
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
                    sdk = true;
                    DownloadPackage(sdk2, "SDK_2");
                }
                else if (sdkType == 1)
                {
                    downloading = true;
                    sdk = true;
                    DownloadPackage(sdk3Avatar, "SDK3_Avatar");
                }
                else if (sdkType == 2)
                {
                    downloading = true;
                    sdk = true;
                    DownloadPackage(sdk3World, "SDK3_World");
                }
            }
        }
        Texture materialTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Lmon Store/Images/Material Transfer.png");

        int heightIndex = 0;

        GUI.DrawTexture(new Rect(10, (50 + (30 * heightIndex)) + yOffset, 25, 25), materialTexture);
        if (GUI.Button(new Rect(40, 50 + (30 * heightIndex) + yOffset, r.width - 40, 25), "Download Material Transfer"))
        {
            downloading = true;
            DownloadPackage("MaterialTransfer");

        }
        heightIndex++;

        //EditorGUI.BeginDisabledGroup(true);
        //Texture prefabTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Lmon Store/Images/Prefab Transfer.png");
        //
        //GUI.DrawTexture(new Rect(10, (50 + (30 * heightIndex)) + yOffset, 25, 25), prefabTexture);
        //if (GUI.Button(new Rect(40, (50 + (30 * heightIndex)) + yOffset, r.width - 40, 25),"Download Prefab Transfer"))
        //{
        //    
        //}
        //EditorGUI.EndDisabledGroup();
        //
        //heightIndex++;

        Texture avatarThumbnailTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Lmon Store/Images/Thumbnail Creator.png");
        GUI.DrawTexture(new Rect(10, (50 + (30 * heightIndex)) + yOffset, 25, 25), avatarThumbnailTexture);
        if (GUI.Button(new Rect(40, (50 + (30 * heightIndex)) + yOffset, r.width - 40, 25), "Download Avatar Thumbnail Creator"))
        {
            downloading = true;
            DownloadPackage("AvatarThumbnailCreator");
        }
        heightIndex++;

        Texture dynmaicBoneTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Lmon Store/Images/Dynamic Bone Fixer.png");

        bool disableDynFix = true;

        string[] dynBone = AssetDatabase.FindAssets("DynamicBone");

        if (dynBone.Length > 0)
        {
            for (int i = 0; i < dynBone.Length; i++)
            {
                if (AssetDatabase.GUIDToAssetPath(dynBone[i]) == "Assets/DynamicBone/Scripts/DynamicBone.cs")
                {
                    disableDynFix = false;
                    break;
                }
            }
        }

        EditorGUI.BeginDisabledGroup(disableDynFix);

        GUI.DrawTexture(new Rect(10, (50 + (30 * heightIndex)) + yOffset, 25, 25), dynmaicBoneTexture);
        if (GUI.Button(new Rect(40, (50 + (30 * heightIndex)) + yOffset, r.width - 40, 25), "Download Dynamic Bone Setter"))
        {
            downloading = true;
            DownloadPackage("DynamicBoneSetter");
        }
        EditorGUI.EndDisabledGroup();
        heightIndex++;
        if (disableDynFix)
        {
            EditorGUI.HelpBox(new Rect(40, (50 + (30 * heightIndex)) + yOffset, r.width - 40, 25), "Install Dynamic Bones", MessageType.Error);
        }
        else
        {
            EditorGUI.HelpBox(new Rect(40, (50 + (30 * heightIndex)) + yOffset, r.width - 40, 25), "Dynamic Bones Installed", MessageType.Info);
        }

        EditorGUI.EndDisabledGroup();
        heightIndex++;

        GUILayout.EndArea();
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
        } else
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