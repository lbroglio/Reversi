using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhantomPiece : GamePiece
{
    // Start is called before the first frame update
    void Start()
    {
        // Make piece opaque
        Color c = gameObject.GetComponent<Renderer>().material.color;
        gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(c.r, c.g, c.b, 0.5f));


    }

    // Update is called once per frame
    void Update()
    {


        GameController controller = GameObject.Find("GameController").GetComponent<GameController>();

        // If not  animating
        if(controller.CurrentPlayer != null )
        {
            // Get the mouse position 
            Point v = controller.MouseSquare();

            // Move to v
            transform.position = controller.GetSquareCoords(v);

            // Set to player color
            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();

            Color = gc.CurrentPlayer.Color;
        }



    }
}
