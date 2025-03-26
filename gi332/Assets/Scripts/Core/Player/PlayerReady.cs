using Unity.Netcode;
public class PlayerReady : NetworkBehaviour
{
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>();

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }
}