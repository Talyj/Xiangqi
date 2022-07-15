using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;


public class GameManager : MonoBehaviourPunCallbacks
{
    [Tooltip("The list of prefab that represent the differente characters")]
    public GameObject playerPrefabs;
        
    private int numberPlayer = 1;

    public void Start()
    {
        if (SceneManager.GetActiveScene().name == "WaitingRoom")
        {
            //LoadArena(); 
            return;
        }
        if (playerPrefabs == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (Players.localPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene().name);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(playerPrefabs.name, new Vector3(0f, 110f, 0f), Quaternion.identity, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }

    private void SpawnDifferentAxis(bool isMasterClient)
    {
        if (isMasterClient)
        {
            PhotonNetwork.Instantiate(playerPrefabs.name, new Vector3(0f, 110f, 0f), Quaternion.LookRotation(new Vector3(0f, 0f, 0f), new Vector3(0f, 110f, -300f)), 0);
        }
        else
        {
            PhotonNetwork.Instantiate(playerPrefabs.name, new Vector3(0f, 110f, 0f), Quaternion.LookRotation(new Vector3(0f, 0f, 0f), new Vector3(0f, 10f, 300f)), 0);
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Launcher");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        if (PhotonNetwork.CurrentRoom.PlayerCount > numberPlayer)
        {
            PhotonNetwork.LoadLevel("MainGame");
        }
        else
        {
            PhotonNetwork.LoadLevel("WaitingRoom");
        }
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        Debug.Log($"OnPlayerEnteredRoom() {other.NickName}"); // not seen if you're the player connecting


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            LoadArena();
        }
    }

}
