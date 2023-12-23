using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    public string folderPath = "Assets/Screenshots";
    public string fileNamePrefix = "screenshot";
    private Camera camera;

    void Start()
    {
        // Ensure the folder exists
        System.IO.Directory.CreateDirectory(folderPath);
    }

    void Update()
    {
        // For example, take a screenshot when the user presses the 'S' key
        if (Input.GetKeyDown(KeyCode.S))
        {
            string fullPath = System.IO.Path.Combine(folderPath, fileNamePrefix + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
            TakeScreenshot(fullPath);
        }
    }

    void TakeScreenshot(string fullPath)
    {
        if (camera == null)
        {
            camera = GetComponent<Camera>();
        }

        RenderTexture rt = new RenderTexture(1028, 1028, 48);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1028, 1028, TextureFormat.ARGB32, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1028, 1028), 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;

        if (Application.isEditor)
        {
            DestroyImmediate(rt);
        }
        else
        {
            Destroy(rt);
        }

        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, bytes);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
