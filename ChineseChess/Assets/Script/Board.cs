using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Board : MonoBehaviourPun, IPunObservable
{
    public static Board Instance { get; set; }

    public Pieces[,] pieces = new Pieces[10, 9];

    //Red
    public GameObject redCar;
    public GameObject redHorse;
    public GameObject redElephant;
    public GameObject redAdvisor;
    public GameObject redKing;
    public GameObject redTiger;
    public GameObject redSoldier;

    //Black
    public GameObject blackCar;
    public GameObject blackHorse;
    public GameObject blackElephant;
    public GameObject blackAdvisor;
    public GameObject blackKing;
    public GameObject blackTiger;
    public GameObject blackSoldier;

    public GameObject redTurn;
    public GameObject blackTurn;
    public float step = 1;

    public GameObject KingCheckedText;
    public GameObject invalidMoveText;
    public float invalidAlpha;
    public float generalAlpha;

    public GameObject currentBoard;

    private float mouseOverX;
    private float mouseOverY;
    private Vector3 mousePosition;
    private Vector2 boardPosition;

    private Pieces selectedPiece;
    private Vector2 startDrag;
    private bool dragging = false;
    private Vector3 originalPosition;

    private bool moveCompleted;
    private bool isRedTurn = true;
    private bool isRed;

    private Vector2 kingRedPos;
    private Vector2 kingBlackPos;
    private List<Vector2> redPiecesPos = new List<Vector2>();
    private List<Vector2> blackPiecesPos = new List<Vector2>();



    public void Start()
    {
        Instance = this;
        isRed = PhotonNetwork.IsMasterClient;
        if (PhotonNetwork.IsMasterClient)
        {
            GenerateBoard();
            KingCheckedText = PhotonNetwork.Instantiate(KingCheckedText.name, new Vector3(0, 7, 0), Quaternion.identity);
            KingCheckedText.GetComponent<TextMesh>().color = new Color(0f, 0f, 0f, 0f);
            invalidMoveText = PhotonNetwork.Instantiate(invalidMoveText.name, new Vector3(0, 7, 0), Quaternion.identity);
            invalidMoveText.GetComponent<TextMesh>().color = new Color(0f, 0f, 0f, 0f);
            redTurn = PhotonNetwork.Instantiate(redTurn.name, new Vector3(-60f, 7f, 30f), Quaternion.identity);
            blackTurn = PhotonNetwork.Instantiate(blackTurn.name, new Vector3(-60f, -7f, -30f), Quaternion.identity);
            currentBoard = PhotonNetwork.Instantiate(currentBoard.name, new Vector3(-60f, 7f, 30f), Quaternion.identity);
        }
    }

    public void Update()
    {
        Playing();
        if (FindObjectOfType<BoardState>().board.Length == 0) 
        {
            pieces = FindObjectOfType<BoardState>().board;
        }
    }

    [PunRPC]
    public void SyncValues(Pieces[,] p)
    {
        isRedTurn = !isRedTurn;
        pieces = p;
    }

    // Convert an object to a byte array
    //private byte[] ObjectToByteArray(Pieces[,] obj)
    //{
    //    if (obj == null)
    //        return null;

    //    BinaryFormatter bf = new BinaryFormatter();
    //    MemoryStream ms = new MemoryStream();
    //    bf.Serialize(ms, obj);

    //    return ms.ToArray();
    //}

    //// Convert a byte array to an Object
    //private Pieces[,] ByteArrayToObject(byte[] arrBytes)
    //{
    //    MemoryStream memStream = new MemoryStream();
    //    BinaryFormatter binForm = new BinaryFormatter();
    //    memStream.Write(arrBytes, 0, arrBytes.Length);
    //    memStream.Seek(0, SeekOrigin.Begin);
    //    Pieces[,] obj = (Pieces[,])binForm.Deserialize(memStream);

    //    return obj;
    //}

    public void SetPieces(Pieces[,] p)
    {
        pieces = p;
    }

    public void SetIsRedTurn(bool r)
    {
        isRedTurn = r;
    }

    public void Playing()
    {
        UpdateMouseOver();

        int x = (int)boardPosition.x;
        int y = (int)boardPosition.y;

        if (PhotonNetwork.IsMasterClient)
        {
            if (generalAlpha > 0)
            {
                generalAlpha -= 0.01f;
                KingCheckedText.GetComponent<TextMesh>().color = new Color(0f, 0f, 0f, generalAlpha);
            }
            if (invalidAlpha > 0)
            {
                invalidAlpha -= 0.01f;
                invalidMoveText.GetComponent<TextMesh>().color = new Color(0f, 0f, 0f, invalidAlpha);
            }

            if (isRedTurn)
            {
                if (redTurn.transform.position.x < -59)
                {
                    step = 1;
                }
                else if (redTurn.transform.position.x > -60)
                {
                    step = -1;
                }
                redTurn.transform.position = new Vector3(redTurn.transform.position.x + step, 7f, 30f);
            }
            else
            {
                if (blackTurn.transform.position.x < -59)
                {
                    step = 1;
                }
                else if (blackTurn.transform.position.x > -60)
                {
                    step = -1;
                }
                blackTurn.transform.position = new Vector3(blackTurn.transform.position.x + step, 7f, -30f);
            }
        }

        if (dragging && selectedPiece != null)
        {
            DragPiece(selectedPiece);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isRed == isRedTurn)
            {
                SelectPiece(x, y);
                if (selectedPiece != null && selectedPiece.GetRed() == isRedTurn)
                {
                    dragging = true;
                    originalPosition = selectedPiece.transform.position;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selectedPiece != null && selectedPiece.GetRed() == isRedTurn)
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
                dragging = false;
                selectedPiece = null;
                if (moveCompleted)
                {
                    string msg = "CMOV|";
                    msg += ((int)startDrag.x).ToString() + "|";
                    msg += ((int)startDrag.y).ToString() + "|";
                    msg += x.ToString() + "|";
                    msg += y.ToString();

                    //client.Send(msg);

                    if (isRedTurn)
                    {
                        redTurn.transform.position = new Vector3(-139.9f, 81, 10f);
                        blackTurn.transform.position = new Vector3(-139.9f, -81f, 300f);
                    }
                    else
                    {
                        redTurn.transform.position = new Vector3(-139.9f, 81, 300f);
                        blackTurn.transform.position = new Vector3(-139.9f, -81f, 10f);
                    }
                }
            }
        }
    }

    private void UpdateMouseOver()
    {
        if (!Camera.main) //Check if camera exists
        {
            Debug.Log("Unable to find main camera.");
            return;
        }
        mouseOverX = (Input.mousePosition.x);
        mouseOverY = (Input.mousePosition.y);
        mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseOverX, mouseOverY, 186.75f));
        boardPosition = GetBoardPosition(mousePosition.x, mousePosition.z);
    }

    private void SelectPiece(int x, int y)
    {
        if (x < 0 || x > 9 || y < 0 || y > 8)
        {
            return;
        }

        Pieces p = pieces[x, y];

        if (p != null)
        {
            selectedPiece = p;
            startDrag = boardPosition;
        }
    }

    private void DragPiece(Pieces sP)
    {
        if (sP.GetComponent<Pieces>().type == Pieces.typePiece.horse)
        {
            sP.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mouseOverX, mouseOverY, 50));
        }
        else
        {
            sP.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mouseOverX, mouseOverY, 50));
        }
    }

    public void TryMove(int startX, int startY, int endX, int endY)
    {
        startDrag = new Vector2(startX, startY);
        selectedPiece = pieces[startX, startY];
        bool invalidMove = false;
        if (isRed == isRedTurn)
        {
            selectedPiece.transform.position = originalPosition;
        }
        if (endX >= 0 && endX < 10 && endY >= 0 && endY < 9 && selectedPiece != null)
        {
            if (isValidMove(startX, startY, endX, endY, selectedPiece.type))
            {
                if (!GeneralChecked(isRedTurn))
                {
                    MovePiece(selectedPiece, endX, endY);
                    moveCompleted = true;
                    isRedTurn = !isRedTurn;
                    currentBoard.GetComponent<BoardState>().board = pieces;
                    //photonView.RPC("SyncValues", RpcTarget.OthersBuffered, pieces);
                    return;
                }
                else
                {
                    Pieces[,] copyBoard = (Pieces[,])pieces.Clone();
                    Vector2 originalgeneralRedPos = new Vector2(kingRedPos.x, kingRedPos.y);
                    Vector2 originalgeneralBluePos = new Vector2(kingBlackPos.x, kingBlackPos.y);
                    Vector2 removePos = new Vector2(-1, -1);
                    bool removePosIsRed = false;
                    if (!isRedTurn)
                    {
                        for (int i = 0; i < blackPiecesPos.Count; i++)
                        {
                            if (blackPiecesPos[i].x == startX && blackPiecesPos[i].y == startY)
                            {
                                if (blackPiecesPos[i].x == kingBlackPos.x && blackPiecesPos[i].y == kingBlackPos.y)
                                {
                                    kingBlackPos = new Vector2(endX, endY);
                                }
                                if (pieces[endX, endY] != null)
                                {
                                    removePos = new Vector2(endX, endY);
                                    removePosIsRed = pieces[endX, endY].GetRed();
                                    RemovePiece(pieces[endX, endY].GetRed(), endX, endY);
                                }
                                blackPiecesPos[i] = new Vector2(endX, endY);
                                pieces[endX, endY] = selectedPiece;
                                pieces[startX, startY] = null;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < redPiecesPos.Count; i++)
                        {
                            if (redPiecesPos[i].x == startX && redPiecesPos[i].y == startY)
                            {
                                if (redPiecesPos[i].x == kingRedPos.x && redPiecesPos[i].y == kingRedPos.y)
                                {
                                    kingRedPos = new Vector2(endX, endY);
                                }
                                if (pieces[endX, endY] != null)
                                {
                                    removePos = new Vector2(endX, endY);
                                    removePosIsRed = pieces[endX, endY].GetRed();
                                    RemovePiece(pieces[endX, endY].GetRed(), endX, endY);
                                }
                                redPiecesPos[i] = new Vector2(endX, endY);
                                pieces[endX, endY] = selectedPiece;
                                pieces[startX, startY] = null;
                            }
                        }
                    }
                    bool isGeneralChecked = GeneralChecked(isRedTurn);

                    if (removePos.x != -1 && removePos.y != -1)
                    {
                        if (removePosIsRed)
                        {
                            redPiecesPos.Add(removePos);
                        }
                        else
                        {
                            blackPiecesPos.Add(removePos);
                        }
                    }

                    if (isRedTurn)
                    {
                        for (int i = 0; i < redPiecesPos.Count; i++)
                        {
                            if (redPiecesPos[i].x == endX && redPiecesPos[i].y == endY)
                            {
                                redPiecesPos[i] = new Vector2(startX, startY);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < blackPiecesPos.Count; i++)
                        {
                            if (blackPiecesPos[i].x == endX && blackPiecesPos[i].y == endY)
                            {
                                blackPiecesPos[i] = new Vector2(startX, startY);
                            }
                        }
                    }
                    pieces = copyBoard;
                    if (isGeneralChecked)
                    {
                        generalAlpha = 1;
                        KingCheckedText.GetComponent<TextMesh>().color = new Color(0f, 0f, 0f, generalAlpha);
                        moveCompleted = false;
                        kingBlackPos = originalgeneralBluePos;
                        kingRedPos = originalgeneralRedPos;
                        return;
                    }
                    else
                    {
                        kingBlackPos = originalgeneralBluePos;
                        kingRedPos = originalgeneralRedPos;
                        MovePiece(selectedPiece, endX, endY);
                        moveCompleted = true;
                        isRedTurn = !isRedTurn;
                        return;
                    }
                }
            }
            else
            {
                invalidMove = true;
            }
        }
        else
        {
            invalidMove = true;
        }
        if (invalidMove)
        {
            invalidAlpha = 1;
            invalidMoveText.GetComponent<TextMesh>().color = new Color(0f, 0f, 0f, invalidAlpha);
        }
        moveCompleted = false;

    }

    private void MovePiece(Pieces piece, int x, int y)
    {
        if (piece == null)
        {
            return;
        }
        int startX = (int)piece.GetBoardPosition().x;
        int startY = (int)piece.GetBoardPosition().y;
        int xDifference = x - startX;
        int yDifference = y - startY;
        piece.transform.position = piece.transform.position + new Vector3(yDifference * 20f, 0.5f, xDifference * 10.0f);
        if (pieces[x, y] != null)
        {
            RemovePiece(pieces[x, y].GetRed(), x, y);
            pieces[x, y].SetBoardPosition(-1, -1);
            pieces[x, y].transform.position = pieces[x, y].transform.position + new Vector3(0f, 0f, 300f);
        }
        pieces[startX, startY] = null;
        if (piece.GetRed())
        {
            for (int i = 0; i < redPiecesPos.Count; i++)
            {
                if (piece.GetBoardPosition() == redPiecesPos[i])
                {
                    redPiecesPos[i] = new Vector2(x, y);
                }
            }
        }
        else
        {
            for (int i = 0; i < blackPiecesPos.Count; i++)
            {
                if (piece.GetBoardPosition() == blackPiecesPos[i])
                {
                    blackPiecesPos[i] = new Vector2(x, y);
                }
            }
        }
        if (piece.type == Pieces.typePiece.king)
        {
            if (piece.GetRed())
            {
                kingRedPos = new Vector2(x, y);
            }
            else
            {
                kingBlackPos = new Vector2(x, y);
            }
        }
        piece.SetBoardPosition(x, y);
        pieces[x, y] = piece;
    }

    private void RemovePiece(bool isRed, int x, int y)
    {
        int removePos = -1;
        if (isRed)
        {
            for (int i = 0; i < redPiecesPos.Count; i++)
            {
                if (redPiecesPos[i].x == x && redPiecesPos[i].y == y)
                {
                    removePos = i;
                }
            }
            redPiecesPos.RemoveAt(removePos);
        }
        else
        {
            for (int i = 0; i < blackPiecesPos.Count; i++)
            {
                if (blackPiecesPos[i].x == x && blackPiecesPos[i].y == y)
                {
                    removePos = i;
                }
            }
            blackPiecesPos.RemoveAt(removePos);
        }
    }

    private Vector2 GetBoardPosition(float x, float z)
    {
        int yResult = (int)Math.Round((x + 80.0) / 20.0);
        int xResult = (int)Math.Round((z + 90.0) / 20.0);
        Vector2 boardPos = new Vector2(xResult, yResult);
        return boardPos;
    }

    private void GenerateBoard()
    {

        //Generate Black Team
        GenerateChariot(0, 0, -47f, 7f, -47f, false);
        GenerateChariot(0, 8, 47f, 7f, -47f, false);

        blackPiecesPos.Add(new Vector2(0, 0));
        blackPiecesPos.Add(new Vector2(0, 8));

        GenerateHorse(0, 1, -36f, 7f, -47f, false);
        GenerateHorse(0, 7, 36f, 7f, -47f, false);

        blackPiecesPos.Add(new Vector2(0, 1));
        blackPiecesPos.Add(new Vector2(0, 7));

        GenerateElephant(0, 2, -24f, 7f, -47f, false);
        GenerateElephant(0, 6, 24f, 7f, -47f, false);

        blackPiecesPos.Add(new Vector2(0, 2));
        blackPiecesPos.Add(new Vector2(0, 6));

        GenerateAdvisor(0, 3, -12.5f, 7f, -47f, false);
        GenerateAdvisor(0, 5, 12.5f, 7f, -47f, false);

        blackPiecesPos.Add(new Vector2(0, 3));
        blackPiecesPos.Add(new Vector2(0, 5));

        GenerateGeneral(0, 4, 0f, 7f, -47f, false);

        blackPiecesPos.Add(new Vector2(0, 4));
        kingBlackPos = new Vector2(0, 4);

        GenerateCannon(2, 1, -36f, 7f, -28f, false);
        GenerateCannon(2, 7, 36f, 7f, -28f, false);

        blackPiecesPos.Add(new Vector2(2, 1));
        blackPiecesPos.Add(new Vector2(2, 7));

        GenerateSoldier(3, 0, -47f, 7f, -19f, false);
        GenerateSoldier(3, 2, -24f, 7f, -19f, false);
        GenerateSoldier(3, 4, 0f, 7f, -19f, false);
        GenerateSoldier(3, 6, 24f, 7f, -19f, false);
        GenerateSoldier(3, 8, 47f, 7f, -19f, false);

        blackPiecesPos.Add(new Vector2(3, 0));
        blackPiecesPos.Add(new Vector2(3, 2));
        blackPiecesPos.Add(new Vector2(3, 4));
        blackPiecesPos.Add(new Vector2(3, 6));
        blackPiecesPos.Add(new Vector2(3, 8));

        //Generate Red Team
        GenerateChariot(9, 0, -47f, 7f, 47f, true);
        GenerateChariot(9, 8, 47f, 7f, 47f, true);

        redPiecesPos.Add(new Vector2(9, 0));
        redPiecesPos.Add(new Vector2(9, 8));

        GenerateHorse(9, 1, -36f, 7f, 47f, true);
        GenerateHorse(9, 7, 36f, 7f, 47f, true);

        redPiecesPos.Add(new Vector2(9, 1));
        redPiecesPos.Add(new Vector2(9, 7));

        GenerateElephant(9, 2, -24f, 7f, 47f, true);
        GenerateElephant(9, 6, 24f, 7f, 47f, true);

        redPiecesPos.Add(new Vector2(9, 2));
        redPiecesPos.Add(new Vector2(9, 6));

        GenerateAdvisor(9, 3, -12.5f, 7f, 47f, true);
        GenerateAdvisor(9, 5, 12.5f, 7f, 47f, true);

        redPiecesPos.Add(new Vector2(9, 3));
        redPiecesPos.Add(new Vector2(9, 5));

        GenerateGeneral(9, 4, 0f, 7f, 47f, true);

        redPiecesPos.Add(new Vector2(9, 4));
        kingBlackPos = new Vector2(9, 4);

        GenerateCannon(7, 1, -36f, 7f, 28f, true);
        GenerateCannon(7, 7, 36f, 7f, 28f, true);

        redPiecesPos.Add(new Vector2(7, 1));
        redPiecesPos.Add(new Vector2(7, 7));

        GenerateSoldier(6, 0, -47f, 7f, 19f, true);
        GenerateSoldier(6, 2, -24f, 7f, 19f, true);
        GenerateSoldier(6, 4, 0f, 7f, 19f, true);
        GenerateSoldier(6, 6, 24f, 7f, 19f, true);
        GenerateSoldier(6, 8, 47f, 7f, 19f, true);

        redPiecesPos.Add(new Vector2(6, 0));
        redPiecesPos.Add(new Vector2(6, 2));
        redPiecesPos.Add(new Vector2(6, 4));
        redPiecesPos.Add(new Vector2(6, 6));
        redPiecesPos.Add(new Vector2(6, 8));
    }

    private bool GeneralChecked(bool isRed)
    {
        if (isRed)
        {
            for (int i = 0; i < blackPiecesPos.Count; i++)
            {
                Vector2 pos = blackPiecesPos[i];
                if (isValidMove((int)pos.x, (int)pos.y, (int)kingRedPos.x, (int)kingRedPos.y, pieces[(int)pos.x, (int)pos.y].type))
                {
                    return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < redPiecesPos.Count; i++)
            {
                Vector2 pos = redPiecesPos[i];
                if (isValidMove((int)pos.x, (int)pos.y, (int)kingBlackPos.x, (int)kingBlackPos.y, pieces[(int)pos.x, (int)pos.y].type))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool isValidMove(int startX, int startY, int endX, int endY, Pieces.typePiece type)
    {
        if (pieces[endX, endY] != null)
        {
            if (pieces[startX, startY].GetRed() == pieces[endX, endY].GetRed())
            {
                return false;
            }
        }
        ArrayList possibleMoves = new ArrayList();
        bool isRed = pieces[startX, startY].GetRed();
        if (type == Pieces.typePiece.car)
        {
            if (startX != endX && startY != endY) return false;
            if (startX == endX && startY == endY) return false;
            int start;
            int end;
            if (startY != endY)
            {
                if (endY > startY)
                {
                    start = startY;
                    end = endY;
                }
                else
                {
                    start = endY;
                    end = startY;
                }
                for (int i = start + 1; i < end; i++)
                {
                    if (pieces[startX, i] != null) return false;
                }
                return true;
            }
            else if (startX != endX)
            {
                if (endX > startX)
                {
                    start = startX;
                    end = endX;
                }
                else
                {
                    start = endX;
                    end = startX;
                }
                for (int i = start + 1; i < end; i++)
                {
                    if (pieces[i, startY] != null) return false;
                }
                return true;
            }
        }
        else if (type == Pieces.typePiece.horse)
        {
            if (isInBounds(startX + 1, startY))
            {
                if (pieces[startX + 1, startY] == null)
                {
                    possibleMoves.Add(new Vector2(startX + 2, startY - 1));
                    possibleMoves.Add(new Vector2(startX + 2, startY + 1));
                }
            }
            if (isInBounds(startX - 1, startY))
            {
                if (pieces[startX - 1, startY] == null)
                {
                    possibleMoves.Add(new Vector2(startX - 2, startY - 1));
                    possibleMoves.Add(new Vector2(startX - 2, startY + 1));
                }
            }
            if (isInBounds(startX, startY - 1))
            {
                if (pieces[startX, startY - 1] == null)
                {
                    possibleMoves.Add(new Vector2(startX + 1, startY - 2));
                    possibleMoves.Add(new Vector2(startX - 1, startY - 2));
                }
            }
            if (isInBounds(startX, startY + 1))
            {
                if (pieces[startX, startY + 1] == null)
                {
                    possibleMoves.Add(new Vector2(startX + 1, startY + 2));
                    possibleMoves.Add(new Vector2(startX - 1, startY + 2));
                }
            }
        }
        else if (type == Pieces.typePiece.elephant)
        {
            if (isInBounds(startX + 1, startY + 1))
            {
                if (pieces[startX + 1, startY + 1] == null)
                {
                    if (!isRed)
                    {
                        if (startX + 2 <= 4)
                        {
                            possibleMoves.Add(new Vector2(startX + 2, startY + 2));
                        }
                    }
                    else
                    {
                        possibleMoves.Add(new Vector2(startX + 2, startY + 2));
                    }
                }
            }
            if (isInBounds(startX + 1, startY - 1))
            {
                if (pieces[startX + 1, startY - 1] == null)
                {
                    if (!isRed)
                    {
                        if (startX + 2 <= 4)
                        {
                            possibleMoves.Add(new Vector2(startX + 2, startY - 2));
                        }
                    }
                    else
                    {
                        possibleMoves.Add(new Vector2(startX + 2, startY - 2));
                    }
                }
            }
            if (isInBounds(startX - 1, startY + 1))
            {
                if (pieces[startX - 1, startY + 1] == null)
                {
                    if (isRed)
                    {
                        if (startX - 2 >= 5)
                        {
                            possibleMoves.Add(new Vector2(startX - 2, startY + 2));
                        }
                    }
                    else
                    {
                        possibleMoves.Add(new Vector2(startX - 2, startY + 2));
                    }
                }
            }
            if (isInBounds(startX - 1, startY - 1))
            {
                if (pieces[startX - 1, startY - 1] == null)
                {
                    if (isRed)
                    {
                        if (startX - 2 >= 5)
                        {
                            possibleMoves.Add(new Vector2(startX - 2, startY - 2));
                        }
                    }
                    else
                    {
                        possibleMoves.Add(new Vector2(startX - 2, startY - 2));
                    }
                }
            }
        }
        else if (type == Pieces.typePiece.advisor)
        {
            ArrayList advisorBox = new ArrayList();
            if (isRed)
            {
                advisorBox.Add(new Vector2(7, 3));
                advisorBox.Add(new Vector2(7, 5));
                advisorBox.Add(new Vector2(8, 4));
                advisorBox.Add(new Vector2(9, 3));
                advisorBox.Add(new Vector2(9, 5));
            }
            else
            {
                advisorBox.Add(new Vector2(0, 3));
                advisorBox.Add(new Vector2(0, 5));
                advisorBox.Add(new Vector2(1, 4));
                advisorBox.Add(new Vector2(2, 3));
                advisorBox.Add(new Vector2(2, 5));
            }
            possibleMoves.Add(new Vector2(startX + 1, startY + 1));
            possibleMoves.Add(new Vector2(startX + 1, startY - 1));
            possibleMoves.Add(new Vector2(startX - 1, startY + 1));
            possibleMoves.Add(new Vector2(startX - 1, startY - 1));
            foreach (Vector2 pos in possibleMoves)
            {
                if (pos.x == endX && pos.y == endY && advisorBox.Contains(pos))
                {
                    if (isInBounds(endX, endY))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else if (type == Pieces.typePiece.king)
        {
            ArrayList generalBox = new ArrayList();
            if (isRed)
            {
                for (int i = 7; i < 10; i++)
                {
                    for (int j = 3; j < 6; j++)
                    {
                        generalBox.Add(new Vector2(i, j));
                    }
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 3; j < 6; j++)
                    {
                        generalBox.Add(new Vector2(i, j));
                    }
                }
            }
            possibleMoves.Add(new Vector2(startX + 1, startY));
            possibleMoves.Add(new Vector2(startX - 1, startY));
            possibleMoves.Add(new Vector2(startX, startY + 1));
            possibleMoves.Add(new Vector2(startX, startY - 1));
            foreach (Vector2 pos in possibleMoves)
            {
                if (pos.x == endX && pos.y == endY && generalBox.Contains(pos))
                {
                    if (isInBounds(endX, endY))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else if (type == Pieces.typePiece.tiger)
        {
            if (startX != endX && startY != endY) { return false; } //cannot move diagonally
            if (startX == endX && startY == endY) { return false; } //cannot move to the same current place.
            int start;
            int end;
            if (pieces[endX, endY] != null)
            {
                if (pieces[endX, endY].GetRed() == pieces[startX, startY].GetRed()) { return false; }
                else
                {
                    int countBetween = 0;
                    if (startX != endX)
                    {
                        if (startX < endX) { start = startX; end = endX; } else { start = endX; end = startX; }
                        for (int i = start + 1; i < end; i++)
                        {
                            if (pieces[i, startY] != null) countBetween++;
                        }
                    }
                    else if (startY != endY)
                    {
                        if (startY < endY) { start = startY; end = endY; } else { start = endY; end = startY; }
                        for (int i = start + 1; i < end; i++)
                        {
                            if (pieces[startX, i] != null) countBetween++;
                        }
                    }
                    return countBetween == 1;
                }
            }
            else
            {
                if (startX != endX)
                {
                    if (startX < endX) { start = startX; end = endX; } else { start = endX; end = startX; }
                    for (int i = start + 1; i < end; i++)
                    {
                        if (pieces[i, startY] != null) return false;
                    }
                }
                else if (startY != endY)
                {
                    if (startY < endY) { start = startY; end = endY; } else { start = endY; end = startY; }
                    for (int i = start + 1; i < end; i++)
                    {
                        if (pieces[startX, i] != null) return false;
                    }
                }
                return true;
            }
        }
        else if (type == Pieces.typePiece.soldier)
        {
            bool crossedRiver = false;
            if (isRed)
            {
                possibleMoves.Add(new Vector2(startX - 1, startY));
                if (startX <= 4) { crossedRiver = true; }
            }
            else
            {
                possibleMoves.Add(new Vector2(startX + 1, startY));
                if (startX >= 5) { crossedRiver = true; }
            }
            if (crossedRiver)
            {
                possibleMoves.Add(new Vector2(startX, startY - 1));
                possibleMoves.Add(new Vector2(startX, startY + 1));
            }
        }
        foreach (Vector2 pos in possibleMoves)
        {
            if (pos.x == endX && pos.y == endY)
            {
                if (isInBounds(endX, endY))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool isInBounds(int x, int y)
    {
        if (x >= 0 && x < 10 && y >= 0 && y < 9)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void GenerateChariot(int x, int y, float px, float py, float pz, bool red)
    {
        GameObject go;
        if (red)
        {
            go = PhotonNetwork.Instantiate(redCar.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        else
        {
            go = PhotonNetwork.Instantiate(blackCar.name, new Vector3(px, py, pz), Quaternion.identity);
        }

        go.GetComponent<Pieces>().type = Pieces.typePiece.car;
        go.GetComponent<Pieces>().setIsRed(red);
        go.GetComponent<Pieces>().SetBoardPosition(x, y);
        go.transform.position = new Vector3(px, py, pz);
        Pieces p = go.GetComponent<Pieces>();
        pieces[x, y] = p;
    }
    private void GenerateHorse(int x, int y, float px, float py, float pz, bool red)
    {
        GameObject go;
        if (red)
        {
            go = PhotonNetwork.Instantiate(redHorse.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        else
        {
            go = PhotonNetwork.Instantiate(blackHorse.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        go.GetComponent<Pieces>().type = Pieces.typePiece.horse;
        go.GetComponent<Pieces>().setIsRed(red);
        go.GetComponent<Pieces>().SetBoardPosition(x, y);
        go.transform.position = new Vector3(px, py, pz);
        Pieces p = go.GetComponent<Pieces>();
        pieces[x, y] = p;
    }
    private void GenerateElephant(int x, int y, float px, float py, float pz, bool red)
    {
        GameObject go;
        if (red)
        {
            go = PhotonNetwork.Instantiate(redElephant.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        else
        {
            go = PhotonNetwork.Instantiate(blackElephant.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        go.GetComponent<Pieces>().type = Pieces.typePiece.elephant;
        go.GetComponent<Pieces>().setIsRed(red);
        go.GetComponent<Pieces>().SetBoardPosition(x, y);
        go.transform.position = new Vector3(px, py, pz);
        Pieces p = go.GetComponent<Pieces>();
        pieces[x, y] = p;
    }
    private void GenerateAdvisor(int x, int y, float px, float py, float pz, bool red)
    {
        GameObject go;
        if (red)
        {
            go = PhotonNetwork.Instantiate(redAdvisor.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        else
        {
            go = PhotonNetwork.Instantiate(blackAdvisor.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        go.GetComponent<Pieces>().type = Pieces.typePiece.advisor;
        go.GetComponent<Pieces>().setIsRed(red);
        go.GetComponent<Pieces>().SetBoardPosition(x, y);
        go.transform.position = new Vector3(px, py, pz);
        Pieces p = go.GetComponent<Pieces>();
        pieces[x, y] = p;
    }
    private void GenerateGeneral(int x, int y, float px, float py, float pz, bool red)
    {
        GameObject go;
        if (red)
        {
            go = PhotonNetwork.Instantiate(redKing.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        else
        {
            go = PhotonNetwork.Instantiate(blackKing.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        go.GetComponent<Pieces>().type = Pieces.typePiece.king;
        go.GetComponent<Pieces>().setIsRed(red);
        go.GetComponent<Pieces>().SetBoardPosition(x, y);
        go.transform.position = new Vector3(px, py, pz);
        Pieces p = go.GetComponent<Pieces>();
        pieces[x, y] = p;
    }
    private void GenerateCannon(int x, int y, float px, float py, float pz, bool red)
    {
        GameObject go;
        if (red)
        {
            go = PhotonNetwork.Instantiate(redTiger.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        else
        {
            go = PhotonNetwork.Instantiate(blackTiger.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        go.GetComponent<Pieces>().type = Pieces.typePiece.tiger;
        go.GetComponent<Pieces>().setIsRed(red);
        go.GetComponent<Pieces>().SetBoardPosition(x, y);
        go.transform.position = new Vector3(px, py, pz);
        Pieces p = go.GetComponent<Pieces>();
        pieces[x, y] = p;
    }
    private void GenerateSoldier(int x, int y, float px, float py, float pz, bool red)
    {
        GameObject go;
        if (red)
        {
            go = PhotonNetwork.Instantiate(redSoldier.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        else
        {
            go = PhotonNetwork.Instantiate(blackSoldier.name, new Vector3(px, py, pz), Quaternion.identity);
        }
        go.GetComponent<Pieces>().type = Pieces.typePiece.soldier;
        go.GetComponent<Pieces>().setIsRed(red);
        go.GetComponent<Pieces>().SetBoardPosition(x, y);
        go.transform.position = new Vector3(px, py, pz);
        Pieces p = go.GetComponent<Pieces>();
        pieces[x, y] = p;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isRedTurn);
            //stream.SendNext(ObjectToByteArray(pieces));
        }
        else
        {
            SetIsRedTurn((bool)stream.ReceiveNext());
            //SetPieces((Pieces[,])stream.ReceiveNext());
        }
    }
}
