using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    private List<Skeleton> _enemies = new List<Skeleton>();
    public Transform Player;
    // Start is called before the first frame update
    void Awake()
    {
        Skeleton[] enemiesAtStart = GetComponentsInChildren<Skeleton>(true);
        Debug.Log(enemiesAtStart.Length);
        foreach (var enemy in enemiesAtStart)
        {
            enemy.Arena = this;
            _enemies.Add(enemy);
        }
    }

    void OnEnemyDestroyed(Skeleton enemy)
    {
        //remove enemy from list, until it reaches 0
        _enemies.Remove(enemy);
        if(_enemies.Count == 0)
        {
            OnArenaWon();
        }
    }

    void OnArenaWon()
    {
        
        //PlayAnimation to unlock Path ?
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Player = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            Player = null;
        }
    }
}
