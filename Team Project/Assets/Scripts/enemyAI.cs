using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] int HP;
    [SerializeField] Renderer model;

    Color colorOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Save the original color of the model
        colorOrig = model.material.color;

        // Update the goal count
        gameManager.instance.UpdatedGameGoal(1);
    }

    public void takeDamage(int amount)
    {
        // Reduce the HP by the amount
        HP -= amount;

        // start the flash red coroutine
        StartCoroutine(flashRed());

        if (HP <= 0) // when HP is less than or equal to 0
        {
            // Update the goal count
            gameManager.instance.UpdatedGameGoal(-1);

            // Destroy the game object
            Destroy(gameObject);
        }
    }

    IEnumerator flashRed()
    {
        // Change the color of the model to red
        model.material.color = Color.red;

        // Wait for 0.1 seconds
        yield return new WaitForSeconds(0.1f);

        // Change the color of the model back to the original color
        model.material.color = colorOrig;
    }
}
