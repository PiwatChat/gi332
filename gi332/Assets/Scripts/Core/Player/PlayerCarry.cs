using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerCarry : NetworkBehaviour
{
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private GameObject carryOffset;
    [SerializeField] private float airForce = 1.25f;
    [SerializeField] private Animator anim;
    
    public NetworkVariable<NetworkObjectReference> carriedPlayer = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<NetworkObjectReference> carriedBy = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<float> flipPlayer = new NetworkVariable<float>(1f);
    public NetworkVariable<bool> isCarrying = new NetworkVariable<bool>();
    public NetworkVariable<bool> isCarried = new NetworkVariable<bool>();
    public NetworkVariable<bool> isThrow = new NetworkVariable<bool>();

    private PlayerMovement movementScript;

    private void Awake()
    {
        movementScript = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        isCarried.OnValueChanged += OnCarriedStatusChanged;
        carriedBy.OnValueChanged += OnCarrierChanged;
    }
    
    private void OnCarrierChanged(NetworkObjectReference previous, NetworkObjectReference current)
    {
        if (IsServer)
        {
            UpdateFacingDirection();
        }
    }

    private void OnCarriedStatusChanged(bool previous, bool current)
    {
        movementScript.enabled = !current;
        Collider2D col = GetComponent<Collider2D>();
        col.enabled = !current;
    
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = current;
        }
    }

    [ServerRpc]
    public void PickupServerRpc()
    {
        if (isCarrying.Value) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerCarry>(out var player) && player != this)
            {
                if (player.isCarrying.Value) return;
                
                carriedPlayer.Value = player.NetworkObject;
                player.isCarried.Value = true;
                player.carriedBy.Value = this.NetworkObject;
                player.anim.SetBool("IsGrabing", true);
                isCarrying.Value = true;
                break;
            }
        }
    }

    [ServerRpc]
    public void ReleaseServerRpc()
    {
        if (!isCarrying.Value) return;

        if (carriedPlayer.Value.TryGet(out NetworkObject obj))
        {
            carriedPlayer.Value = default;
            isCarrying.Value = false;
            if (obj.TryGetComponent<PlayerCarry>(out var player))
            {
                player.carriedBy.Value = default;
                StartCoroutine(IsThrow(player));
                player.anim.SetBool("IsGrabing", false);
                player.isCarried.Value = false;
            }
        }
    }

    private IEnumerator IsThrow(PlayerCarry player)
    {
        player.isThrow.Value = true;
        yield return new WaitForSeconds(1f);
        player.isThrow.Value = false;
    }

    private void Throw()
    {
        transform.Translate(new Vector2(flipPlayer.Value, airForce) * throwForce * Time.deltaTime);
    }
    
    private void UpdateFacingDirection()
    {
        if (carriedBy.Value.TryGet(out NetworkObject carrierObj))
        {
            if (carrierObj.TryGetComponent<PlayerMovement>(out var carrierMovement))
            {
                float newFlipValue = carrierMovement.IsFacingRight.Value ? 1f : -1f;
                
                if (IsServer)
                {
                    flipPlayer.Value = newFlipValue;
                }
                else
                {
                    UpdateFacingDirectionServerRpc(newFlipValue);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateFacingDirectionServerRpc(float newFlipValue)
    {
        flipPlayer.Value = newFlipValue;
    }


    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.E)) PickupServerRpc();
            if (Input.GetKeyDown(KeyCode.Q)) ReleaseServerRpc();
        }

        if (isCarrying.Value)
        {
            if (carriedPlayer.Value.TryGet(out NetworkObject obj))
            {
                obj.transform.position = carryOffset.transform.position + new Vector3(0, 0.2f, 0);
            }
        }
        
    }

    private void FixedUpdate()
    {
        if (isCarried.Value)
        {
            UpdateFacingDirection();
        }
        
        if (isThrow.Value)
        {
            Throw();
        }
    }
}
