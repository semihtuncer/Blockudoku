using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables
    [Header("Scores")]
    public bool isHighscore;
    public int score;
    public int highScore;

    public int scoreIncreaseOnRows;
    public int scoreIncreaseOnSquare;
    public int scoreIncreaseOnPlace;

    [Space(5)]
    public Transform particlesPoint;
    public GameObject highScoreParticles;

    [Header("Game States")]
    public bool debug;
    public bool gameOver;

    public static GameManager instance;
    #endregion

    void Awake()
    {
        instance = this;

        highScore = PlayerPrefs.GetInt("HIGHSCORE");
    }
    void Start()
    {
        Camera.main.orthographicSize = 11;
    }

    public void RestartGame()
    {
        gameOver = false;
        SceneManager.LoadScene(0);
    }

    public void ChangeScore(int i, bool isCombo)
    {
        score += i;

        UIManager.instance.anim.SetTrigger("Pop");

        if (isCombo)
        {
            AudioManager.instance.PlaySound("score");
        }

        if (score > highScore)
        {
            if (!isHighscore)
            {
                AudioManager.instance.PlaySound("highscore");
                Destroy(Instantiate(highScoreParticles, particlesPoint.position, particlesPoint.rotation), 4f);
            }

            isHighscore = true;
            highScore = score;
            PlayerPrefs.SetInt("HIGHSCORE", score);
        }
    }
}