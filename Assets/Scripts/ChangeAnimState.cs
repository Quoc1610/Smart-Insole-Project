using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeAnimState : MonoBehaviour
{
    public Animator anim;
    public GameObject goChar;
    private GameObject spawnChar;
    private Animator spawnAnim;
    public List<Button> buttons= new List<Button>();
    public Vector3 positions;
    private void Start() {
        positions=goChar.transform.position;
    }
    public void OnButtonClicked(int index)
    {
        Debug.Log("!"+index.ToString());
        spawnChar = GameObject.FindGameObjectWithTag("Character");
        Debug.Log("!Run");
        spawnAnim = spawnChar.GetComponent<Animator>();
        if (index == 0)
        {
            spawnAnim.SetTrigger("Walk");
            spawnAnim.SetBool("IsWalk", true);
            spawnAnim.SetBool("IsJog", false);
            spawnAnim.SetBool("IsRun", false);
            spawnAnim.SetBool("IsStair", false);
        }
        else if (index == 1)
        {
            spawnAnim.SetBool("IsWalk", false);
            spawnAnim.SetBool("IsJog", true);
            spawnAnim.SetBool("IsRun", false);
            spawnAnim.SetBool("IsStair", false);
        }
        else if (index == 2)
        {
            spawnAnim.SetBool("IsWalk", false);
            spawnAnim.SetBool("IsJog", false);
            spawnAnim.SetBool("IsRun", true);
            spawnAnim.SetBool("IsStair", false);
        }
        else if (index == 3)
        {
            spawnAnim.SetBool("IsWalk", false);
            spawnAnim.SetBool("IsJog", false);
            spawnAnim.SetBool("IsRun", false);
            spawnAnim.SetBool("IsStair", true);
        }

    }
}
