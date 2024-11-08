using UnityEngine;

public class SelfDeleter : MonoBehaviour
{
    [SerializeField] private float timeToDeletion = 10f;

    private float timeElapsed;

    // Start is called before the first frame update
    void Start()
    {
        timeElapsed = 0f;
    }

    void FixedUpdate()
    {
        timeElapsed += Time.fixedDeltaTime;
        if (timeElapsed >= timeToDeletion)
        {
            Destroy(gameObject);
        }
    }
}
