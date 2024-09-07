using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeAnimState : MonoBehaviour
{
    public Animator anim;
    public GameObject goChar;
    public List<Button> buttons= new List<Button>();
    public Vector3 positions;
    private void Start() {
        positions=goChar.transform.position;
    }
    public void OnButtonClicked(int index)
    {
        if(index == 0)
        {
            anim.SetBool("IsWalk", true);
            anim.SetBool("IsJog", false);
            anim.SetBool("IsRun", false);
            anim.SetBool("IsStair", false);
            goChar.transform.position=positions;
        }
        else if(index == 1)
        {
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsJog", true);
            anim.SetBool("IsRun", false);
            anim.SetBool("IsStair", false);
            goChar.transform.position=positions;
        }
        else if(index == 2)
        {
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsJog", false);
            anim.SetBool("IsRun", true);
            anim.SetBool("IsStair", false);
            goChar.transform.position=positions;
        }
        else if(index == 3)
        {
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsJog", false);
            anim.SetBool("IsRun", false);
            anim.SetBool("IsStair", true);
            goChar.transform.position=positions;
        }
    }
}
