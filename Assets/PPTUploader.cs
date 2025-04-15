using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using SFB; // 需要添加StandaloneFileBrowser插件

public class PPTUploader : MonoBehaviour
{
    public PPTProjector pptProjector;
    public Button uploadButton;

    void Start()
    {
        if (uploadButton != null)
        {
            uploadButton.onClick.AddListener(OpenFileBrowser);
        }
    }

    public void OpenFileBrowser()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Select folder containing PPT slide images", "", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string selectedPath = paths[0];
            Debug.Log("Selected folder: " + selectedPath);
            CopyPPTSlidesToStreamingAssets(selectedPath);
        }
    }

    private void CopyPPTSlidesToStreamingAssets(string sourcePath)
    {
        string destPath = Path.Combine(Application.streamingAssetsPath, "PPTSlides");

        if (!Directory.Exists(destPath))
            Directory.CreateDirectory(destPath);
        else
        {
            foreach (string file in Directory.GetFiles(destPath))
                File.Delete(file);
        }

        string[] imageFiles = Directory.GetFiles(sourcePath, "*.jpg");
        if (imageFiles.Length == 0)
            imageFiles = Directory.GetFiles(sourcePath, "*.png");

        foreach (string file in imageFiles)
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destPath, fileName);
            File.Copy(file, destFile);
        }

        Debug.Log("Copied " + imageFiles.Length + " files to " + destPath);

        if (pptProjector != null)
        {
            pptProjector.ImportPPTFromPath("PPTSlides");
        }
    }
}