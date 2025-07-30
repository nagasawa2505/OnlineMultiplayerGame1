using UnityEngine;

public class GameTimer : MonoBehaviour
{
    const float MatchTimeSec = Config.MatchTimeSec;
    float timer;
    bool isMatchEnd;
    GameState currentGameState;
    UIController ui;

    // Start is called before the first frame update
    void Start()
    {
        ui = GetComponent<UIController>();
        timer = MatchTimeSec;
    }

    void FixedUpdate()
    {
        currentGameState = GameController.GetGameState();    
    }

    // Update is called once per frame
    void Update()
    {
        if (isMatchEnd)
        {
            return;
        }

        if (currentGameState == GameState.Start)
        {
            timer -= Time.deltaTime;
        }

        if (timer < 0)
        {
            GameController.EndGame();

            // 得点集計
            // GameControllerにやらせたいけどUIと繋ぎにくい
            Post[] posts = FindObjectsOfType<Post>();
            int myScore = 0;
            int otherScore = 0;

            foreach (var post in posts)
            {
                if (post.teamNumber == GameController.GetTeamNumber())
                {
                    myScore = post.GetScore();
                }
                else
                {
                    int score = post.GetScore();
                    if (otherScore < score)
                    {
                        otherScore = score;
                    }
                }
            }

            GameResult result;
            if (myScore > otherScore)
            {
                result = GameResult.Win;
            }
            else if (myScore < otherScore)
            {
                result = GameResult.Lose;
            }
            else
            {
                result = GameResult.Draw;
            }

            // 結果表示
            ui.ShowResult(result);

            isMatchEnd = true;
        }
    }

    public float GetTime()
    {
        return timer;
    }
}
