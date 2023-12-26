using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    public string folderPath = "Assets/Screenshots";
    public string fileNamePrefix = "screenshot";

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
        Camera screenshotCamera = GetComponent<Camera>(); // Get the Camera component once
        if (screenshotCamera == null)
        {
            Debug.LogError("Camera component not found on the object.");
            return;
        }

        RenderTexture rt = new RenderTexture(1028, 1028, 24);
        screenshotCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1028, 1028, TextureFormat.ARGB32, false);
        screenshotCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1028, 1028), 0, 0);
        screenShot.Apply();

        // Reset the target texture and active render texture
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null; // JC: Added to fix compile error

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

        // Refresh the AssetDatabase if we're in the Unity Editor
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
