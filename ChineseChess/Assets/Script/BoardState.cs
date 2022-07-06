using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardState : MonoBehaviourPun
{
    public Pieces[,] board = new Pieces[10, 9];


    public void Update()
    {
        Debug.Log("Ouais");
    }
}
