using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour, IObserver<ReversiMove>
{
    /// <summary>
    /// Holds a GameObject being animated and associated information
    /// </summary>
    struct AnimatedObject
    {
        public MonoBehaviour ToAnimate;
        public SpotState EndColor;
        public Vector3 MovingTo;
        public Vector3 ArcTop;
        public Vector3 DistPerArc;

        public AnimatedObject(MonoBehaviour g, Vector3 p, SpotState c)
        {
            ToAnimate = g;
            MovingTo = p;

            Vector3 dist = p - ToAnimate.transform.position;
            float xMid = ToAnimate.transform.position.x - dist.x;
            float zMid =  ToAnimate.transform.position.z - dist.z;

            ArcTop = new Vector3 (xMid, 3, zMid);

            DistPerArc = new Vector3 (dist.x / 2, 3, dist.z / 2);

            EndColor = c;

        }
    }

    /// <summary>
    /// The prefab to instatiate to add an AI to the game
    /// </summary>
    public GameObject AIController;
    
    /// <summary>
    /// The prefab to instatiate to add a second human player to the game
    /// </summary>
    public GameObject PlayerController;

    /// <summary>
    /// Prefab to instantiate for the game pieces
    /// </summary>
    public GamePiece gamePiece;

    /// <summary>
    /// Track the size of a square on the board
    /// </summary>
    private float _squareSize;

    /// <summary>
    /// Queue Serving the pieces to place
    /// </summary>
    private Queue<GamePiece> _pieceList;

    /// <summary>
    /// Holds the state of the board in the game world with objects for the pieces
    /// </summary>
    private GamePiece[,] _board;

    /// <summary>
    /// Stores a representation of the current state of the board which can be passed to bots and stores empty slots
    /// </summary>
    private ReversiBoard _boardState;

    private Queue<AnimatedObject> _animQueue;

    /// <summary>
    /// Stores a representation of the current state of the board which can be passed to bots and stores empty slots
    /// </summary>
    public ReversiBoard BoardState
    {
        get { return _boardState; }
    }

    /// <summary>
    /// The player currently allowed to make moves
    /// </summary>
    private PlayAgent _currentPlayer;

    /// <summary>
    /// Stores the current player while an animation is happeneing
    /// </summary>
    private PlayAgent _currPlayerAnim;

    /// <summary>
    /// Player currently waiting to move
    /// </summary>
    private PlayAgent _offPlayer;


    public PlayAgent CurrentPlayer
    {
        get { return _currentPlayer;}
    }

    /// <summary>
    /// Get the coordinates of a square of the board
    /// </summary>
    /// <param name="boardLoc"><see cref="Point"/> holding the location of the square as an index starting at the left corner <br/> 
    /// x: Goes from 0 for left column -> 7 for right column <br/>
    /// y: Goes from 0 for top row -> 7 for bottom row
    /// </param>
    public Vector3 GetSquareCoords(Point boardLoc)
    {
        // Calculate the x and y coords

        //Get x offset into center of a square
        float boxX = transform.position.x  + (_squareSize / 2.0f);
        // Shift x into the correct square
        boxX += _squareSize * (boardLoc.X - 4.0f);

        //Get z offset into center of a square
        float boxZ = transform.position.z + (_squareSize / 2.0f);
        // Shift x into the correct square
        boxZ += _squareSize * (boardLoc.Y - 4.0f);


        //Return coords
        return new Vector3 (boxX, 2.1f, boxZ);

    }

    /// <summary>
    /// Returns the location of the gsquare the mouse is currently in <br/>
    /// 
    /// If the mouse is outside the board both Vector components will be -1
    /// <returns>The x and y positions of the square as indexes of the rows and columns</returns>
    internal Point MouseSquare()
    {
        // Shoot from camera to mouse
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        Physics.Raycast(r, out hitInfo);

        //  Get the world x and y of the mouse position
        float worldX = hitInfo.point.x;
        float worldZ = hitInfo.point.z;




        float xDiff = worldX - transform.position.x;

        // Divide by the size of a square
        xDiff /= _squareSize;

        //Add 3 to normalize positive / neagtive values
        xDiff += 3;
        int xDiffI = Mathf.CeilToInt(xDiff);

        //  Get the distance to the y location if the mouse from the GameController
        float yDiff = worldZ - transform.position.z;

        // Divide by the size of a square and round up 
        yDiff /= _squareSize;
        int yDiffI = Mathf.CeilToInt(yDiff);

        //Add 3 to normalize positive / neagtive values
        yDiffI += 3;

        // Return (-1, -1) if the mouse is off the board
        if(xDiffI < 0 || xDiffI > 7 || yDiffI < 0 || yDiffI > 7)
        {
            xDiffI = -1;
            yDiffI = -1;
        }

        // Return the square coords
        return new Point(xDiffI, yDiffI);
    }



    /// <summary>
    /// Handles placing the next available piece on the board
    /// </summary>
    /// <param name="boardLocation"><see cref="Point"/> with the location on the Board to place the piece</param>
    /// <param name="color">The Color this piece should be. (White or Black)</param>
    /// <returns> True if the piece was sucessfully placed; false otherwise</returns>
    private bool PlacePiece(Point boardLocation, SpotState color)
    {
        // Get world coords to place piece
        Vector3 coords = GetSquareCoords(boardLocation);

        // Move the piece
        // THIS WILL NEED TO BE UPDATED TO INCLUDE AN ANIMATION LATER

        // Get the piece and set its rotation / color
        GamePiece toPlay = _pieceList.Dequeue();
        toPlay.transform.rotation = Quaternion.Euler(0, 0, 0);

        //Set the piece in the board state array
        _board[boardLocation.X, boardLocation.Y] = toPlay;

        // Add piece to animation queue
        _animQueue.Enqueue(new AnimatedObject(toPlay, coords, color));

        //Return true because piece has been set up for being placed
        return true;
    }


    // Start is called before the first frame update
    void Start()
    {
        //Initialize the animation Queue
        _animQueue = new Queue<AnimatedObject>();

        //Initialize board
        _board = new GamePiece[8, 8];

        //Create starting board state
        _boardState = new ReversiBoard();


        //Align self to game board
        GameObject gb = GameObject.Find("GameBoard");

        transform.position = new Vector3(gb.transform.position.x, 5, gb.transform.position.z);

        // Set size of board squares

        // Get the size
        Vector2 gbSize = gb.GetComponent<Renderer>().bounds.size;

        // Set instance var
        _squareSize = (gbSize.x - 1) / 8.0f;


        // Add Pieces
        _pieceList = new Queue<GamePiece>();



        Vector3 shootPos = GameObject.Find("ShootMarker").transform.position;
        Vector3 shootPos2 = GameObject.Find("ShootMarker1").transform.position;
        // Create pieces 
        for (int i =0; i < 64; i+=2)
        {
            _pieceList.Enqueue(Instantiate(gamePiece, new Vector3(shootPos.x, 3 + i, shootPos.z), Quaternion.identity));
            _pieceList.Enqueue(Instantiate(gamePiece, new Vector3(shootPos2.x, 3 + i, shootPos2.z), Quaternion.identity));

        }

        //TODO -- Add wait for pieces to enter

        // Move first 4 pieces onto the board
        PlacePiece(new Point(3, 3), SpotState.BLACK);
        PlacePiece(new Point(3, 4),SpotState.WHITE);
        PlacePiece(new Point(4, 3), SpotState.WHITE);
        PlacePiece(new Point(4, 4), SpotState.BLACK);

        // Set the current player at the start
        _currentPlayer = GameObject.Find("PlayerController").GetComponent<PlayerController>();

        //Add second player
        //TODO: THIS IS CURRENTLY ONLY A BOT NEED TO IMPLEMENT OPTION TO ADD SECOND PLAYER
        _offPlayer = Instantiate(AIController).GetComponent<AIController>();

        //Subscribe to the Players
        PlayerController player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        player.Subscribe(this);
        _offPlayer.Subscribe(this);
    }

    // Update is called once per frame
    void Update()
    {
        // Wait for all pieces to be in place to start play
        // If currently animating
        if(_animQueue.Count > 0)
        {
            _currPlayerAnim = _currentPlayer;
            _currentPlayer = null;

            //Continue animating the top element in the queue
            AnimatedObject ob = _animQueue.Peek();

            // If on the upslope 
            if(ob.ToAnimate.transform.position.x < ob.ArcTop.x)
            {
                float xPos = ob.ToAnimate.transform.position.x + (ob.DistPerArc.x * Time.deltaTime);
                float yPos = 0.3f * (xPos * xPos);
                float zPos = ob.ToAnimate.transform.position.z + (ob.DistPerArc.z * Time.deltaTime);

                ob.ToAnimate.transform.position = new Vector3(xPos, yPos, zPos);
            }
            // If on the downslope
            else if(ob.ToAnimate.transform.position.x < ob.MovingTo.x)
            {
                float xPos = ob.ToAnimate.transform.position.x + (ob.DistPerArc.x * Time.deltaTime);
                float yPos = 0.3f * (-1 * (xPos * xPos)) + 3;
                float zPos = ob.ToAnimate.transform.position.z + (ob.DistPerArc.z * Time.deltaTime);

                ob.ToAnimate.transform.position = new Vector3(xPos, yPos, zPos);
            }
            // If directly above  the  spot
            else
            {
                // Set  rotation  and  switch to non-kinematic
                ob.ToAnimate.transform.rotation = Quaternion.Euler(0, 0, 0);
                ob.ToAnimate.GetComponent<GamePiece>().Color = ob.EndColor;

                ob.ToAnimate.GetComponent<Rigidbody>().isKinematic = false;

                //Remove  from anim queue
                _animQueue.Dequeue();
            }

            //If anim queue is empty return control to player 
            if(_animQueue.Count == 0)
            {
                _currentPlayer = _currPlayerAnim;
            }

        }
    }

    // As of right now these do nothing
    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    //Perform a signaled move
    public void OnNext(ReversiMove value)
    {
        // Place piece object on the on screen board
        PlacePiece(value.PlaceLoc, value.Color);

        //Make the move on the board simulating the game
        _boardState = ReversiPlay.MakeMove(value, _boardState);

        // Match board state to the games
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                // If this is a piece
                if (_boardState[i,j] != SpotState.EMPTY)
                {
                    // Match state of this square
                    //TODO -- ADD ANIMATION
                    _board[i, j].Color = _boardState[i, j];
                }
            }
        }

        // Swap the player
        PlayAgent tmp = _currentPlayer;
        _currentPlayer = _offPlayer;
        _offPlayer = tmp;
    }
}
