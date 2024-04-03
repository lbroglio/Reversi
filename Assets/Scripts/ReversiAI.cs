using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds methods used for having an AI play reversi
/// </summary>
public class ReversiAI 
{
    // How many moves ahead this AI can look. Affects difficulty 
    private int _maxDepth;

    //The color of player of the AI's pieces
    private SpotState _color;

    /// <summary>
    /// Assigns a numeric value for how favorable a given board is for the AI
    /// </summary>
    /// <param name="board">The board to evaluate the value of</param>
    /// <returns></returns>
    private int EvaluateBoard(ReversiBoard board)
    {
        // Set up arrays for keeping track of pieces for each color
        // For each array index 0 = pieces in the normal spaces, 1 = pieces on an edge, 2 = pieces on a corner
        int[] aiPieces = {0, 0, 0};
        int[] oppPieces = {0, 0, 0};
        // Track the number of empty spaces
        int emptySpaces = 0;


        //Count the pieces in every square on the board
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                SpotState curr = board[i, j];

                // If the space is empty
                if(curr == SpotState.EMPTY)
                {
                    emptySpaces++;
                }
                // If this is a friendly piece
                else if(curr == _color)
                {
                    // If this piece is on an edge
                    if(i==0 || i==7 || j==0|| j == 7)
                    {
                        // If this piece is in a corner
                        if ((i == 0 && j == 0) || (i == 0 && j == 7) || (i==7 && j==0) || (i==7 && j==7))
                        {
                            // Increment the corner pieces count
                            aiPieces[2]++;
                        }
                        // Increment the edge pieces count
                        aiPieces[1]++;
                    }
                    //Increment the standard pieces count
                    aiPieces[0]++;
                }
                // If this is an  opponent piece
                else
                {
                    // If this piece is on an edge
                    if (i == 0 || i == 7 || j == 0 || j == 7)
                    {
                        // If this piece is in a corner
                        if ((i == 0 && j == 0) || (i == 0 && j == 7) || (i == 7 && j == 0) || (i == 7 && j == 7))
                        {
                            // Increment the corner pieces count
                            oppPieces[2]++;
                        }
                        // Increment the edge pieces count
                        oppPieces[1]++;
                    }
                    //Increment the standard pieces count
                    oppPieces[0]++;
                }
            }

        }

        // If there are no empty spaces calculate a winner
        if(emptySpaces == 0)
        {
            int aiPiecesTotal = aiPieces[0] + aiPieces[1] + aiPieces[2];
            int oppPiecesTotal = oppPieces[0] + oppPieces[1] + oppPieces[2];

            // If the AI wins return INT_MAX as this in a winning board
            if(aiPiecesTotal > oppPiecesTotal)
            {
                return int.MaxValue;
            }
            // If this is a tie return 0
            else if(aiPiecesTotal == oppPiecesTotal)
            {
                //TODO -- Review this might want a lower value
                return 0;
            }
            // IF this is a loss retirn INT_MIN
            else
            {
                return int.MinValue;
            }
        }

        // Calculate the difference between friendly and opponent pieces; weighting edges and corners heavier
        int boardVal = (aiPieces[0] + (5 * aiPieces[1]) + (20 * aiPieces[2])) - (oppPieces[0] + (5 * oppPieces[1]) + (20 * oppPieces[2]));
        return boardVal;
    }

    /// <summary>
    /// Recursive function for performing the MiniMax algorithm
    /// </summary>
    /// <param name="board">The board to choose a move on</param>
    /// <param name="currLayer">How many moves ahead the move being chosen is</param>
    /// <param name="color">The color that is making the next move</param>
    private (ReversiMove, int) MiniMax(ReversiBoard board, int currLayer, SpotState color)
    {
        // Track whether this is a max or min levrl
        bool isMax = true;
        if(color != _color)
        {
            isMax = false;
        }

        // Set the opposite sides color
        SpotState oppColor;
        if(_color == SpotState.WHITE)
        {
            oppColor = SpotState.BLACK;
        }
        else
        {
            oppColor = SpotState.WHITE;
        }


        // Make a list of all possible moves by checking if placing in each square is legal and including it if it is
        // Each move is stored in a tuple assoocaited with the value of its board
        List<(ReversiMove, int)> moves = new List<(ReversiMove, int)>();
        for(int i = 0; i < 8; i++)
        {
            for(int j =0; j < 8; j++)
            {
                ReversiMove tmp = new ReversiMove(new Point(i, j), color);
                ReversiMoveEvaluator eval = new ReversiMoveEvaluator(board);
                // If the move is legal calculate its value and add it
                if (eval.CheckMoveLegal(tmp))
                {
                    // Make a copy of the board for moving on
                    ReversiBoard copy = board.Clone();

                    //Evaluate the move
                    int moveVal = EvaluateBoard(ReversiPlay.MakeMove(tmp, copy));

                    // Save the move and its value together
                    moves.Add((tmp, moveVal));

                }
            }
        }


        // Setup var to track the max or min value and its index
        ReversiMove trackedMove = null;
        int trackedVal = 0;
        if (isMax)
        {
            trackedVal = int.MinValue;
        }
        else
        {
            trackedVal = int.MaxValue;
        }

        // If this is at the maximum lookahead return the  min / max board
        if (currLayer == _maxDepth)
        {
            foreach(var pair in moves)
            {
                // If getting max and this is bigger
                if(isMax && trackedVal < pair.Item2)
                {
                    trackedVal = pair.Item2;
                    trackedMove = pair.Item1;
                }
                // If getting min andf this is smaller
                else if(!isMax && trackedVal > pair.Item2)
                {
                    trackedVal = pair.Item2;
                    trackedMove = pair.Item1;
                }
            }
        }
        // If this isn't the maximum lookeahead recursively call for all moves
        else
        {
            // For every move
            foreach (var pair in moves)
            {
                // If this move is a terminal position don't recurse 
                if(pair.Item2 == int.MaxValue || pair.Item2 == int.MinValue)
                {
                    // Track this move is necessary 

                    // If this a max layer and the tracked value is bigger
                    if(isMax  && trackedVal < pair.Item2)
                    {
                        trackedVal = pair.Item2;
                        trackedMove = pair.Item1;
                    }
                    // If this is a min layer and the tracked value is smaller
                    // If getting min andf this is smaller
                    else if (!isMax && trackedVal > pair.Item2)
                    {
                        trackedVal = pair.Item2;
                        trackedMove = pair.Item1;
                    }
                }

                // Copy the board
                ReversiBoard cpy = board.Clone();
                // Play this move on the board
                cpy = ReversiPlay.MakeMove(pair.Item1, cpy);

                //Recruse differently if this level is currently min or max

                if (isMax)
                {
                    (ReversiMove, int) recVal = MiniMax(cpy, currLayer + 1, oppColor);

                    // If this move beats the current max track it 
                    if(trackedVal < recVal.Item2)
                    {
                        trackedVal = recVal.Item2;
                        trackedMove = pair.Item1;
                    }

                }
                else
                {
                    (ReversiMove, int) recVal = MiniMax(cpy, currLayer + 1, _color);

                    // If this move is less than the current min track it
                    if (trackedVal > recVal.Item2)
                    {
                        trackedVal = recVal.Item2;
                        trackedMove = pair.Item1;
                    }

                }

            }
        }

        //Return the tracked move and values
        return (trackedMove, trackedVal);




    }

    public ReversiMove ChooseMove(ReversiBoard board)
    {
        // Call MiniMax algorithm
        return MiniMax(board, 0, _color).Item1;
    }

    public ReversiAI(int maxDepth, SpotState color)
    {
        _maxDepth = maxDepth;
        _color = color;
    }   


}
