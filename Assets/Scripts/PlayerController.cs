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


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        // If the mouse has been clicked
        if(Input.GetMouseButtonDown(0)) { 
            // Check if its this player's turn
            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
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
                        NotifyMove(move);
                    }
                }
            }
        }

    }
}
