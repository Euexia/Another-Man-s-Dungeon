using UnityEngine;

public class OpenGC : MonoBehaviour
{
    // Cette m�thode sera appel�e lorsque le bouton est cliqu�
    public void OpenGooglePage()
    {
        // Ouvrir la page Google
        Application.OpenURL("https://gamingcampus.fr");
    }
}
