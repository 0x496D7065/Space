using UnityEngine;
using Mirror;
using System;
using UnityEngine.InputSystem;

public class Interactable : NetworkBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;
    public virtual void Interact(InputAction.CallbackContext ctx)
    {
        if (audioSource != null && audioClip != null && ctx.started)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}
