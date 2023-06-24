using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class Board : MonoBehaviour
{
[Header("Art stuff")]
[SerializeField] private Material tileMaterial;
private float tilesize = 1.0f;
private float yOffset = 0.2f;

[Header("prefabs & materials")]
[SerializeField] private GameObject[] prefabs;
[SerializeField] private Material[] teamMaterials;


 private GamePiece[,] gamePieces;
//this variable will be the current piece being moved
private GamePiece currentPieceSelected;
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

    UIGameplay UIGameplay;

    private void Awake()
    {
        //
        isturn = true;
        GenerateAllTiles(1, X_Tiles, Y_Tiles);
        SpawnAllPieces();
        PositionAllPieces();

        UIGameplay = GameObject.Find("GameplayUI").GetComponent<UIGameplay>();
    }

    private void Start()
    {
        UIGameplay.UpdateLabel("Blue's Turn", 30);
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
                    if ((gamePieces[position.x,position.y].team == 0 && isturn) || (gamePieces[position.x, position.y].team == 1 && !isturn))
                    {
                        currentPieceSelected = gamePieces[position.x, position.y];
                        //get a list of avaible moves and highlight all pathways
                        availablemoves = currentPieceSelected.GenerateMoves(ref gamePieces, X_Tiles,Y_Tiles);
                        HighlightPathways();

                        if (isturn != true)
                        {
                            UIGameplay.UpdateLabel("Blue's Turn", 30);
                        }
                        else
                        {
                            UIGameplay.UpdateLabel("Green's Turn", 30);
                        }
                    }
                }
            }
            //if we release the mouse button
            if (currentPieceSelected != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previosPosition = new Vector2Int(currentPieceSelected.CurrentX, currentPieceSelected.CurrentY);
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
        gamePieces = new GamePiece[X_Tiles,Y_Tiles];

        int blueTeam = 0;
        int greenTeam =1;

        //blue team piece placement
        gamePieces[4, 2] = SpawnSinglePiece(PieceType.King, blueTeam);
        gamePieces[3, 1] = SpawnSinglePiece(PieceType.Scout, blueTeam);
        gamePieces[5, 1] = SpawnSinglePiece(PieceType.Scout, blueTeam);
        gamePieces[2, 0] = SpawnSinglePiece(PieceType.Tank, blueTeam);
        gamePieces[6, 0] = SpawnSinglePiece(PieceType.Tank, blueTeam);

        //green team
        gamePieces[4, 6] = SpawnSinglePiece(PieceType.King, greenTeam);
        gamePieces[3, 7] = SpawnSinglePiece(PieceType.Scout, greenTeam);
        gamePieces[5, 7] = SpawnSinglePiece(PieceType.Scout, greenTeam);
        gamePieces[2, 8] = SpawnSinglePiece(PieceType.Tank, greenTeam);
        gamePieces[6, 8] = SpawnSinglePiece(PieceType.Tank, greenTeam);
    }
    private GamePiece SpawnSinglePiece(PieceType type,int team)
    {
        GamePiece gp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<GamePiece>();

        gp.pieceType = type; 
        gp.team = team;
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
        gamePieces[x, y].CurrentX = x;
        gamePieces[x, y].CurrentY = y;
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

    private bool NextPosition(GamePiece current, int x, int y)
    {
        if (!ContainsValidMove(ref availablemoves, new Vector2Int(x, y)))
            return false;

        Vector2Int PreviosPosition = new Vector2Int(current.CurrentX, current.CurrentY);

        if (gamePieces[x, y] != null)
        {
            GamePiece Otherpiece = gamePieces[x, y];

            if (current.team == Otherpiece.team)
            {
                return false;
            }
            else
            {
                if (current.pieceType == PieceType.King)
                {
                    Checkmate(current.team);
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


    private void Checkmate(int winningTeam)
    {
        if (winningTeam == 0)
        {
            UIGameplay.UpdateLabel("Blue Team Won!", 20);
        }
        if (winningTeam == 1)
        {
            UIGameplay.UpdateLabel("Green Team Won!", 20);
        }
    }

}
