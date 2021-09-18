using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public class TargetDirectory
{
    public string DirectPath;
    public string FolderName;
    public TargetDirectory[] subPaths;
    public bool endPath = false;
    public bool showDirectories = false;

    public TargetDirectory(string DirectPath)
    {
        this.DirectPath = DirectPath.Replace('\\','/');
        string[] folderPath = this.DirectPath.Split('/');
        this.FolderName = folderPath[folderPath.Length-1];

        GetDirectories();
    }

    public void GetDirectories()
    {
        string[] childPaths = Directory.GetDirectories(DirectPath,"*",SearchOption.TopDirectoryOnly);

        if (childPaths.Length == 0)
        {
            endPath = true;
            return;
        }

        subPaths = new TargetDirectory[childPaths.Length];

        for (int i = 0; i < subPaths.Length; i++)
        {
            subPaths[i] = new TargetDirectory(childPaths[i]);
        }
    }

    public void Show(int tabIndex)
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fixedWidth = 150;

        EditorGUILayout.BeginHorizontal();
        if (!endPath)
        {
            GUIStyle style = new GUIStyle(EditorStyles.foldoutHeader);
            RectOffset offset = new RectOffset();
            offset.left = 10 * tabIndex;
            style.margin = offset;
            showDirectories = EditorGUILayout.Foldout(showDirectories,FolderName,false,style);

            if (GUILayout.Button("Select Directory",buttonStyle))
            {
                AssetFinder.targetDirectory = this;
            }

            EditorGUILayout.EndHorizontal();
            if (showDirectories)
            {
                if (subPaths != null)
                {
                    for (int i = 0; i < subPaths.Length; i++)
                    {
                        subPaths[i].Show(tabIndex + 1);
                    }
                }
            }
        } else
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.contentOffset = new Vector2(10 * (tabIndex+1), 0);
            EditorGUILayout.LabelField(FolderName,style);
            
            if (GUILayout.Button("Select Directory", buttonStyle))
            {
                AssetFinder.targetDirectory = this;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

public class AssetFinder : EditorWindow
{
    public static AssetFinder currentWindow;

    public static TargetDirectory assetDirectory;

    public static TargetDirectory targetDirectory;

    [MenuItem("Lmon/Misc/Asset Finder")]
    public static void ShowWindow()
    {
        GetDirectories();

        AssetFinder window = GetWindow<AssetFinder>("Asset Finder");
        window.maxSize = new Vector2(400, 395);
        window.minSize = window.maxSize;

        currentWindow = window;
    }


    string[] fileTypes = new string[]
    {
        "Materials",
        "Scripts",
        "FBX",
        "Animations",
        "Animator Controllers",
        "Prefabs"
    };

    string[] fileExtensions = new string[]
    {
        ".mat",
        ".cs",
        ".fbx",
        ".anim",
        ".controller",
        ".prefab"
    };

    bool listAllFolder = false;

    int selectedExtension = 0;

    Vector2 scrollView = Vector2.zero;

    Vector2 objScrollView = Vector2.zero;

    private void OnGUI()
    {
        Rect r = new Rect(10, 10, position.width - 20, position.height - 20);
        Rect displayBox = new Rect(r.x - 5, r.y - 5, r.width + 10, r.height + 10);
        GUI.Box(displayBox, "");
        GUILayout.BeginArea(r);

        if (assetDirectory == null)
        {
            GetDirectories();
        }

        if (!listAllFolder)
        {
            if (GUILayout.Button("Show All Folders"))
            {
                listAllFolder = true;
            }

        } else
        {
            if (GUILayout.Button("Hide All Folders"))
            {
                listAllFolder = false;
            }
        }

        if (listAllFolder)
        {
            if (GUILayout.Button("Refresh"))
            {
                GetDirectories();
            }
            if (targetDirectory == null)
            {
                EditorGUILayout.HelpBox("Please Select Directory", MessageType.Warning);
            } else
            {
                EditorGUILayout.HelpBox("Selected Directory: " + targetDirectory.FolderName,MessageType.Info);
            }
            scrollView = EditorGUILayout.BeginScrollView(scrollView,GUILayout.Height(300));
            assetDirectory.Show(0);
            EditorGUILayout.EndScrollView();
        } else
        {
            EditorGUILayout.Space(30);

            EditorGUI.BeginDisabledGroup(targetDirectory == null);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Extension");
            selectedExtension = EditorGUILayout.Popup(selectedExtension, fileTypes);
            EditorGUILayout.EndHorizontal();

            if (targetDirectory != null)
            {

                if (GUILayout.Button("Search " + targetDirectory.FolderName))
                {
                    GetFiles(targetDirectory, fileExtensions[selectedExtension]);
                }

            }
            else
            {
                GUILayout.Button("Select Directory");
            }

            EditorGUI.EndDisabledGroup();

            if (allObjects != null)
            {
                objScrollView = EditorGUILayout.BeginScrollView(objScrollView, GUILayout.Height(300));
                for (int i = 0; i < allObjects.Length; i++)
                {
                    EditorGUILayout.ObjectField(allObjects[i], typeof(Object), false);
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.EndScrollView();
            }
        }

        GUILayout.EndArea();
    }

    static void GetDirectories()
    {
        assetDirectory = new TargetDirectory(Application.dataPath.Replace('\\', '/'));
    }

    public Object[] allObjects;

    void GetFiles(TargetDirectory target,string fileExtension)
    {
        string[] allFiles = Directory.GetFiles(target.DirectPath, "*" + fileExtension, SearchOption.AllDirectories);

        allObjects = new Object[allFiles.Length];

        for (int i = 0; i < allFiles.Length; i++)
        {
            try
            {
                allObjects[i] = AssetDatabase.LoadAssetAtPath<Object>(allFiles[i].Replace(Application.dataPath, "Assets"));
            }
            catch
            {
                
            }
        }
    }
}
#endif