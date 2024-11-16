using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/*
Classe que lida com o menu e controla os elementos de UI
*/
public class MainMenu : MonoBehaviour
{
    private CompositeButton startButton, restartButton, tutorialButton, quitButton;
    [SerializeField] private GameObject tutorialWindow;
    [SerializeField] private CanvasGroup pauseImage;
    private CanvasGroup canvasGroup;
    private bool isOpen = true;


    [SerializeField] private CanvasGroup rocketUI, applicationTitle;
    [SerializeField] private Image blackScreen;
    private Coroutine pauseCoroutine;

    void Start()
    {
        Time.timeScale = 0;

        canvasGroup = GetComponent<CanvasGroup>();
        startButton = transform.GetChild(0).GetComponent<CompositeButton>();
        restartButton = transform.GetChild(1).GetComponent<CompositeButton>();
        tutorialButton = transform.GetChild(2).GetComponent<CompositeButton>();
        quitButton = transform.GetChild(3).GetComponent<CompositeButton>();

        tutorialButton.onClick.AddListener(()=>tutorialWindow.SetActive(true));
        quitButton.onClick.AddListener(()=>Application.Quit());
        startButton.onClick.AddListener(()=>StartGame());
        restartButton.onClick.AddListener(()=>Restart());

        GameState.onFail.AddListener(() => 
        {
            pauseImage.GetComponentInChildren<TextMeshProUGUI>().text = GameState.failReason;
            ToggleMenu(true);
        });
    }

    private void Restart() => StartCoroutine(RestartCoroutine());

    private IEnumerator RestartCoroutine()
    {
        yield return BlackenScreen(1f);
        yield return new WaitForSecondsRealtime(1f);
        GameState.failed = false;
        GameState.gameStarted = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator BlackenScreen(float duration)
    {
        blackScreen.gameObject.SetActive(true);
        float timer = 0;
        while(timer < duration)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;
            blackScreen.color = new Color(0,0,0, Mathf.Lerp(0,1, timer/duration));
        }
    }

    private void StartGame()
    {
        if(GameState.gameStarted) return;

        GameState.gameStarted = true;
        StartCoroutine(StartGameCoroutine(1f));
        ToggleMenu(false);
        VirtualCameraControl.FocusOnCamera(1);
    }

    private IEnumerator StartGameCoroutine(float duration)
    {
        pauseImage.gameObject.SetActive(false);
        float timer = 0;
        while(timer < duration)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            rocketUI.alpha = Mathf.Lerp(0, 1, timer/duration);
            applicationTitle.alpha = Mathf.Lerp(1, 0, timer/duration);
        }
        rocketUI.blocksRaycasts = true;

        startButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(true);
        pauseImage.gameObject.SetActive(true);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !GameState.failed && GameState.gameStarted) ToggleMenu(!isOpen);
    }

    private void ToggleMenu(bool state)
    {
        if(isOpen == state) return;

        isOpen = state;
        Time.timeScale = isOpen ? 0 : 1;
        if (!isOpen) tutorialWindow.SetActive(false);
        if(pauseCoroutine != null) StopCoroutine(pauseCoroutine);
        pauseCoroutine = StartCoroutine(ToggleCoroutine(0.5f, isOpen));
    }

    private IEnumerator ToggleCoroutine(float duration, bool open)
    {
        float alphaFrom = canvasGroup.alpha;
        float alphaTo = open ? 1 : 0;

        if(!open) canvasGroup.blocksRaycasts = false;

        float timer = 0;
        while(timer < duration)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            canvasGroup.alpha = Mathf.Lerp(alphaFrom, alphaTo, timer/duration);
            pauseImage.alpha = Mathf.Lerp(alphaFrom, alphaTo, timer/duration);
        }

        if(open) canvasGroup.blocksRaycasts = true;
    }
}
