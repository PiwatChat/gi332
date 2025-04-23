using Unity.Netcode;
using UnityEngine;

public class GoalPlayerState : NetworkBehaviour
{
    private NetworkVariable<bool> isVisible = new NetworkVariable<bool>(true);

    private void Start()
    {
        isVisible.OnValueChanged += OnVisibilityChanged;
        OnVisibilityChanged(false, isVisible.Value);
    }

    private void OnVisibilityChanged(bool oldValue, bool newValue)
    {
        if (TryGetComponent(out SpriteRenderer sR))
        {
            sR.enabled = newValue;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleVisibilityServerRpc()
    {
        isVisible.Value = !isVisible.Value;
    }
}