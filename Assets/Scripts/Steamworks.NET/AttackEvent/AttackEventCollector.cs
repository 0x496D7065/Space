using System.Collections.Generic;
using UnityEngine;

public class AttackEventCollector : MonoBehaviour
{
    public static AttackEventCollector Instance { get; private set; }

    private readonly List<AttackEvent> attackEvents = new(1000);// 1000 act as a maximum for now, in case the maximum amount of enemies are able to attack on the exact same frame

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple AttackEventCollector instances!");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void RegisterAttack(AttackEvent attackEvent)
    {
        attackEvents.Add(attackEvent);
    }

    public List<AttackEvent> GetCollectedEvents()
    {
        return attackEvents;
    }

    public void Clear()
    {
        attackEvents.Clear();
    }
}
