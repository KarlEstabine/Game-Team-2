using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] string buttonInfo;
    Animator anim;
    [Range(1, 4), SerializeField] float animSpeed;

    bool inTrigger;

    bool openDoor;
    float doorProgress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (openDoor)
        {
            doorProgress = Mathf.Lerp(doorProgress, 1, Time.deltaTime * animSpeed); // Multiply to control speed
            anim.SetFloat("Door Closed", doorProgress);
        }
        else
        {
            doorProgress = Mathf.Lerp(doorProgress, 0, Time.deltaTime * animSpeed); // Multiply to control speed
            anim.SetFloat("Door Closed", doorProgress);
        }

        if (inTrigger)
        {
            if (Input.GetButtonDown("Interact"))
            {
                openDoor = true;
                GameManager.instance.butttonInteract.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        IOpen open = other.GetComponent<IOpen>();

        if (open != null)
        {
            inTrigger = true;
            GameManager.instance.butttonInteract.SetActive(true);
            GameManager.instance.buttonInfo.text = buttonInfo;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        inTrigger = false;

        if (other.isTrigger) return;

        IOpen open = other.GetComponent<IOpen>();

        if (open != null)
        {
            openDoor = false;
            GameManager.instance.butttonInteract.SetActive(false);
            GameManager.instance.buttonInfo.text = null;
        }
    }
}
