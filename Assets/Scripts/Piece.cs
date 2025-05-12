using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public int row;
    public int col;
    public bool isKing = false;
    public bool isPlayer1;
    public GameObject kingImage;
    public string colorHex;

    
    //Initializes the piece with its position, player and color.
    public void Initialize(int _row, int _col, bool _isPlayer1, string colorString)
    {
        row = _row;
        col = _col;
        isPlayer1 = _isPlayer1;
        colorHex = colorString;
        
        if (ColorUtility.TryParseHtmlString(colorHex, out Color parsedColor))
        {
            GetComponent<SpriteRenderer>().color = parsedColor;
        }
        else
        {
            Debug.LogError($"Error to parse color: {colorHex}");
        }
    }

    public void SetColor(string newColor)
    {
        colorHex = newColor;
        if (ColorUtility.TryParseHtmlString(colorHex, out Color parsedColor))
        {
            GetComponent<SpriteRenderer>().color = parsedColor;
        }
    }
    
    //Checks if a move is valid, including normal and capturing moves.
    public bool IsValidMove(int newRow, int newCol, Piece[,] board, out Piece capturedPiece)
    {
        capturedPiece = null;
        
        //Calculate how many rows and columns the piece will move
        int deltaRow = newRow - row;
        int deltaCol = newCol - col;
        
        
        //We check if the box is empty
        if (board[newRow, newCol] != null)
        {
            return false; // ocupated box
        }

        //Normal movement
        if (Mathf.Abs(deltaRow) == 1 && Mathf.Abs(deltaCol) == 1)
        {
            if (!isKing)
            {
                if (isPlayer1 && deltaRow != 1) return false; // Player 1 add +1 in X axis
                if (!isPlayer1 && deltaRow != -1) return false; // Player 2 rest -1 in X axis
            }
            return true; //diagonal movement allowed in both directions for king movement
        }

        //Capture movement jump over the enemy piece.
        if (Mathf.Abs(deltaRow) == 2 && Mathf.Abs(deltaCol) == 2)
        {
            int middleRow = row + deltaRow / 2;
            int middleCol = col + deltaCol / 2;
            
            Piece midPiece = board[middleRow, middleCol];

            if (midPiece != null && midPiece.isPlayer1 != this.isPlayer1)
            {
                // 🔄 Restriction: Only allow forward captures if the piece is not a king
                if (!isKing)
                {
                    if (isPlayer1 && deltaRow != 2) return false; // Player 1 only captures upwards
                    if (!isPlayer1 && deltaRow != -2) return false; // Player 2 only captures downwards
                }
                capturedPiece = midPiece;
                return true;
            }
        }
        return false;
    }
    
    //Move the piece to the new position and handle captures if necessary.
    public bool MovePiece(int newRow, int newCol, Piece [,] board, out bool madeCapture)
    {
        if (!IsValidMove(newRow, newCol, board, out Piece captured))
        {
            Debug.Log("Invalid move.");
            madeCapture = false;
            return false;
        }

        //Find target box
        Box targetBox = FindBoxByGridPosition(newRow, newCol);
        if (targetBox == null)
        {
            madeCapture = false;
            return false;
        }
            

        //Delete the previous position of the piece
        board[row, col] = null;
        
        //Update the position of the piece on the board in the array
        board[newRow, newCol] = this;

        // if a piece is captured
        if (captured != null)
        {
            board[captured.row, captured.col] = null;
            captured.gameObject.SetActive(false);
            Debug.Log("Captured enemy piece!");
        }
        
        madeCapture = captured != null;

        //Move the piece
        row = newRow;
        col = newCol;
        transform.position = targetBox.transform.position;

        CheckPromotion();
        
        
        return true;

    }
    
    
    //Find the available position 
    private Box FindBoxByGridPosition(int newRow, int newCol)
    {
        foreach (Box box in FindObjectsByType<Box>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (box.row == newRow && box.col == newCol)
            {
                return box;
            }
        }
        return null;
    }
    
    //Gets the available boxes to move the piece, including normal and capturing moves.
    public List<Box> GetAvailableMoves(Piece[,] board)
    {
        List<Box> availableMoves = new List<Box>();

        int[] rowDirections = isKing ? new int[] { -1, 1 } : new int[] { isPlayer1 ? 1 : -1 };
        int[] colDirections = { -1, 1 };

        foreach (int dRow in rowDirections)
        {
            foreach (int dCol in colDirections)
            {
                int targetRow = row + dRow;
                int targetCol = col + dCol;

                if (IsValidPosition(targetRow, targetCol) && board[targetRow, targetCol] == null)
                {
                    Box targetBox = FindBoxByGridPosition(targetRow, targetCol);
                    if (targetBox != null)
                    {
                        availableMoves.Add(targetBox);
                    }
                }
            }
            
            // 🔄 capture movements 
            int[] captureRowDirections = isKing ? new int[] { -2, 2 } : new int[] { isPlayer1 ? 2 : -2 };
            int[] captureColDirections = { -2, 2 };

            foreach (int captureRow in captureRowDirections)
            {
                foreach (int captureCol in captureColDirections)
                {
                    int targetRow = row + captureRow;
                    int targetCol = col + captureCol;
                    int midRow = row + captureRow / 2;
                    int midCol = col + captureCol / 2;

                    if (IsValidPosition(targetRow, targetCol) && board[targetRow, targetCol] == null)
                    {
                        Piece midPiece = board[midRow, midCol];
                        if (midPiece != null && midPiece.isPlayer1 != this.isPlayer1)
                        {
                            Box targetBox = FindBoxByGridPosition(targetRow, targetCol);
                            if (targetBox != null)
                            {
                                availableMoves.Add(targetBox);
                            }
                        }
                    }
                }
            }
        }

        return availableMoves;
    }
    //Check if the position is inside to the board
    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < 8 && col >= 0 && col < 8;
    }
    
    //Check if the piece has available captures
    public bool HasAvailableCaptures(Piece[,] board)
    {
        int[] rowDirections = isKing ? new int[] { -2, 2 } : new int[] { isPlayer1 ? 2 : -2 };
        int[] colDirections = { -2, 2 };
        

        foreach (int dRow in rowDirections)
        {
            foreach (int dCol in colDirections)
            {
                int targetRow = row + dRow;
                int targetCol = col + dCol;

                //We check that stay inside the board
                if (targetRow < 0 || targetRow >= board.GetLength(0) || targetCol < 0 ||
                    targetCol >= board.GetLength(1))
                    continue;
                
                int midRow = row + dRow / 2;
                int midCol = col + dCol / 2;
                
                Piece midPiece = board[midRow, midCol];
                //We check if the enemy piece is in the target box
                //Check that the piece is different to the piece of the current player
                //Check if exists and available box after the enemy piece for make the capture
                if (midPiece != null && midPiece.isPlayer1 != this.isPlayer1 && board[targetRow, targetCol] == null )
                {
                    // 🔄 Restriction: Only allow forward captures if the piece is not a king
                    if (!isKing)
                    {
                        if (isPlayer1 && dRow != 2) continue; // Player 1 only captures upwards
                        if (!isPlayer1 && dRow != -2) continue; // Player 2 only captures downwards
                    }
                    return true;
                }
            }
        }
        return false;
    }

    //Checks if the piece has reached the last row and turns it into a King.
    private void CheckPromotion()
    {
        if (!isKing)
        {
            if ((isPlayer1 && row == 7) || (!isPlayer1 && row == 0)) // Check the last row for each player
            {
                isKing = true;
                kingImage.SetActive(true);
            }
        }
    }
}
