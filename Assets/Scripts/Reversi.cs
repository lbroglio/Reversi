using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Stores the three possible states a spot on a ReversiBoard can be in <br/>
/// Occupied by a white piece <br/>
/// Occupied by a black piece <br/>
/// Empty
/// </summary>
public enum SpotState
{
    WHITE = 0,
    BLACK = 1,
    EMPTY = 2
}

/// <summary>
/// Stores a representation of a Reversi Board <br/>
/// Starts compeltely empty starting pieces must be placed by User.
/// </summary>
public class ReversiBoard 
{

    public SpotState this[int row, int col]
    {
        get { return _board[row, col]; }
        set { _board[row, col] = value; }
    }

    private SpotState[,] _board;

    public ReversiBoard() {
        _board = new SpotState[8, 8];

        // Set Board to starting state
        
        //Start by setting all spots to empty
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                _board[i, j] = SpotState.EMPTY;
            }
        }

        //Add startting pieces
        _board[3, 3] =  SpotState.BLACK;
        _board[3, 4] = SpotState.WHITE;
        _board[4, 3] = SpotState.WHITE;
        _board[4, 4] = SpotState.BLACK;
    }


    public ReversiBoard Clone()
    {
        ReversiBoard newBoard = new ReversiBoard();
        
        for(int i=0; i<8; i++)
        {
            for(int j=0; j<8; j++)
            {
                newBoard._board[i, j] = _board[i, j];
            }
        }

        return newBoard;
    }

    /// <summary>
    /// Print a text representation of the board
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        String toPrint = "";

        // List to map Enum to letter
        String[] printEnum = { "W", "B", " " };

        for(int i =0; i < 8; i++)
        {
            for(int j =0; j < 8; j++)
            {
                toPrint += printEnum[(int)_board[j, i]];
                toPrint += " ";
            }
            toPrint += "\n";
        }

        return toPrint;
    }
}

/// <summary>
/// Stores a representation of a move made on a Reversi board by adding a piece
/// </summary>
public class ReversiMove
{
    /// <summary>
    /// Where on the board to place the piece
    /// </summary>
    private Point _placeLoc;

    /// <summary>
    /// What color (Player) the added piece will be
    /// </summary>
    private SpotState _color;

    /// <summary>
    /// Where on the board to place the piece
    /// </summary>
    public Point PlaceLoc {  get { return _placeLoc; } set {  _placeLoc = value; } }

    /// <summary>
    /// What color (Player) the added piece will be
    /// </summary>
    public SpotState Color { get { return _color; } set { _color = value; } }

    /// <summary>
    /// Create a new move. Represents adding a piece of the given color at the given location
    /// </summary>
    /// <param name="placeLoc">Where on the board to add the piece as an index from the bottom right corner</param>
    /// <param name="color">What color the new piece is</param>
    public ReversiMove(Point placeLoc, SpotState color)
    {
        _placeLoc = placeLoc;
        _color = color;
    }
}

/// <summary>
/// Object which evalautes ReversiMoves to see if their legal under the rules.<br/>
/// Tracks a conistent board so the object can evaluate moves in succession
/// </summary>
public class ReversiMoveEvaluator
{
    /// <summary>
    /// The current Board this MoveEvaluator is checking moves for
    /// </summary>
    private ReversiBoard _currentBoard;

    /// <summary>
    /// The current Board this MoveEvaluator is checking moves for
    /// </summary>
    public ReversiBoard CurrentBoard {  
        get { return _currentBoard; } 
        set {
            // Create a new Board for the current one
            _currentBoard = new ReversiBoard();
            
            // Copy the given backing array of the given Board into the newly created one
            for(int i = 0;i < 8; i++)
            {
                for(int j=0; j < 8; j++)
                {
                    _currentBoard[i, j] = value[i, j];
                }
            }
         } 
    }

    /// <summary>
    /// Create a new MoveEvaluator with a given board set for checking moves against
    /// </summary>
    /// <param name="currentBoard">The ReversiBoard to check moves on</param>
    public ReversiMoveEvaluator( ReversiBoard currentBoard) {  CurrentBoard = currentBoard; }

    /// <summary>
    /// Create a Move Evaluator with no starting board
    /// </summary>
    public ReversiMoveEvaluator() { 
        _currentBoard = null;
    }

