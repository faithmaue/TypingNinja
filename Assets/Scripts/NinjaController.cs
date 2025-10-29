using UnityEngine;

public class NinjaController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Swing()
    {
        animator.SetTrigger("Swing");
    }
}
