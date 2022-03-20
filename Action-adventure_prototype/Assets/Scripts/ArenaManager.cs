using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaManager : MonoBehaviour
{
    private List<Skeleton> _enemies = new List<Skeleton>();
    private List<Gate> _gates = new List<Gate>();
    public Transform Player;
    public bool IsBossArena;
    // Start is called before the first frame update
    void Awake()
    {
        Skeleton[] enemiesAtStart = GetComponentsInChildren<Skeleton>(true);
        foreach (var enemy in enemiesAtStart)
        {
            enemy.Arena = this;
            _enemies.Add(enemy);
        }

        Gate[] gatesAtStart = GetComponentsInChildren<Gate>(true);
        foreach (var gate in gatesAtStart)
        {
            _gates.Add(gate);
            gate.IsOpen = true;
        }
    }

    public void OnEnemyDestroyed(Skeleton enemy)
    {
        //remove enemy from list, until it reaches 0
        _enemies.Remove(enemy);
        if(_enemies.Count == 0)
        {
            OnArenaWon();
        }
    }

    private void OnArenaWon()
    {
        foreach (var gate in _gates)
        {
            gate.IsOpen = true;
        }

        if (IsBossArena)
        {
            SceneManager.LoadScene("End");
        }
    }

    private void OnArenaStart()
    {
        foreach (var gate in _gates)
        {
            gate.IsOpen = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Player = other.transform;
            OnArenaStart();
        }
    }

}
