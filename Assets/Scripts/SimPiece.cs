using System.Collections.Generic;
using UnityEngine;

public class SimPiece
{
    public int row;
    public int col;
    public bool isKing;
    public bool isPlayer1;
    public bool isActive; // Check whether the piece is still on the auxiliary board


    public SimPiece(int _row, int _col, bool _isPlayer1, bool _isKing)
    {
        row = _row;
        col = _col;
        isPlayer1 = _isPlayer1;
        isKing = _isKing;
        isActive = true; // At the start all pieces are active
    }

    // Simulates the movement of the piece on the auxiliary board
    public void MoveTo(int newRow, int newCol, SimPiece[,] board, out bool madeCapture)
    {
        madeCapture = false;

        if (!IsValidMove(newRow, newCol, board, out SimPiece capturedPiece))
        {
            return; // Invalid movement
        }

        // Deactivated the captured piece in the simulation
        if (capturedPiece != null)
        {
            capturedPiece.isActive = false;
            madeCapture = true;
        }

        // Update the position on the simulated board
        board[row, col] = null;
        board[newRow, newCol] = this;

        row = newRow;
        col = newCol;

        CheckPromotion();
    }

    // Simulates the validation of a movement
    public bool IsValidMove(int newRow, int newCol, SimPiece[,] board, out SimPiece capturedPiece)
    {
        capturedPiece = null;

        int deltaRow = newRow - row;
        int deltaCol = newCol - col;

        // Check if the box is occupied
        if (board[newRow, newCol] != null)
        {
            return false;
        }

        // Normal movement
        if (Mathf.Abs(deltaRow) == 1 && Mathf.Abs(deltaCol) == 1)
        {
            if (!isKing)
            {
                if (isPlayer1 && deltaRow != 1) return false;
                if (!isPlayer1 && deltaRow != -1) return false;
            }
            return true;
        }

        // Capture movement
        if (Mathf.Abs(deltaRow) == 2 && Mathf.Abs(deltaCol) == 2)
        {
            int middleRow = row + deltaRow / 2;
            int middleCol = col + deltaCol / 2;

            SimPiece midPiece = board[middleRow, middleCol];

            if (midPiece != null && midPiece.isPlayer1 != this.isPlayer1 && midPiece.isActive)
            {
                if (!isKing)
                {
                    if (isPlayer1 && deltaRow != 2) return false;
                    if (!isPlayer1 && deltaRow != -2) return false;
                }
                capturedPiece = midPiece;
                return true;
            }
        }

        return false;
    }

    // Gets the moves available on the simulated board
    public List<SimBox> GetAvailableMoves(SimPiece[,] board)
    {
        List<SimBox> availableMoves = new List<SimBox>();

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
                    availableMoves.Add(new SimBox(targetRow, targetCol));
                }
            }

            // Capture movements
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
                        SimPiece midPiece = board[midRow, midCol];
                        if (midPiece != null && midPiece.isPlayer1 != this.isPlayer1 && midPiece.isActive)
                        {
                            availableMoves.Add(new SimBox(targetRow, targetCol));
                        }
                    }
                }
            }
        }

        return availableMoves;
    }

    //Check if the piece has captures available
    public bool HasAvailableCaptures(SimPiece[,] board)
    {
        int[] rowDirections = isKing ? new int[] { -2, 2 } : new int[] { isPlayer1 ? 2 : -2 };
        int[] colDirections = { -2, 2 };

        foreach (int dRow in rowDirections)
        {
            foreach (int dCol in colDirections)
            {
                int targetRow = row + dRow;
                int targetCol = col + dCol;

                if (!IsValidPosition(targetRow, targetCol)) continue;

                int midRow = row + dRow / 2;
                int midCol = col + dCol / 2;

                SimPiece midPiece = board[midRow, midCol];

                if (midPiece != null && midPiece.isPlayer1 != this.isPlayer1 && midPiece.isActive && board[targetRow, targetCol] == null)
                {
                    if (!isKing)
                    {
                        if (isPlayer1 && dRow != 2) continue;
                        if (!isPlayer1 && dRow != -2) continue;
                    }
                    return true;
                }
            }
        }

        return false;
    }

    //Check if the piece should become a king 
    private void CheckPromotion()
    {
        if (!isKing)
        {
            if ((isPlayer1 && row == 7) || (!isPlayer1 && row == 0))
            {
                isKing = true;
            }
        }
    }

    //Check if the position is within the board
    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < 8 && col >= 0 && col < 8;
    }
    
}
