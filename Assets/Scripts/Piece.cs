using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public int row;
    public int col;
    public bool isKing = false;
    public bool isPlayer1;


    public void Initialize(int _row, int _col, bool _isPlayer1, Color color)
    {
        row = _row;
        col = _col;
        isPlayer1 = _isPlayer1;
        GetComponent<SpriteRenderer>().color = color;
    }
    
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
                // 🔄 Restricción: Solo permitir capturas hacia adelante si la pieza no es rey
                if (!isKing)
                {
                    if (isPlayer1 && deltaRow != 2) return false; // Jugador 1 solo captura hacia arriba
                    if (!isPlayer1 && deltaRow != -2) return false; // Jugador 2 solo captura hacia abajo
                }
                capturedPiece = midPiece;
                return true;
            }
        }
        return false;
    }
    
    

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
                if (midPiece != null && midPiece.isPlayer1 != this.isPlayer1 && board[targetRow, targetCol] == null )
                {
                    // 🔄 Restricción: Solo permitir capturas hacia adelante si la pieza no es rey
                    if (!isKing)
                    {
                        if (isPlayer1 && dRow != 2) continue; // Jugador 1 solo captura hacia arriba
                        if (!isPlayer1 && dRow != -2) continue; // Jugador 2 solo captura hacia abajo
                    }
                    return true;
                }
            }
        }
        return false;
    }

    private void CheckPromotion()
    {
        if (!isKing && (row == 7 || row == 0)) // Última fila
        {
            isKing = true;
            GetComponent<SpriteRenderer>().color = Color.yellow; // Indicador visual de promoción
            Debug.Log("Piece promoted to King!");
        }

    }
}
