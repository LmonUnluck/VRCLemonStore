using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public class DynamicBoneSetter : EditorWindow
{

    Transform currentAvatar;

    [MenuItem("Lmon/Dynamic Bone Fixer")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<DynamicBoneSetter>("Dynamic Bone Fixer");
        window.maxSize = new Vector2(400, 50);
        window.minSize = window.maxSize;
    }

    private void OnGUI()
    {
        Color defaultColor = GUI.backgroundColor;
        Rect r = new Rect(10, 10, position.width - 20, 40);
        Rect displayBox = new Rect(r.x - 5, r.y - 5, r.width + 10, r.height + 10);
        GUI.backgroundColor = defaultColor * 0.75f;
        GUI.Box(displayBox,"");
        GUI.backgroundColor = defaultColor;
        GUILayout.BeginArea(r);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target Avatar");
        currentAvatar = (Transform)EditorGUILayout.ObjectField(currentAvatar, typeof(Transform), true);
        EditorGUILayout.EndHorizontal();
        if (currentAvatar != null)
        {
            if (GUILayout.Button("Fixed Copyed Dynamic Bones"))
            {
                FixDynamicBones(currentAvatar);
            }
        }
        GUILayout.EndArea();

        
    }

    public void FixDynamicBones(Transform targetAvatar)
    {
        DynamicBone[] allBones = targetAvatar.GetComponentsInChildren<DynamicBone>(true);
        List<DynamicBoneColliderBase> allColliders = new List<DynamicBoneColliderBase>(targetAvatar.GetComponentsInChildren<DynamicBoneColliderBase>(true));
        Transform[] allTransforms = targetAvatar.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < allBones.Length; i++)
        {
            allBones[i].m_Root = allBones[i].transform;
            List<DynamicBoneColliderBase> oldColliders = new List<DynamicBoneColliderBase>(allBones[i].m_Colliders);
            List<DynamicBoneColliderBase> copyColliders = new List<DynamicBoneColliderBase>();
            foreach (DynamicBoneColliderBase oldCollider in oldColliders)
            {
                foreach (DynamicBoneColliderBase newCollider in allColliders)
                {
                    if (oldCollider.name == newCollider.name)
                    {
                        copyColliders.Add(newCollider);
                        break;
                    }
                }
            }
            allBones[i].m_Colliders = copyColliders;

            List<Transform> exclusions = new List<Transform>();

            foreach (Transform tran in allBones[i].m_Exclusions)
            {
                for (int x = 0; x < allTransforms.Length; x++)
                {
                    if (tran.name == allTransforms[x].name)
                    {
                        exclusions.Add(allTransforms[x]);
                        break;
                    }
                }
            }

            allBones[i].m_Exclusions = exclusions;
            
        }
    }
}
#endif