using UnityEngine;
using UnityEngine.UI;

public class SelfieCameraSetup : MonoBehaviour
{
    public Camera selfieCamera;
    public RenderTexture selfieRenderTexture;
    public RawImage selfieImage;

    void Start()
    {
        // Configure the selfie camera
        selfieCamera.targetTexture = selfieRenderTexture;

        // Configure the Raw Image to display the selfie camera's Render Texture
        selfieImage.texture = selfieRenderTexture;

        // Set the position and size of the Raw Image
        RectTransform rt = selfieImage.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -10);  // Position in the top-left corner with some margin
        rt.sizeDelta = new Vector2(150, 150);  // Size of the selfie camera display
    }
}
