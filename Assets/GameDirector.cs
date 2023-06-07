using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameDirector : MonoBehaviour
{
    public static GameDirector currentGame;

    private void Awake()
    {
        currentGame = this;
    }

    public void BaseStart()
    {
        
    }

    public void BaseUpdate()
    {
        
    }

    public virtual void AddClient(string client, NetMode mode)
    {

    }

    public virtual void PrepareClient(NetworkData NetData)
    {

    }
    public virtual void PrepareHost(NetworkData NetData)
    {

    }

    public abstract GameState GameState(NetworkData NetData);

    public abstract void SetGameState(GameState gameState);

    public abstract void SetGameState(NetworkData NetData);

    public abstract void ApplyGameState(NetworkData NetData);

    public abstract void RecoverGameState(string data);

    public abstract void ProcessControlInput(string player, string input);
}
