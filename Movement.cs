using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public float topSpeed = 20;
    public float acceleration = 15;
    public float turnSpeed = 45;
    public float turnAcceleration = 10;
    public float jumpForce = 10;

    float speedReal;
    float turnReal;
    float yRote;
    float jumpMagnitude;

    public float baseAltitude;
    public float speedAnimMult = 1;
    public float pointRate;

    Rigidbody rb;
    Animator anim;
    Player player;

    RigidbodyConstraints constraints;

    public ParticleSystem speedLines;
    public Animator cameraAnim;
    public Animator cameraAnimReal;
    public GroundCheck check;
    public Slider speedSlider;

    bool playing;
    bool controlling;
    bool canSlashBoss;
    bool dead;
    bool jumping;
    bool canDive;
    bool diveBlock;
    bool eventState;
    bool stiffStick;
    public bool engaging;
    public bool animVelocityMult = true;

    Transform cameraTransform;

    public Camera rearView;
    public Camera mainCamera;

    float cameraMagnitude;
    public float fovMult = 0.6f;
    public float zoomMult = 0.1f;
    public float cameraAcceleration = 1;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();

        cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        engaging = false;

        cameraAnimReal = cameraAnim;

        constraints = rb.constraints;
    }

    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            if (rb.velocity.y < 0 && jumping)
            {
                jumping = false;
            }

            float speedTarget = topSpeed - ((Mathf.Abs(turnReal) / 45) * (topSpeed * 0.1f));

            if (playing && !eventState)
            {
                float tempAcceleration = acceleration;
                if (stiffStick)
                    tempAcceleration = 10;

                if (speedReal < speedTarget)
                    speedReal += tempAcceleration * Time.deltaTime;
                else
                    speedReal -= tempAcceleration * 0.1f * (speedReal - speedTarget) * Time.deltaTime;
                
                    rb.velocity = (transform.forward * speedReal) + (Vector3.up * rb.velocity.y);
            }
            else
            {
                rb.velocity = Vector3.zero;
            }

            if (controlling && !eventState)
            {
                if (!engaging)
                {
                    turnReal += Time.deltaTime * turnAcceleration * ((turnSpeed * Input.GetAxis("Horizontal")) - turnReal);
                    if (stiffStick)
                        turnReal = Mathf.Clamp(turnReal, -turnAcceleration * 0.8f, turnAcceleration * 0.8f);
                    transform.Rotate(0, turnReal * Time.deltaTime, 0);
                }

                player.AddPoint(pointRate * speedReal * speedReal * Time.deltaTime);

                if (Input.GetButtonDown("Jump") && check.Touching())
                {
                    anim.SetTrigger("Jump");
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
                    jumpMagnitude = 1;
                    jumping = true;
                }

                if (Input.GetButtonDown("Jump") && !check.Touching() && canDive && !diveBlock && !player.attacking)
                {
                    anim.SetTrigger("Dive");
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(-jumpForce * Vector3.up, ForceMode.Impulse);
                    canDive = false;
                }
            }

            if (engaging)
            {
                yRote = transform.eulerAngles.y;
                if (yRote > 180)
                    yRote -= 360;
                yRote = Mathf.Clamp(yRote, -15, 15);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRote, transform.eulerAngles.z);

                if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
                {
                    turnReal += Time.deltaTime * turnAcceleration * ((turnSpeed * Input.GetAxis("Horizontal")) - turnReal);
                    transform.Rotate(0, turnReal * Time.deltaTime, 0);
                }
                else
                {
                    turnReal = 0;
                    transform.Rotate(0, Time.deltaTime * turnAcceleration * (-yRote / 10), 0);
                }
            }
            else if (eventState)
            {
                rb.velocity = Vector3.zero;
            }
        }

        if (dead)
        {
            cameraTransform.LookAt(transform.position + Vector3.up);
            if (speedLines.isPlaying)
                speedLines.Stop();
        }
    }

    void FixedUpdate()
    {
        if(speedReal > 40)
        {
            var emission = speedLines.emission;
            emission.rateOverTime = (speedReal - 40) * 2.5f;

            if (!speedLines.isPlaying)
                speedLines.Play();
        }
        else
        {
            if (speedLines.isPlaying)
                speedLines.Stop();
        }

        if (speedReal > 10 && playing)
            cameraMagnitude += (speedReal - cameraMagnitude) * cameraAcceleration;
        else
            cameraMagnitude += (10 - cameraMagnitude) * cameraAcceleration;

        mainCamera.fieldOfView = (60) + fovMult * (cameraMagnitude - 10);
        mainCamera.transform.localPosition = Vector3.forward * zoomMult * (cameraMagnitude - 10);


        if (jumpMagnitude != 0)
        {
            jumpMagnitude -= (jumpMagnitude / Mathf.Abs(jumpMagnitude) * Time.deltaTime * 2);
            if (Mathf.Abs(jumpMagnitude) < 0.1f)
                jumpMagnitude = 0;

            anim.SetFloat("JumpMagnitude", jumpMagnitude);
        }

        anim.SetFloat("Speed", speedReal);
        anim.SetFloat("Turn", turnReal);
        anim.SetFloat("Upward", rb.velocity.y);
        if(!anim.GetBool("Grounded") && check.Touching())
            jumpMagnitude = -1;
        anim.SetBool("Grounded", check.Touching());

        if (check.Touching())
            canDive = true;

        speedSlider.value = topSpeed;

        if (cameraAnim != null)
        {
            cameraAnim.SetFloat("Turn", turnReal);
            cameraAnim.SetFloat("Upward", rb.velocity.y);
        }

        float animSpeed = speedReal * speedAnimMult;
        if(animSpeed < 1 || !check.Touching() || jumping || player.attacking)
            anim.speed = 1;
        else if(animVelocityMult)
            anim.speed = (speedReal * speedAnimMult);

        if (transform.position.y < baseAltitude - 5 && !dead)
        {
            playing = false;
            player.Die();
        }
    }

    public void StartPlaying()
    {
        playing = true;
        cameraAnim.SetTrigger("Behind");
        //cameraAnim.speed = 5;

        topSpeed *= 0.5f;
    }

    public void StartControlling()
    {
        controlling = true;
        cameraAnim.SetTrigger("Start");
        //cameraAnim.speed = 1;

        topSpeed *= 2;
    }

    public void ChangeSpeed(float _difference, bool _directEffect)
    {
        topSpeed += _difference;
        if (_directEffect)
            speedReal = topSpeed;
        //speedReal *= 0.5f;
        acceleration += (_difference * 0.25f);
        turnSpeed += (_difference * 0.5f);
        turnAcceleration += (_difference * 0.5f);
        topSpeed = Mathf.Clamp(topSpeed, 0, 65);
    }

    public void StiffStick(bool _stiff)
    {
        stiffStick = _stiff;
    }

    public void Die()
    {
        dead = true;
        Destroy(rearView.gameObject);
        //cameraTransform = FindObjectOfType<Camera>().transform;
        cameraAnim.transform.parent = null;
        Destroy(cameraAnim);
    }

    public void StartBossAttack()
    {
        controlling = false;
        eventState = true;
        speedReal = 0;
        cameraAnimReal.gameObject.SetActive(true);
        cameraAnim = cameraAnimReal;
        cameraAnim.SetTrigger("BossSlash");
        engaging = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        anim.speed = 1;
    }

    public void CanSlash()
    {
        canSlashBoss = true;
    }

    public void CantSlash()
    {
        canSlashBoss = false;
    }

    public void TogglePlayer(bool _active)
    {
        controlling = _active;
        if (controlling)
        {
            rb.constraints = constraints;
            if (eventState)
            {

            }
            eventState = false;
        }
    }

    public void AnimVelocityToggle(bool _av)
    {
        animVelocityMult = _av;
        anim.speed = 1;
    }

    public float GetSpeed()
    {
        return topSpeed;
    }

    public void ToggleCantDive(bool _diveBlock)
    {
        diveBlock = _diveBlock;
    }
}
