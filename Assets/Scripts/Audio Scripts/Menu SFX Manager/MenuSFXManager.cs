using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuSFXManager : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip pressedSound;
    private SoundFXManager soundFXManager;

    void Start()
    {
        soundFXManager = FindAnyObjectByType<SoundFXManager>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        soundFXManager.PlaySoundFXClip(hoverSound, transform, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        soundFXManager.PlaySoundFXClip(pressedSound, transform, 1f);
    }
}
