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
    private Vector3 _previousPosition;
    
    public float moveSpeed = 50f;
    public float rotationSpeed = 2f;

    public Transform goal;

    public override void Initialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // TODO: remove position observations, use only raycasts instead
        Vector3 startPos = new Vector3(-9f, 0.25f, UnityEngine.Random.Range(-9f, 9f));
        transform.localPosition = startPos;
        transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _previousPosition = startPos;


    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // TODO: change to discrete actions?
        // Forward velocity
        var linearSpeed = Mathf.Clamp(vectorAction[0], -0.3f, 1.0f);
        var angularSpeed = Mathf.Clamp(vectorAction[1], -1f, 1f);
        
        Vector3 velocity = transform.forward * linearSpeed * moveSpeed;
        // Debug.Log(vectorAction[0]);
        // _rigidbody.velocity = velocity;
        _rigidbody.AddForce(velocity);

        // Rotation
        // var rotateDir = transform.up * vectorAction[1];
        // transform.Rotate(rotateDir, Time.fixedDeltaTime * rotationSpeed);
        
        Vector3 rotation = transform.rotation.eulerAngles + Vector3.up * angularSpeed * rotationSpeed;
        _rigidbody.MoveRotation(Quaternion.Euler(rotation));
    }

    public override void Heuristic(float[] actionsOut)
    {
        var forwardValue = 0f;
        var rotationValue = 0f;
        
        if (Input.GetKey(KeyCode.W)) forwardValue = 1f;
        if (Input.GetKey(KeyCode.S)) forwardValue = -1f;
        
        if (Input.GetKey(KeyCode.D)) rotationValue = 1f;
        if (Input.GetKey(KeyCode.A)) rotationValue = -1f;

        
        actionsOut[0] = forwardValue;
        actionsOut[1] = rotationValue;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 position = transform.localPosition;
        Vector3 rotation = transform.localRotation.eulerAngles;
        Vector3 velocity = _rigidbody.velocity;
        // Vector3 angularVelocity = _rigidbody.angularVelocity;
        Vector3 goalPosition = goal.localPosition;
        
        sensor.AddObservation(position.x / 20f);
        sensor.AddObservation(position.z / 20f);
        sensor.AddObservation(rotation.y / 360f);
        // sensor.AddObservation(velocity.x);
        // sensor.AddObservation(velocity.z);
        // sensor.AddObservation(angularVelocity);
        // sensor.AddObservation(goalPosition);
        
        // Debug.Log($"Position {position}");
        // Debug.Log($"Rotation {rotation}");
        // Debug.Log($"Velocity {velocity}");
        // Debug.Log($"Angular {angularVelocity}");
        // Debug.Log($"Goal {goalPosition}");
        // Debug.Log($"Reward {GetCumulativeReward()}");
        
        // Compute the distance-based reward
        var prevDistance = Vector3.Distance(_previousPosition, goalPosition);
        var currentDistance = Vector3.Distance(position, goalPosition);
        var diff = prevDistance - currentDistance;
        AddReward(2f * diff);  // Add reward for getting closer to the goal
        AddReward(-1e-2f);  // Small penalty at each step
        // Debug.Log($"Distance {currentDistance}");
        // Debug.Log($"Distance difference {diff}");

        _previousPosition = position;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            AddReward(2f);
            Debug.Log("Got the goal!");
            EndEpisode();
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.collider.CompareTag("Obstacle") || other.collider.CompareTag("Agent"))
        {
            AddReward(-0.5f);
            Debug.Log($"Collision with an {other.collider.tag}!");
        }
    }

}
