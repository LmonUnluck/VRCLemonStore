using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
public class AvatarThumbnailCreator : EditorWindow
{

    public Object thumbnail;

    string[] placementType = {"Stretch", "Fit", "Centered" };
    int selectedType = 0;

	[MenuItem("Lmon/Avatars/Avatar Thumbnail")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<AvatarThumbnailCreator>("Avatar Thumbnail");
        window.maxSize = new Vector2(400,395);
        window.minSize = window.maxSize;
    }

    private void OnGUI()
    {
        Color defaultColor = GUI.backgroundColor;
        Rect r = new Rect(10, 10, position.width - 20, 380);
        Rect displayBox = new Rect(r.x - 5, r.y - 5, r.width + 10, r.height + 10);
        GUI.backgroundColor = defaultColor * 0.75f;
        GUI.Box(displayBox, "");
        GUI.backgroundColor = defaultColor;
        GUILayout.BeginArea(r);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Thumbnail");
        thumbnail = EditorGUILayout.ObjectField(thumbnail, typeof(Sprite), false);
        EditorGUILayout.EndHorizontal();

        selectedType = EditorGUILayout.Popup("Placement Type", selectedType, placementType);

        if (thumbnail != null)
        {
            Sprite s = thumbnail as Sprite;

            Texture t = s.texture;
            float scale = s.rect.width / s.rect.height;

            if (selectedType == 0)
            {
                EditorGUI.DrawPreviewTexture(new Rect(10, 80, 300, 300), t);
            } else if (selectedType == 1) {
                float xScale = 1;
                float yScale = 1;
                float endSize = 1;
                if (s.rect.width < s.rect.height)
                {
                    xScale = s.rect.width / s.rect.height;
                }
                else
                {
                    yScale = s.rect.height / s.rect.width;
                }
                EditorGUI.DrawPreviewTexture(new Rect(10 + ((300 - (300 * xScale)) / 2), 80 + ((300 - (300 * yScale)) / 2), 300 * xScale, 300 * yScale), t);

                EditorGUI.DrawRect(new Rect(10 + ((300 - (300 * endSize)) / 2), 80, 300 * endSize, 5), Color.red);
                EditorGUI.DrawRect(new Rect(10 + ((300 - (300 * endSize)) / 2), 80, 5, (300 * endSize)), Color.red);
                EditorGUI.DrawRect(new Rect(5 + ((300 + (300 * endSize)) / 2), 80, 5, (300 * endSize)), Color.red);
                EditorGUI.DrawRect(new Rect(10 + ((300 - (300 * endSize)) / 2), 80 - 5 + (300 * endSize), 300 * endSize, 5), Color.red);
            } else if (selectedType == 2)
            {
                float xScale = 1;
                float yScale = 1;
                float endSize = 1;
                if (s.rect.width < s.rect.height)
                {
                    xScale = s.rect.width / s.rect.height;
                    endSize = xScale;
                    EditorGUI.DrawPreviewTexture(new Rect(10, 80, 300 * xScale, 300 * yScale), t);
                    EditorGUI.DrawRect(new Rect(10 , 80 + ((300 - (300 * endSize)) / 2), 300 * endSize, 5), Color.red);
                    EditorGUI.DrawRect(new Rect(10 , 80 + ((300 - (300 * endSize)) / 2), 5, (300 * endSize)), Color.red);
                    EditorGUI.DrawRect(new Rect(5 + (((300 * endSize))), 80 + ((300 - (300 * endSize)) / 2), 5, (300 * endSize)), Color.red);
                    EditorGUI.DrawRect(new Rect(10 , 80 - 5 + (((300*yScale)/2) + ((300 * endSize)/2)), 300 * endSize, 5), Color.red);
                }
                else
                {
                    yScale = s.rect.height / s.rect.width;
                    endSize = yScale;
                    EditorGUI.DrawPreviewTexture(new Rect(10, 80, 300 * xScale, 300 * yScale), t);
                    EditorGUI.DrawRect(new Rect(10 + ((300 - (300 * endSize)) / 2), 80, 300 * endSize, 5), Color.red);
                    EditorGUI.DrawRect(new Rect(10 + ((300 - (300 * endSize)) / 2), 80, 5, (300 * endSize)), Color.red);
                    EditorGUI.DrawRect(new Rect(5 + ((300 + (300 * endSize)) / 2), 80, 5, (300 * endSize)), Color.red);
                    EditorGUI.DrawRect(new Rect(10 + ((300 - (300 * endSize)) / 2), 80 - 5 + (300 * endSize), 300 * endSize, 5), Color.red);
                }
            }

            if (Application.isPlaying)
            {
                if (GameObject.Find("VRCCam") != null)
                {
                    if (GUILayout.Button("Apply Thumbnail"))
                    {
                        SetThumbnail();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Cant find VRCam", MessageType.Error, true);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Begin upload to set the thumbnail", MessageType.Warning, true);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Make sure the Texture Type of the image is set to Sprite", MessageType.Info, true);
        }


        GUILayout.EndArea();
    }

    public GameObject _image;

    public void SetThumbnail()
    {
        if (_image == null)
        {
            Camera targetCamera = GameObject.Find("VRCCam").GetComponent<Camera>();

            GameObject go = new GameObject("ThumbnailCanvas");
            go.layer = 5;
            Canvas uiCanvase = go.AddComponent<Canvas>();

            uiCanvase.renderMode = RenderMode.ScreenSpaceCamera;
            uiCanvase.worldCamera = targetCamera;
            targetCamera.cullingMask = (1 << LayerMask.NameToLayer("UI"));
            targetCamera.orthographic = true;

            _image = new GameObject("Thumbnail");
            _image.layer = 5;
            Image targetImage = _image.AddComponent<Image>();
            targetImage.transform.parent = uiCanvase.transform;
            RectTransform rt = targetImage.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.sizeDelta = Vector2.zero;
            rt.localPosition = Vector3.zero;




            rt.localRotation = Quaternion.identity;

            targetImage.sprite = thumbnail as Sprite;

            Vector3 sizeScale = Vector3.one;

            if (selectedType == 1)
            {
                float xScale = 1;
                float yScale = 1;
                if (targetImage.sprite.rect.width < targetImage.sprite.rect.height)
                {
                    xScale = targetImage.sprite.rect.width / targetImage.sprite.rect.height;
                }
                else
                {
                    yScale = targetImage.sprite.rect.height / targetImage.sprite.rect.width;
                }

                sizeScale = new Vector3(xScale, yScale, 1);
            }
            else if (selectedType == 2)
            {
                float xScale = 1;
                float yScale = 1;
                if (targetImage.sprite.rect.width < targetImage.sprite.rect.height)
                {
                    yScale = targetImage.sprite.rect.width / targetImage.sprite.rect.height;
                }
                else
                {
                    xScale = targetImage.sprite.rect.height / targetImage.sprite.rect.width;
                }

                sizeScale = new Vector3(1 / xScale, 1 / yScale, 1);
            }

            rt.localScale = sizeScale;
        } else
        {
            Image targetImage = _image.GetComponent<Image>();
            RectTransform rt = targetImage.GetComponent<RectTransform>();
            targetImage.sprite = thumbnail as Sprite;

            Vector3 sizeScale = Vector3.one;

            if (selectedType == 1)
            {
                float xScale = 1;
                float yScale = 1;
                if (targetImage.sprite.rect.width < targetImage.sprite.rect.height)
                {
                    xScale = targetImage.sprite.rect.width / targetImage.sprite.rect.height;
                }
                else
                {
                    yScale = targetImage.sprite.rect.height / targetImage.sprite.rect.width;
                }

                sizeScale = new Vector3(xScale, yScale, 1);
            }
            else if (selectedType == 2)
            {
                float xScale = 1;
                float yScale = 1;
                if (targetImage.sprite.rect.width < targetImage.sprite.rect.height)
                {
                    yScale = targetImage.sprite.rect.width / targetImage.sprite.rect.height;
                }
                else
                {
                    xScale = targetImage.sprite.rect.height / targetImage.sprite.rect.width;
                }

                sizeScale = new Vector3(1 / xScale, 1 / yScale, 1);
            }

            rt.localScale = sizeScale;
        }
    }
}
#endif
