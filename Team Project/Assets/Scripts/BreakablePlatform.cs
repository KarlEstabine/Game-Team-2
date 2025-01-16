using UnityEngine;

public class BreakingPlatform : MonoBehaviour
{
    //fields for break time and what model to change colors
    [SerializeField] Renderer model;

    [SerializeField] int breakTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        //if it collides with the player then change color to red and break after breakTime
        if (other.CompareTag("Player"))
        {
            model.material.color = Color.red;
            Destroy(model.gameObject, breakTime);
        }


    }


}