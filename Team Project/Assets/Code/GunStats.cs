using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu]

public class GunStats : ScriptableObject
{
    [Header("--- Weapon Stats ---")]
    [Space]
    public GameObject weaponModel;
    public int shootDamage;
    public int shootDist;
    public int ammoCur, ammoMax, totalMags;

    [Header("--- Effects ---")]
    [Space]

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    [Range(0, 200)] public float shootSoundVol;
    [Space]

    public AudioClip audioClipHolster;
    public AudioClip audioClipUnholster;
    public AudioClip audioClipReload;
    public AudioClip audioClipReloadEmpty;
    public AudioClip audioClipFireEmpty;
    [Space]

    public Animator animator;
    public Sprite spriteBody;
}
