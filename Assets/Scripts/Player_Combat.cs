using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player_Combat : MonoBehaviour
{
    public Animator anim;
   
    public void Attact()
    {
        anim.SetBool("isAttacking", true);

    }
    public void FinishAttacking()
    {
        anim.SetBool("isAttacking", false);
    }

}
