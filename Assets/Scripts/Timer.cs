using System.Collections;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private void Start()
    {
        StartCoroutine(nameof(SecondUpdate));
    }

    private IEnumerator SecondUpdate()
    {
        while (true)
        {
            int secondsLeft = (int)GameManager.Instance.TimeLeft;
            int minsLeft = secondsLeft / 60;
            secondsLeft %= 60;
            text.text = $"{minsLeft:00}:{secondsLeft:00}";
            yield return new WaitForSeconds(1f);
        }
    }
}
