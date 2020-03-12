using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonManager : MonoBehaviour, IMatchmakingCallbacks, IInRoomCallbacks
{
    [SerializeField]
    private ClientList clientList;

    [HideInInspector]
    public static PhotonManager Instance { get; private set; } // static singleton

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    public virtual void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public virtual void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom");

        if (clientList != null)
            clientList.AddClientItem();
    }

    public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) { }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }

    public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }

    public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) { }

    public void OnFriendListUpdate(List<FriendInfo> friendList) { }

    public void OnCreatedRoom()
    {
        Debug.Log("onCreatedRoom");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed");
    }

    public void OnJoinedRoom()
    {
        if (clientList != null)
            clientList.AddClientItem();
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed");
    }

    public void OnJoinRandomFailed(short returnCode, string message) { }

    public void OnLeftRoom() { }
}
