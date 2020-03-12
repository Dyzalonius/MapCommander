using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private string clientName;

    [SerializeField]
    private bool autoConnect = true;
    
    private void Start()
    {
        if (autoConnect)
            Connect();
    }

    private void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinOrCreateRoom();");
        RoomOptions options = new RoomOptions() { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("mainroom", options, TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected. This script now calls: PhotonNetwork.JoinOrCreateRoom();");
        RoomOptions options = new RoomOptions() { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("mainroom", options, TypedLobby.Default);
    }
}
