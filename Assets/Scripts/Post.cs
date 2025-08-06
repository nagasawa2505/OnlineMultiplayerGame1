using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Post : MonoBehaviour
{
    int currentScore;

    public int teamNumber;
    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;
    Dictionary<int, int> score = new();
    GameState currentGameState;

    void FixedUpdate()
    {
        currentGameState = GameController.GetGameState();
        if (currentGameState == GameState.Start || currentGameState == GameState.End)
        {
            if (!scorePanel.activeSelf)
            {
                scorePanel.SetActive(true);
            }
            scoreText.text = currentScore.ToString();
        }
        else
        {
            if (scorePanel.activeSelf)
            {
                scorePanel.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (GameController.GetGameState() != GameState.Start)
        {
            return;
        }

        Item item = other.GetComponent<Item>();
        if (item == null)
        {
            return;            
        }

        int itemId = item.GetItemId();

        if (score.TryGetValue(itemId, out int count))
        {
            if (count == 0)
            {
                score[itemId] = 1;
                currentScore += item.points;
            }
        }
        else
        {
            score[itemId] = 1;
            currentScore += item.points;
        }

        item.Posted();
    }

    void OnTriggerExit(Collider other)
    {
        if (GameController.GetGameState() != GameState.Start)
        {
            return;
        }

        Item item = other.GetComponent<Item>();
        if (item == null)
        {
            return;
        }

        int itemId = item.GetItemId();

        if (score.TryGetValue(itemId, out int count))
        {
            if (count == 1)
            {
                score[itemId] = 0;
                currentScore -= item.points;
            }
        }

        item.Unposted();
    }

    public int GetScore()
    {
        return currentScore;
    }
}
