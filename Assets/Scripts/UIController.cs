using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    string activeSceneName;
    GameState currentGameState;

    public GameObject titlePanel;

    public GameObject timerPanel;
    public TextMeshProUGUI timerText;

    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    GameTimer gameTimer;
    float time;

    Color32 textColorWin = new Color32(0, 100, 255, 200);
    Color32 textColorLose = new Color32(255, 100, 0, 200);
    Color32 textColorDraw = new Color32(200, 200, 200, 255);

    // Start is called before the first frame update
    void Start()
    {
        gameTimer = GetComponent<GameTimer>();
    }

    void FixedUpdate()
    {
        time = gameTimer.GetTime();
        if (time >= 0)
        {
            timerText.text = Mathf.Ceil(time).ToString();
        }
        else
        {
            timerText.text = "0";
        }

        string sceneName = SceneManager.GetActiveScene().name;
        GameState gameState = GameController.GetGameState();

        // 初期化しないで起動直後も呼ばせる
        if (sceneName != activeSceneName || gameState != currentGameState)
        {
            switch (sceneName)
            {
                case "Title":
                    titlePanel.SetActive(true);
                    timerPanel.SetActive(false);
                    resultPanel.SetActive(false);
                    break;
                default:
                    if (GameController.GetGameState() == GameState.Start)
                    {
                        titlePanel.SetActive(false);
                        timerPanel.SetActive(true);
                        resultPanel.SetActive(false);
                    }
                    else if (GameController.GetGameState() == GameState.End)
                    {
                        titlePanel.SetActive(false);
                        timerPanel.SetActive(false);
                        resultPanel.SetActive(true);
                    }
                    else
                    {
                        titlePanel.SetActive(false);
                        timerPanel.SetActive(false);
                        resultPanel.SetActive(false);
                    }
                    break;
            }
            activeSceneName = sceneName;
            currentGameState = gameState;
        }
    }

    public void ShowResult(GameResult result)
    {
        ClearResult();

        switch (result)
        {
            case GameResult.Win:
                resultText.color = textColorWin;
                resultText.text = "WIN";
                break;

            case GameResult.Lose:
                resultText.color = textColorLose;
                resultText.text = "LOSE";
                break;

            case GameResult.Draw:
                resultText.color = textColorDraw;
                resultText.text = "DRAW";
                break;
            }
            resultPanel.SetActive(true);
    }

    void ClearResult()
    {
        resultPanel.SetActive(false);
        resultText.color = new Color(0, 0, 0);
        resultText.text = string.Empty;
    }

    public void BackToTitle()
    {
        GameController.SetGameState(GameState.Wait);
        SceneManager.LoadScene("Title");
    }
}
