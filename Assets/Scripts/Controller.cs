using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEditor.SceneManagement;
using UnityEngine.PlayerLoop;
using Random = System.Random;

public class Controller : Agent
{
    private Rigidbody _rigidbody;
    public float moveSpeed = 10f;
    public float rotationSpeed = 10f;

    private Vector3 _lastPosition;
    
    public Transform goal;

    public override void Initialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        Vector3 startPos = new Vector3(-8.5f, 0.5f, 0f);
        transform.localPosition = startPos;
        _lastPosition = startPos;
        transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        /*goal.localPosition = new Vector3(
            UnityEngine.Random.Range(-6f, 0f),
            0.5f,
            UnityEngine.Random.Range(-3f, 3f)
        );*/
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        
        AddReward(4f * DistanceChange());
        _lastPosition = transform.localPosition;

        // Forward velocity
        Vector3 velocity = transform.forward * vectorAction[0] * moveSpeed;
        _rigidbody.velocity = velocity;
        _rigidbody.AddForce(velocity);
        
        // Rotation
        Vector3 rotation = transform.rotation.eulerAngles + Vector3.up * vectorAction[1] * rotationSpeed;
        _rigidbody.MoveRotation(Quaternion.Euler(rotation));
        
        // Add reward neg proportional (?) to distance
        // AddReward(-0.05f*GoalDistance());

    }
    

    public override void Heuristic(float[] actionsOut)
    {
        float forwardValue = 0f;
        float rotationValue = 0f;
        
        if (Input.GetKey(KeyCode.W)) forwardValue = 1f;
        if (Input.GetKey(KeyCode.S)) forwardValue = -1f;
        
        if (Input.GetKey(KeyCode.D)) rotationValue = 1f;
        if (Input.GetKey(KeyCode.A)) rotationValue = -1f;

        
        actionsOut[0] = forwardValue;
        actionsOut[1] = rotationValue;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 position = transform.localPosition / 10f;
        Vector3 rotation = transform.localRotation.eulerAngles / 360f;
        Vector3 velocity = _rigidbody.velocity / 4;
        // Vector3 angularVelocity = _rigidbody.angularVelocity;
        Vector3 goalPosition = goal.localPosition / 10f;
        
        sensor.AddObservation(position);
        sensor.AddObservation(rotation);
        sensor.AddObservation(velocity);
        // sensor.AddObservation(angularVelocity);
        sensor.AddObservation(goalPosition);
        
        // Debug.Log($"Position {position}");
        // Debug.Log($"Rotation {rotation}");
        // Debug.Log($"Velocity {velocity}");
        // Debug.Log($"Angular {angularVelocity}");
        // Debug.Log($"Goal {goalPosition}");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            //AddReward(5f);
            Debug.Log("Got the goal!");
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Obstacle"))
        {
            AddReward(-3f);
            Debug.Log("Collision!");
        }
    }

    private float GoalDistance()
    {
        return Vector3.Distance(transform.localPosition, goal.localPosition);
    }

    private float DistanceChange()
    {
        float currentDistance = GoalDistance();
        float previousDistance = Vector3.Distance(_lastPosition, goal.localPosition);

        return previousDistance - currentDistance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
