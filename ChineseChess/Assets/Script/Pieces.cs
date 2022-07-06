using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pieces : MonoBehaviour
{
    public enum typePiece
    {
        king,
        advisor,
        elephant,
        tiger,
        car,
        horse,
        soldier
    }
    [HideInInspector] 
    public typePiece type 
    {
        get;
        set;
    }
        
    private bool isRed;
    private bool atRiver;
    private Vector2 boardPos;

    public Pieces(typePiece t, bool isRed, int x, int y)
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
}
