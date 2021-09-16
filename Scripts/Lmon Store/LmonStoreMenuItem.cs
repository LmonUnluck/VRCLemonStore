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
public class LmonStoreMenuItem : ScriptableObject
{
    public const string version = "v1.2";
    public string AssetName;

    public string displayText;

    public DownloadType DownloadType = DownloadType.Direct;
    public StoreCategory category = StoreCategory.Misc;

    public string directLink = "";


    public string gitHubLink = "";
    public bool removeV = false;
    public string[] linkSegments = new string[0];
    
    public Object targetImage;

    public string disableScript = "";
    public bool findDisableScript = false;

    public static Color[] categoryColors = new Color[] { Color.red, Color.blue, Color.green };

    public static bool DisplayLmonAsset(LmonStoreMenuItem target, Rect r, float yOffset, ref int index, Vector2 scrollView)
    {
        bool disable = true;

        if (target.findDisableScript)
        {
            string[] splitPath = target.disableScript.Split('/');
            string[] getAssetName = splitPath[splitPath.Length - 1].Split('.');
            string[] foundAsset = AssetDatabase.FindAssets(getAssetName[0]);

            if (foundAsset.Length > 0)
            {
                for (int i = 0; i < foundAsset.Length; i++)
                {
                    if (AssetDatabase.GUIDToAssetPath(foundAsset[i]) == target.disableScript)
                    {
                        disable = false;
                        break;
                    }
                }
            }

            EditorGUI.BeginDisabledGroup(disable);
        }

        Rect buttonDisplay = new Rect(40, (30 * index) + yOffset, r.width - 60, 25);

        if (target.targetImage != null)
        {
            if (target.DownloadType == DownloadType.Lmon)
            {
                Texture targetTexture = (Texture)target.targetImage;

                GUI.DrawTexture(new Rect(10, ((30 * index)) + yOffset, 25, 25), targetTexture);
            }
        }
        if (target.DownloadType == DownloadType.GitHub)
        {
            Texture targetTexture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Lmon Store/Images/GitHub-Mark-Light-64px.png", typeof(Texture));
            if (!EditorGUIUtility.isProSkin)
            {
                targetTexture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Lmon Store/Images/GitHub-Mark-64px.png", typeof(Texture));
            }

            GUI.DrawTexture(new Rect(10, ((30 * index)) + yOffset, 25, 25), targetTexture);
        }

        Color defaultColor = GUI.backgroundColor;
        GUI.backgroundColor = defaultColor * (categoryColors[(int)target.category] * 0.35f);
        if (GUI.Button(buttonDisplay, "Download " + target.displayText))
        {
            return true;

        }
        GUI.backgroundColor = defaultColor;
        index++;

        if (target.findDisableScript)
        {
            EditorGUI.EndDisabledGroup();

            if (disable)
            {
                EditorGUI.HelpBox(new Rect(40, ((30 * index)) + yOffset, r.width - 60, 25), "Install: " + target.disableScript, MessageType.Error);
            }
            else
            {
                EditorGUI.HelpBox(new Rect(40, ((30 * index)) + yOffset, r.width - 60, 25), target.disableScript + " Installed", MessageType.Info);
            }
            index++;
        }
        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LmonStoreMenuItem))]
public class LmonStoreObjectEditor : Editor
{
    bool arrayToggle = false;
    int newLength;
    bool firstRun = true;
    string debugString = "v1.0";

    public override void OnInspectorGUI()
    {
        LmonStoreMenuItem lmonObject = (LmonStoreMenuItem)target;

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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Remove 'v'");
            lmonObject.removeV = EditorGUILayout.Toggle(lmonObject.removeV);
            EditorGUILayout.EndHorizontal();
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
                        if (lmonObject.removeV)
                        {
                            outputString += lmonObject.linkSegments[i] + debugString.Replace("v","");
                        } else
                        {
                            outputString += lmonObject.linkSegments[i] + debugString;
                        }
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