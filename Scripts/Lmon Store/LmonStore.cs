using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using System.Net;
using System;
public class LmonStore : EditorWindow
{

    [MenuItem("Lmon/Store")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<LmonStore>("Lmon Store");
        window.maxSize = new Vector2(400, 395);
        window.minSize = window.maxSize;
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
        } else
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
        EditorGUI.BeginDisabledGroup(downloading);
        
        sdkType = EditorGUILayout.Popup("SDK Type",sdkType,allSdk);
        float yOffset = 0;
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
                DownloadPackage(string.Format("https://github.com/MerlinVR/UdonSharp/releases/download/{0}/UdonSharp_{0}.unitypackage", GetLatestGitVersion(udonSharpSdK)), "UdonSharp.unitypackage");
            }
            EditorGUI.EndDisabledGroup();
            yOffset = 20;
        }

        if (GUILayout.Button("Download SDK"))
        {
            if (downloading == false)
            {
                if (sdkType == 0)
                {
                    downloading = true;
                    sdk = true;
                    DownloadPackage(sdk2, "SDK_2.unitypackage");
                }
                else if (sdkType == 1)
                {
                    downloading = true;
                    sdk = true;
                    DownloadPackage(sdk3Avatar, "SDK3_Avatar.unitypackage");
                }
                else if (sdkType == 2)
                {
                    downloading = true;
                    sdk = true;
                    DownloadPackage(sdk3World, "SDK3_World.unitypackage");
                }
            }
        }
        Texture materialTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Lmon Store/Images/Material Transfer.png");

        GUI.DrawTexture(new Rect(10,50 + yOffset,25,25),materialTexture);
        if (GUI.Button(new Rect(40,50 + yOffset, r.width-40,25),"Download Material Transfer"))
        {
            downloading = true;
            DownloadPackage("MaterialTransfer.unitypackage");
        }

        EditorGUI.BeginDisabledGroup(true);
        Texture prefabTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Lmon Store/Images/Prefab Transfer.png");

        GUI.DrawTexture(new Rect(10, 80 + yOffset, 25, 25), prefabTexture);
        if (GUI.Button(new Rect(40, 80 + yOffset, r.width - 40, 25),"Download Prefab Transfer"))
        {
            
        }
        EditorGUI.EndDisabledGroup();

        Texture avatarThumbnailTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Lmon Store/Images/Thumbnail Creator.png");

        GUI.DrawTexture(new Rect(10, 110 + yOffset, 25, 25), avatarThumbnailTexture);
        if (GUI.Button(new Rect(40, 110 + yOffset, r.width - 40, 25), "Download Avatar Thumbnail Creator"))
        {
            downloading = true;
            DownloadPackage("AvatarThumbnailCreator.unitypackage");
        }
        
        Texture dynmaicBoneTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Lmon Store/Images/Dynamic Bone Fixer.png");

        GUI.DrawTexture(new Rect(10, 140 + yOffset, 25, 25), dynmaicBoneTexture);
        if (GUI.Button(new Rect(40, 140 + yOffset, r.width - 40, 25), "Download Dynamic Bone Fixer"))
        {
            downloading = true;
            DownloadPackage("DynamicBoneFixer.unitypackage");
        }

        EditorGUI.EndDisabledGroup();
        GUILayout.EndArea();
    }

    void DownloadPackage(string fileName)
    {
        import = true;
        webRequest = new UnityWebRequest("http://45.76.121.115/PublicAssets/" + fileName, UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath + "/Avatars", fileName);
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
        string path = Path.Combine(Application.persistentDataPath, outputName);
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
}
#endif