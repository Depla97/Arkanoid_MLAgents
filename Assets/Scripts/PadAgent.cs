using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
public class PadAgent : Agent
{

    private BufferSensorComponent BallSensor;
    private BufferSensorComponent BonusSensor;
    private Pad MyPad;
    [SerializeField]
    private GameLogic logic;
    private int gameStat,counter;
    private Dictionary<string, float> bonusTypes;
    private void Start()
    {
        BallSensor = GetComponents<BufferSensorComponent>()[0];
        BonusSensor = GetComponents<BufferSensorComponent>()[1];
        MyPad = GetComponent<Pad>();
        //logic = GameObject.Find("GameLogic").GetComponent<GameLogic>();
        gameStat = 0;
        counter = 0;
        bonusTypes = new Dictionary<string, float>();
        bonusTypes.Add("MultiballBonus",0f);
        bonusTypes.Add("WidePadBonus",1f);
        bonusTypes.Add("StickyBonus",2f);
        bonusTypes.Add("LaserBonus",3f);
        
    }

    public void CatchBallScoring()
    {
        AddReward(10f);
        gameStat += 1;
        //Debug.Log("add 10 score: ball collided with the pad");
    }
    
    public void HitBrickScoring()
    {
        AddReward(15f);
        gameStat += 1;
        //Debug.Log("add 15 score: ball hit with the Brick");
    }

    public void BonusScoring(int weight = 1)
    {
        AddReward(weight*30f);
    }
    
    public override void OnEpisodeBegin(){
        GetComponent<Pad>().FireBallsInRandomDirections();
        counter++;
        if (counter > 5)
        {
            Debug.Log("TotalScore: "+gameStat);
            counter = 0;
            gameStat = 0;
            logic.ReloadLevel(1);
            
        }

    }

    public override void CollectObservations(VectorSensor sensor){
        //sensor.AddObservation(ballTransform.localPosition);
        sensor.AddObservation(this.gameObject.transform.localPosition);
        List<Ball> Balls = GetComponent<Pad>().LocalBalls;
        foreach (Ball ball in Balls)
        {
            //Every ball should have 4 variables: x,y position and x,y velocity
            float[] ballinfo = new float[4];
            ballinfo[0] = ball.getBody().transform.localPosition.x;
            ballinfo[1] = ball.getBody().transform.localPosition.y;
            //ballpos[2] = ball.getBody().transform.position.z; //z is always 0
            
            //since velocity is the same. we dont need to worry about it :)
            ballinfo[2] = ball.getBody().velocity.x;
            ballinfo[3] = ball.getBody().velocity.y;
            //Debug.Log("Ball info pos"+ballinfo[0] +" " + ballinfo[1]);
            
            BallSensor.AppendObservation(ballinfo);
        }

        foreach (var Bonus in MyPad.LocalBonus)
        {
            //Every ball should have 4 variables: x,y position and x,y velocity
            float[] bonusInfo = new float[4];
            
            bonusInfo[0] = bonusTypes[Bonus.tag];
            bonusInfo[1] = Bonus.transform.localPosition.x;
            bonusInfo[2] = Bonus.transform.localPosition.y;
            
            //since velocity is the same. we dont need to worry about it :)
            bonusInfo[3] = Bonus.GetComponent<Rigidbody2D>().velocity.y;
            
            BonusSensor.AppendObservation(bonusInfo);
        }

        

    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        float moveX = actionBuffers.ContinuousActions[0];
        this.gameObject.transform.localPosition += new Vector3(moveX, 0, 0) * Time.deltaTime * 30;

    }
    
    public void Death()
    {
        //Debug.Log("death registered");
        //TODO migliora penalita' in base alla distanza della "morte"
        AddReward(-150f);
        gameStat -= 1;
        //Debug.Log("Gamestat:"+gameStat);
        EndEpisode();
        
    }
        
}
