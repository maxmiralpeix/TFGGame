using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeRigidBody : MonoBehaviour
{
    public Vector3 velocity, angularVelocity;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Time.deltaTime * velocity;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + angularVelocity * Time.deltaTime);
    }
}