    /// <summary>
    /// Check if a move is legal on the currently saved board.
    /// If the move is legal it will be applied to the saved Board
    /// </summary>
    /// <param name="move">The move to evaluate</param>
    /// <returns>
    /// true -- The move can be legally played on the saved board <br/>
    /// false -- The move cannot be legally made
    /// </returns> 
    public bool CheckMoveLegal(ReversiMove move)
    {
        // If no board has been given throw and exception
        if(_currentBoard == null)
        {
            throw new InvalidOperationException("This ReversiMoveEvaluator must be provided with a Board to evaluate moves");
        }

        // Check if there is already a piece at the given space
        if (_currentBoard[move.PlaceLoc.X, move.PlaceLoc.Y] != SpotState.EMPTY)
        {
            return false;
        }

        //Check if each direction to see if a capture is made

        //Iterate through every possible direction the can be traveled int
        for(int xDirec = -1; xDirec < 2; xDirec++)
        {


            for(int yDirec = -1; yDirec < 2; yDirec++)
            {
                // Flag to track if an oppoents piece is encounterd
                bool oppPiece = false;
                // Skip this iteration if both are zero
                if (xDirec == 0 && yDirec == 0)
                {
                    continue;
                } 

                // Advance from the place location until the edge is reached
                Point loc = new Point(move.PlaceLoc.X + xDirec,move.PlaceLoc.Y + yDirec);

                while (loc.X > -1 && loc.X < 8 && loc.Y > -1 && loc.Y < 8)
                {
                    // Get the state of the current piece
                    SpotState placeState = _currentBoard[loc.X, loc.Y];


                    bool shouldBreak = false;
                    switch (placeState)
                    {
                        case SpotState.EMPTY:
                            // If the place is empty no capture can be made here so break out of while loop
                            // Sets the flag to break while loop
                            shouldBreak = true;
                            break;
                        case SpotState.WHITE:
                            //If this is matching the played piece return legal (and make move) if an oppoenets piece has been encountered
                            //and break out if not.
                            // If this is the opposite color mark it as encountered
                            if (move.Color == SpotState.WHITE)
                            {
                                if (oppPiece)
                                {
                                    _currentBoard = ReversiPlay.MakeMove(move, _currentBoard);
                                    return true;
                                }
                                shouldBreak = true;
                            }
                            else
                            {
                                oppPiece = true;
                            }
                            break;
                        case SpotState.BLACK:
                            //If this is matching the played piece return legal (and make move) if an oppoenets piece has been encountered
                            //and break out if not.
                            // If this is the opposite color mark it as encountered
                            if (move.Color == SpotState.BLACK)
                            {
                                if (oppPiece)
                                {
                                    _currentBoard = ReversiPlay.MakeMove(move, _currentBoard);
                                    return true;
                                }
                                shouldBreak = true;
                            }
                            else
                            {
                                oppPiece = true;
                            }
                            break;
                    }
                    // If the flag was set to break out of the while loop
                    if (shouldBreak)
                    {
                        break;
                    }

                    //Advance to next square
                    loc.X += xDirec;
                    loc.Y += yDirec;

                }
            }
        }
        // If this point is reached move is illegal
        return false;
    }


    /// <summary>
    /// Check if a move is legal on a newly provided Board. The given Board will be saved in this Evaluator
    /// If the move is legal it will be applied to the saved Board
    /// </summary>
    /// <param name="move">The move to evaluate</param>
    /// <param name="newBoard">The Board to check the move on</param>
    /// <returns>
    /// true -- The move can be legally played on the saved board <br/>
    /// false -- The move cannot be legally made
    /// </returns> 
    public bool CheckMoveLegal(ReversiMove move, ReversiBoard newBoard)
    {
        CurrentBoard = newBoard;
        return CheckMoveLegal(move);
    }
}

/// <summary>
/// Holds methods used for simulating play of reversi
/// </summary>
public static class ReversiPlay
{
    /// <summary>
    /// Makes the given move on the given ReversiBoard. <br/>
    /// The given move must be legal and behavior for illegal moves is undefined.
    /// </summary>
    /// <param name="move">The move to make</param>
    /// <param name="playOn">The board to play the move on</param>
    /// <returns>The given Board with the effects on the given move applied</returns>
    public static ReversiBoard MakeMove(ReversiMove move, ReversiBoard playOn)
    {
        //Add the new piece to the board
        playOn[move.PlaceLoc.X, move.PlaceLoc.Y] = move.Color;

        // Set the player and oppoent colors
        SpotState playerCol = move.Color;
        SpotState oppCol;
        if(playerCol == SpotState.WHITE)
        {
            oppCol = SpotState.BLACK;
        }
        else
        {
            oppCol = SpotState.WHITE;
        }

        //Make necessary captures in every direction
        for (int xDirec = -1; xDirec < 2; xDirec++)
        {
            

            for (int yDirec = -1; yDirec < 2; yDirec++)
            {
                // Tracks pieces to flip by their locations
                Queue<Point> toFlip = new Queue<Point>();

                // If both elements are zero skip this loop
                if (xDirec == 0 && yDirec == 0)
                {
                    continue;
                }

                //Move along the board in the current direction until the end is reached
                Point loc = new Point(move.PlaceLoc.X + xDirec, move.PlaceLoc.Y + yDirec);
                while (loc.X > -1 && loc.X < 8 && loc.Y > -1 && loc.Y < 8)
                {
                    // Get the state of the current spot
                    SpotState curr = playOn[loc.X, loc.Y];

                    // Choose move based on state

                    // If this square is one of the players pieces
                    if(curr == playerCol)
                    {
                        // Make captures and break 
                        while(toFlip.Count > 0)
                        {
                         
                            Point toCap = toFlip.Dequeue();
                            playOn[toCap.X, toCap.Y] = playerCol;
                        }
                        break;
                    }
                    // If this is one the players pieces mark it for capture if a players piece is found
                    else if(curr == oppCol)
                    {
                        toFlip.Enqueue(new Point(loc.X, loc.Y));
                    }
                    // Else if this space is empty no captures will be made so break
                    else
                    {
                        break;
                    }

                    //Advance to next space
                    loc.X += xDirec;
                    loc.Y += yDirec;
                }
            }
        }

        return playOn;
    }
}