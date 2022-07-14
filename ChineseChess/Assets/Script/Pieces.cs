using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pieces : MonoBehaviourPun, IPunObservable
{
    //public enum typePiece
    //{
    //    king,
    //    advisor,
    //    elephant,
    //    tiger,
    //    car,
    //    horse,
    //    soldier
    //}
    //[HideInInspector] 
    //public typePiece type 
    //{
    //    get;
    //    set;
    //}

    public string type;        
    public bool atRiver;
    public bool isRed;
    public Vector2 boardPos;

    //public Pieces(typePiece t, bool isRed, int x, int y)
    public Pieces(string t, bool isRed, int x, int y)
    {
        type = t;
        this.isRed = isRed;
        boardPos = new Vector2(x, y);
    }

    public void setIsRed(bool r)
    {
        isRed = r;
    }
    public bool GetRed()
    {
        return isRed;
    }

    public void SetBoardPosition(int x, int y)
    {
        boardPos = new Vector2(x, y);
    }

    public Vector2 GetBoardPosition()
    {
        return boardPos;
    }

    public void SetAtRiver(bool value)
    {
        atRiver = value;
    }

    public void SetType(string value)
    {
        type = value;
    }

    //Every time someone act on the online it is call
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            var color = GetRed();
            stream.SendNext(type);
            stream.SendNext(atRiver);
            stream.SendNext(color);
            stream.SendNext((int)GetBoardPosition().x);
            stream.SendNext((int)GetBoardPosition().y);
        }
        else if (stream.IsReading)
        {
            SetType(stream.ReceiveNext().ToString());
            SetAtRiver((bool)stream.ReceiveNext());
            setIsRed((bool)stream.ReceiveNext());
            var x = (int)stream.ReceiveNext();
            var y = (int)stream.ReceiveNext();
            SetBoardPosition(x, y);
        }
    }
}
