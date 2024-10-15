using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace myScripts
{
    public class ChangeAnimState : MonoBehaviour
    {
        public static ChangeAnimState Instance;

        public Animator anim;
        public GameObject goChar;
        public List<Button> buttons = new List<Button>();
        public Vector3 positions;

        private void Awake()
        {
            Instance = this;
        }
        private void OnEnable()
        {
            ARPlacementExtension.OnObjectPlacedEvent += OnObjectSpawned;
        }

        private void OnDisable()
        {
            ARPlacementExtension.OnObjectPlacedEvent -= OnObjectSpawned;
        }

        public void OnObjectSpawned(GameObject obj)
        {
            anim = obj.GetComponent<Animator>();
        }

        public void OnButtonClicked(int index)
        {

            switch (index)
            {
                case 0:
                    anim.SetBool("IsWalk", true);
                    anim.SetBool("IsJog", false);
                    anim.SetBool("IsRun", false);
                    anim.SetBool("IsStair", false);
                    break;

                case 1:
                    anim.SetBool("IsWalk", false);
                    anim.SetBool("IsJog", true);
                    anim.SetBool("IsRun", false);
                    anim.SetBool("IsStair", false);
                    break;

                case 2:
                    anim.SetBool("IsWalk", false);
                    anim.SetBool("IsJog", false);
                    anim.SetBool("IsRun", true);
                    anim.SetBool("IsStair", false);
                    break;

                case 3:
                    anim.SetBool("IsWalk", false);
                    anim.SetBool("IsJog", false);
                    anim.SetBool("IsRun", false);
                    anim.SetBool("IsStair", true);
                    break;

                default:
                    Debug.LogWarning("Invalid button index.");
                    break;
            }
        }
    }
}