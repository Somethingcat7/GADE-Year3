using System;
using UnityEngine;
using System.Collections.Generic;

public class NeuralBoard : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    private float tilesize = 1.0f;
    private float yOffset = 0.2f;

    [Header("prefabs & materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;


    private GamePieces[,] gamePieces;
    //this variable will be the current piece being moved
    private GamePieces currentPieceSelected;
    //this is the size of the chessboard dimens ions for now its 8X8
    //size number of tiles on x axis
    private int X_Tiles = 9;
    //size number of tiles on y axis
    private int Y_Tiles = 9;
    // double arry that conatians cordainate 0f tile position
    private GameObject[,] tiles;
    //camera variable to make the camera watch the game 
    private Camera currentCamera;
    private Vector2Int currentSelected;
    private Vector3 bounds;
    // private bool isturn; 
    private AIController cpu;

    public List<Vector2Int> availablemoves = new List<Vector2Int>();

    private void Awake()
    {
        //
        //isturn = true;
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
            if (currentSelected == -Vector2Int.one)
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
                    if (gamePieces[position.x, position.y].team == 0)
                    {
                        currentPieceSelected = gamePieces[position.x, position.y];
                        //get a list of avaible moves and highlight all pathways
                        availablemoves = currentPieceSelected.GenerateMoves(ref gamePieces, X_Tiles, Y_Tiles);
                        NHighlightPathways();
                    }
                }
            }
            //if we release the mouse button
            if (currentPieceSelected != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previosPosition = new Vector2Int(currentPieceSelected.CurrentX, currentPieceSelected.CurrentY);
                bool vaildmove = NNextPosition(currentPieceSelected, position.x, position.y);
                NRemoveHighlightPathways();
                if (!vaildmove)
                {
                    currentPieceSelected.transform.position = GetTileCenter(previosPosition.x, previosPosition.y);
                    currentPieceSelected = null;
                    cpu.MakeAIMove();
                }
                else
                {
                    currentPieceSelected = null;
                }
            }
        }
        else
        {

            if (currentSelected != -Vector2Int.one)
            {
                tiles[currentSelected.x, currentSelected.y].layer = (ContainsValidMove(ref availablemoves, currentSelected)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentSelected = -Vector2Int.one;
            }

            if (currentPieceSelected && Input.GetMouseButtonUp(0))
            {
                currentPieceSelected = null;
                NRemoveHighlightPathways();
            }
        }

    }

    // generating tiles methods
    private void GenerateAllTiles(float tileSize, int TileCountX, int TileCountY)
    {

        tiles = new GameObject[TileCountX, TileCountY];
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
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0, (y + 1) * tileSize);
        vertices[2] = new Vector3((x + 1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x + 1) * tileSize, 0, (y + 1) * tileSize);

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
        gamePieces = new GamePieces[X_Tiles, Y_Tiles];

        int blueTeam = 0;
        int greenTeam = 1;

        //blue team piece placement
        gamePieces[4, 2] = SpawnSinglePiece(NPieceType.King, blueTeam);
        gamePieces[3, 1] = SpawnSinglePiece(NPieceType.Scout, blueTeam);
        gamePieces[5, 1] = SpawnSinglePiece(NPieceType.Scout, blueTeam);
        gamePieces[2, 0] = SpawnSinglePiece(NPieceType.Tank, blueTeam);
        gamePieces[6, 0] = SpawnSinglePiece(NPieceType.Tank, blueTeam);

        //green team
        gamePieces[4, 6] = SpawnSinglePiece(NPieceType.King, greenTeam);
        gamePieces[3, 7] = SpawnSinglePiece(NPieceType.Scout, greenTeam);
        gamePieces[5, 7] = SpawnSinglePiece(NPieceType.Scout, greenTeam);
        gamePieces[2, 8] = SpawnSinglePiece(NPieceType.Tank, greenTeam);
        gamePieces[6, 8] = SpawnSinglePiece(NPieceType.Tank, greenTeam);
    }
    private GamePieces SpawnSinglePiece(NPieceType type, int team)
    {
        GamePieces gp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<GamePieces>();

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
                if (gamePieces[x, y] != null)
                    PositionPiece(x, y, true);
    }
    private void PositionPiece(int x, int y, bool force = false)
    {
        gamePieces[x, y].CurrentX = x;
        gamePieces[x, y].CurrentY = y;
        gamePieces[x, y].transform.position = GetTileCenter(x, y);
    }
    public Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tilesize, yOffset, y * tilesize) - bounds + new Vector3(tilesize / 2, 0, tilesize / 2);
    }
    //Highlighted tiles method
    public void NHighlightPathways()
    {
        for (int i = 0; i < availablemoves.Count; i++)
        {
            tiles[availablemoves[i].x, availablemoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    public void NRemoveHighlightPathways()
    {
        for (int i = 0; i < availablemoves.Count; i++)
        {
            tiles[availablemoves[i].x, availablemoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availablemoves.Clear();
    }
    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 position)
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
                if (tiles[x, y] == info)
                    return new Vector2Int(x, y);

        return -Vector2Int.one;//invalid output
    }

    public bool NNextPosition(GamePieces current, int x, int y)
    {
        if (!ContainsValidMove(ref availablemoves, new Vector2Int(x, y)))
            return false;

        Vector2Int PreviosPosition = new Vector2Int(current.CurrentX, current.CurrentY);

        if (gamePieces[x, y] != null)
        {
            GamePieces Otherpiece = gamePieces[x, y];

            if (current.team == Otherpiece.team)
            {
                return false;
            }
            else
            {
                Destroy(Otherpiece.gameObject);
            }
        }


        gamePieces[x, y] = current;
        gamePieces[PreviosPosition.x, PreviosPosition.y] = null;

        PositionPiece(x, y, true);

        //isturn = !isturn;
        return true;

    }


    //Accessors
    public GamePieces[,] GetGamePieces()
    {
        return gamePieces;
    }

    public int GetX_Tiles()
    {
        return X_Tiles;
    }

    public int GetY_Tiles()
    {
        return Y_Tiles;
    }
}