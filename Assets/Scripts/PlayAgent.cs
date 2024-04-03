using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An agent either a human or computer player which takes a turn in the game
/// </summary>
public abstract class PlayAgent : MonoBehaviour, IObservable<ReversiMove>
{
    // The id the next started instance of this class will take
    private static int nextId = 0;

    // The ID of this specific instance of a PlayAgent
    private int id;

    /// <summary>
    /// Keeps track of the object observing this PlayAgent
    /// </summary>
    private List<IObserver<ReversiMove>> _observers;

    /// <summary>
    /// The Color associated with this PlayAgent
    /// </summary>
    public SpotState Color;


    protected virtual void Awake()
    {
        // Claim an ID
        id = nextId;
        nextId++;

        // Create observers list
        _observers = new List<IObserver<ReversiMove>>();
    }


    // Update is called once per frame
    void Update()
    {

    }

    public override bool Equals(object other)
    {
        Debug.Log("EQUALS");
        // Check type
        if (typeof(PlayAgent).Equals(other))
        {
            // Check if ID's are equivalent
            PlayAgent agent = (PlayAgent)other;
            if (id == agent.id)
            {
                return true;
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Calling the function (as opposed to just returning the id) is almost certainly unecessary.
        return id.GetHashCode();
    }

    public void NotifyMove(ReversiMove move)
    {
        foreach(IObserver<ReversiMove> observer in _observers)
        {
            observer.OnNext(move);
        }
    }

    public IDisposable Subscribe(IObserver<ReversiMove> observer)
    {
        _observers.Add(observer);
        return null;
    }
}

