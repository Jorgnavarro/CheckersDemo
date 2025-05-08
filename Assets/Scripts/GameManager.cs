using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI currentPlayer;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    private int _player1Score = 0;
    private int _player2Score = 0;
    private bool _isPlayer1Turn = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateTurnUI();
        UpdateScoreUI();
    }

    public void ChangeTurn()
    {
        _isPlayer1Turn = !_isPlayer1Turn;
        UpdateTurnUI();
    }

    public void AddScore(bool isPlayer1)
    {
        if (isPlayer1)
        {
            _player1Score++;
        }
        else
        {
            _player2Score++;
        }
        UpdateScoreUI();
    }

    private void UpdateTurnUI()
    {
        currentPlayer.text = _isPlayer1Turn ? "Current turn: Player 1" : "Current turn: Player 2";
    }

    private void UpdateScoreUI()
    {
        player1ScoreText.text = "Player 1: " + _player1Score;
        player2ScoreText.text = "Player 2: " + _player2Score;
    }

    public bool GetCurrentTurn()
    {
        return _isPlayer1Turn;
    }

    public void CheckWinCondition()
    {
        int player1Pieces = 0;
        int player2Pieces = 0;

        foreach (Piece piece in FindObjectsByType<Piece>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (piece.gameObject.activeSelf)
            {
                if(piece.isPlayer1)
                    player1Pieces++;
                else
                    player2Pieces++;
            }
        }
        
        //Check if the player have not pieces
        if (player1Pieces == 0)
        {
            Debug.Log("Player 2 wins!");
            ShowWinMessage("Player 2 wins!");
        }
        else if(player2Pieces == 0)
        {
            Debug.Log("Player 1 wins!");
            ShowWinMessage("Player 1 wins!");
        }
    }

    private void ShowWinMessage(string playerWins)
    {
        currentPlayer.text = playerWins;
    }
}
