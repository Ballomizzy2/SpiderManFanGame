using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class PlayerController : MonoBehaviour
{
    private const float GRAVITY = -9.8f;

    //Editor References
    [SerializeField]
    private Transform spidey, pivoter;
    [SerializeField]
    public Transform webSpawner;
    [SerializeField]
    LineRenderer webSample;
    [SerializeField]
    GameObject swingHelperGO;
    [SerializeField]
    private GameObject ragdoll;
    [SerializeField]
    private CinemachineVirtualCamera vCam, vDeadCam, vDiveCam;
    [SerializeField]
    private Transform pointer;

    // Web / Swinging
    LineRenderer currentWeb;
    Rigidbody rb;
    Rigidbody swingHelper;
    public HingeJoint webJoint;
    LineRenderer webSupport;

    [SerializeField]
    Transform swingHandRight, swingHandLeft;
    Transform currentSwingHand;
    private enum SwingHand
    {
        Left, Right
    }
    private SwingHand currentHandEnum;

    //Animations
    private Animator animator;
    private string StartSwingAnim = "StartSwing";
    private string StopSwingAnim = "StoptSwing";
    private string isLeftAnim = "isLeft";
    private string JumpAnim = "Jump";
    private string FallAnim = "Fall";
    private string SwitchSwingAnim = "SwitchSwing";
    private string DiveAnim = "Dive";
    private string SwingSpeedAnim = "SwingSpeed";
    private string RunAnim = "Run";
    private string IdleAnim = "Idle";


    // Others
    Vector3 lastCollisionPoint;
    GameManager gameManager;
    DimensionManager dimManager;


    [SerializeField]
    private float swingForce = 10f,
                  jumpForce = 10f,
                  moveSpeed = 10f,
                  pullForce = 0.0005f;
    private float angleX, angleZ;

    private bool isSwinging = false;
    private bool isDiving = false;
    private bool isGrounded = false;
    private bool isDead = false;

    //VFX
    [SerializeField]
    private GameObject speedLines;

    private AudioManager audioManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = spidey.GetComponent<Animator>();
        webSample.enabled = false;
        gameManager = FindObjectOfType<GameManager>();
        dimManager = gameManager.GetComponent<DimensionManager>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        if (gameManager.gameState != GameManager.GameState.During)
            return;
        if (isDead)
        {
            speedLines.SetActive(false);
            return;
        }
        pointer.transform.localPosition = transform.localPosition;
        if (isSwinging)
        {
            UpdateSwing();

            if (Input.GetMouseButtonDown(1)) // right click to release swing
                StopSwinging();
        }
        else
            pivoter.transform.localEulerAngles = Vector3.zero;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded && !isSwinging)
            {
                Jump();
            }
            if(isSwinging)
                Jump(1);
        } 

        if(isSwinging && Input.GetKeyDown(KeyCode.W))
        {
            Jump(2);
        }

        if (Input.GetMouseButtonDown(0)) // right click to start swinging
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.collider.gameObject.CompareTag("Player"))
                    return;
                StartSwing(hitInfo.point, hitInfo.transform);
            }
        }    
        
        if(!isSwinging && !isGrounded && !isDiving)
        {
            animator.SetTrigger(FallAnim);
            animator.ResetTrigger(IdleAnim);
        }

        if (isDiving) 
        {
            StopSwinging();
            speedLines.SetActive(true);
            animator.ResetTrigger(FallAnim);
            SwitchCam(vDiveCam);
            //Horizontal movement to dodge
            float xMov = transform.position.x + Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
            transform.position = new Vector3(Mathf.Clamp(xMov, -22.3f, 52.1f), transform.position.y, transform.position.z); // make sure spiderman does not go beyond the bounds


            /*if(rb.velocity.y < 0)//if after diving, spiderman starts going down, fall
            {
                isDiving = false;
                SwitchCam(vCam);
            }*/
        }
        else
            speedLines.SetActive(false);


    }

    public void Die()
    {
        isDead = true;
        //vfx etc
        //Ragdoll
        ragdoll.SetActive(true);
        Destroy(GetComponentInChildren<SpiderManController>().gameObject);
        SwitchCam(vDeadCam);
        gameManager.StopGame();
    }
    private void Jump(int jumpType = 0) // 0: if on the floor, 1: if mid air
    {
        if(jumpType == 0)
        {
            animator.SetTrigger(JumpAnim);
            animator.ResetTrigger(IdleAnim);
            animator.ResetTrigger(FallAnim);
            animator.ResetTrigger(StartSwingAnim);
            animator.ResetTrigger(SwitchSwingAnim);
        }
        else if (jumpType == 1)
        {
            isDiving = true;
            StopSwinging();
            animator.ResetTrigger(FallAnim);
            animator.ResetTrigger(SwitchSwingAnim);
            animator.ResetTrigger(StartSwingAnim);
            animator.SetTrigger(DiveAnim);
            
            rb.AddForce(new Vector3(0, 1, 1) * jumpForce, ForceMode.Impulse);
            //rb.AddForce(-(transform.position - swingHelper.position) * pullForce, ForceMode.Impulse); // pull the player toward the web
        }
        else if (jumpType == 2)
        {
            isDiving = true;
            StopSwinging();
            animator.ResetTrigger(FallAnim);
            animator.ResetTrigger(SwitchSwingAnim);
            animator.ResetTrigger(StartSwingAnim);
            animator.SetTrigger(DiveAnim);

            rb.AddForce(new Vector3(0, 1, 1) * pullForce * 2, ForceMode.Impulse);
            if(swingHelper != null)
            {
                Vector3 dir = -(transform.position - swingHelper.position);
                rb.AddForce(Vector3.Normalize(dir) * pullForce * 2, ForceMode.Impulse); // pull the player toward the web
            }
        }
        Debug.Log("Jumped");
        StopSwinging();
    }

    public void JumpTrigger()
    {
        rb.AddForce(lastCollisionPoint * jumpForce, ForceMode.Impulse);
    }

    private void StartSwing(Vector3 swingTarget, Transform targetObject) 
    {
        ChangeSwingHand();
        if (isSwinging)
        {
            StopSwinging();
            animator.SetTrigger(SwitchSwingAnim);
        }
        if (isDiving)
        {
            animator.SetTrigger(SwitchSwingAnim);
        }
        else
            animator.SetTrigger(StartSwingAnim);
        animator.ResetTrigger(FallAnim);
        animator.ResetTrigger(DiveAnim);

        webJoint = gameObject.AddComponent<HingeJoint>();

        StartCoroutine(WaitBeforeSwing(swingTarget, targetObject));
        isSwinging = true;
        StopDiving();

        Debug.Log("Initiated Swing");
    }

    IEnumerator WaitBeforeSwing(Vector3 swingTarget, Transform targetObject)
    {
        yield return new WaitForSeconds(0.3f);
        Swing(swingTarget, targetObject);
        
    }

    private void Swing(Vector3 swingPoint, Transform targetObject) 
    {
        LineRenderer web = GameObject.Instantiate(webSample) as LineRenderer;        

        //Swing Helper
        swingHelper = Instantiate(swingHelperGO.gameObject, swingPoint, Quaternion.identity).GetComponent<Rigidbody>();
        webJoint.connectedBody = swingHelper;

        //Web Renderer
        web.enabled = true;
        web.positionCount = 2;
        web.SetPosition(0, webSpawner.position); //web from hand
        web.SetPosition(1, webSpawner.position); //web at swing target

        web.transform.parent = targetObject;
        Debug.Log("Spawned Web");
        currentWeb = web;
        StartCoroutine(ShootWeb());

        //Swing Force
        Vector3 swingDirection = CalculateSwingDirection(); // Calculate the swing direction based on input or other factors
        rb.AddForce(swingDirection * swingForce, ForceMode.Impulse);

        //Animations + vfx
        //float swingTime = ((2 * Mathf.Abs(transform.position.z - swingPoint.z)) + (2 * Mathf.Abs(transform.position.y - swingPoint.y))) / swingForce; //distance / speed
        float swingTime = Mathf.Abs(2 * Mathf.PI * (Mathf.Sqrt(Vector3.Distance(swingHelper.position, transform.position))/GRAVITY));
        animator.SetFloat(SwingSpeedAnim, swingTime);

        //Just for visuals
        webSupport = GameObject.Instantiate(webSample) as LineRenderer;
        webSupport.enabled = true;
    }

    private IEnumerator ShootWeb()
    {
        if (currentWeb == null)
            StopAllCoroutines();
        if (swingHelper == null)
            StopAllCoroutines();
        float step = 0;
        while(step < 1)
        {
            step += 0.1f;            
            currentWeb.SetPosition(1, Vector3.Lerp(currentWeb.GetPosition(1), swingHelper.transform.position, step)); //web at swing target
            yield return new WaitForSeconds(0.02f);

        }
        audioManager.PlayAudio("WebShoot", this.transform.position);
    }
    private Vector3 CalculateSwingDirection()
    {
        Vector3 dir = currentWeb.GetPosition(1) - currentWeb.GetPosition(0);
        dir.z *= swingForce;
        dir.Normalize();
        return dir;
    }

    private void StopSwinging()
    {
        if (swingHelper == null && currentWeb == null)
            return;
        GameObject lastSwingHelper = swingHelper.gameObject;
        GameObject lastWeb = currentWeb.gameObject;        
        currentWeb = null;
        swingHelper = null;
        isSwinging = false;
        Destroy(lastWeb);
        lastWeb.GetComponent<Dissolve>().DissolveEnable();
        Destroy(webSupport);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        if (webJoint != null) 
        {
            webJoint.connectedBody = null;
            Destroy(webJoint);
        }
        if (lastSwingHelper != null) 
        {
            LineRenderer lastLine = lastWeb.GetComponent<LineRenderer>();
            Vector3[] initialVars = { lastLine.GetPosition(0), lastLine.GetPosition(1) };
            lastWeb.transform.SetParent(lastSwingHelper.transform, true);
            lastLine.SetPosition(0, initialVars[0]);
            lastLine.SetPosition(1, initialVars[1]);
            //lastSwingHelper.transform.Rotate(lastSwingHelper.transform.localEulerAngles.x + 60, 0, 0);
            Destroy(lastSwingHelper.gameObject, 5);
        }
    }

    private void StopDiving()
    {
        //Handle diving stop
        isDiving = false;
        SwitchCam(vCam);
    }

    private void ChangeSwingHand(bool adjustSwing = false)
    {
        switch (currentHandEnum)
        {
            case SwingHand.Right: //If the current hand is left, inverse to right
                currentHandEnum = SwingHand.Left;
                currentSwingHand = swingHandLeft;
                if(!adjustSwing)
                    animator.SetBool("isLeft", true);

                //webSpawner.SetParent(swingHandLeft);
                //webSpawner.SetPositionAndRotation(swingHandLeft.position, swingHandLeft.rotation);
                break;
            case SwingHand.Left: //If the current hand is right, inverse to left
                currentHandEnum = SwingHand.Right;
                currentSwingHand = swingHandRight;
                if (!adjustSwing)
                    animator.SetBool("isLeft", false);
                //webSpawner.SetParent(swingHandRight);
                //webSpawner.SetPositionAndRotation(swingHandRight.position, swingHandRight.rotation);
                break;
        }
    }

    private void SwitchCam(CinemachineVirtualCamera _cam)
    {
        vCam.Priority = 11;
        vDeadCam.Priority = 11;
        vDiveCam.Priority = 11;
        _cam.Priority = 12;
    }

    private void UpdateSwing()
    {
        if (!currentWeb)
            return;
        currentWeb.SetPosition(0, new Vector3(currentSwingHand.position.x, currentSwingHand.position.y, currentSwingHand.position.z));
        webSupport.SetPosition(0, swingHandLeft.position);
        webSupport.SetPosition(1, swingHandRight.position);
        /*float tiltAngleZ = 57 * (Mathf.Asin((Mathf.Abs(swingHelperGO.transform.position.x - transform.position.x)) / Mathf.Abs((swingHelperGO.transform.position.y - transform.position.y)))); //theta = tan^-1 * x/y
         pivoter.transform.rotation = Quaternion.Euler(0, 0, tiltAngleZ * 5);*/
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
        if (!collision.gameObject.CompareTag("Floor"))
            return;
        isGrounded = true;
        lastCollisionPoint = collision.GetContact(0).normal;
        animator.ResetTrigger(FallAnim);
        animator.SetTrigger(IdleAnim);
        animator.ResetTrigger(StartSwingAnim);
        animator.ResetTrigger(DiveAnim);
        animator.ResetTrigger(SwitchSwingAnim);


        StopDiving();
        StopSwinging();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
            Die();
        if (!collision.gameObject.CompareTag("Floor"))
            return;
        isGrounded = true;
        animator.ResetTrigger(FallAnim);
        animator.SetTrigger(IdleAnim);
        if (collision.contactCount > 0)
            lastCollisionPoint = collision.GetContact(0).normal;
        //animator.SetTrigger(IdleAnim);
        StopSwinging();
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
        if (collision.contactCount > 0)
            lastCollisionPoint = collision.GetContact(0).normal;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TileSpawnner"))
            gameManager.SpawnNewFloorTile(other.transform.parent.gameObject);
        if (other.gameObject.CompareTag("Portal"))
        {
            other.GetComponent<PortalController>().Collided();
            audioManager.PlayAudio("Portal", this.transform.position);
        }
    }
}
