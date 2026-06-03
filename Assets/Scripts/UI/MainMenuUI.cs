using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject critterParade;

    private string[] subtitles = {
        "58 Peaks. Countless Critters. One Camera.",
        "The goats are waiting.",
        "Altitude: High. Standards: Low.",
        "Gerald the Marmot sends his regards.",
        "Now with 73% more marmot.",
        "Warning: May cause spontaneous hiking.",
        "The pikas believe in you.",
        "Thin air, thick vibes.",
        "No critters were harmed. Mostly."
    };

    private void Start()
    {
        if (subtitleText != null)
            subtitleText.text = subtitles[Random.Range(0, subtitles.Length)];
    }

    public void StartGame()
    {
        SceneManager.LoadScene("PeakSelect");
    }

    public void OpenFieldGuide()
    {
        SceneManager.LoadScene("FieldGuideView");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
