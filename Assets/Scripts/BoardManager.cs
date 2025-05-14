using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public Piece[,] board = new Piece[8, 8];
    public GameObject piecePrefab;
    private List<Box> _blackSquares = new List<Box>();
    private Piece _selectedPiece;

    
    [Header("Board Settings")]
    public GameObject boxPrefab;
    private string _customColor1 = "#594F4F";
    private string _customColor2 = "#D6C7B1";
    private string _customColorPlayer1Piece = "#000000";
    private string _customColorPlayer2Piece = "#938D90";
    private Box[,] _grid = new Box[8, 8];
    

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

    /*
    private void Start()
    {
        GenerateBoard();
        FindBlackSquares();
        PlacePieces();
    }
    */

    public void InitializeGame()
    {
        GenerateBoard();
        FindBlackSquares();
        PlacePieces();
    }

    //Generate the board by instantiating the boxes and assigning custom colors.
    private void GenerateBoard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Vector3 position = new Vector3(col - 4, row - 4, 0); // Adjust the position on the scene
                GameObject boxObject = Instantiate(boxPrefab, position, Quaternion.identity, transform);
                Box boxScript = boxObject.GetComponent<Box>();
                boxScript.row = row;
                boxScript.col = col;
                
                Color boxColor;

                //Assign custom colors to the boxes
                SpriteRenderer renderer = boxObject.GetComponent<SpriteRenderer>();
                if ((row + col) % 2 == 0)
                {
                    ColorUtility.TryParseHtmlString(_customColor1, out boxColor);
                }
                else
                {
                    ColorUtility.TryParseHtmlString(_customColor2, out boxColor);
                }
                
                boxObject.GetComponent<SpriteRenderer>().color = boxColor;

                _grid[row, col] = boxScript;
                
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            DetectPieceClick(mousePosition);
            DetectBoxClick(mousePosition);
        }
    }
    //Detects if the Player has clicked on a Piece
    //If the Piece belongs to the current Player, call HandlePieceSelection()
    private void DetectPieceClick(Vector2 mousePosition)
    {
        RaycastHit2D hitPiece = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Piece"));

        if (hitPiece.collider != null)
        {
            Piece clickedPiece = hitPiece.collider.gameObject.GetComponent<Piece>();

            if (clickedPiece != null)
            {
                HandlePieceSelection(clickedPiece);
            }
        }
    }

    //Only attempt to move the Piece if a Piece already selected
    //Detect the destination Box
    //If there is a valid Box, call HandleMove()
    private void DetectBoxClick(Vector2 mousePosition)
    {
        if (_selectedPiece == null) return;

        RaycastHit2D hitBox = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Box"));

        if (hitBox.collider != null)
        {
            Box clickedBox = hitBox.collider.GetComponent<Box>();

            if (clickedBox != null)
            {
                HandleMove(clickedBox);
            }
        }

    }

    //Attempt to move the piece with MovePiece()
    //If the move is valid, change the piece´s color, deselect, and change the turn
    private void HandleMove(Box clickedBox)
    {
        if (_selectedPiece.row == clickedBox.row && _selectedPiece.col == clickedBox.col)
        {
            Debug.Log("Piece is not moved");
            return; // No executed MovePiece()
        }

        bool captured;
        
        bool validMove = _selectedPiece.MovePiece(clickedBox.row, clickedBox.col, board, out captured);

        if (validMove)
        {
            ClearHighlightedBoxes(); //Clean motion images before updating the position
            
            GameManager.Instance.CheckWinCondition(); //Check if anyone won

           if (captured)
           {
               GameManager.Instance.AddScore(_selectedPiece.isPlayer1); //Add score to the current player
               if (_selectedPiece.HasAvailableCaptures(board))
               {
                   Debug.Log("You must continue capturing with the same piece.");
                   StartCoroutine(WaitAndEndTurn(_selectedPiece, 3f));
                   return;
               }
           }
           else
           {
               AudioManager.Instance.PlayMoveSound();
           }
           
            // End of turn
            _selectedPiece.SetColor(_selectedPiece.isPlayer1 ? _customColorPlayer1Piece : _customColorPlayer2Piece);
            _selectedPiece = null;
            GameManager.Instance.ChangeTurn(); //Switch turn
            
        }

    }

    //Wait a while before ending your turn if there are captures available and reset de color selection to the default one!
    private IEnumerator WaitAndEndTurn(Piece piece, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (piece.HasAvailableCaptures(board))
        {
            piece.SetColor(piece.isPlayer1 ? _customColorPlayer1Piece : _customColorPlayer2Piece);
            Debug.Log("Time out, automatic turn change.");
            _selectedPiece = null;
            GameManager.Instance.ChangeTurn();
        }
    }
    

    
    //Checks if the Piece belongs to the current player.
    //If a piece was selected, resets its color before switching to the new Piece
    //Marks the new Piece green.
    //Highlights your available moves.
    private void HandlePieceSelection(Piece clickedPiece)
    {
        /*------------------------ Version for the Rule that block captures------------------------
        // Check if exists another piece with better capture available
        List<Piece> piecesWithCaptures = new List<Piece>();

        foreach (Piece piece in FindObjectsByType<Piece>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (piece.gameObject.activeSelf && piece.HasAvailableCaptures(board) &&
                ((GameManager.Instance.GetCurrentTurn() && piece.isPlayer1) || (!GameManager.Instance.GetCurrentTurn() && !piece.isPlayer1)))
            {
                piecesWithCaptures.Add(piece);
            }
        }

        // If there are captured pieces and the selected piece is not one of them, do not allow the change
        if (piecesWithCaptures.Count > 0 && !piecesWithCaptures.Contains(clickedPiece))
        {
            Debug.Log("You must select a piece with an available capture.");
            return;
        }

        // Clear previous selection
        if (_selectedPiece != null)
        {
            ClearHighlightedBoxes();
            _selectedPiece.SetColor(_selectedPiece.isPlayer1 ? _customColorPlayer1Piece : _customColorPlayer2Piece);
        }

        // Select new piece
        _selectedPiece = clickedPiece;
        _selectedPiece.SetColor("#FFFF00"); // Yellow selection
        HighlightAvailableMoves(_selectedPiece);
        */
        
        //-----------------Version for free movements
        // Check if the piece belongs to the current player
        if ((GameManager.Instance.GetCurrentTurn() && clickedPiece.isPlayer1) || (!GameManager.Instance.GetCurrentTurn() && !clickedPiece.isPlayer1))
        {
            // Clear previous selection
            if (_selectedPiece != null)
            {
                ClearHighlightedBoxes();
                _selectedPiece.SetColor(_selectedPiece.isPlayer1 ? _customColorPlayer1Piece : _customColorPlayer2Piece);
            }

            // Select new piece
            _selectedPiece = clickedPiece;
            _selectedPiece.SetColor("#FFFF00"); // Amarillo
            HighlightAvailableMoves(_selectedPiece);
        }
        
        
    }
    

    //Find all the black boxes where the game will be played.
    private void FindBlackSquares()
    {
        _blackSquares.Clear(); // Clean the list before full it 
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 0)
                {
                    _blackSquares.Add(_grid[row, col]);
                }
            }
        }
        _blackSquares.Sort((a, b) => a.row != b.row ? a.row.CompareTo(b.row) : a.col.CompareTo(b.col));
    }

    //Place the pieces in their initial positions.
    private void PlacePieces()
    {
        int downIndex = 0;
        int topIndex = _blackSquares.Count - 12;

        for (int i = 0; i < 12; i++)
        {
            SpawnPiece(_blackSquares[downIndex], true, _customColorPlayer1Piece);
            downIndex++;
        }

        for (int i = 0; i < 12; i++)
        {
            SpawnPiece(_blackSquares[topIndex], false, _customColorPlayer2Piece);
            topIndex++;
        }
    }

    //Instance the pieces and configure their position on the grid, before placing them.
    private void SpawnPiece(Box box, bool _isPlayer1, string customColorPiece)
    {
        GameObject piece = Instantiate(piecePrefab, box.transform.position, Quaternion.identity, transform);
        Piece pieceScript = piece.GetComponent<Piece>();
        pieceScript.Initialize(box.row, box.col, _isPlayer1, customColorPiece);
        board[box.row, box.col] = pieceScript;
    }
    
    //Highlights the available boxes where the selected piece can move.
    /// Activates the movement image on valid squares.
    private void HighlightAvailableMoves(Piece piece)
    {
        ClearHighlightedBoxes(); // Clear any previous highlights

        List<Box> availableMoves = piece.GetAvailableMoves(board);

        foreach (Box box in availableMoves)
        {
            if (_blackSquares.Contains(box)) // Only activate if the box is in the play area
            {
                box.ShowMoveIndicator(true, Color.yellow);
            }
        }
    }
    
    private void ClearHighlightedBoxes()
    {
        foreach (Box box in _blackSquares) // Just clean the black boxes
        {
            box.ShowMoveIndicator(false); // Deactivated the image indicator
        }
        
    }

    public void HandleAIMove(Piece aiPiece, Box targetBox)
    {
        bool captured;
        bool validMove = aiPiece.MovePiece(targetBox.row, targetBox.col, board, out captured);

        if (validMove)
        {
            ClearHighlightedBoxes();
            GameManager.Instance.CheckWinCondition();

            /*
            if (captured && aiPiece.HasAvailableCaptures(board))
            {
                Debug.Log("AI must continue capturing.");
                AIManager.Instance.AIMove(); // If possible the AI continues capturing
                return;
            }
            */
            
            if (captured)
            {
                GameManager.Instance.AddScore(false); // Actualiza el puntaje de la IA

                if (aiPiece.HasAvailableCaptures(board))
                {
                    Debug.Log("AI must continue capturing.");
                    AIManager.Instance.AIMove(); // Si puede, la IA sigue capturando
                    return;
                }
            }
            else
            {
                AudioManager.Instance.PlayMoveSound();
            }
            
            GameManager.Instance.ChangeTurn(); //Switch the turn
        }
        
    }
    
    public Box GetBox(int row, int col)
    {
        if (row < 0 || row >= 8 || col < 0 || col >= 8)
        {
            return null; // Out of bounds of the board
        }
    
        return _grid[row, col]; // return the corresponding box
    }


}
