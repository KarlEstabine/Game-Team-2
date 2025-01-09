using System.Collections;
using UnityEngine;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;

    [SerializeField] int HP;

    Color colorOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        Debug.Log("HP: " + HP + "\nDamage: " + amount);

        StartCoroutine(flashRed());

        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
}
