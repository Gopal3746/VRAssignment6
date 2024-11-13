using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float startingTimeSec;
    [SerializeField] private Animator firstDoor;
    [SerializeField] private Animator exitDoor;

    public static GameManager Instance;
    public float TimeLeft { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.LogWarning("Warning: more than one GameManager detected in the scene. Disabling this one.");
            enabled = false;
        }
    }
    
    void Start()
    {
        TimeLeft = startingTimeSec;
    }

    public void OpenFirstDoor()
    {
        firstDoor.SetTrigger("Open");
        firstDoor.GetComponent<AudioSource>().Play();
    }

    public void OpenExitDoor()
    {
        exitDoor.SetTrigger("Open");
        firstDoor.GetComponent<AudioSource>().Play();
    }

    void FixedUpdate()
    {
        // update timer
        TimeLeft -= Time.fixedDeltaTime;
        if (TimeLeft <= 0)
        {
            Debug.Log("Lose! Ran out of time."); // TODO: implement losing logic
        }
    }
}
