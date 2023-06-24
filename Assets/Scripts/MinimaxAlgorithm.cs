using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimaxAlgorithm : MonoBehaviour
{
    private void Start()
    {
        var gameState = new List<int> { 3, 7, 2, 1, 8, 4 };
        List<int> bestMove = null;
        int bestScore = int.MinValue;

        foreach (var move in GenerateMoves(gameState))
        {
            int score = Minimax(move, 3, false);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        Debug.Log("Best move: " + string.Join(", ", bestMove));
        Debug.Log("Best score: " + bestScore);
    }

    private int Minimax(List<int> state, int depth, bool maximizingPlayer)
    {
        if (depth == 0 || IsTerminal(state))
        {
            return Evaluate(state);
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (var move in GenerateMoves(state))
            {
                int eval = Minimax(move, depth - 1, false);
                maxEval = Mathf.Max(maxEval, eval);
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in GenerateMoves(state))
            {
                int eval = Minimax(move, depth - 1, true);
                minEval = Mathf.Min(minEval, eval);
            }
            return minEval;
        }
    }

    private bool IsTerminal(List<int> state)
    {
        // Implement your own terminal state condition
        // For example, if the game is over when all numbers have been used
        return state.Count == 0;
    }

    private int Evaluate(List<int> state)
    {
        // Implement your own evaluation function
        // This function should assign a score to the state
        // based on its desirability for the maximizing player
        return state.Count > 0 ? state[0] : 0; // In this example, we return the first number in the state
    }

    private List<List<int>> GenerateMoves(List<int> state)
    {
        // Implement your own move generation function
        // This function should generate all possible moves
        // based on the current state of the game
        var moves = new List<List<int>>();
        for (int i = 0; i < state.Count; i++)
        {
            var newState = new List<int>(state);
            newState.RemoveAt(i); // Remove one number from the state
            moves.Add(newState);
        }
        return moves;
    }
}
