using MyStateMachine;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Collider2D coll;

    public void ActiveCollider()
    {
        coll.enabled = true;
    }

    public void DeactiveCollider()
    {
        coll.enabled = false;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != CharacterStateMachine.Instance.gameObject) return;
        anim.SetBool("go", true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject != CharacterStateMachine.Instance.gameObject) return;
        anim.SetBool("go", false);
    }
}
