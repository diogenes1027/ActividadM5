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
    public Transform gun;

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
    public bool tookDamage = false;
    public float elapsedEvadeTime;
    public float evadeTime = 3.0f;
    public Vector3 escapePoint;
    //Nuevos
    public float AlertRange;

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
        //Si la vida del tanque y de los enemigos llega a 0, destruirlos.
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

    //Punto 1 de la actividad.
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

    // Puntos 2 y 6 de la actividad.
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
            int SignX = (int)Mathf.Sign(playerTransform.position.x - transform.position.x);
            int SignZ = (int)Mathf.Sign(playerTransform.position.z - transform.position.z);
            print("SignX is: " + SignX);
            print("SignZ is: " + SignX);
            if(SignX == -1 && SignZ == 1)
                escapePoint = new Vector3(transform.position.x - Random.Range(10,15), 0, transform.position.z + Random.Range(5,10));
            else if(SignX == 1 && SignZ == 1)
                escapePoint = new Vector3(transform.position.x + Random.Range(10,15), 0, transform.position.z + Random.Range(5,10));
            else if(SignX == -1 && SignZ == -1)
                escapePoint = new Vector3(transform.position.x - Random.Range(10,15), 0, transform.position.z - Random.Range(5,10));
            else if(SignX == 1 && SignZ == -1)
                escapePoint = new Vector3(transform.position.x + Random.Range(10,15), 0, transform.position.z - Random.Range(5,10));
            print("Switch to Evade state");
            currentState = FSMStates.Evade;
        }
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,AlertRange);
    }

    //Punto 3 de la actividad.
    void UpdateAim()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) <= chaseRadius)
        {
            Quaternion targetRotationg = Quaternion.LookRotation(playerTransform.position - gun.position);
            gun.rotation = Quaternion.Slerp(gun.rotation, targetRotationg, Time.deltaTime * rotSpeed);

            elapsedTime += Time.deltaTime;
            if (elapsedTime >= shootRate)
            {
                currentState = FSMStates.Shoot;
            }
        }
        else currentState = FSMStates.Patrol;

    }

    //Punto 4 de la actividad.
    void UpdateShoot()
    {
        Instantiate(bullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
        elapsedTime = 0.0f;
        currentState = FSMStates.Patrol;
    }

    //Punto 5 de la actividad.
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
