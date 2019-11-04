using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{

    private RayPerception m_RayPer;
    private Rigidbody m_AgentRb;
    public Transform Target;    
    public bool useVectorObs;

    // Start is called before the first frame update
    void Start()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_RayPer = GetComponent<RayPerception>();
    }

    public override void AgentReset()
    {
        if (this.transform.position.y < 0)
        {
            // If the Agent fell, zero its momentum
            this.m_AgentRb.angularVelocity = Vector3.zero;
            this.m_AgentRb.velocity = Vector3.zero;
            this.transform.position = new Vector3(0, 0.5f, 0);
            this.transform.rotation = Quaternion.identity;
        }

        // Move the target to a new spot
        Target.position = new Vector3(Random.value * 14,
                                      0.5f,
                                      Random.value * 14);
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {
            const float rayDistance = 35f;
            float[] rayAngles = { 20f, 90f, 160f, 45f, 135f, 70f, 110f };
            float[] rayAngles1 = { 25f, 95f, 165f, 50f, 140f, 75f, 115f };
            float[] rayAngles2 = { 15f, 85f, 155f, 40f, 130f, 65f, 105f };

            string[] detectableObjects = { "goal" };
            AddVectorObs(m_RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
            AddVectorObs(m_RayPer.Perceive(rayDistance, rayAngles1, detectableObjects, 0f, 5f));
            AddVectorObs(m_RayPer.Perceive(rayDistance, rayAngles2, detectableObjects, 0f, 10f));
            //AddVectorObs(m_SwitchLogic.GetState());
            AddVectorObs(transform.InverseTransformDirection(m_AgentRb.velocity));
        }

    }

   public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.Continuous)
        {
            dirToGo = transform.forward * Mathf.Clamp(act[0], -1f, 1f);
            rotateDir = transform.up * Mathf.Clamp(act[1], -1f, 1f);
        }
        else
        {
            var action = Mathf.FloorToInt(act[0]);
            switch (action)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
                case 3:
                    rotateDir = transform.up * 1f;
                    break;
                case 4:
                    rotateDir = transform.up * -1f;
                    break;
            }
        }
        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * 0.8f, ForceMode.VelocityChange);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-1f / agentParameters.maxStep);
        MoveAgent(vectorAction);

        // Fell off platform
        if (this.transform.position.y < 0)
        {
            AddReward(-1f / agentParameters.maxStep);
            Done();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("goal"))
        {
            SetReward(1f);
            Done();
        }
    }

}
