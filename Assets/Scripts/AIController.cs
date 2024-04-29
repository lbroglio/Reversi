using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : PlayAgent
{
    public int MaxDepth;

    ReversiAI ai;

    // Start is called before the first frame update
    void Start()
    {
        ai = new ReversiAI(MaxDepth, Color);
    }

    // Update is called once per frame
    void Update()
    {
        // Get the Game Controller
        GameController gc = GameObject.Find("GameController").GetComponent<GameController>();

        bool legalFound = false;
        // If its this AI's turn
        if (gc.CurrentPlayer == this)
        {
            ReversiBoard boardState = gc.BoardState;
            // Check if there is a legal move this AI can make
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // If any player can make a legal move at this position
                    ReversiMove m = new ReversiMove(new Point(i, j), Color);
                    ReversiMoveEvaluator e = new ReversiMoveEvaluator(boardState);
                    if (e.CheckMoveLegal(m))
                    {
                        legalFound = true;
                        i += 8;
                        j += 8;
                    }
                }
            }

            // If a legal move is found submit it
            if (legalFound)
            {
                Debug.Log("Choosing Move");
                //Choose the move to make
                ReversiMove toMake = ai.ChooseMove(gc.BoardState);
                Debug.Log("Move Chosen");
                NotifyMove(toMake);
            }
            // If there is no legal move send null
            else
            {
                NotifyMove(null);
            }

            

            Debug.Log("End If Block");
        }



    }
}
