using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    string activeSceneName;
    GameState currentGameState;
    bool isMobileDevice;

    public GameObject mobilePadPanel;

    public GameObject titlePanel;

    public GameObject waitingPanel;
    public TextMeshProUGUI waitingText;

    public GameObject startPanel;

    public GameObject guidePanel;

    public GameObject timerPanel;
    public TextMeshProUGUI timerText;

    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    GameTimer gameTimer;
    int roomCapacity;
    float time;

    Color32 textColorWin = new Color32(0, 100, 255, 200);
    Color32 textColorLose = new Color32(255, 100, 0, 200);
    Color32 textColorDraw = new Color32(200, 200, 200, 255);

    // Start is called before the first frame update
    void Start()
    {
        gameTimer = GetComponent<GameTimer>();
        isMobileDevice = GameController.IsMobileDevice();
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

        if (sceneName != activeSceneName || gameState != currentGameState)
        {
            switch (sceneName)
            {
                case "Title":
                    mobilePadPanel.SetActive(false);
                    titlePanel.SetActive(true);
                    waitingPanel.SetActive(false);
                    startPanel.SetActive(false);
                    guidePanel.SetActive(false);
                    timerPanel.SetActive(false);
                    resultPanel.SetActive(false);
                    break;
                default:
                    if (GameController.GetGameState() == GameState.Wait)
                    {
                        mobilePadPanel.SetActive(isMobileDevice);
                        titlePanel.SetActive(false);
                        waitingPanel.SetActive(true);
                        startPanel.SetActive(false);
                        guidePanel.SetActive(!isMobileDevice);
                        timerPanel.SetActive(false);
                        resultPanel.SetActive(false);
                    }
                    else if (GameController.GetGameState() == GameState.Start)
                    {
                        mobilePadPanel.SetActive(isMobileDevice);
                        titlePanel.SetActive(false);
                        waitingPanel.SetActive(false);
                        startPanel.SetActive(true);
                        guidePanel.SetActive(!isMobileDevice);
                        timerPanel.SetActive(true);
                        resultPanel.SetActive(false);

                        Invoke("HideStartPanel", 2f);
                    }
                    else if (GameController.GetGameState() == GameState.End)
                    {
                        mobilePadPanel.SetActive(isMobileDevice);
                        titlePanel.SetActive(false);
                        waitingPanel.SetActive(false);
                        startPanel.SetActive(false);
                        guidePanel.SetActive(false);
                        timerPanel.SetActive(false);
                        resultPanel.SetActive(true);
                    }
                    else
                    {
                        mobilePadPanel.SetActive(false);
                        titlePanel.SetActive(false);
                        waitingPanel.SetActive(false);
                        startPanel.SetActive(false);
                        guidePanel.SetActive(false);
                        timerPanel.SetActive(false);
                        resultPanel.SetActive(false);
                    }
                    break;
            }
            activeSceneName = sceneName;
            currentGameState = gameState;
        }

        if (waitingPanel.activeSelf)
        {
            if (roomCapacity == 0)
            {
                roomCapacity = GameController.GetRoomCapacity();
            }
            int playerCount = PlayersController.GetPlayerCount();
            waitingText.text = $"Waiting for other players...({playerCount}/{roomCapacity})";
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

            default:
                break;
        }
        resultPanel.SetActive(true);
    }

    public void BackToTitle()
    {
        GameController.BackToTitle();
    }

    void ClearResult()
    {
        resultPanel.SetActive(false);
        resultText.color = new Color();
        resultText.text = string.Empty;
    }

    void HideStartPanel()
    {
        startPanel.SetActive(false);
    }
}
