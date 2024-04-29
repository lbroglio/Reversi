using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController : MonoBehaviour, IObserver<ReversiMove>
{
    /// <summary>
    /// Holds a GameObject being animated and associated information
    /// </summary>
    struct AnimatedObject
    {

        public enum AnimType
        {
            MOVE,
            FLIP
        }

        public MonoBehaviour ToAnimate;
        public SpotState EndColor;
        public Vector3 MovingTo;
        public Vector3 Step;
        public AnimType Type;
        public Quaternion CachedRot;
        public float TotalRot;

        public AnimatedObject(MonoBehaviour g, Vector3 p, SpotState c,  AnimType t)
        {
            ToAnimate = g;
            MovingTo = p;

            Vector3 dist = p - ToAnimate.transform.position;

            EndColor = c;

            Step = new Vector3(dist.x * 2.5f, 0, dist.z * 2.5f);

            Type = t;

            CachedRot = Quaternion.Euler(g.transform.rotation.eulerAngles.x, g.transform.rotation.eulerAngles.y,g.transform.rotation.eulerAngles.z);

            TotalRot = 0;
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

    /// <summary>
    /// Set to true when the game ends
    /// </summary>
    private bool gameOver = false;

    /// <summary>
    /// Holds the winner of the game. Empty means the game is tied
    /// </summary>
    private SpotState _gameWinner;

    /// <summary>
    /// The object currently being animated
    /// </summary>
    private AnimatedObject _beingAnimated;

    /// <summary>
    /// Vector where the piece at the bottom is located
    /// </summary>
    public Vector3 RestingPlace;

    private bool needNewAnim = true;

    /// <summary>
    /// Used to track when a pass message should be ended
    /// </summary>
    private bool _passFlag = false;

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
        _currPlayerAnim = _currentPlayer;
        _currentPlayer = null;
        _animQueue.Enqueue(new AnimatedObject(toPlay, coords, color, AnimatedObject.AnimType.MOVE));

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



        Vector3 spawnPos = GameObject.Find("ShootMarker").transform.position;
        // Create pieces 
        for (int i =0; i < 130; i+=2)
        {
            _pieceList.Enqueue(Instantiate(gamePiece, new Vector3(spawnPos.x, 6 + i, spawnPos.z), Quaternion.identity));

        }

        //TODO -- Add wait for pieces to enter

        // Move first 4 pieces onto the board
        PlacePiece(new Point(3, 3), SpotState.BLACK);
        PlacePiece(new Point(3, 4),SpotState.WHITE);
        PlacePiece(new Point(4, 3), SpotState.WHITE);
        PlacePiece(new Point(4, 4), SpotState.BLACK);

        // Set the current player at the start
        _currentPlayer = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        _currPlayerAnim = CurrentPlayer;

        //Add second player
        //TODO: THIS IS CURRENTLY ONLY A BOT NEED TO IMPLEMENT OPTION TO ADD SECOND PLAYER
        _offPlayer = Instantiate(AIController).GetComponent<AIController>();

        //TODO ADD USER CONFIGURATION
        _offPlayer.GetComponent<AIController>().MaxDepth = Config.AIDifficult;

        //Subscribe to the Players
        PlayerController player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        player.Subscribe(this);
        _offPlayer.Subscribe(this);

        if(Config.PlayerColor == SpotState.WHITE)
        {
            _currentPlayer.Color = SpotState.WHITE; 
            _offPlayer.Color = SpotState.BLACK;
        }
        else
        {
            PlayAgent tmp = _currentPlayer;
            tmp.Color = SpotState.BLACK;

            _currentPlayer = _offPlayer;
            _offPlayer.Color = SpotState.WHITE;

            _offPlayer = tmp;
            _currPlayerAnim = CurrentPlayer;

        }
    }

    // Update is called once per frame
    void Update()
    {
        //Wait for all of the game pieces to be ready
        if (GamePiece.PiecesInPlace < 65)
        {
            _currentPlayer = null;
            return;
        }



        // Setup
        // Wait for all pieces to be in place to start play
        // If currently animating
        if (_animQueue.Count > 0 || needNewAnim == false)
        {
            //Continue animating the top element in the queue
            if(needNewAnim)
            {
                _beingAnimated = _animQueue.Dequeue();
                needNewAnim = false;
            }

            // If ob is a moving type animation
            if (_beingAnimated.Type == AnimatedObject.AnimType.MOVE)
            {
                Vector3 dist = _beingAnimated.MovingTo - _beingAnimated.ToAnimate.transform.position;
                // If still moving to the spot
                if ((Math.Abs(dist.x) > 0.075 && Math.Abs(dist.z) > 0.075) && /*Fallback if piece isn't stopped*/ Math.Abs(dist.x) < 15)
                {
                    // Calculate step for each
                    float xStep = _beingAnimated.Step.x * Time.deltaTime;
                    float zStep = _beingAnimated.Step.z * Time.deltaTime;

                    // Advance
                    float xPos = _beingAnimated.ToAnimate.transform.position.x + xStep;
                    float yPos = _beingAnimated.ToAnimate.transform.position.y;
                    float zPos = _beingAnimated.ToAnimate.transform.position.z + zStep;

                    _beingAnimated.ToAnimate.transform.position = new Vector3(xPos, yPos, zPos);
                }
                // If the spot is reached
                else
                {
                    // Set  rotation  and  switch to non-kinematic

                    // Snap to position
                    _beingAnimated.ToAnimate.transform.position = new Vector3(_beingAnimated.MovingTo.x, _beingAnimated.ToAnimate.transform.position.y, _beingAnimated.MovingTo.z);
                    _beingAnimated.ToAnimate.GetComponent<GamePiece>().Color = _beingAnimated.EndColor;

                    _beingAnimated.ToAnimate.GetComponent<Rigidbody>().isKinematic = false;

                }

                // Dequeue when at the board
                if (_beingAnimated.ToAnimate.transform.position.y <= 2.1)
                {
                    // Reset being animated 
                    needNewAnim = true;
                }
            }
            // If this is a flip type animation
            else
            {
                float amtRotated = _beingAnimated.ToAnimate.transform.rotation.eulerAngles.z - _beingAnimated.CachedRot.eulerAngles.z;

                //Set y shift based on whether over or below 90 degress of rotation 
                float yShift = 0.5f * 2 * Time.deltaTime;

                if(_beingAnimated.TotalRot >= 90)
                {
                    yShift *= -1;
                }


                //Move piece
                float xPos = _beingAnimated.ToAnimate.transform.position.x;
                float yPos = _beingAnimated.ToAnimate.transform.position.y + yShift;
                float zPos = _beingAnimated.ToAnimate.transform.position.z;

                _beingAnimated.ToAnimate.transform.position = new Vector3(xPos, yPos, zPos);

                //Rotate piece
                float amtToRotate = (180 * 2 * Time.deltaTime);
                
                //Rotation values
                float xRot = _beingAnimated.ToAnimate.transform.rotation.eulerAngles.x;
                float yRot = _beingAnimated.ToAnimate.transform.rotation.eulerAngles.y;
                float zRot = _beingAnimated.ToAnimate.transform.rotation.eulerAngles.z;

                _beingAnimated.ToAnimate.transform.rotation = Quaternion.Euler(xRot, yRot, zRot + amtToRotate);
                _beingAnimated.TotalRot += amtToRotate;
                // If this piece has been rotated 
                if (_beingAnimated.TotalRot >= 180)
                {
                   // Snap rotation and position
                   _beingAnimated.ToAnimate.transform.rotation = Quaternion.Euler(new Vector3(_beingAnimated.CachedRot.eulerAngles.x, _beingAnimated.CachedRot.eulerAngles.y, _beingAnimated.CachedRot.eulerAngles.z + 180));
                   _beingAnimated.ToAnimate.transform.position = new Vector3(_beingAnimated.ToAnimate.transform.position.x, 2.1f, _beingAnimated.ToAnimate.transform.position.z);
                   _beingAnimated.ToAnimate.GetComponent<Rigidbody>().isKinematic = false;

                    // Flag to Reset being animated 
                    needNewAnim = true;
                }
            }


            //If anim queue is empty return control to player 
            if (_animQueue.Count == 0 && needNewAnim)
            {
                _currentPlayer = _currPlayerAnim;
                _currPlayerAnim = null;
            }

        }

        // If the game is won
        if (gameOver)
        {
            // Display end text
            GameObject winText = GameObject.Find("WinnerDisplay");
            //Set message based on outcome
            if (_gameWinner == SpotState.WHITE)
            {
                winText.GetComponent<TextMeshPro>().text = "White Wins!";
            }
            else if (_gameWinner == SpotState.BLACK)
            {
                winText.GetComponent<TextMeshPro>().text = "Black Wins!";

            }
            else
            {
                winText.GetComponent<TextMeshPro>().text = "Tie Game!";

            }
            winText.GetComponent<MeshRenderer>().enabled = true;

            System.Random rand  = new System.Random();
            // Flip a piece at random
            int ran1 = rand.Next(7);
            int ran2 = rand.Next(7);

            if (_boardState[ran1, ran2] != SpotState.EMPTY)
            {
                AddToAnim(_board[ran1, ran2]);
            }


        }
    }

    /// <summary>
    /// Drop all of the pieces still in the dispenser
    /// </summary>
    private void dropPiece()
    {
        _pieceList.Peek().transform.position = RestingPlace;
    }

    /// <summary>
    /// When called a game piece will be added to the animation queue for rotation
    /// </summary>
    /// <param name="gp">The GamePiece to add</param>
    public void AddToAnim(GamePiece gp)
    {
        gp.GetComponent<Rigidbody>().isKinematic = true;
        AnimatedObject ao = new AnimatedObject(gp, Vector3.zero, SpotState.EMPTY, AnimatedObject.AnimType.FLIP);
        _animQueue.Enqueue(ao);
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
        // Undo pass if necessary
        if(_passFlag)
        {
            GameObject player = GameObject.Find("PlayerController");
            if(_currentPlayer == player.GetComponent<PlayerController>() && value != null)
            {
                GameObject text = GameObject.Find("WinnerDisplay");
                text.GetComponent<MeshRenderer>().enabled = false;
                _passFlag = false;

            }

        }


        // Swap the player
        PlayAgent tmp = _currentPlayer;
        _currentPlayer = _offPlayer;
        _offPlayer = tmp;


        // If this move is a pass
        if(value == null)
        {
            Debug.Log("Move Passed");
            // Notify user of pass
            GameObject text = GameObject.Find("WinnerDisplay");

            if(_offPlayer.Color == SpotState.WHITE)
            {
                text.GetComponent<TextMeshPro>().text = "White Passes";

            }
            else
            {
                text.GetComponent<TextMeshPro>().text = "Black Passes";

            }

            text.GetComponent<MeshRenderer>().enabled = true;


            // End function
            _passFlag = true;
        }
        else
        {
            // Place piece object on the on screen board
            PlacePiece(value.PlaceLoc, value.Color);

            dropPiece();

            //Make the move on the board simulating the game
            _boardState = ReversiPlay.MakeMove(value, _boardState);

            // Match board state to the games
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // If this is a piece
                    if (_boardState[i, j] != SpotState.EMPTY)
                    {
                        // If this isn't the piece being added
                        if (i != value.PlaceLoc.X || j != value.PlaceLoc.Y)
                        {
                            // Match state of this square
                            _board[i, j].FlipToColor(_boardState[i, j]);
                        }
                    }
                }
            }
        }




        //Check if a legal move exists in the new state
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                // If any player can make a legal move at this position
                ReversiMove m1 = new ReversiMove(new Point(i, j), SpotState.WHITE);
                ReversiMove m2 = new ReversiMove(new Point(i, j), SpotState.BLACK);
                ReversiMoveEvaluator e1 = new ReversiMoveEvaluator(BoardState);
                ReversiMoveEvaluator e2 = new ReversiMoveEvaluator(BoardState);
                if (e1.CheckMoveLegal(m1) || e2.CheckMoveLegal(m2)) 
                {
                    return;
                }
            }
        }

        Debug.Log("GAME OVER");
        // Evaluate winner
        int whitePieces = 0;
        int blackPieces = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (BoardState[i, j] == SpotState.WHITE)
                {
                    whitePieces++;
                }
                else if (BoardState[i, j] == SpotState.BLACK)
                {
                    blackPieces++;
                }

            }
        }

        //Set winner vals
        gameOver = true;
        if (whitePieces > blackPieces)
        {
            _gameWinner = SpotState.WHITE;
        }
        else if (blackPieces > whitePieces)
        {
            _gameWinner = SpotState.BLACK;
        }
        else
        {
            _gameWinner = SpotState.EMPTY;
        }


    }
}
