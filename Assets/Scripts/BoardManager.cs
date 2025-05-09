using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
  public static BoardManager Instance;
    [SerializeField] Piece[,] board = new Piece[8, 8];
    public GameObject piecePrefab;
    private List<Box> _blackSquares = new List<Box>();
    private Piece _selectedPiece;

    
    [Header("Board Settings")]
    public GameObject boxPrefab;
    private string _customColor1 = "#594F4F";
    private string _customColor2 = "#D6C7B1";
    private Box[,] _grid = new Box[8, 8];
    

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateBoard();
        FindBlackSquares();
        PlacePieces();
    }

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

                // Asignar color según la regla
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
            ClearHighlightedBoxes(); // 🔄 Limpiar imágenes de movimiento antes de actualizar la posición

           if (captured)
           {
               GameManager.Instance.AddScore(_selectedPiece.isPlayer1); //Add score to the current player
               GameManager.Instance.CheckWinCondition(); 
               
               if (_selectedPiece.HasAvailableCaptures(board))
               {
                   Debug.Log("You must continue capturing with the same piece.");
                   StartCoroutine(WaitAndEndTurn(_selectedPiece, 3f));
                   return;
               }
           }
           
            // Fin del turno
            _selectedPiece.GetComponent<SpriteRenderer>().color = _selectedPiece.isPlayer1 ? Color.black : Color.red;
            _selectedPiece = null;
            GameManager.Instance.ChangeTurn(); // 🔄 Switch turn
        }

    }

    private IEnumerator WaitAndEndTurn(Piece piece, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (piece.HasAvailableCaptures(board))
        {
            piece.GetComponent<SpriteRenderer>().color = piece.isPlayer1 ? Color.black : Color.red;
            Debug.Log("Time out, automatic turn change.");
            _selectedPiece = null;
            GameManager.Instance.ChangeTurn();
        }
    }
    

    
    //Checks if the Piece belongs to the current player.
    //If a piece was selected, resets its color before switching to the new Piece
    //Marks the new Piece green.
    private void HandlePieceSelection(Piece clickedPiece)
    {

        if (_selectedPiece != null && _selectedPiece.HasAvailableCaptures(board))
        {
            return;
        }
        
        //Check if the Piece belong to the Player in turn
        if ((GameManager.Instance.GetCurrentTurn() && clickedPiece.isPlayer1) || (!GameManager.Instance.GetCurrentTurn() && !clickedPiece.isPlayer1))
        {
            if (_selectedPiece != null)
            {
                ClearHighlightedBoxes();
                _selectedPiece.GetComponent<SpriteRenderer>().color = _selectedPiece.isPlayer1 ? Color.black : Color.red;
            }

            _selectedPiece = clickedPiece;
            _selectedPiece.GetComponent<SpriteRenderer>().color = Color.yellow;
            HighlightAvailableMoves(_selectedPiece);
        }
    }

    //

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

    private void PlacePieces()
    {
        int downIndex = 0;
        int topIndex = _blackSquares.Count - 12;

        for (int i = 0; i < 12; i++)
        {
            SpawnPiece(_blackSquares[downIndex], true, Color.black);
            downIndex++;
        }

        for (int i = 0; i < 12; i++)
        {
            SpawnPiece(_blackSquares[topIndex], false, Color.red);
            topIndex++;
        }
    }

    private void SpawnPiece(Box box, bool _isPlayer1, Color color)
    {
        GameObject piece = Instantiate(piecePrefab, box.transform.position, Quaternion.identity, transform);
        Piece pieceScript = piece.GetComponent<Piece>();
        pieceScript.Initialize(box.row, box.col, _isPlayer1, color);
        board[box.row, box.col] = pieceScript;
    }
    
    private void HighlightAvailableMoves(Piece piece)
    {
        ClearHighlightedBoxes(); // Limpiar cualquier resaltado anterior

        List<Box> availableMoves = piece.GetAvailableMoves(board);

        foreach (Box box in availableMoves)
        {
            if (_blackSquares.Contains(box)) // Solo activar si la casilla está en el área de juego
            {
                if (IsCaptureMove(piece, box)) // 🔄 Verificar si es un movimiento de captura
                {
                    box.ShowMoveIndicator(true, Color.red); // Resaltar en rojo si es captura
                }
                else
                {
                    box.ShowMoveIndicator(true, Color.yellow); // Resaltar en amarillo si es movimiento normal
                }
            }


        }
    }
    
    private bool IsCaptureMove(Piece piece, Box targetBox)
    {
        int deltaRow = targetBox.row - piece.row;
        int deltaCol = targetBox.col - piece.col;

        if (Mathf.Abs(deltaRow) == 2 && Mathf.Abs(deltaCol) == 2) // Movimiento de captura (salto de 2)
        {
            int midRow = piece.row + deltaRow / 2;
            int midCol = piece.col + deltaCol / 2;

            Piece midPiece = board[midRow, midCol];
            return midPiece != null && midPiece.isPlayer1 != piece.isPlayer1; // Verificar si hay una pieza enemiga
        }

        return false;
    }





    private void ClearHighlightedBoxes()
    {
        foreach (Box box in _blackSquares) // Solo limpiar casillas negras
        {
            box.ShowMoveIndicator(false); // Desactivar la imagen de movimiento
        }
        
    }
}
