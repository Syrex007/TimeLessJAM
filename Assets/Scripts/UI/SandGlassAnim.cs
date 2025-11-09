using UnityEngine;

public class SandGlassAnim : MonoBehaviour
{

    [SerializeField] Animator anim;
    [SerializeField] private int currentState = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetState(int state)
    {
        if (currentState == state) return;

        currentState = state;
        anim.SetInteger("SandGlassState", state);
    }
}
