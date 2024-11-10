using System.Text;
using TMPro;
using UnityEngine;

public class LockCodePuzzle : MonoBehaviour
{
    [SerializeField] private int[] correctCombination = new int[4];
    [SerializeField] private TMP_Text comboText;

    private int[] currentCombo;
    private int currentComboIndex;
    private bool completedPuzzle;

    private void Start()
    {
        currentCombo = new int[] { -1, -1, -1, -1 };
        currentComboIndex = 0;
        completedPuzzle = false;
    }

    public void PressNumber(int number)
    {
        if (completedPuzzle) return;
        Debug.Log(number + " pressed");
        currentCombo[currentComboIndex++] = number;
        comboText.color = Color.white;
        UpdateComboText();
        if (currentComboIndex >= correctCombination.Length)
        {
            // check current combination and reset if wrong
            bool isCorrect = true;
            for (int i = 0; i < correctCombination.Length; i++)
            {
                if (currentCombo[i] != correctCombination[i])
                {
                    isCorrect = false;
                    break;
                }
            }
            if (isCorrect) {
                // puzzle pass, open door, disable interaction
                completedPuzzle = true;
                comboText.color = Color.green;
                OpenDoor();
            } else
            {
                currentCombo = new int[] { -1, -1, -1, -1 };
                currentComboIndex = 0;
                comboText.color = Color.red;
                UpdateComboText();
            }
        }
    }

    private void UpdateComboText()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < currentCombo.Length; i++)
        {
            if (i < currentComboIndex)
            {
                sb.Append(currentCombo[i]).Append(' ');
            } else
            {
                sb.Append("- ");
            }
        }
        comboText.text = sb.ToString();
    }

    private void OpenDoor()
    {
        Debug.Log("Opening door");
        GameManager.Instance.OpenFirstDoor();
    }
}
