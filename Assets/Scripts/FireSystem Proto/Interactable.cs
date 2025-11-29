using UnityEngine;
using Mirror;
using System;

public class Interactable : NetworkBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;
    public virtual void Interact()
    {
        AudioSource.PlayClipAtPoint(audioClip, this.transform.position);
    }
}
