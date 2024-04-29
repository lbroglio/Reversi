using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : PlayAgent
{
    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public static bool operator==(PlayerController left, PlayerController right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PlayerController left, PlayerController right)
    {
        return !left.Equals(right);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }


    private bool _legalFound = false;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        GameController gc = GameObject.Find("GameController").GetComponent<GameController>();

        // If its this players turn and a legal move hasnt already been confirmed
        if (gc.CurrentPlayer == this && !_legalFound)
        {
            ReversiBoard boardState = gc.BoardState;
            // Check if there is a legal move this player can make
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // If any player can make a legal move at this position
                    ReversiMove m = new ReversiMove(new Point(i, j), Color);
                    ReversiMoveEvaluator e = new ReversiMoveEvaluator(boardState);
                    if (e.CheckMoveLegal(m))
                    {
                        _legalFound = true;
                        i += 8;
                        j += 8; 
                    }
                }
            }

            // If there is no legal move pass
            if (!_legalFound)
            {
                NotifyMove(null); 
            }
        }



        // If the mouse has been clicked
        if (Input.GetMouseButtonDown(0)) { 
            // Check if its this player's turn
            if(gc.CurrentPlayer == this)
            {
      
                //Get square to place the piece
                Point placeLoc = gc.MouseSquare();

                //If mouse isn;t out of bounds
                if(placeLoc.X != -1)
                {
                    //Define the move to make
                    ReversiMove move = new ReversiMove(placeLoc, Color);

                    //Check if move is legal
                    bool isLegal = (new ReversiMoveEvaluator(gc.BoardState)).CheckMoveLegal(move);

                    //Only place if move is legal
                    if ((isLegal))
                    {
                        _legalFound = false;
                        NotifyMove(move);
                    }
                }
            }
        }

    }
}
