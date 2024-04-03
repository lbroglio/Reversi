using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a Point in 2D space represented by two integer coordinates <br/>
/// Stores an integer X and Y component.
/// </summary>
public class Point 
{
    private int _x;
    private int _y;

    public int X
    {
        get { return _x; }
        set { _x = value; }
    }

    public int Y
    {
        get { return _y; }
        set { _y = value; }
    }

    public Point(int x, int y)
    {
        _x = x;
        _y = y;
    }

}
