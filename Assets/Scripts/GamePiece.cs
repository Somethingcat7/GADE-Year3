using UnityEngine;
using System.Collections.Generic;


public enum PieceType
{
    none = 0,
    King = 1,
    Scout = 2,
    Tank = 3 

}

public class GamePiece : MonoBehaviour
{
    public int team;
    public int CurrentX;
    public int CurrentY;
    public bool nirvana = false;
    public PieceType pieceType;

    private Vector3 desiredPosition;

    public virtual List<Vector2Int> GenerateMoves(ref GamePiece[,] board, int X_tiles,int Y_tiles)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        moves.Add(new Vector2Int(3, 3));
        moves.Add(new Vector2Int(3, 4));
        moves.Add(new Vector2Int(4, 3)); 
        moves.Add(new Vector2Int(4, 4));
        return moves;
    }
    


    public virtual void SetPostion(Vector3 position,bool force = false)
    {
        desiredPosition = position;
        if (force)
        transform.position = desiredPosition;
    }
}
