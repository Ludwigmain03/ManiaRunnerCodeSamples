using UnityEngine;

public class Wyrm : MonoBehaviour
{
    public GameObject wyrmholePrefab;
    public float attackInterval = 3;
    float waitReal;

    public GameObject swipePrefab;
    public Rigidbody bombPrefab;

    public Transform headTransform;

    Animator anim;
    Boss boss;

    bool attacking;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        boss = GetComponent<Boss>();
        GameObject newHole = Instantiate(wyrmholePrefab, transform.position, transform.rotation);
        Destroy(newHole, 10);
    }

    void FixedUpdate()
    {
        if (attacking)
        {
            waitReal -= Time.deltaTime;
            if (waitReal < 0)
            {
                float distance = boss.GetDistance();

                if (distance < boss.engageDistance * 3)
                {
                    anim.SetTrigger("Bomb");
                }
                else if (distance < 75)
                {
                    int idx = Random.Range(0, 3);
                    anim.SetTrigger("Fire" + idx);
                }
                else
                {
                    anim.SetTrigger("Swipe");
                }

                waitReal = attackInterval;
            }
        }
    }

    public void StartAttacking()
    {
        attacking = true;
        waitReal = attackInterval;
    }

    public void SwipeAttack()
    {
        Vector3 offset = Vector3.up * (1 - boss.hoverDistance);
        GameObject swipeAttack = Instantiate(swipePrefab, transform.position + offset, transform.rotation);
        //Destroy(swipeAttack, 30);
    }

    public void BombAttack()
    {
        Vector3 offset = Vector3.up * (1 - boss.hoverDistance);
        Rigidbody bomb = Instantiate(bombPrefab, headTransform.position, transform.rotation);
        bomb.velocity = (Vector3.forward * boss.speed) + (Vector3.right * Random.Range(-10.0f, 10.0f));
        Destroy(bomb.gameObject, 10);
    }

    public void EndAttacking()
    {
        attacking = false;
    }
}