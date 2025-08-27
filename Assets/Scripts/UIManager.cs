using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class UIManager : MonoBehaviour
{
    #region Variables
    [Header("Pages")]
    public float waitTime;
    public GameObject ingamePage;
    public GameObject gameoverPage;

    [Header("Sound Toggle")]
    public bool isSoundOn;
    public Sprite soundOnImage;
    public Sprite soundOffImage;
    public Button soundToggleButton;

    [Header("Undo")]
    public Image radialUndoBar;
    public GameObject watchAdButton;

    [Header("Ads")]
    public string gameId;
    public bool testMode;

    [Header("High Score")]
    public Text scoreTextInGame;
    public Text scoreTextGameOver;
    public Text highScoreTextInGame;
    public Text highScoreTextGameOver;

    public Animator anim;

    public static UIManager instance;
    #endregion

    void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        isSoundOn = PlayerPrefs.GetInt("SOUND", 1) == 1 ? true : false;
        SetSound();

        Advertisement.Initialize(gameId, testMode);
        Advertisement.Load("Interstitial_Android");
    }
    void Update()
    {
        scoreTextInGame.text = GameManager.instance.score.ToString();
        scoreTextGameOver.text = GameManager.instance.score.ToString();

        highScoreTextInGame.text = GameManager.instance.highScore.ToString();
        highScoreTextGameOver.text = GameManager.instance.highScore.ToString();

        if (GridManager.instance.undoCount == 0)
        {
            watchAdButton.SetActive(true);
        }
        else
        {
            watchAdButton.SetActive(false);
        }

        radialUndoBar.fillAmount = (float)GridManager.instance.undoCount / (float)GridManager.instance.maxUndoCount;
    }

    public void GoToPage(string pageName)
    {
        switch (pageName)
        {
            case "ingame":
                ingamePage.SetActive(true);
                gameoverPage.SetActive(false);
                break;
            case "gameover":
                StartCoroutine(GameOverPage());
                break;
        }
    }
    IEnumerator GameOverPage()
    {
        yield return new WaitForSeconds(waitTime);
        ingamePage.SetActive(false);
        gameoverPage.SetActive(true);
    }

    void SetSound()
    {        
        AudioListener.volume = isSoundOn ? 1 : 0;
        soundToggleButton.image.sprite = isSoundOn ? soundOnImage : soundOffImage;
    }
    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("SOUND", isSoundOn ? 1 : 0);
        SetSound();
    }

    public void WatchAdButton()
    {
        Advertisement.Show("Interstitial_Android");
        GridManager.instance.undoCount = 3;
    }
}