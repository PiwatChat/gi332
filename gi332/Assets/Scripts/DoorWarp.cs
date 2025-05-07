using Unity.Netcode;
using UnityEngine;

public class DoorWarp : NetworkBehaviour
{
    [SerializeField]
    private Transform warpTarget;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out NetworkObject netObj) && netObj.IsOwner)
            {
                RequestWarpServerRpc(netObj.OwnerClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestWarpServerRpc(ulong clientId)
    {
        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObj != null)
        {
            playerObj.transform.position = warpTarget.position;
        }
    }
}
