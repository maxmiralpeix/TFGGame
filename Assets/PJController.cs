using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PJController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public bool groundedPlayer;
    public float playerSpeed = 2.0f;
    Vector3 target = Vector3.zero;
    Vector3 move;

    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = (Vector3.Distance(target, transform.position) > 1f)? target : Vector3.zero;
        controller.Move(move * Time.deltaTime * playerSpeed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void RecieveInput(Vector3 o, Vector3 d)
    {
        MoveOrder(ThrowMouseRay(o, d));
    }

    void MoveOrder((GameObject,Vector3) input)
    {
        target = input.Item2 + (Vector3.up * controller.height);
        if (input.Item1 != null)
        {
            if (input.Item1.tag.Equals("SPECIAL EFFECTS HERE"))
            {
                move =  input.Item2;
            }
            else move = (input.Item2 - transform.position).normalized;
        }
        else move = input.Item2;
    }

    (GameObject, Vector3) ThrowMouseRay(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 500))
        {
            return (hit.collider.gameObject, hit.point);
        }
        return (null, Vector3.zero);
    }
}
