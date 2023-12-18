using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI interactPromptText;

    // A FIFO queue of hints to display
    private HashSet<Hint> _hintQueue = new();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
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

    public void UpdateCrosshairSize(float size) 
    { 
        playerUI.UpdateCrosshair(size);
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
            fadeImage.enabled = true;
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
            while (Math.Abs(fadeImageColor.a) > 0.01f)
            {
                fadeImageColor.a = Mathf.Lerp(fadeImageColor.a, 0f, 5f * Time.deltaTime);
                fadeImage.color = fadeImageColor;
                yield return null;
            }

            fadeImage.enabled = false;
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
        overlay.SetOverlayVisibility(false);
    }
    
    /// <summary>
    /// Shows the victory screen with a specific message
    /// </summary>
    public void ShowVictoryScreen(string message = "")
    {
        victoryUI.SetMessage(message);
        victoryUI.gameObject.SetActive(true);
        overlay.SetOverlayVisibility(false);
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

    /// <summary>
    /// Shows a series of text message as an intro to the game, then fades in UI
    /// </summary>
    public void ShowOpeningScrawl()
    {
        FadeScreenOut(0f);
        var messages = new List<string>
        {
            "I wake up, dazed and confused...",
            "I remember being kidnapped, and now I'm locked in a cell...",
            "I need to find a way out of here...",
            "There's a phone just outside the cell, if only I could get to it..."
        };
        
        var messagesShown = 0;
        
        StartCoroutine(ShowTextAndHold());
        
        return;
        
        IEnumerator ShowTextAndHold()
        {
            overlay.SetOverlayVisibility(true);
            while (messagesShown < messages.Count)
            {
                overlay.SetFullScreenMessage(messages[messagesShown]);
                yield return new WaitForSeconds(5f);
                overlay.SetFullScreenMessage(string.Empty);
                yield return new WaitForSeconds(1.5f);
                messagesShown++;
            }
            yield return null;
            FadeScreenIn(5f);
            ShowHint("Press 'E' to interact with yellow highlighted objects", 5f);
            GameManager.Instance.EnablePlayers();
            ShowPlayerUI();
        }
    }
    
    /// <summary>
    /// Shows a series of text message as an intro to the game, then fades in UI
    /// </summary>
    public void ShowPhoneCutscene()
    {
        FadeScreenOut(0f);
        GameManager.Instance.DisablePlayer();
        HidePlayerUI();
        var messages = new List<string>
        {
            "I've managed to call the police...",
            "They traced the call but can't get here for a while...",
            "I need to find a way out of this basement, but I can't be seen...",
        };
        
        var messagesShown = 0;
        
        StartCoroutine(ShowTextAndHold());
        
        return;
        
        IEnumerator ShowTextAndHold()
        {
            overlay.SetOverlayVisibility(true);
            while (messagesShown < messages.Count)
            {
                overlay.SetFullScreenMessage(messages[messagesShown]);
                yield return new WaitForSeconds(5f);
                overlay.SetFullScreenMessage(string.Empty);
                yield return new WaitForSeconds(1.5f);
                messagesShown++;
            }
            yield return null;
            FadeScreenIn(5f);
            GameManager.Instance.EnablePlayers();
            GameManager.Instance.EnableEnemies();
            ShowPlayerUI();
        }
    }
    
    public void UpdateTimeLeft(TimeSpan timeLeft)
    {
        overlay.UpdateTimeLeft(timeLeft);
    }
    
    public void SetCountdownVisibility(bool isVisible)
    {
        overlay.SetCountdownVisibility(isVisible);
    }

    

    public void ShowInteractPrompt(string interactKey)
    {
        interactPromptText.text = $"Press [{interactKey}] to interact";
        interactPromptText.gameObject.SetActive(true);
    }

    public void HideInteractPrompt()
    {
        interactPromptText.gameObject.SetActive(false);
    }

    // Processes and displays hints from the queue
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