using UnityEngine;
using System.Collections.Generic;

public class Tank : GamePiece
{
    public override List<Vector2Int> GenerateMoves(ref GamePiece[,] board, int X_tiles, int Y_tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        int direction = (team == 0) ? 1 : -1;

        //1 forward space
        if (board[CurrentX, CurrentY + direction] == null || board[CurrentX, CurrentY + direction].team != team)
            moves.Add(new Vector2Int(CurrentX, CurrentY + direction));

        //2 forward moves
        if (team == 0 && CurrentX == 4 && CurrentY == 2 && board[CurrentX, CurrentY + (direction * 2)] == null || team == 0 && CurrentX == 4 && CurrentY == 2 && board[CurrentX, CurrentY + (direction * 2)].team != team)
            moves.Add(new Vector2Int(CurrentX, CurrentY + (direction * 2)));
        if (team == 1 && CurrentX == 4 && CurrentY == 6 && board[CurrentX, CurrentY + (direction * 2)] == null || team == 1 && CurrentX == 4 && CurrentY == 6 && board[CurrentX, CurrentY + (direction * 2)].team != team)
            moves.Add(new Vector2Int(CurrentX, CurrentY + (direction * 2)));

        //1 diagonal 
        if (board[CurrentX + 1, CurrentY + direction] == null || board[CurrentX + 1, CurrentY + direction].team != team)
            moves.Add(new Vector2Int(CurrentX + 1, CurrentY + direction));
        if (board[CurrentX - 1, CurrentY + direction] == null || board[CurrentX - 1, CurrentY + direction].team != team)
            moves.Add(new Vector2Int(CurrentX - 1, CurrentY + direction));
        return moves;
    }
}
