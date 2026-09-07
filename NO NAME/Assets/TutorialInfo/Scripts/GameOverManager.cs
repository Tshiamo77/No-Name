using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image bloodImage; // Drag your BloodDripImage here
    [SerializeField] private float dripDuration = 2.0f;
    [SerializeField] private string mainMenuSceneName = "MAIN_MENU";

    private static GameOverManager instance;
    public static GameOverManager Instance => instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (bloodImage != null)
        {
            bloodImage.gameObject.SetActive(false);
        }
    }

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (bloodImage != null)
        {
            bloodImage.gameObject.SetActive(true);
        }

        Color startColor = bloodImage != null ? bloodImage.color : Color.red;
        startColor.a = 1f; // Keep fully opaque so it looks like dripping blood
        if (bloodImage != null) bloodImage.color = startColor;

        RectTransform rectTrans = bloodImage != null ? bloodImage.GetComponent<RectTransform>() : null;
        float elapsed = 0f;

        while (elapsed < dripDuration)
        {
            float t = elapsed / dripDuration;

            if (rectTrans != null)
            {
                // Drip downward by stretching the height from 0 to full screen height
                Vector2 size = rectTrans.sizeDelta;
                size.y = Mathf.Lerp(0f, Screen.height, t);
                rectTrans.sizeDelta = size;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it fully covers the screen at the end
        if (rectTrans != null)
        {
            Vector2 size = rectTrans.sizeDelta;
            size.y = Screen.height;
            rectTrans.sizeDelta = size;
        }

        yield return new WaitForSeconds(1.0f);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}