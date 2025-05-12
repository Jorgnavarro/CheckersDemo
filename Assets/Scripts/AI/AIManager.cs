using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void AIMove()
    {
        StartCoroutine(AIMoveCoroutine());
    }

    private IEnumerator AIMoveCoroutine()
    {
        yield return new WaitForSeconds(1f); //Wait one second before the AI move a piece

        List<Piece> aiPieces = new List<Piece>();
        Piece[,] board = BoardManager.Instance.board;

        foreach (Piece piece in FindObjectsByType<Piece>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!piece.isPlayer1 && piece.gameObject.activeSelf) // Only AI pieces
            {
                aiPieces.Add(piece);
            }
        }
        
        List<(Piece, Box, int)> captureMoves = new List<(Piece, Box, int)>();
        List<(Piece, Box)> normalMoves = new List<(Piece, Box)>();

        SimPiece[,] simBoard = GetSimulatedBoard(); // Get simulated board

        foreach (Piece piece in aiPieces)
        {
            SimPiece simPiece = simBoard[piece.row, piece.col]; // Get simulated version of the piece
            List<SimBox> bestSequence = GetCaptureSequence(simPiece, simBoard);

            if (bestSequence.Count > 0)
            {
                Box targetBox = BoardManager.Instance.GetBox(bestSequence[0].row, bestSequence[0].col); // Get the real box
                if (targetBox != null)
                {
                    captureMoves.Add((piece, targetBox, bestSequence.Count));
                }
            }
            else
            {
                List<Box> availableMoves = piece.GetAvailableMoves(board);
                foreach (Box box in availableMoves)
                {
                    normalMoves.Add((piece, box));
                }
            }
        }

        //Choose the best movement
        if (captureMoves.Count > 0)
        {
            captureMoves.Sort((a, b) => b.Item3.CompareTo(a.Item3)); // Sort by number of captures
            var bestMove = captureMoves[0]; // Choosing the best capture
            BoardManager.Instance.HandleAIMove(bestMove.Item1, bestMove.Item2);
        }
        else if (normalMoves.Count > 0)
        {
            var move = normalMoves[Random.Range(0, normalMoves.Count)]; // Random normal movement
            BoardManager.Instance.HandleAIMove(move.Item1, move.Item2);
        }


    }
    
    private List<SimBox> GetCaptureSequence(SimPiece piece, SimPiece[,] board)
    {
        List<SimBox> bestSequence = new List<SimBox>();
        List<SimBox> currentSequence = new List<SimBox>();

        void FindCaptures(SimPiece currentPiece, List<SimBox> sequence)
        {
            List<SimBox> availableMoves = currentPiece.GetAvailableMoves(board);
            
            foreach (SimBox box in availableMoves)
            {
                SimPiece capturedPiece;
                
                if (currentPiece.IsValidMove(box.row, box.col, board, out capturedPiece) && capturedPiece != null && capturedPiece.isActive)
                {
                    sequence.Add(box);
                    capturedPiece.isActive = false; // Simulate the removal of the piece
                    SimPiece tempPiece = new SimPiece(currentPiece.row, currentPiece.col, currentPiece.isPlayer1, currentPiece.isKing);
                    tempPiece.MoveTo(box.row, box.col, board, out _);
                    FindCaptures(tempPiece, sequence); // Search for more captures from the new position
                
                    if (sequence.Count > bestSequence.Count)
                    {
                        bestSequence = new List<SimBox>(sequence);
                    }
                
                    sequence.Remove(box); // Go back to try other options
                    capturedPiece.isActive = true; // Restore the piece for other simulations
                }
            }
        }

        FindCaptures(piece, currentSequence);
        return bestSequence;
    }



    private SimPiece[,] GetSimulatedBoard()
    {
        Piece[,] realBoard = BoardManager.Instance.board;
        SimPiece[,] simBoard = new SimPiece[realBoard.GetLength(0), realBoard.GetLength(1)];
        
        foreach (Piece piece in FindObjectsByType<Piece>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (piece.gameObject.activeSelf) // Only active pieces
            {
                simBoard[piece.row, piece.col] = new SimPiece(piece.row, piece.col, piece.isPlayer1, piece.isKing);
            }
        }
        return simBoard;
    }
    
}
