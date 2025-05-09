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
        
        List<(Piece, Box)> captureMoves = new List<(Piece, Box)>();
        List<(Piece, Box)> normalMoves = new List<(Piece, Box)>();

        foreach (Piece piece in aiPieces)
        {
            List<Box> availableMoves = piece.GetAvailableMoves(board);

            foreach (Box box in availableMoves)
            {
                Piece capturedPiece;
                
                //If the AI identify a capture then add to the list of captureMoves
                //In the condition we force to choose the capture over the normal movement
                if (piece.IsValidMove(box.row, box.col, board, out capturedPiece) && capturedPiece != null)
                {
                    captureMoves.Add((piece, box));
                }
                else
                {
                    normalMoves.Add((piece, box));
                }
            }
        }

        //Choose the best movement
        if (captureMoves.Count > 0)
        {
            var move = captureMoves[Random.Range(0, captureMoves.Count)]; // choose a random capture if the AI have many available
            BoardManager.Instance.HandleAIMove(move.Item1, move.Item2);
        }
        else if (normalMoves.Count > 0)
        {
            var move = normalMoves[Random.Range(0, normalMoves.Count)]; // choose a random normal movement
            BoardManager.Instance.HandleAIMove(move.Item1, move.Item2);
        }
    }
    
}
