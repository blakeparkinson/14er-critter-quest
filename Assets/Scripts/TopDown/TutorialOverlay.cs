using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialOverlay : MonoBehaviour
{
    private GameObject panel;
    private TextMeshProUGUI tutText;
    private int step;
    private Transform player;
    private bool complete;
    private float stepTimer;
    private Vector3 playerStartPos;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) playerStartPos = player.position;
        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        // wait for title screen to dismiss
        yield return new WaitForSeconds(0.5f);
        while (Time.timeScale < 0.5f)
            yield return new WaitForSecondsRealtime(0.1f);

        yield return new WaitForSeconds(1f);

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) yield break;

        panel = new GameObject("Tutorial");
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.2f);
        rt.anchorMax = new Vector2(0.5f, 0.2f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 60);
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform, false);
        var trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(10, 5);
        trt.offsetMax = new Vector2(-10, -5);
        tutText = textObj.AddComponent<TextMeshProUGUI>();
        tutText.fontSize = 16;
        tutText.alignment = TextAlignmentOptions.Center;
        tutText.color = Color.white;

        // Step 1: Move
        tutText.text = "Use WASD to walk. Head up the trail!";
        yield return WaitUntilPlayerMoves(3f, 10f);

        // Step 2: Jog
        tutText.text = "Hold SHIFT to jog faster! Watch your stamina.";
        yield return new WaitForSeconds(4f);

        // Step 3: Camera
        tutText.text = "Press TAB to pull out your camera.";
        yield return WaitForKey(UnityEngine.InputSystem.Key.Tab, 15f);

        // Step 4: Photo
        tutText.text = "Click near a critter to photograph it!";
        yield return new WaitForSeconds(4f);

        // Step 5: Guide
        tutText.text = "Press G to check your Field Guide. Now go find them all!";
        yield return new WaitForSeconds(4f);

        // Step 6: Goal
        tutText.text = "Follow the arrow to the summit. Photograph every critter along the way!";
        yield return new WaitForSeconds(4f);

        // fade out
        var cg = panel.AddComponent<CanvasGroup>();
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            cg.alpha = 1f - t;
            yield return null;
        }
        Destroy(panel);
    }

    private IEnumerator WaitUntilPlayerMoves(float minDist, float timeout)
    {
        float t = 0;
        while (t < timeout)
        {
            t += Time.deltaTime;
            if (player != null && Vector3.Distance(player.position, playerStartPos) > minDist)
                yield break;
            yield return null;
        }
    }

    private IEnumerator WaitForKey(UnityEngine.InputSystem.Key key, float timeout)
    {
        float t = 0;
        while (t < timeout)
        {
            t += Time.deltaTime;
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current[key].wasPressedThisFrame)
                yield break;
            yield return null;
        }
    }
}
