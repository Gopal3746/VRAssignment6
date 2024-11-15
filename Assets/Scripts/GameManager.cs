using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float startingTimeSec;
    [SerializeField] private Animator firstDoor;
    [SerializeField] private Animator exitDoor;
    public bool gameActive = true;

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
        gameActive = false;
    }

    void FixedUpdate()
    {
        if (!gameActive)
        {
            return;
        }
        // update timer
        TimeLeft -= Time.fixedDeltaTime;
        if (TimeLeft <= 0)
        {
            gameActive = false;
            Debug.Log("Lose! Ran out of time."); // TODO: implement losing logic
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("RoomLight"))
            {
                obj.GetComponent<Light>().color = Color.red;
                obj.GetComponent<Light>().intensity = 50;
                obj.GetComponent<Light>().range = 15;
            }
            GetComponent<AudioSource>().Play();
        }
    }
}
