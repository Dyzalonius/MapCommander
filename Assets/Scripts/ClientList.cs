using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientList : MonoBehaviour
{
    [SerializeField]
    private ClientItem clientItemPrefab;

    private List<ClientItem> clientItems = new List<ClientItem>();

    public void AddClientItem()
    {
        ClientItem newClientItem = Instantiate(clientItemPrefab, transform).GetComponent<ClientItem>();
        clientItems.Add(newClientItem);
    }
}
