using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.VersionControl;
#endif

[CreateAssetMenu(fileName = "Item", menuName = "Lmon/New Menu Item", order = 3)]
public class LmonStoreObject : ScriptableObject
{
    public string AssetName;

    public string displayText;

    public bool LmonAsset;
    public StoreCategory category;

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

        if (lmonObject.LmonAsset)
        {
            if (GUILayout.Button("Use Other Link"))
            {
                lmonObject.LmonAsset = !lmonObject.LmonAsset;
            }

            lmonObject.targetImage = EditorGUILayout.ObjectField((Object)lmonObject.targetImage, typeof(Texture), false);
            
            EditorGUILayout.HelpBox("Image Path: " + AssetDatabase.GetAssetPath(lmonObject.targetImage),MessageType.Info);
        }
        else
        {
            if (GUILayout.Button("Use LmonStore Git"))
            {
                lmonObject.LmonAsset = !lmonObject.LmonAsset;
            }

            arrayToggle = EditorGUILayout.BeginToggleGroup("Link Segments", arrayToggle);

            if (arrayToggle)
            {
                
                List<string> copyArray = new List<string>(lmonObject.linkSegments);
                if (firstRun)
                {
                    newLength = copyArray.Count;
                    firstRun = false;
                }
                newLength = EditorGUILayout.IntField("Length", newLength);
                
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
                                copyArray.RemoveAt(i-1);
                            }
                        }

                        lmonObject.linkSegments = copyArray.ToArray();
                    }

                    if (newLength == copyArray.Count)
                    {
                        for (int i = 0; i < lmonObject.linkSegments.Length; i++)
                        {
                            lmonObject.linkSegments[i] = EditorGUILayout.TextField(i+":",lmonObject.linkSegments[i]);
                        }
                    }
                } else
                {
                    newLength = 0;
                } 
            } else
            {
                firstRun = true;
            }
            EditorGUILayout.EndToggleGroup();

            if (lmonObject.linkSegments.Length > 0)
            {
                debugString = EditorGUILayout.TextField("Test Version Number", debugString);

                string outputString = "";
                for (int i = 0; i < lmonObject.linkSegments.Length; i++)
                {
                    if (i < lmonObject.linkSegments.Length-1)
                    {
                        outputString += lmonObject.linkSegments[i] + debugString;
                    } else
                    {
                        outputString += lmonObject.linkSegments[i];
                    }
                }

                EditorGUILayout.HelpBox("Compiled Link: "+outputString, MessageType.Info);
            }
        }
    }
}
#endif