using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

using System.Collections.Generic;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Pad : MonoBehaviour
{   
    
    public static event Action OnPadHitsBall;
    public static event Action<string> OnBonusPickup;
    public static event Action OnLostLife;



    [SerializeField]
    float maxAcceleration;

    [SerializeField]
    float maxSpeed;

    [SerializeField]
    GameObject ballPrefab;

    [SerializeField]
    float ballFireAngleRange;

    [SerializeField]
    GameObject laserBeamPrefab;

    [SerializeField]
    float laserTime;

    [SerializeField]
    float laseFireRate;

    [SerializeField]
    BonusesLogic bonusLogic;

    [SerializeField]
    float widePadTime;

    SpriteRenderer spriteRenderer; // needed for widening the pad + determining size of the pad for lasers
    Animator animator; // needed to change the animation based on whether we have laser or not

    float velocity;

    // private vars related to bonuses:
    bool glueBall;
    bool useLaser;
    bool useWidePad;
    float laserActiveTime;
    float laserCanFireTime;
    float widePadActiveTime;

    List<Ball> ballsOnPad;
    

    public List<Ball> LocalBalls;
    public List<GameObject> LocalBonus;
    
    // private void OnTriggerEnter2D(Collider other){
    //     if(other.CompareTag("Ball")){
    //         AddReward(20f);
    //         EndEpisode();
    //     }
    // }

    void Start()
    {
        velocity = 0f;

        // Bonuses:
        glueBall = true;
        useLaser = false;
        useWidePad = false;
        laserActiveTime = 0f;
        laserCanFireTime = 0f;
        widePadActiveTime = 0f;

        ballsOnPad = new List<Ball>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        LocalBonus = new List<GameObject>();
        SpawnBallOnPad();
    }

    void FixedUpdate()
    {
        //float userInput = Input.GetAxis("Horizontal");

        // One way is to treat input as desired velocity
        // and use maxAcceleration to control sensitivity:
        //
        // desiredVelocity = userInput * maxSpeed;
        // float maxSpeedChange = maxAcceleration * Time.deltaTime;
        // velocity = Mathf.MoveTowards(velocity, desiredVelocity, maxSpeedChange);

        // Another way is to directly control velocity:
        //
        // velocity = userInput * maxSpeed; // 35 seems to work fine

        // And the third way is just to use -1 or 1 for velocity:
        float userInput;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Z))
            userInput = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.X))
            userInput = 1f;
        else
            userInput = 0;

        velocity = userInput * maxSpeed;

        var displacement = velocity * Time.fixedDeltaTime;
        var pos = transform.localPosition;
        var newX = pos.x + displacement;

        float padRange = useWidePad ? 14.2f : 15.5f;
        newX = Mathf.Clamp(newX, -padRange, padRange);

        var newPos = new Vector3(newX, pos.y, 0);

        transform.localPosition = newPos;

        foreach (Ball ball in LocalBalls)
        {
            if (ball.getBody().velocity.magnitude == 0)
            {
                Debug.Log("ball bad velocity");
            }
        }

        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("MultiballBonus"))
        {
            bonusLogic.SpawnMultiBalls(this);
            OnBonusPickup?.Invoke(collision.gameObject.tag);
            LocalBonus.Remove(collision.gameObject);
            GetComponent<PadAgent>().BonusScoring("MultiballBonus");
        }
        else if (collision.gameObject.CompareTag("WidePadBonus"))
        {
            WidenPad();
            OnBonusPickup?.Invoke(collision.gameObject.tag);
            LocalBonus.Remove(collision.gameObject);
            GetComponent<PadAgent>().BonusScoring("WidePadBonus");
        }
        else if (collision.gameObject.CompareTag("StickyBonus"))
        {
            MakeSticky();
            OnBonusPickup?.Invoke(collision.gameObject.tag);
            LocalBonus.Remove(collision.gameObject);
            GetComponent<PadAgent>().BonusScoring("StickyBonus");
        }
        else if (collision.gameObject.CompareTag("LaserBonus"))
        {
            UseLaser();
            OnBonusPickup?.Invoke(collision.gameObject.tag);
            LocalBonus.Remove(collision.gameObject);
            GetComponent<PadAgent>().BonusScoring("LaserBonus");
        }
        
        Destroy(collision.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Ball ball;

        if (collision.gameObject.TryGetComponent<Ball>(out ball))
        {
            if (glueBall && !ballsOnPad.Contains(ball) && HitsFromTop(collision))
            {
                ball.GlueToPad(this.gameObject);
                ballsOnPad.Add(ball);
            }
            else if (!ballsOnPad.Contains(ball))
            {
                OnPadHitsBall?.Invoke();
                GetComponent<PadAgent>().CatchBallScoring();
            }
        }
    }

    bool HitsFromTop(Collision2D collision)
    {
        const float ANGLE_TOLERANCE = 3f;
        if (collision.contactCount > 0)
        {
            ContactPoint2D contact = collision.GetContact(0);
            float angle = Vector2.Angle(contact.normal, Vector2.down);
            return angle < ANGLE_TOLERANCE;
        }

        return false;
    }

    void MakeSticky()
    {
        //dont need glue ball yet
        glueBall = true;
    }

    void WidenPad()
    {
        useWidePad = true;
        spriteRenderer.size = new Vector2(6f, 1f);
        widePadActiveTime = Time.time + widePadTime;
    }

    void UseLaser()
    {
        useLaser = true;
        animator.SetBool("Has Laser", useLaser);
        laserActiveTime = Time.time + laserTime;
    }

    void PowerDownWidePad()
    {
        useWidePad = false;
        Bounds bounds = spriteRenderer.sprite.bounds;
        var defaultWidth = bounds.extents.x / bounds.extents.y;
        spriteRenderer.size = new Vector2(defaultWidth, 1f);
    }

    void PowerDownLaser()
    {
        useLaser = false;
        animator.SetBool("Has Laser", useLaser);
    }

    public void FireBallsInRandomDirections()
    {
        
        glueBall = false;

        foreach (Ball b in this.ballsOnPad)
        {
            float angle = Random.Range(-ballFireAngleRange, ballFireAngleRange);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up; 
            b.Fire(direction);
        };

        this.ballsOnPad = new List<Ball>();
    }

    void SpawnBallOnPad()
    {
        GameObject ballObj = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
        ballObj.transform.parent = transform;
        Ball ball = ballObj.GetComponent<Ball>();
        //LocalBalls.Add(ball);
        
        ball.AssignPad(this);

        ballObj.transform.localPosition = ball.CenterOnPad(this.gameObject);
        ballsOnPad.Add(ball);
        //FireBallsInRandomDirections();
    }

    void FireLaser()
    {
        laserCanFireTime = Time.time + laseFireRate;

        float height = spriteRenderer.sprite.bounds.size.y;
        float width = height * spriteRenderer.size.x;

        Vector3 leftPos = new Vector3(-width / 2f + 0.18f, height - 0.2f, 0f);
        Vector3 rightPos = new Vector3(width / 2f - 0.18f, height - 0.2f, 0f);

        Instantiate(laserBeamPrefab, transform.position + leftPos, Quaternion.identity);
        Instantiate(laserBeamPrefab, transform.position + rightPos, Quaternion.identity);
    }

    void HandleOnLostLife()
    {
        SpawnBallOnPad();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (ballsOnPad.Count == 0)
                SpawnBallOnPad();
        }

        if (useLaser && Time.time > laserActiveTime)
        {
            PowerDownLaser();
        }

        if (useWidePad && Time.time > widePadActiveTime)
        {
            PowerDownWidePad();
        }
        
    }

    public void CheckLostBall(Ball ball)
    {
        //Debug.Log(LocalBalls.Count);
        if (LocalBalls.Count==0)
        {
            OnLostLife?.Invoke();
            GetComponent<PadAgent>().Death(ball.transform.localPosition);
            HandleOnLostLife();
        }
    }

    public List<Ball> getBalls()
    {
        return this.ballsOnPad;
    }


    public void Fire(){
        FireBallsInRandomDirections();
        if (useLaser && Time.time > laserCanFireTime)
                FireLaser();
    }
}