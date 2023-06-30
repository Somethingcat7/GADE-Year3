using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIController : MonoBehaviour
{
    private NeuralNetwork neuralNetwork;
    private NeuralBoard board;

    private void Awake()
    {
        neuralNetwork = GetComponent<NeuralNetwork>();
        board = FindObjectOfType<NeuralBoard>();
    }

    public void MakeAIMove()
    {
        GamePieces[,] gamePieces = board.GetGamePieces();
        int x = board.GetX_Tiles();
        int y = board.GetY_Tiles();

        // Collect input data for the neural network
        float[] inputData = CollectInputData(gamePieces, x, y);

        // Pass the input data to the neural network
        float[] outputData = neuralNetwork.Brain(inputData);

        // Process the output data and get the chosen move
        Vector2Int chosenMove = ProcessedOutputData(outputData);

        // Perform the chosen move
        if (chosenMove != Vector2Int.zero)
        {
            GamePieces currentPiece = gamePieces[chosenMove.x, chosenMove.y];
            if (currentPiece != null)
            {
                GamePieces currentPieceSelected = currentPiece;
                board.availablemoves = currentPiece.GenerateMoves(ref gamePieces, x, y);
                board.NHighlightPathways();
                Vector2Int prevPosition = new Vector2Int(currentPiece.CurrentX, currentPiece.CurrentY);
                bool validMove = board.NNextPosition(currentPiece, chosenMove.x, chosenMove.y);
                board.NRemoveHighlightPathways();
                if (!validMove)
                {
                    currentPiece.transform.position = board.GetTileCenter(prevPosition.x, prevPosition.y);
                    currentPieceSelected = null;
                }
                else
                {
                    currentPieceSelected = null;
                }
            }
        }
    }

    private float[] CollectInputData(GamePieces[,] gamePieces, int X_Tiles, int Y_Tiles)
    {
        float[] inputData = new float[X_Tiles * Y_Tiles * 4];

        int index = 0;
        for (int x = 0; x < X_Tiles; x++)
        {
            for (int y = 0; y < Y_Tiles; y++)
            {
                if (gamePieces[x, y] != null)
                {
                    // Encode the piece type and team as one-hot vectors
                    int pieceType = (int)gamePieces[x, y].pieceType;
                    int team = gamePieces[x, y].team;
                    inputData[index + pieceType] = 1;
                    inputData[index + 4 + team] = 1;
                }
                index += 8; // Move to the next set of one-hot vectors
            }
        }

        return inputData;
    }

    private Vector2Int ProcessedOutputData(float[] outputData)
    {
        List<Vector2Int> validMoves = GetValidMoves();
        float maxOutput = outputData.Max();
        int index = Array.IndexOf(outputData, maxOutput);

        if (index < validMoves.Count)
        {
            return validMoves[index];
        }

        return Vector2Int.zero;
    }

    private List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        GamePieces[,] gamePieces = board.GetGamePieces();
        int xTiles = board.GetX_Tiles();
        int yTiles = board.GetY_Tiles();
        int team = 1; // AI's team

        for (int x = 0; x < xTiles; x++)
        {
            for (int y = 0; y < yTiles; y++)
            {
                if (gamePieces[x, y] != null && gamePieces[x, y].team == team)
                {
                    List<Vector2Int> moves = gamePieces[x, y].GenerateMoves(ref gamePieces, xTiles, yTiles);
                    validMoves.AddRange(moves);
                }
            }
        }

        return validMoves;
    }
}
