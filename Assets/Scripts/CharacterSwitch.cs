using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public GameObject[] characters;  // Array to hold the character GameObjects
    private int currentCharacterIndex = 0;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        // Ensure only the first character is active at the start
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == currentCharacterIndex);
        }

        UpdateAnimator();
    }

    public void SwitchCharacter()
    {
        characters[currentCharacterIndex].SetActive(false);

        // Update the index to the next character (wrap around to 0 if we reach the end)
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Length;

        characters[currentCharacterIndex].SetActive(true);

        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        // Update the animator in PlayerMovement
        if (playerMovement != null)
        {
            Animator newAnimator = characters[currentCharacterIndex].GetComponent<Animator>();
            if (newAnimator != null)
            {
                playerMovement.mAnimator = newAnimator; // Assign the new Animator
            }
        }
    }
}
