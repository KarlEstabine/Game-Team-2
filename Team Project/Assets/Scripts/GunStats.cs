using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu]

public class GunStats : ScriptableObject
{
    [Header("--- Weapon Stats ---")]
    [Space]
    public GameObject weaponModel;
    [Space]
    public int shootDamage;
    public int shootDist;
    public float shootRate;

    [Space]
    [HideInInspector]public int ammoCur;
    [HideInInspector] public int ammoMax;
    public int ammoPerMag;
    public int totalMags;

    [Header("--- Effects ---")]
    [Space]
    public ParticleSystem hitEffect;
    [Range(0, 200)] public float shootSoundVol;
    [Space]

    [Header("--- Audio Clips ---")]
    [Space]
    public AudioClip audioClipHolster;
    public AudioClip audioClipUnholster;
    public AudioClip audioClipReload;
    public AudioClip audioClipReloadEmpty;
    public AudioClip audioClipFireEmpty;
    public AudioClip[] shootSound;

    //[Space]

    //public Animator animator;
    //public Sprite spriteBody;
}
