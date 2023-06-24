using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer
{
    private int maxDepth = 3; // Maximum depth to explore in the Minimax algorithm
    private int CPU; // AI player ID
    private int opponent; // Human player ID
    private int X_Tiles;
    private int Y_Tiles;

    public AIPlayer(int AI, int player, int xTiles, int yTiles)
    {
        CPU = AI;
        opponent = player;
        X_Tiles = xTiles;
        Y_Tiles = yTiles;
    }

    private int Minimax(AIGamePieces[,] gamePieces, int depth, bool maximizingPlayer)
    {
        if (depth == maxDepth)
        {
            return Evaluate(gamePieces);
        }

        if (maximizingPlayer)
        {
            int bestScore = int.MinValue;

            // Generate all possible moves for the AI player
            List<Move> possibleMoves = new List<Move>();
            GenerateMoves(gamePieces, CPU, possibleMoves);

            foreach (Move move in possibleMoves)
            {
                // Make the move on a copy of the game state
                AIGamePieces[,] tempGamePieces = Clone(gamePieces);
                MakeMove(tempGamePieces, move);

                // Recursively call Minimax for the opponent
                int score = Minimax(tempGamePieces, depth + 1, false);

                // Update the best score
                bestScore = Math.Max(bestScore, score);
            }

            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;

            // Generate all possible moves for the opponent
            List<Move> possibleMoves = new List<Move>();
            GenerateMoves(gamePieces, opponent, possibleMoves);

            foreach (Move move in possibleMoves)
            {
                // Make the move on a copy of the game state
                AIGamePieces[,] tempGamePieces = Clone(gamePieces);
                MakeMove(tempGamePieces, move);

                // Recursively call Minimax for the AI player
                int score = Minimax(tempGamePieces, depth + 1, true);

                // Update the best score
                bestScore = Math.Min(bestScore, score);
            }

            return bestScore;
        }
    }

    public Move GetBestMove(AIGamePieces[,] gamePieces)
    {
        // Create a list to store all possible moves
        List<Move> possibleMoves = new List<Move>();

        // Generate all possible moves for the AI player
        GenerateMoves(gamePieces, CPU, possibleMoves);

        // Evaluate each possible move using the Minimax algorithm
        int bestScore = int.MinValue;
        Move bestMove = null;

        foreach (Move move in possibleMoves)
        {
            // Make the move on a copy of the game state
            AIGamePieces[,] tempGamePieces = Clone(gamePieces);
            MakeMove(tempGamePieces, move);

            // Use the Minimax algorithm to determine the score of the move
            int score = Minimax(tempGamePieces, 0, false);

            // Update the best move if a higher score is found
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private void GenerateMoves(AIGamePieces[,] gamePieces, int player, List<Move> possibleMoves)
    {
        // Iterate over the game board and generate all possible moves for the given player
        for (int x = 0; x < X_Tiles; x++)
        {
            for (int y = 0; y < Y_Tiles; y++)
            {
                if (gamePieces[x, y] != null && gamePieces[x, y].AIteam == player)
                {
                    // Generate moves for the current piece
                    List<Vector2Int> moves = gamePieces[x, y].GenerateMoves( ref gamePieces, X_Tiles, Y_Tiles);

                    // Add the moves to the list of possible moves
                    foreach (Vector2Int move in moves)
                    {
                        possibleMoves.Add(new Move(x, y, move.x, move.y));
                    }
                }
            }
        }
    }

    private AIGamePieces[,] Clone(AIGamePieces[,] gamePieces)
    {
        List <AIGamePieces> copy = new List <AIGamePieces>();
        // Create a new game state with the same size
        AIGamePieces[,] clonedGamePieces = new AIGamePieces[X_Tiles, Y_Tiles];

        // Copy the game state by creating new GamePiece objects
        for (int x = 0; x < X_Tiles; x++)
        {
            for (int y = 0; y < Y_Tiles; y++)
            {
                if (gamePieces[x, y] != null)
                {
                    clonedGamePieces[x, y] = new AIGamePieces(gamePieces[x, y].AIteam);
                }
            }
        }

        return clonedGamePieces;
    }

    private int Evaluate(AIGamePieces[,] gamePieces)
    {
        // Implement your evaluation function here
        // Assign a score to the current game state
        // Higher scores for better positions for the AI player
        // Lower scores for better positions for the human player

        // Example evaluation function: Count the number of AI pieces minus the number of human pieces
        int aiPiecesCount = 0;
        int humanPiecesCount = 0;

        for (int x = 0; x < X_Tiles; x++)
        {
            for (int y = 0; y < Y_Tiles; y++)
            {
                if (gamePieces[x, y] != null)
                {
                    if (gamePieces[x, y].AIteam == CPU)
                        aiPiecesCount++;
                    else if (gamePieces[x, y].AIteam == opponent)
                        humanPiecesCount++;
                }
            }
        }

        return aiPiecesCount - humanPiecesCount;
    }

    private void MakeMove(AIGamePieces[,] gamePieces, Move move)
    {
        int startX = move.startX;
        int startY = move.startY;
        int endX = move.endX;
        int endY = move.endY;

        // Perform the move by updating the game state
        // based on your game's rules
        // Make sure to handle capturing and other game-specific logic
    }
}




public class Move
{
    public int startX;
    public int startY;
    public int endX;
    public int endY;
    public int score;

    public Move(int startX, int startY, int endX, int endY)
    {
        this.startX = startX;
        this.startY = startY;
        this.endX = endX;
        this.endY = endY;
        this.score = 0;
    }
}


