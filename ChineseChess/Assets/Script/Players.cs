using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Players : MonoBehaviourPun
{
    public static GameObject localPlayerInstance;
    private Camera viewCamera;

    public void Awake()
    {
        viewCamera = Camera.main;

        if (photonView.IsMine)
        {
            localPlayerInstance = gameObject;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        CameraWork();
    }

    public void CameraWork()
    {
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();


        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.player = gameObject.transform;
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }
    }
}
