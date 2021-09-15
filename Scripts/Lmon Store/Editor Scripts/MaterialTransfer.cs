using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public class MaterialTransfer : EditorWindow
{
    Transform currentAvatar;
    Transform targetAvatar;

    Vector2 scrollView;
    List<SkinnedMeshRenderer> originalMeshes;
    List<SkinnedMeshRenderer> targetMeshes;
    List<bool[]> meshesTransfers;
    bool gotSkinMeshes = false;

    static float windowSize = 400;

    [MenuItem("Lmon/Avatars/Material Transfer")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<MaterialTransfer>("Material Transfer");
        window.maxSize = new Vector2(400, windowSize);
        window.minSize = window.maxSize;
    }

    private void OnGUI()
    {
        Color defaultColor = GUI.backgroundColor;
        Rect r = new Rect(10, 10, position.width - 20, windowSize - 10);
        Rect displayBox = new Rect(r.x - 5, r.y - 5, r.width + 10, r.height + 10);
        GUI.backgroundColor = defaultColor * 0.75f;
        GUI.Box(displayBox, "");
        GUI.backgroundColor = defaultColor;
        GUILayout.BeginArea(r);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Original Avatar");
        currentAvatar = (Transform)EditorGUILayout.ObjectField(currentAvatar, typeof(Transform), true);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target Avatar");
        targetAvatar = (Transform)EditorGUILayout.ObjectField(targetAvatar, typeof(Transform), true);
        GUILayout.EndHorizontal();

        if (currentAvatar != null)
        {
            if (targetAvatar != null)
            {
                if (GUILayout.Button("Quick Copy Materials"))
                {
                    TransferMaterials();
                }
                if (GUILayout.Button("Get Skin Meshes"))
                {
                    BetterTransferMaterials();
                }

                if (gotSkinMeshes)
                {
                    GUI.backgroundColor = defaultColor * 0.5f;
                    Rect scrollRect = new Rect(0, 80, position.width - 20, 268);
                    GUI.Box(scrollRect, "");
                    GUI.backgroundColor = defaultColor * 0.85f;
                    scrollView = EditorGUILayout.BeginScrollView(scrollView);
                    for (int i = 0; i < originalMeshes.Count; i++)
                    {
                        EditorGUILayout.Space();
                        if (i != 0)
                        {
                            EditorGUILayout.LabelField("----------------------------------------------------------------------");
                            EditorGUILayout.Space();
                        }
                        EditorGUILayout.ObjectField(originalMeshes[i], typeof(SkinnedMeshRenderer), false);
                        for (int x = 0; x < originalMeshes[i].sharedMaterials.Length; x++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            meshesTransfers[i][x] = EditorGUILayout.BeginToggleGroup("Element " + x,meshesTransfers[i][x]);
                            EditorGUILayout.ObjectField(originalMeshes[i].sharedMaterials[x], typeof(Material), false);
                            EditorGUILayout.EndToggleGroup();
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space();
                        GUI.backgroundColor = defaultColor * 0.95f;
                        if (GUILayout.Button("Apply Materials"))
                        {
                            Material[] allMaterials = new Material[targetMeshes[i].sharedMaterials.Length];
                            for (int x = 0; x < allMaterials.Length; x++)
                            {
                                if (meshesTransfers[i][x])
                                {
                                    allMaterials[x] = originalMeshes[i].sharedMaterials[x];
                                } else
                                {
                                    allMaterials[x] = targetMeshes[i].sharedMaterials[x];
                                }
                            }
                            targetMeshes[i].sharedMaterials = allMaterials;
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.HelpBox("Orignal avatar copy's its materials to the target avatar", MessageType.Info);
            } else
            {
                gotSkinMeshes = false;
            }
        } else
        {
            gotSkinMeshes = false;
        }
        GUILayout.EndArea();
    }

    void TransferMaterials()
    {
        SkinnedMeshRenderer[] allSkinMeshes;
        allSkinMeshes = currentAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        SkinnedMeshRenderer[] otherAllSkinMeshes;
        otherAllSkinMeshes = targetAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        MeshRenderer[] allNormalMeshes;
        allNormalMeshes = currentAvatar.GetComponentsInChildren<MeshRenderer>(true);

        MeshRenderer[] otherNormalMeshes;
        otherNormalMeshes = currentAvatar.GetComponentsInChildren<MeshRenderer>(true);

        for (int i = 0; i < allSkinMeshes.Length; i++)
        {
            for (int x = 0; x < otherAllSkinMeshes.Length; x++)
            {
                if (allSkinMeshes[i].transform.name == otherAllSkinMeshes[x].transform.name)
                {
                    otherAllSkinMeshes[x].sharedMaterials = allSkinMeshes[i].sharedMaterials;
                }
            }
        }

        for (int i = 0; i < allNormalMeshes.Length; i++)
        {
            for (int x = 0; x < otherNormalMeshes.Length; x++)
            {
                if (allNormalMeshes[i].transform.name == otherNormalMeshes[x].transform.name)
                {
                    otherNormalMeshes[x].sharedMaterials = allNormalMeshes[i].sharedMaterials;
                }
            }
        }
    }

    

    void BetterTransferMaterials()
    {
        meshesTransfers = new List<bool[]>();
        SkinnedMeshRenderer[] allSkinMeshes;
        allSkinMeshes = currentAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        SkinnedMeshRenderer[] otherAllSkinMeshes;
        otherAllSkinMeshes = targetAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        originalMeshes = new List<SkinnedMeshRenderer>();
        targetMeshes = new List<SkinnedMeshRenderer>();

        for (int i = 0; i < allSkinMeshes.Length; i++)
        {
            for (int x = 0; x < otherAllSkinMeshes.Length; x++)
            {
                if (allSkinMeshes[i].transform.name == otherAllSkinMeshes[x].transform.name)
                {
                    if (!originalMeshes.Contains(allSkinMeshes[i]))
                    {
                        originalMeshes.Add(allSkinMeshes[i]);
                    }
                    if (!targetMeshes.Contains(otherAllSkinMeshes[x]))
                    {
                        targetMeshes.Add(otherAllSkinMeshes[x]);
                    }
                    bool[] newArray = new bool[otherAllSkinMeshes[x].sharedMaterials.Length];
                    for (int y = 0; y < newArray.Length; y++)
                    {
                        newArray[y] = true;
                    }
                    meshesTransfers.Add(newArray);
                }
            }
        }

        gotSkinMeshes = true;
    }
}
#endif
