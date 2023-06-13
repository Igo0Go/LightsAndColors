using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ConstructionDetailScript : MonoBehaviour
{
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public Quaternion targetRotation;

    /// <summary>
    /// Запомнить положение детали
    /// </summary>
    public void CheckTarget()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if(Vector3.Distance(transform.position, targetPosition) > 100) //если деталь вышла из зоны сборки
        {
            rb.velocity = Vector3.zero;
            transform.position = targetPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Finish"))    //если деталь упала в запретную зону
        {
            rb.velocity = Vector3.zero;
            transform.position = targetPosition;
        }
    }
}
