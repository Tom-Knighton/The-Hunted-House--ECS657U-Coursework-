using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UI.Models;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private OverlayUI overlay;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private CanvasSingleMessage victoryUI;
    [SerializeField] private CanvasSingleMessage defeatUI;
    [SerializeField] private RawImage fadeImage;

    // A FIFO queue of hints to display
    private HashSet<Hint> _hintQueue = new();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            overlay.gameObject.SetActive(true);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        HidePlayerUI();
        StartCoroutine(ProcessHintQueue());
    }

    private void OnDestroy()
    {
        StopCoroutine(ProcessHintQueue());
    }

    public void ShowPlayerUI()
    {
        playerUI.gameObject.SetActive(true);
    }

    public void HidePlayerUI()
    {
        playerUI.gameObject.SetActive(false);
    }

    public void UpdatePlayerHealth(float health, float maxHealth)
    {
        var healthPercentage = (health / maxHealth) * 100f;
        playerUI.UpdateHealth(healthPercentage);
    }

    public void UpdatePlayerStamina(float stamina, float maxStamina)
    {
        var staminaPercentage = (stamina / maxStamina) * 100f;
        playerUI.UpdateStamina(staminaPercentage);
    }

    public void UpdateAttackCooldownPercentage(float percentage)
    {
        playerUI.UpdateAttackCooldown(percentage);
    }

    /// <summary>
    /// Fades the screen out to black over a period of time
    /// </summary>
    public void FadeScreenOut(float fadeTime = 1f)
    {
        StartCoroutine(FadeOut());
        return;

        IEnumerator FadeOut()
        {
            var fadeImageColor = fadeImage.color;
            while (Math.Abs(fadeImageColor.a) < 1f)
            {
                fadeImageColor.a = Mathf.Lerp(fadeImageColor.a, 1f, 5f * Time.deltaTime);
                fadeImage.color = fadeImageColor;
                yield return null;
            }
            yield return null;
        }
    }
    
    /// <summary>
    /// Fades the screen in  over a period of time
    /// </summary>
    public void FadeScreenIn(float fadeTime = 1f)
    {
        StartCoroutine(FadeIn());
        return;

        IEnumerator FadeIn()
        {
            var fadeImageColor = fadeImage.color;
            while (Math.Abs(fadeImageColor.a) > 0f)
            {
                fadeImageColor.a = Mathf.Lerp(fadeImageColor.a, 0f, 5f * Time.deltaTime);
                fadeImage.color = fadeImageColor;
                yield return null;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Shows the defeat screen with a specified message
    /// </summary>
    public void ShowDefeatScreen(string message = "")
    {
        defeatUI.SetMessage(message);
        defeatUI.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Shows the victory screen with a specific message
    /// </summary>
    public void ShowVictoryScreen(string message = "")
    {
        victoryUI.SetMessage(message);
        victoryUI.gameObject.SetActive(true);
    }

    /// <summary>
    /// Displays hint text in the top left of the screen
    /// </summary>
    public void ShowHint(string message, float showFor = 5f)
    {
        var hint = new Hint
        {
            HintText = message,
            ShowForTime = showFor,
        };

        _hintQueue.Add(hint);
    }

    public void ShowOpeningScrawl()
    {
        var messages = new List<string>
        {
            "You've escaped the basement your captor left you in, but you're not safe yet...",
            "You're still trapped in the house, and your captor is still out there...",
            "You've managed to alert the local police but they'll take time to arrive...",
            "Good luck"
        };

        var messagesShown = 0;

        StartCoroutine(ShowTextAndHold());

        return;
    
        IEnumerator ShowTextAndHold()
        {
            while (messagesShown < messages.Count)
            {
                overlay.SetFullScreenMessage(messages[messagesShown]);
                yield return new WaitForSeconds(6f);
                overlay.SetFullScreenMessage(string.Empty);
                yield return new WaitForSeconds(2.5f);
                messagesShown++;
            }
            yield return null;
            FadeScreenIn(5f);
            ShowHint("Avoid the boss! They're somewhere in the house...", 6f);
            ShowPlayerUI();
        }
    }

    private IEnumerator ProcessHintQueue()
    {
        while (true)
        {
            if (_hintQueue.Any())
            {
                var hint = _hintQueue.First();
                overlay.ShowHint(hint.HintText);
                yield return new WaitForSeconds(hint.ShowForTime);
                overlay.HideHint();
                _hintQueue.Remove(hint);
            }

            yield return null;
        }
    }
}