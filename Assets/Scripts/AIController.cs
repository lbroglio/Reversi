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
        
        // If its this AI's turn
        if(gc.CurrentPlayer == this)
        {
            //Choose the move to make
            ReversiMove toMake = ai.ChooseMove(gc.BoardState);

            NotifyMove(toMake);
        }

    }
}
