using UnityEngine;

public class Boss : MonoBehaviour
{
    public string bossName = "Wyrm";

    public float speed = 35;
    public float turn = 1.5f;
    Transform target;
    float speedOffset;

    int targetIdx;

    public float hoverDistance = 10;

    public float engageDistance = 20;
    float distToPlayer;

    Transform playerTransform;
    Animator anim;
    GameManager gm;
    TrackManager tm;

    bool engaged;

    public int maxHealth;
    int health;

    public bool bossSlash;
    // Start is called before the first frame update
    void Awake()
    {
        gm = GameManager.gm;

        health = maxHealth;

        playerTransform = FindObjectOfType<Player>().transform;
        anim = GetComponent<Animator>();

        tm = gm.tm;
        targetIdx = (tm.allTracks - 1);

        FindTarget();

        Vector3 newPos = new Vector3(transform.position.x, hoverDistance, transform.position.z);

        newPos -= transform.forward * speed * 0.75f;
        transform.position = newPos;

        if (gm.controlling)
            transform.LookAt(target.position + (Vector3.up * hoverDistance));
    }

    void FixedUpdate()
    {
        distToPlayer = Vector3.Distance(playerTransform.position, transform.position - (Vector3.up * hoverDistance));
        if(distToPlayer < engageDistance && gm.controlling && !engaged)
        {
            Debug.Log("Engaging");
            engaged = true;
            gm.EngageBoss();
            anim.SetTrigger("Engage");
            SendMessage("StartAttacking");
        }

        Vector3 targetPos = Vector3.zero;
        float distance = 0;
        if (!engaged)
        {
            targetPos = (target.position + (Vector3.up * hoverDistance));
            distance = Vector3.Distance(targetPos, transform.position);

            Vector3 targetDirection = targetPos - transform.position;
            float singleStep = turn * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else if (distToPlayer < engageDistance * 3)
        {
            speedOffset = ((engageDistance * 3) - distToPlayer) * 3;
        }
        else
            speedOffset = 0;

        if (distance < 4 && !engaged)
        {
            FindTarget();
        }
        else
        {
            if(!bossSlash)
                transform.Translate(0, 0, (speed + speedOffset) * Time.deltaTime);
        }
    }

    void FindTarget()
    {
        if (engaged)
            return;

        TrackSegment[] allTracks = FindObjectsOfType<TrackSegment>();
        Transform tempTarget = null;
        for (int i = 0; i < allTracks.Length; i++)
        {
            if (allTracks[i].thisIdx == targetIdx)
                tempTarget = allTracks[i].spawnPoint;
        }

        if (tempTarget != null)
        {
            targetIdx++;
            target = tempTarget;
        }
    }

    public void ChangeHealth(int _difference)
    {
        if (_difference < 0)
            anim.SetTrigger("Hit");

        health += _difference;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health <= 0)
        {
            anim.SetTrigger("Die");
            gm.BossDies();
        }
        else if(health < maxHealth - 2 && !bossSlash)
        {
            gm.StartBossSlashSequence();
            bossSlash = true;
            SendMessage("EndAttacking");
        }
    }

    public float GetDistance()
    {
        return distToPlayer;
    }

    public void Slash()
    {
        ChangeHealth(-1);
    }
}
