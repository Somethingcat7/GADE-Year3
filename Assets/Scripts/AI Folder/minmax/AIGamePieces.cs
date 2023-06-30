using UnityEngine;
using System.Collections.Generic;


public enum AIPieceType
{
    none = 0,
    King = 1,
    Scout = 2,
    Tank = 3 
}

public class AIGamePieces : MonoBehaviour
{
    public int AIteam;
    public int AICurrentX;
    public int AICurrentY;
    public bool nirvana = false;
    public AIPieceType pieceType;

    private Vector3 desiredPosition;

    public AIGamePieces(int team)
    {
        this.AIteam = team;
    }

    public  List<Vector2Int> GenerateMoves(ref AIGamePieces[,] board, int X_tiles, int Y_tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        int direction = (AIteam == 0) ? 1 : -1;

        //1 forward space
        if (board[AICurrentX, AICurrentY + direction] == null || board[AICurrentX, AICurrentY + direction].AIteam != AIteam)
            moves.Add(new Vector2Int(AICurrentX, AICurrentY + direction));

        ////1 backspace
        //if (board[CurrentX, CurrentY - direction] == null || board[CurrentX, CurrentY - direction].team != team)
        //    moves.Add(new Vector2Int(CurrentX, CurrentY - direction));

        //2 forward moves
        if (AIteam == 0 && AICurrentX == 4 && AICurrentY == 2 && board[AICurrentX, AICurrentY + (direction * 2)] == null || AIteam == 0 && AICurrentX == 4 && AICurrentY == 2 && board[AICurrentX, AICurrentY + (direction * 2)].AIteam != AIteam)
            moves.Add(new Vector2Int(AICurrentX, AICurrentY + (direction * 2)));
        if (AIteam == 1 && AICurrentX == 4 && AICurrentY == 6 && board[AICurrentX, AICurrentY + (direction * 2)] == null || AIteam == 1 && AICurrentX == 4 && AICurrentY == 6 && board[AICurrentX, AICurrentY + (direction * 2)].AIteam != AIteam)
            moves.Add(new Vector2Int(AICurrentX, AICurrentY + (direction * 2)));

        //1 diagonal 
        if (board[AICurrentX + 1, AICurrentY + direction] == null || board[AICurrentX + 1, AICurrentY + direction].AIteam != AIteam)
            moves.Add(new Vector2Int(AICurrentX + 1, AICurrentY + direction));

        if (board[AICurrentX - 1, AICurrentY + direction] == null || board[AICurrentX - 1, AICurrentY + direction].AIteam != AIteam)
            moves.Add(new Vector2Int(AICurrentX - 1, AICurrentY + direction));
        return moves;
    }

    public virtual void SetPostion(Vector3 position,bool force = false)
    {
        desiredPosition = position;
        if (force)
        transform.position = desiredPosition;
    }
}
