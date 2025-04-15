using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;

public class PPTProjector : MonoBehaviour
{
    [Tooltip("Material used for displaying the PPT")]
    public Material projectionMaterial;

    [Tooltip("Target object to project the PPT (e.g., whiteboard or screen)")]
    public Renderer targetRenderer;

    [Tooltip("Texture property name in the material (usually _MainTex)")]
    public string texturePropertyName = "_MainTex";

    [Tooltip("Folder containing PPT images (relative to StreamingAssets)")]
    public string pptFolderPath = "PPTSlides";

    private List<Texture2D> pptSlides = new List<Texture2D>();
    private int currentSlideIndex = 0;

    void Start()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError("Please assign the target Renderer (whiteboard or screen)");
                return;
            }
        }

        if (projectionMaterial == null)
        {
            projectionMaterial = new Material(targetRenderer.material);
            targetRenderer.material = projectionMaterial;
        }
        else
        {
            targetRenderer.material = projectionMaterial;
        }

        StartCoroutine(LoadPPTSlides());
    }

    IEnumerator LoadPPTSlides()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, pptFolderPath);
        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning("PPT folder does not exist: " + fullPath);
            Directory.CreateDirectory(fullPath);
            yield break;
        }

        string[] files = Directory.GetFiles(fullPath, "*.jpg");
        if (files.Length == 0)
            files = Directory.GetFiles(fullPath, "*.png");

        System.Array.Sort(files);

        foreach (string file in files)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + file);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                pptSlides.Add(texture);

                if (pptSlides.Count == 1)
                    ShowSlide(0);
            }
            else
            {
                Debug.LogError("Failed to load slide: " + file + " - " + www.error);
            }
        }

        Debug.Log("Loaded " + pptSlides.Count + " PPT slides.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.PageDown))
            NextSlide();
        else if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.PageUp))
            PreviousSlide();
    }

    public void NextSlide()
    {
        if (pptSlides.Count == 0) return;
        currentSlideIndex = (currentSlideIndex + 1) % pptSlides.Count;
        ShowSlide(currentSlideIndex);
    }

    public void PreviousSlide()
    {
        if (pptSlides.Count == 0) return;
        currentSlideIndex = (currentSlideIndex - 1 + pptSlides.Count) % pptSlides.Count;
        ShowSlide(currentSlideIndex);
    }

    private void ShowSlide(int index)
    {
        if (pptSlides.Count == 0 || index < 0 || index >= pptSlides.Count)
        {
            Debug.LogWarning("Invalid slide index: " + index);
            return;
        }

        projectionMaterial.SetTexture(texturePropertyName, pptSlides[index]);
        Debug.Log("Displaying slide " + (index + 1) + "/" + pptSlides.Count);
    }

    public void ShowSlideByIndex(int index)
    {
        if (index >= 0 && index < pptSlides.Count)
        {
            currentSlideIndex = index;
            ShowSlide(currentSlideIndex);
        }
    }

    public bool ImportPPTFromPath(string folderPath)
    {
        pptSlides.Clear();
        currentSlideIndex = 0;
        pptFolderPath = folderPath;
        StartCoroutine(LoadPPTSlides());
        return true;
    }
}