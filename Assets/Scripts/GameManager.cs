using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode
{
    PlayerVsPlayer,
    PlayerVsAI,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameMode currentGameMode = GameMode.PlayerVsAI; //Default game mode

    [Header("Win Panel settings")]
    public GameObject winPanel;
    public TextMeshProUGUI winText;

    public TextMeshProUGUI currentPlayer;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    private int _player1Score = 0;
    private int _player2Score = 0;
    private bool _isPlayer1Turn = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateTurnUI();
        UpdateScoreUI();
        winPanel.SetActive(false);
    }

    public void ChangeTurn()
    {
        _isPlayer1Turn = !_isPlayer1Turn;
        UpdateTurnUI();
        if (currentGameMode == GameMode.PlayerVsAI && !_isPlayer1Turn)
        {
            AIManager.Instance.AIMove();
        }
    }
    
    public void AddScore(bool isPlayer1)
    {
        if (isPlayer1)
        {
            _player1Score+=10;
        }
        else
        {
            _player2Score+=10;
        }
        AudioManager.Instance.PlayCaptureSound();
        UpdateScoreUI();
    }

    private void UpdateTurnUI()
    {
        if (currentGameMode == GameMode.PlayerVsAI && !_isPlayer1Turn)
        {
            currentPlayer.text = "Current turn: Machine";
        }
        else
        {
            currentPlayer.text = _isPlayer1Turn ? "Current turn: Player 1" : "Current turn: Player 2";
        }
    }

    private void UpdateScoreUI()
    {
        player1ScoreText.text = "Score player 1: " + _player1Score;

        if (currentGameMode == GameMode.PlayerVsAI)
        {
            player2ScoreText.text = "Score machine: " + _player2Score;   
        }
        else
        { 
            player2ScoreText.text = "Score player 2: " + _player2Score;   
        }
        
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
                if (piece.isPlayer1)
                    player1Pieces++;
                else
                    player2Pieces++;
            }
        }

        // Check if player 1 lose all the pieces
        if (player1Pieces == 0)
        {
            Debug.Log("Player 2 wins!");
            ShowWinMessage("Player 2 wins!");
        }
        else if (player2Pieces == 0)
        {
            Debug.Log("Player 1 wins!");
            ShowWinMessage("Player 1 wins!");
        }
        else
        {
            CheckPlayerBlocked(); // Check if the player don't have more movements available
        }
    }

    private void CheckPlayerBlocked()
    {
        bool player1HasMoves = false;
        bool player2HasMoves = false;

        Piece[,] board = BoardManager.Instance.board; // Access to the board array from the BoardManager

        foreach (Piece piece in FindObjectsByType<Piece>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (piece.gameObject.activeSelf)
            {
                if (piece.isPlayer1 && piece.GetAvailableMoves(board).Count > 0)
                {
                    player1HasMoves = true;
                }
                else if (!piece.isPlayer1 && piece.GetAvailableMoves(board).Count > 0)
                {
                    player2HasMoves = true;
                }
            }
        }

        // Check if the player is blocked
        if (!player1HasMoves)
        {
            if (currentGameMode == GameMode.PlayerVsAI)
            {
                Debug.Log("Machine wins! (Player 1 is blocked)");
                ShowWinMessage("Machine wins!");
            }
            else
            {
                Debug.Log("Player 2 wins! (Player 1 is blocked)");
                ShowWinMessage("Player 2 wins!");
            }
        }
        else if (!player2HasMoves)
        {
            if (currentGameMode == GameMode.PlayerVsAI)
            {
                Debug.Log("Player 1 wins! (Machine is blocked)");
                ShowWinMessage("Player 1 wins!");
            }
            else
            {
                Debug.Log("Player 1 wins! (Player 2 is blocked)");
                ShowWinMessage("Player 1 wins!");
            }
        }
        
    }

    private void ShowWinMessage(string winner)
    {

        if (currentGameMode == GameMode.PlayerVsAI)
        {
            if (winner.Contains("Player 2"))
            {
                winner = "Machine wins, good luck for next!";
            }
        }
        winText.text = winner;
        winPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
