using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using FirstGearGames.SmoothCameraShaker;

public class PlayerShip : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] private ShakeData explosionShakeData;
    [Header("Settings")]
    [SerializeField] public float evasion = 0.5f;// will later be linked to engine room status

    public static PlayerShip Instance { get; private set; }
    public readonly Dictionary<ShipRoomType, ShipRoom> roomMap = new();
    private Slider ShipHullHpSlider;
    private AudioSource AlarmSound;
    private AudioSource BoomSound;
    public Collider playerShipCollider;
    private bool isDead;

    [SyncVar(hook = nameof(OnHealthChanged))]
    [SerializeField] private int hullIntegrity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple PlayerShip instances!");
            Destroy(this);
            return;
        }

        Instance = this;

        ShipHullHpSlider = GameObject.Find("ShipHpSlider")?.GetComponent<Slider>();
        AlarmSound = GameObject.Find("AlarmAudioSource")?.GetComponent<AudioSource>();
        BoomSound = GameObject.Find("ShipHitAudioSource")?.GetComponent<AudioSource>();
        playerShipCollider = GameObject.Find("PlayerShipCollider")?.GetComponent<Collider>();
    }

    void OnHealthChanged(int oldHealth, int NewHealth)
    {
        if (ShipHullHpSlider != null)
            ShipHullHpSlider.value = NewHealth;
        if (AlarmSound != null && !AlarmSound.isPlaying)
            AlarmSound.Play();
        if (BoomSound != null)
        {
            BoomSound.Stop();
            BoomSound.Play();
            if (explosionShakeData != null)
                CameraShakerHandler.Shake(explosionShakeData);
        }
    }

    [Server]
    public void TakeDamage(ShipRoomType targetRoom, int amount)
    {
        if (isDead) return;

        if (roomMap.TryGetValue(targetRoom, out var room))
        {
            room.HandleHit(amount);
        }

        hullIntegrity -= amount;
        Debug.Log($"PlayerShip hit, hullIntegrity at {hullIntegrity}");
        if (hullIntegrity <= 0)
        {
            isDead = true;
            Debug.Log($"PlayerShip dead");
            //Player lose
        }
    }

    [Server]
    public void RegisterRoom(ShipRoom room)
    {
        if (!roomMap.ContainsKey(room.roomType))
        {
            roomMap.Add(room.roomType, room);
            Debug.Log($"Room {room.roomType} registered.");
        }
        else
        {
            Debug.LogWarning($"Room {room.roomType} already registered.");
        }
    }
}
