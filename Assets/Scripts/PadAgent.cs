using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;
public class PadAgent : Agent
{

    private BufferSensorComponent BuffSensor;
    private Pad MyPad;
    private void Start()
    {
        BuffSensor = GetComponent<BufferSensorComponent>();
        MyPad = GetComponent<Pad>();
    }

    public void CatchBallScoring()
    {
        AddReward(10f);
        Debug.Log("add 10 score: ball collided with the pad");
    }
    public override void OnEpisodeBegin(){
        GetComponent<Pad>().FireBallsInRandomDirections();
    }

    public override void CollectObservations(VectorSensor sensor){
        //sensor.AddObservation(ballTransform.localPosition);
        sensor.AddObservation(this.gameObject.transform.localPosition);
        //OUCH! Expensive!
        //TODO:append existing balls to an array instead of using FindObject
        Ball[] Balls = Object.FindObjectsOfType<Ball>(); 
        foreach (Ball ball in Balls)
        {
            //Every ball should have 4 variables: x,y position and x,y velocity
            float[] ballinfo = new float[4];
            ballinfo[0] = ball.getBody().transform.position.x/18f;
            ballinfo[1] = ball.getBody().transform.position.y/18f;
            //ballpos[2] = ball.getBody().transform.position.z; //z is always 0
            
            //since velocity is the same. we dont need to worry about it :)
            ballinfo[2] = ball.getBody().velocity.x;
            ballinfo[3] = ball.getBody().velocity.y;
            //Debug.Log("Ball info pos"+ballinfo[0] +" " + ballinfo[1]);
            
            BuffSensor.AppendObservation(ballinfo);
        }

    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        float moveX = actionBuffers.ContinuousActions[0];
        this.gameObject.transform.localPosition += new Vector3(moveX, 0, 0) * Time.deltaTime * 30;

    }

    public void Death()
    {
        //Debug.Log("death registered");
        AddReward(-50f);
        EndEpisode();
    }
        
}
