using System;
using UnityEngine;
using System.Collections.Generic;

public class AIBoard : MonoBehaviour
{
[Header("Art stuff")]
[SerializeField] private Material tileMaterial;
private float tilesize = 1.0f;
private float yOffset = 0.2f;

[Header("prefabs & materials")]
[SerializeField] private GameObject[] prefabs;
[SerializeField] private Material[] teamMaterials;

private AIPlayer CPU;
public AIGamePieces[,] gamePieces;
//this variable will be the current piece being moved
private AIGamePieces currentPieceSelected;
//this is the size of the chessboard dimens ions for now its 8X8
//size number of tiles on x axis
private const int X_Tiles = 9;
//size number of tiles on y axis
private const int Y_Tiles = 9;
// double arry that conatians cordainate 0f tile position
private GameObject[,] tiles;
//camera variable to make the camera watch the game 
private Camera currentCamera;
private Vector2Int currentSelected;
private Vector3 bounds;
private bool isturn; 
private List<Vector2Int> availablemoves = new List<Vector2Int>();

    private void Awake()
    {
        //
        isturn = true;
        GenerateAllTiles(1, X_Tiles, Y_Tiles);
        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        if (!currentCamera)
        {
          currentCamera = Camera.main;
            return;
        }
        // this Raycast is used to highlight tiles we have currently selected on the board
        RaycastHit hit;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Tile", "Selected", "Highlight")))
        {
            // return the index i've hit
            Vector2Int position = LookupTileIndex(hit.transform.gameObject);
            //if we have highlighted a tile after not highlighting any tiles
            if(currentSelected == -Vector2Int.one)
            {     
                currentSelected = position;
                tiles[position.x, position.y].layer = LayerMask.NameToLayer("Selected");
            }

            if (currentSelected != -Vector2Int.one)
            {
                tiles[currentSelected.x, currentSelected.y].layer = (ContainsValidMove(ref availablemoves, currentSelected)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentSelected = position;
                tiles[position.x, position.y].layer = LayerMask.NameToLayer("Selected");
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (gamePieces[position.x, position.y] != null)
                {
                    //checking if its our turn
                    if ((gamePieces[position.x,position.y].AIteam == 0 && isturn) /*|| (gamePieces[position.x, position.y].team == 1 && !isturn)*/)
                    {
                        currentPieceSelected = gamePieces[position.x, position.y];
                        //get a list of avaible moves and highlight all pathways
                        availablemoves = currentPieceSelected.GenerateMoves(ref gamePieces, X_Tiles,Y_Tiles);
                        HighlightPathways();
                    }
                    else
                    {
                        // Create an instance of the AIPlayer class
                        AIPlayer aiPlayer = new AIPlayer(1, 0, X_Tiles, Y_Tiles);

                        // Call the AIPlayer's GetBestMove method and pass the gamePieces array
                        Move bestMove = aiPlayer.GetBestMove(gamePieces);

                        // Get the current piece at the selected position
                        AIGamePieces currentPiece = gamePieces[bestMove.startX, bestMove.startY];

                        // Move the piece to the AI's selected position
                        MovePiece(currentPiece, bestMove.endX, bestMove.endY);
                    }
                }
            }
            //if we release the mouse button
            if (currentPieceSelected != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previosPosition = new Vector2Int(currentPieceSelected.AICurrentX, currentPieceSelected.AICurrentY);
                bool vaildmove = NextPosition(currentPieceSelected, position.x, position.y);
                RemoveHighlightPathways();
                if (!vaildmove)
                {
                    currentPieceSelected.transform.position = GetTileCenter(previosPosition.x,previosPosition.y);
                    currentPieceSelected = null; 
                }
                else
                {
                    currentPieceSelected = null;
                }
            }
        }
        else
        {
            
            if(currentSelected != -Vector2Int.one)
            {
                tiles[currentSelected.x, currentSelected.y].layer = (ContainsValidMove(ref availablemoves,currentSelected)) ?  LayerMask.NameToLayer("Highlight"):LayerMask.NameToLayer("Tile");
                currentSelected = -Vector2Int.one;
            }

            if (currentPieceSelected && Input.GetMouseButtonUp(0))
            {
                currentPieceSelected = null;
                RemoveHighlightPathways();
            }
        }
        
    }

    // generating tiles methods
    private void GenerateAllTiles(float tileSize,int TileCountX, int TileCountY)
    {

        tiles = new GameObject[TileCountX,TileCountY];
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(String.Format("X:{0}, Y{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        // add mesh to to the tileObject
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        //rendering and setting to tileObject
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        // setting the vertices for tileobject mesh
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize , 0, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0, (y+1) * tileSize);
        vertices[2] = new Vector3((x+1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x+1) * tileSize, 0, (y+1) * tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        tileObject.layer = LayerMask.NameToLayer("Tile"); 

        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //spawning pieces
    private void SpawnAllPieces()
    {
        gamePieces = new AIGamePieces[X_Tiles,Y_Tiles];

        int blueTeam = 0;
        int greenTeam =1;

        //blue team piece placement
        gamePieces[4, 2] = SpawnSinglePiece(AIPieceType.King, blueTeam);
        gamePieces[3, 1] = SpawnSinglePiece(AIPieceType.Scout, blueTeam);
        gamePieces[5, 1] = SpawnSinglePiece(AIPieceType.Scout, blueTeam);
        gamePieces[2, 0] = SpawnSinglePiece(AIPieceType.Tank, blueTeam);
        gamePieces[6, 0] = SpawnSinglePiece(AIPieceType.Tank, blueTeam);

        //green team
        gamePieces[4, 6] = SpawnSinglePiece(AIPieceType.King, greenTeam);
        gamePieces[3, 7] = SpawnSinglePiece(AIPieceType.Scout, greenTeam);
        gamePieces[5, 7] = SpawnSinglePiece(AIPieceType.Scout, greenTeam);
        gamePieces[2, 8] = SpawnSinglePiece(AIPieceType.Tank, greenTeam);
        gamePieces[6, 8] = SpawnSinglePiece(AIPieceType.Tank, greenTeam);
    }
    private AIGamePieces SpawnSinglePiece(AIPieceType type,int team)
    {
        AIGamePieces gp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<AIGamePieces>();

        gp.pieceType = type; 
        gp.AIteam = team;
        gp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return gp;
    }

    // Positioning game pieces
    private void PositionAllPieces()
    {
        for (int x = 0; x < X_Tiles; x++)
            for (int y = 0; y < Y_Tiles; y++)
                if(gamePieces[x, y] != null)
                PositionPiece(x, y, true);
    }
    private void PositionPiece(int x,int y,bool force = false)
    {
        gamePieces[x, y].AICurrentX = x;
        gamePieces[x, y].AICurrentY = y;
        gamePieces[x,y].transform.position = GetTileCenter(x,y);
    }
    private Vector3 GetTileCenter(int x,int y)
    {
        return new Vector3(x * tilesize,yOffset,y * tilesize) - bounds + new Vector3(tilesize/ 2, 0,tilesize/2);
    }
    //Highlighted tiles method
    private void HighlightPathways()
    {
        for (int i = 0; i < availablemoves.Count; i++)
        {
            tiles[availablemoves[i].x, availablemoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    private void RemoveHighlightPathways()
    {
        for (int i = 0; i < availablemoves.Count; i++)
        {
            tiles[availablemoves[i].x, availablemoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availablemoves.Clear();
    }
    // Operations
    private bool ContainsValidMove(ref List<Vector2Int>moves, Vector2 position)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == position.x && moves[i].y == position.y)
                return true;

        return false;

    }
    private Vector2Int LookupTileIndex(GameObject info)
    {
        for (int x = 0; x < X_Tiles; x++)
            for (int y = 0; y < Y_Tiles; y++)
                if (tiles[x,y] == info)
                return new Vector2Int(x,y);

        return -Vector2Int.one;//invalid output
    }

    private bool NextPosition(AIGamePieces current, int x, int y)
    {
        if (!ContainsValidMove(ref availablemoves, new Vector2Int(x, y)))
            return false;
        
            Vector2Int PreviosPosition = new Vector2Int(current.AICurrentX, current.AICurrentY);

        if (gamePieces[x, y] != null)
        {
            AIGamePieces Otherpiece = gamePieces[x, y];

            if (current.AIteam == Otherpiece.AIteam)
            {
                return false;
            }
            else
            {
                if(current.pieceType == AIPieceType.King)
                {
                    Checkmate(current.AIteam);
                }
                Destroy(Otherpiece.gameObject);
            }
        }


        gamePieces[x, y] = current;
        gamePieces[PreviosPosition.x, PreviosPosition.y] = null;

        PositionPiece(x, y, true);

        isturn = !isturn;
        return true;
      
    }

    //win coditions
    private void Checkmate(int winningTeam)
    {
        if(winningTeam == 0)
        {
            Console.WriteLine("Blue Team Won");
        }
        if(winningTeam == 1)
        {
            Console.WriteLine("Green Team Won");
        }
    }

    //AI implentation


    public void MakeMove(Move move)
    {
        // Check if the move is valid and update the game state accordingly
        int startX = move.startX;
        int startY = move.startY;
        int endX = move.endX;
        int endY = move.endY; 

        if (gamePieces[startX, startY] != null && (gamePieces[endX, endY] == null || gamePieces[endX, endY].AIteam != gamePieces[startX, startY].AIteam))
        {
            // Move the game piece to the end position
            gamePieces[endX, endY] = gamePieces[startX, startY];
            gamePieces[startX, startY] = null;
        }
    }
    public void MovePiece(AIGamePieces currentPiece, int targetX, int targetY)
    {
        Vector2Int position = new Vector2Int(targetX, targetY);

        if (!ContainsValidMove(ref availablemoves, position))
            return;

        // Store the previous position of the current piece
        Vector2Int previousPosition = new Vector2Int(currentPiece.AICurrentX, currentPiece.AICurrentY);

        // Check if there is a game piece at the target position
        if (gamePieces[targetX, targetY] != null)
        {
            AIGamePieces otherPiece = gamePieces[targetX, targetY];

            // If the other piece belongs to the same team, return (invalid move)
            if (currentPiece.AIteam == otherPiece.AIteam)
                return;
            else
            {
                // If the current piece is a king, it's a checkmate
                if (currentPiece.pieceType == AIPieceType.King)
                {
                    Checkmate(currentPiece.AIteam);
                }

                // Destroy the other piece
                Destroy(otherPiece.gameObject);
            }
        }

        // Move the current piece to the target position
        gamePieces[targetX, targetY] = currentPiece;
        gamePieces[previousPosition.x, previousPosition.y] = null;

        // Update the position of the current piece
        currentPiece.AICurrentX = targetX;
        currentPiece.AICurrentY = targetY;

        // Update the visual position of the piece on the game board
        PositionPiece(targetX, targetY, true);

        // Toggle the turn to the next player
        isturn = !isturn;
    }

    //accessors
    public int GetX_tiles()
    {
        return X_Tiles;
    }

    public int GetY_tiles()
    {
        return Y_Tiles;
    }

}
