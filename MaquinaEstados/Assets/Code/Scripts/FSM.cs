using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    private enum FSMStates
    {
        Patrol, Chase, Aim, Shoot, Evade
    }

    [SerializeField]
    private FSMStates currentState = FSMStates.Patrol;
    private int health = 100;
    private Vector3 destPos;

    public GameObject bullet;
    public Transform playerTransform;
    public GameObject bulletSpawnPoint;
    public List<GameObject> pointList;
    public float curSpeed;
    public float rotSpeed = 150.0f;
    public float turretRotSpeed = 10.0f;
    public float maxForwardSpeed = 30.0f;
    public float maxBackwardSpeed = -30.0f;
    public float shootRate = 0.5f;
    private float elapsedTime;
    public float patrolRadius = 10f;
    public float chaseRadius = 25f;
    public float AttackRadius = 20f;
    private int index = -1;
    //public float aimTime = 1.0f;
    //public float elapsedAimTime;
    public bool tookDamage = false;
    public float elapsedEvadeTime;
    public float evadeTime = 3.0f;
    public Vector3 escapePoint;
    //Nuevos
    public float AlertRange;
    public LayerMask PlayerCape;
    public Transform Player;
    public float velocity;
    bool StayAlert;

    // Start is called before the first frame update
    void Start()
    {
        FindNextPoint();
    }

    private void FindNextPoint() 
    {
        print("Finding next point");
        index = (index+1)%pointList.Count; //Random.Range(0, pointList.Count);
        destPos = pointList[index].transform.position;
    }

    void Update()
    {
        if(health <= 0) {
            Destroy(gameObject);
        }
        switch(currentState)
        {
            case FSMStates.Patrol:
                UpdatePatrol();
                break;
                
            case FSMStates.Chase:
                UpdateChase();
                break;
                
            case FSMStates.Aim:
                UpdateAim();
                break;

            case FSMStates.Shoot:
                UpdateShoot();
                break;

            case FSMStates.Evade:
                UpdateEvade();
                break;
        }
    }

    void UpdatePatrol()
    {
        //Find another random patrol point if the current point is reached
        if (Vector3.Distance(transform.position, destPos) <= patrolRadius) 
        {
            print("Reached the destination point -- calculating the next point");
            FindNextPoint();
        }
        //Check the distance with player tank, when the distance is near, transition to chase state
        else if (Vector3.Distance(transform.position, playerTransform.position) <= chaseRadius) 
        {
            print("Switch to Chase state");
            currentState = FSMStates.Chase;
        }

        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    // Puntos 2 y 6 de la actividad de tanques
    void UpdateChase()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if(distance <= AttackRadius) {
            print("Switch to Aim state");
            currentState = FSMStates.Aim;
        }
        else if (distance >= chaseRadius) {
            print("Switch to Patrol state");
            currentState = FSMStates.Patrol;
        }
        else if(distance <= chaseRadius) {
            Quaternion targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
        }
        if(tookDamage) {
            tookDamage = false;
            int xSign = (int)Mathf.Sign(playerTransform.position.x - transform.position.x);
            int zSign = (int)Mathf.Sign(playerTransform.position.z - transform.position.z);
            print("xSign is: " + xSign);
            print("zSign is: " + zSign);
            if(xSign == -1 && zSign == 1)
                escapePoint = new Vector3(transform.position.x - Random.Range(10,15), 0, transform.position.z + Random.Range(5,10));
            else if(xSign == 1 && zSign == 1)
                escapePoint = new Vector3(transform.position.x + Random.Range(10,15), 0, transform.position.z + Random.Range(5,10));
            else if(xSign == -1 && zSign == -1)
                escapePoint = new Vector3(transform.position.x - Random.Range(10,15), 0, transform.position.z - Random.Range(5,10));
            else if(xSign == 1 && zSign == -1)
                escapePoint = new Vector3(transform.position.x + Random.Range(10,15), 0, transform.position.z - Random.Range(5,10));
            print("Switch to Evade state");
            currentState = FSMStates.Evade;
        }
        /*
        StayAlert = Physics.CheckSphere(transform.position, AlertRange, PlayerCape);

        if(StayAlert == true) {
            Vector3 posPlayer = new Vector3(Player.position.x, transform.position.y, Player.position.z);
            transform.LookAt(posPlayer);
            transform.position = Vector3.MoveTowards(transform.position, posPlayer, velocity * Time.deltaTime);
        }
        */
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,AlertRange);
    }

    void UpdateAim()
    {

    }

    void UpdateShoot()
    {

    }

    void UpdateEvade()
    {
        if(elapsedEvadeTime >= evadeTime)
        {
            print("Switch to Chase state");
            currentState = FSMStates.Chase;
        }
        else
        {
            elapsedEvadeTime += Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - escapePoint);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Bullet")
        {
            health -= col.gameObject.GetComponent<BulletController>().damage;
            tookDamage = true;
        }
    }

    private void FixedUpdate() 
    {
        
    }
}
