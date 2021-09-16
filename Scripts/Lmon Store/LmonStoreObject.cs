using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.VersionControl;
#endif

public enum DownloadType
{
    Direct, GitHub, Lmon
}

[CreateAssetMenu(fileName = "Item", menuName = "Lmon/New Menu Item", order = 3)]
public class LmonStoreObject : ScriptableObject
{
    public string AssetName;

    public string displayText;

    public DownloadType DownloadType = DownloadType.Direct;
    public StoreCategory category = StoreCategory.Misc;

    public string directLink = "";


    public string gitHubLink = "";
    public string[] linkSegments = new string[0];
    
    public Object targetImage;

    public string disableScript = "";
    public bool findDisableScript = false;
}

#if UNITY_EDITOR
[CustomEditor(typeof(LmonStoreObject))]
public class LmonStoreObjectEditor : Editor
{
    bool arrayToggle = false;
    int newLength;
    bool firstRun = true;
    string debugString = "v1.0";

    public override void OnInspectorGUI()
    {
        LmonStoreObject lmonObject = (LmonStoreObject)target;

        lmonObject.AssetName = EditorGUILayout.TextField("Asset Name", lmonObject.AssetName);
        lmonObject.displayText = EditorGUILayout.TextField("Display Text", lmonObject.displayText);
        lmonObject.findDisableScript = EditorGUILayout.Toggle("Find Disable Script", lmonObject.findDisableScript);

        if (lmonObject.findDisableScript)
        {
            lmonObject.disableScript = EditorGUILayout.TextField("Disable Script", lmonObject.disableScript);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Menu Category");
        lmonObject.category = (StoreCategory)EditorGUILayout.EnumPopup(lmonObject.category);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Download Type");
        lmonObject.DownloadType = (DownloadType)EditorGUILayout.EnumPopup(lmonObject.DownloadType);
        EditorGUILayout.EndHorizontal();

        if (lmonObject.DownloadType == DownloadType.Lmon)
        {
            lmonObject.targetImage = EditorGUILayout.ObjectField((Object)lmonObject.targetImage, typeof(Texture), false);

            EditorGUILayout.HelpBox("Image Path: " + AssetDatabase.GetAssetPath(lmonObject.targetImage), MessageType.Info);
        }
        else if (lmonObject.DownloadType == DownloadType.GitHub)
        {
            lmonObject.gitHubLink = EditorGUILayout.TextField("Github link (lastest)", lmonObject.gitHubLink);
            arrayToggle = EditorGUILayout.BeginToggleGroup("Link Segments", arrayToggle);



            List<string> copyArray = new List<string>(lmonObject.linkSegments);
            if (firstRun)
            {
                newLength = copyArray.Count;
                firstRun = false;
            }
            newLength = EditorGUILayout.IntField("Length", newLength);


            if (arrayToggle)
            {
                if (newLength > 0)
                {
                    if (newLength != copyArray.Count && newLength >= 0)
                    {
                        if (newLength > copyArray.Count)
                        {
                            for (int i = 0; i < newLength - copyArray.Count; i++)
                            {
                                copyArray.Add("");
                            }
                        }
                        else if (newLength < copyArray.Count)
                        {
                            for (int i = copyArray.Count; i > newLength; i--)
                            {
                                copyArray.RemoveAt(i - 1);
                            }
                        }

                        lmonObject.linkSegments = copyArray.ToArray();
                    }
                }

                if (newLength == copyArray.Count && arrayToggle)
                {
                    for (int i = 0; i < lmonObject.linkSegments.Length; i++)
                    {
                        lmonObject.linkSegments[i] = EditorGUILayout.TextField(i + ":", lmonObject.linkSegments[i]);
                    }
                }
            }
            else
            {
                firstRun = true;
                newLength = copyArray.Count;
            }





            EditorGUILayout.EndToggleGroup();

            if (lmonObject.linkSegments.Length > 0)
            {
                if (lmonObject.gitHubLink == "")
                {
                    EditorGUILayout.HelpBox("No Link Provided", MessageType.Error);
                }
                debugString = EditorGUILayout.TextField("Test Version Number", debugString);

                string outputString = lmonObject.gitHubLink.Replace("latest", "download/" + debugString + "/");
                for (int i = 0; i < lmonObject.linkSegments.Length; i++)
                {
                    if (i < lmonObject.linkSegments.Length - 1)
                    {
                        outputString += lmonObject.linkSegments[i] + debugString;
                    }
                    else
                    {
                        outputString += lmonObject.linkSegments[i];
                    }
                }

                EditorGUILayout.HelpBox("Compiled Link: " + outputString, MessageType.Info);
            }
        }
        else if (lmonObject.DownloadType == DownloadType.Direct)
        {
            lmonObject.directLink = EditorGUILayout.TextField("Direct link", lmonObject.directLink);
        }
    }
}
#endif