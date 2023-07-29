using UnityEngine;

public class Sphereid : MonoBehaviour
{ // The small spherical enemies encountered in the game
    int randomNumb;
    bool intitialTrigger;

    Animator anim;

    Player playerScript;
    Transform playerTransform;

    public float closeDistance = 10;

    float dist;
    public bool dead;
    // Start is called before the first frame update
    public void Init(float _dist)
    {
        dist = _dist;

        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        playerTransform = playerScript.transform;

        anim = GetComponent<Animator>();

        randomNumb = Random.Range(0, 3);
        int randomPos = Random.Range(-1, 2);

        switch (randomNumb) 
        {
            case 0:
                MoveAside(randomPos);
                break;
            case 1:
                transform.rotation = GameManager.gm.transform.rotation;
                MoveAside(randomPos);
                break;
            case 2:
                bool right = (Random.Range(0, 2) == 1);
                if (right)
                {
                    MoveAside(1);
                    transform.LookAt(transform.position + transform.right);
                }
                else
                {
                    MoveAside(-1);
                    transform.LookAt(transform.position - transform.right);
                }
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (dead)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if(distance < closeDistance)
        {
            switch (randomNumb) 
            {
                case 0:
                    if (!intitialTrigger)
                        anim.SetTrigger("Chop");
                    else
                    {
                        Vector3 lookPos = transform.position + (transform.position - playerTransform.position);
                        transform.LookAt(lookPos);
                    }
                    break;
                case 1:
                    if (!intitialTrigger)
                        anim.SetTrigger("Saw");
                    break;
                case 2:
                    if (!intitialTrigger)
                        anim.SetTrigger("Fire");
                    break;
            }

            intitialTrigger = true;
        }
    }

    void MoveAside(int _direction)
    {
        transform.position += transform.right * _direction * dist;
    }

    void OnTriggerEnter(Collider _col)
    {
        if (_col.gameObject.tag == "Player" && !dead)
        {
            dead = true;
            anim.SetTrigger("End");
            Destroy(gameObject, 5);
        }
    }

    public void HitEnemy()
    {
        playerScript.AddPoint(50);
        dead = true;
        anim.SetTrigger("Explode");
        Destroy(gameObject, 5);
    }
}
