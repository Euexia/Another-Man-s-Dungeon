using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Steamworks;
using UnityEngine.UI;

public class MenuSteamController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI PlayerNameText;

    [SerializeField]
    private RawImage PlayerIcon;

    private bool AvatarReceived;

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        AvatarReceived = true;
        return texture;
    }

    void GetPlayerIcon()
    {
        int ImageID = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());
        if (ImageID == -1) { return; }
        Rect uvRect = PlayerIcon.uvRect;
        uvRect.height = -1;
        PlayerIcon.uvRect = uvRect;
        PlayerIcon.texture = GetSteamImageAsTexture(ImageID);
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerNameText.text = SteamFriends.GetPersonaName();
        GetPlayerIcon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
