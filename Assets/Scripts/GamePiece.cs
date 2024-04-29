using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{

    /// <summary>
    /// The number of game piesces which have fallen into place
    /// </summary>
    public static int PiecesInPlace = 0;

    //Location  of this GamePiece of the board (-1, -1) if offboard
    private Vector2 _postion;

    //Track whether this is in its initial fall state
    public bool _initalFall = true;

    /// <summary>
    /// The current side this piece is showing
    /// </summary>
    private SpotState _color;

    //Location  of this GamePiece of the board (-1, -1) if offboard
    public Vector2 Position
    {
        get { return _postion; }
    }

    public SpotState Color
    {
        get { return _color; }
        set { 
            // If the color is changing
            if(_color != value)
            {
                // Flip the piece over the z access
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 180 + transform.rotation.eulerAngles.z);
            }
            // Set the value of color
            _color = value; 
        }
    }

    /// <summary>
    /// If the piece is not currently  the given color animate flipping to it
    /// </summary>
    /// <param name="color">The color to flip to. Nothing will happens if this is the current color</param>
    public void FlipToColor(SpotState color)
    {
        if(_color != color)
        {
            _color = color;
            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
            gc.AddToAnim(this);
        }
    }


    void Awake()
    {
        _postion = new Vector2(-1, -1);
        _color = SpotState.WHITE;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


    }

   
    private void OnCollisionEnter(Collision collision)
    {
        // Switch to kinetic after falling
        if (_initalFall)
        {
            if(PiecesInPlace == 0)
            {
                GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
                gc.RestingPlace = gameObject.transform.position;
            }

            GetComponent<Rigidbody>().isKinematic = true;
            PiecesInPlace++;
            _initalFall=false;
        }
    }
}
