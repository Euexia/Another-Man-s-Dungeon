using UnityEngine;

public class OpenGC : MonoBehaviour
{
    // Cette méthode sera appelée lorsque le bouton est cliqué
    public void OpenGooglePage()
    {
        // Ouvrir la page Google
        Application.OpenURL("https://gamingcampus.fr");
    }
}
