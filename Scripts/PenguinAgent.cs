using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;

public class PenguinAgent : Agent
{
    public GameObject heartPrefab;
    public GameObject regurgitatedFishPrefab;


    private PenguinArea penguinArea;
    [SerializeField]
    private Animator animator;
    private RayPerception3D rayPerception3d;
    private GameObject babyPenguin;
    private bool isFull;

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float forward = vectorAction[0];
        float leftOrRight = 0f;

        if (vectorAction[1] == 1f)
        {
            leftOrRight = -1f;
        }

        else if(vectorAction[1] == 2f)
        {
            leftOrRight = 1f;
        }

        animator.SetFloat("Vertical", forward);
        animator.SetFloat("Horizontal", leftOrRight);
        if (!isFull)
        {
            AddReward(-1f / agentParameters.maxStep);
        } else if (isFull)
        {
            AddReward(-2f / agentParameters.maxStep);
        }
    }

    public override void AgentReset()
    {
        isFull = false;
        penguinArea.ResetArea();
    }

    public override void CollectObservations()
    {
        AddVectorObs(isFull); // Has the penguin eaten

        AddVectorObs(Vector3.Distance(babyPenguin.transform.position, transform.position)); // Distance to the baby penguin

        AddVectorObs((babyPenguin.transform.position - transform.position)); // Direction to the baby penguin

        AddVectorObs(transform.forward); // Direction penguin is facing

        float rayDistance = 20f;
        float[] rayAngles = { 30f, 60f, 90f, 120f, 150f };
        string[] detectableObjects = { "baby", "fish", "wall" };

        AddVectorObs(rayPerception3d.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
    }

    private void Start()
    {
        penguinArea = GetComponentInParent<PenguinArea>();
        babyPenguin = penguinArea.penguinBaby;
        rayPerception3d = GetComponent<RayPerception3D>();
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, babyPenguin.transform.position) < penguinArea.feedRadius)
        {
            RegurgitateFish();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("fish"))
        {
            EatFish(collision.gameObject);
        }
        else if (collision.transform.CompareTag("baby"))
        {
            RegurgitateFish();
        }
    }

    private void EatFish(GameObject fishObject)
    {
        if (isFull)
        {
            return;
        }
        isFull = true;
        penguinArea.RemoveSpecificFish(fishObject);

        AddReward(1f);
    }

    private void RegurgitateFish()
    {
        if (!isFull)
        {
            return;
        }
        isFull = false;

        GameObject regurgitatedFish = Instantiate<GameObject>(regurgitatedFishPrefab);
        regurgitatedFish.transform.parent = transform.parent;
        regurgitatedFish.transform.position = babyPenguin.transform.position;
        Destroy(regurgitatedFish, 4f);

        GameObject heart = Instantiate<GameObject>(heartPrefab);
        heart.transform.parent = transform.parent;
        heart.transform.position = babyPenguin.transform.position + Vector3.up;
        Destroy(heart, 4f);

        AddReward(1f);

    }
}
