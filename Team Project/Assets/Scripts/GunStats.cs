using UnityEngine;

[CreateAssetMenu]

public class GunStats : ScriptableObject
{
    [Header("Gun Stats")]
    public GameObject weaponModel;
    public int shootdamage;
    public int shootDist;
    public int ammoCur, ammoMax;


    [Header("Effects")]
    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    public float shootSoundVol;
}
