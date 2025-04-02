using UnityEngine;

public class InputTest : MonoBehaviour
{

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void InputSomething()
    {
        Debug.Log("Found Beat");
    }

}
