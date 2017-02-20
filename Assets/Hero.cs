using UnityEngine;
using MonsterLove.StateMachine;
using System.Collections;

public class Hero : MonoBehaviour
{
    // editor properties

    public float 		MaxJumpHeight;
    public float 		WalkSpeed;
	public float 		TimeBetweenBullets;
	public bullet 		BulletPrefab;
	public float		BulletSpeed;
	public AudioClip	FireSound;
	public AudioClip	JumpSound;
    public float        MaxWallSlideSpeed;
    public float        WallJumpHorizontalGravity;
    public float        MaxWallJumpWidth;
	public float		WallBuffer;
	public float		ScreenShakeTime;
    public float        HitStun;
    public AudioClip    HurtSound;

    // states for MonsterLove state machine

    public enum HeroState
    {
        Idle,   // NOTE (matthew)
                // right now there really isnt a difference between idle and walk, 
                // but the are sperate so they can have different anims later
        Walk,
        Jump,
        Fall,
		WallSlide,
		WallJump,
        Hurt,
    }

    void Awake()
    {
        // get components

        m_rb = GetComponent<Rigidbody2D>();

        m_col = GetComponent<BoxCollider2D>();

        m_fsm = StateMachine<HeroState>.Initialize(this);

		m_audio = GetComponent<AudioSource>();

		m_tranMuzzle = transform.FindChild ("muzzle");
    }

    void Start()
    {
        // start in idle state

        m_fsm.ChangeState(HeroState.Idle);
    }

	void FixedUpdate()
	{
		// set rb velocity

		m_rb.velocity = new Vector3(m_vh, m_vv, 0);
	}

    // default root motion when nothing special is happening

    void UpdateVDefault()
    {
        // are we deflecting the stick left, right, or not at all

        float h = Input.GetAxisRaw("Horizontal");
        int walkDirection = 0;
        if (h < 0)
        {
            walkDirection = -1;
        }
        else if (h > 0)
        {
            walkDirection = 1;
        }

        // set vh

        m_vh = WalkSpeed * walkDirection;

        // make sure the character is facing the right direction

        if (h < 0)
        {
			m_isFacingRight = false;
			transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (h > 0)
        {
			m_isFacingRight = true;
			transform.localScale = new Vector3(1, 1, 1);
        }
    }

	// default bullet shooting behavior

	void UpdateGunDefault()
	{
		if (Input.GetAxisRaw ("Fire") == 0)
		{
			// if you let go of the fire button you can fire again as soon as you press trigger again

			m_timeLastFire = 0;
		}
		else if (Time.time - m_timeLastFire > TimeBetweenBullets)
		{
			// keep track of when we fired so we know when to fire again

			m_timeLastFire = Time.time;

			// spawn the bullet, and set its position and velocity

			bullet newBullet = Instantiate (BulletPrefab);
			newBullet.SetVelocity (new Vector2 (m_isFacingRight ? BulletSpeed : -BulletSpeed, 0));
			newBullet.transform.position = m_tranMuzzle.position;

			m_audio.PlayOneShot (FireSound);

			Camera.main.GetComponent<CameraFollow>().ScreenShakeUntil(Time.time + ScreenShakeTime);
		}
	}

    // check if we are on ground

    bool Grounded()
    {
        float dist;
        return Grounded(out dist);
    } 

    bool Grounded(out float distance)
    {
        distance = -1;

        float height = m_col.size.y;
        float width = m_col.size.x;

        // cast three rays down. one from the middle bottom, the left bottom, and the right bottom

		Vector2 rayStartCenter = transform.position + new Vector3(0, -height / 2, 0);
        Vector2 rayStartLeft = rayStartCenter + new Vector2(-width / 2, 0);
		Vector2 rayStartRight = rayStartCenter + new Vector2(width / 2, 0);

		// BB (matthew) not sure how big to make this. needs to be big enough to actually hit the ground, but small enough so it doesnt look like the hero is floating

		float raycastDistance = 0.05f;

        // ignore the player when raycasting

		LayerMask mask = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")));

        RaycastHit2D centerHit = Physics2D.Raycast(rayStartCenter, Vector2.down, raycastDistance, mask);
		RaycastHit2D leftHit = Physics2D.Raycast(rayStartLeft, Vector2.down, raycastDistance, mask);
		RaycastHit2D rightHit = Physics2D.Raycast(rayStartRight, Vector2.down, raycastDistance, mask);

        if (centerHit.collider == null &&
            leftHit.collider == null &&
            rightHit.collider == null)
            return false;
        else
        {
            if (centerHit.collider != null)
            {
                distance = centerHit.distance;
            }
            else if (leftHit.collider != null)
            {
                distance = leftHit.distance;
            }
            else
            {
                distance = rightHit.distance;
            }

            return true;
        }
    }

    // like grounded, but for walls

    bool OnWall(float direction)
    {
        float dist;
        return OnWall(direction, out dist);
    }

    bool OnWall(float direction, out float distance)
    {
        distance = -1;

        float height = m_col.size.y;
        float width = m_col.size.x;

        Vector2 rayStartCenter = transform.position + new Vector3(direction * width / 2, 0, 0);
        Vector2 rayStartBottom = rayStartCenter + new Vector2(0, -height / 2);
        Vector2 rayStartTop = rayStartCenter + new Vector2(0, height / 2);

        float raycastDistance = 0.05f;

        // ignore the player when raycasting

		LayerMask mask = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")));

        RaycastHit2D centerHit = Physics2D.Raycast(rayStartCenter, Vector2.right * direction, raycastDistance, mask);
        RaycastHit2D bottomHit = Physics2D.Raycast(rayStartBottom, Vector2.right * direction, raycastDistance, mask);
        RaycastHit2D topHit = Physics2D.Raycast(rayStartTop, Vector2.right * direction, raycastDistance, mask);

        if (bottomHit.collider == null &&
            centerHit.collider == null &&
            topHit.collider == null)
            return false;
        else
        {
            if(bottomHit.collider != null)
            {
                distance = bottomHit.distance;
            }
            else if(centerHit.collider != null)
            {
                distance = centerHit.distance;
            }
            else
            {
                distance = topHit.distance;
            }

            return true;
        }
    }

    // idle state

    void Idle_Enter()
    {
        // if we are idle, we are not moving in the y axis

        m_vv = 0;

        // make sure we are flush with the ground

        float distToGround;
        if(Grounded(out distToGround))
        {
            transform.position -= new Vector3(0, distToGround, 0);
        }
    }

    void Idle_Update()
    {
        UpdateVDefault();
		UpdateGunDefault ();

        if (Input.GetButtonDown("Jump"))
        {
            // if we press the jump button, jump

			Jump ();
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            // if there is horizontal input, start walking

            m_fsm.ChangeState(HeroState.Walk);
        }
        else if (!Grounded())
        {
            // if we are not standing on ground, start falling

            m_fsm.ChangeState(HeroState.Fall);
        }
    }

    // walk state

    void Walk_Enter()
    {
        // if we are walking we are not moving in the y axis 

        m_vv = 0;
    }

    void Walk_Update()
    {
        UpdateVDefault();
		UpdateGunDefault ();

        if (Input.GetButtonDown("Jump"))
        {
            // if we press jump, jump

			Jump ();
        }
        else if (Input.GetAxisRaw("Horizontal") == 0)
        {
            // if we are not deflecting the stick, start idleing

            m_fsm.ChangeState(HeroState.Idle);
        }
        else if (!Grounded())
        {
            // if we are not grounded, start falling

            m_fsm.ChangeState(HeroState.Fall);
        }
    }

    // jump state

	void Jump()
	{
		m_audio.PlayOneShot (JumpSound);

		// calculate launch velocity based on desired jump height

		m_vv = Mathf.Sqrt(-2 * Physics2D.gravity.y * MaxJumpHeight);

		m_fsm.ChangeState (HeroState.Jump);
	}
		

    void Jump_Update()
    {
        // acceleration due to gravity

        m_vv += Physics2D.gravity.y * Time.deltaTime;

        UpdateVDefault();
		UpdateGunDefault ();

        if (Grounded() && m_vv < 0)
        {
            // this is here to handle landing on a ledge at the apex of a jump 
            // if we are grounded the first frame our y velocity is negative just go to 
            // idle instead of fall

            // BB (matthew) is the needed?

            m_fsm.ChangeState(HeroState.Idle);
        }
        else if (m_vv < 0 || !Input.GetButton("Jump"))
        {
            if(m_vv > 0)
            {
                // the behavior when you let go of jump while moving up is to have
                // your upward velocity set to zero

                // NOTE (matthew)
                // this assumes we dont want the hero commited to a jump (i assume we do not)
                // this behavior of setting the v to zero is the best feeling jump i have found,
                // but if someone has a better idea im game

                m_vv = 0;
            }

            // if we are not holding jump, or we have started moving down,
            // start falling

            m_fsm.ChangeState(HeroState.Fall);
        }
		else if ((Input.GetAxisRaw ("Horizontal") < 0 && OnWall (-1)) || (Input.GetAxisRaw ("Horizontal") > 0 && OnWall (1)))
		{
			m_fsm.ChangeState (HeroState.WallSlide);
		}
    }


    // fall state

    void Fall_Update()
    {
        // acceleration due to gravity

        m_vv += Physics2D.gravity.y * Time.deltaTime;

        UpdateVDefault();
		UpdateGunDefault();

        if (Grounded())
        {
            // if we land, start idleing

            // BB (matthew) if we want a landing animation (not just jumping to idle), 
            // we need extra logic here and in the idle_enter to handle that

            m_fsm.ChangeState(HeroState.Idle);
        }
		else if ((Input.GetAxisRaw ("Horizontal") < 0 && OnWall (-1)) || (Input.GetAxisRaw ("Horizontal") > 0 && OnWall (1)))
		{
			m_fsm.ChangeState (HeroState.WallSlide);
		}
    }

	// wallslide state

	void WallSlide_Enter()
	{
        // when we first "stick" to the wall
        // if we are falling, cap our fall speed to the max slide speed

        if(m_vv < -MaxWallSlideSpeed)
            m_vv = -MaxWallSlideSpeed;

        // make sure we are flush with the wall

        float distToWall;
        if (OnWall(1, out distToWall))
        {
            transform.position += new Vector3(distToWall, 0, 0);
        }
        else if (OnWall(-1, out distToWall))
        {
            transform.position += new Vector3(-distToWall, 0, 0);
        }
    }

	void WallSlide_Update()
	{
        // accelration due to gravity, but we cap the fall speed

        if (m_vv > 0 || Mathf.Abs (m_vv) < MaxWallSlideSpeed)
		{
            m_vv += Physics2D.gravity.y * Time.deltaTime;
		}

        // we are on the wall, we aint going left or right

        m_vh = 0;

        m_rb.velocity = new Vector2(m_vh, m_vv);

        // make sure we are faceing the right way (away from the wall)

        if (OnWall(1))
        {
            m_isFacingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (OnWall(-1))
        {
            m_isFacingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }

        // gun

        UpdateGunDefault();

        if (Grounded() && m_vv <= 0)
        {
            // if we slid all the way to the ground go to idle

            m_fsm.ChangeState(HeroState.Idle);
        }
        else if ((OnWall( 1) && Input.GetAxisRaw("Horizontal") <= 0) || 
                 (OnWall(-1) && Input.GetAxisRaw("Horizontal") >= 0))
        {
			if (m_timeWallStickBegin == -1) 
			{
				m_timeWallStickBegin = Time.time;
			}

			if (Input.GetButtonDown("Jump"))
            {
				Jump ();
				m_timeWallStickBegin = -1;
            }
			else if (Time.time - m_timeWallStickBegin > WallBuffer)
            {
                m_fsm.ChangeState(HeroState.Fall);
				m_timeWallStickBegin = -1;
            }
        }
        else if (OnWall(1) && Input.GetAxisRaw("Horizontal") > 0 && Input.GetButtonDown("Jump"))
        {
            m_wallJumpWallDirection = 1;
            m_fsm.ChangeState(HeroState.WallJump);
        }
        else if (OnWall(-1) && Input.GetAxisRaw("Horizontal") < 0 && Input.GetButtonDown("Jump"))
        {
            m_wallJumpWallDirection = -1;
            m_fsm.ChangeState(HeroState.WallJump);
        }
		else if (!OnWall(-1) && !OnWall(1))
		{
			m_fsm.ChangeState(HeroState.Fall);
		}
	}

	// walljump State

	void WallJump_Enter()
	{
        m_audio.PlayOneShot(JumpSound);

        m_vh = Mathf.Sqrt(2 * WallJumpHorizontalGravity * MaxWallJumpWidth);

        float wallJumpMaxTime = 2 * m_vh / WallJumpHorizontalGravity;

        // derive vertical launch velocity
        // v(T) = m_vv + g * T
        // integrate with respect to T
        // s(T) = m_vv * T + g / 2 * T * T
        // MaxWallJumpHeight = m_vv * T + g / 2 * T * T, solve for m_vv gives
        // m_vv = MaxWallJumpHeight / T - g / 2 * T

        m_vv = MaxJumpHeight / wallJumpMaxTime - Physics2D.gravity.y * wallJumpMaxTime / 2;

        // make sure vh is in right direction (away from wall)

        m_vh *= m_wallJumpWallDirection * -1;
    }

	void WallJump_Update()
	{
        m_rb.velocity = new Vector2(m_vh, m_vv);

        m_vh += WallJumpHorizontalGravity * m_wallJumpWallDirection * Time.deltaTime;
        m_vv += Physics2D.gravity.y * Time.deltaTime;

        // make sure we are facing the right direction (towards the wall)

        if (m_wallJumpWallDirection == 1)
        {
            m_isFacingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (m_wallJumpWallDirection == -1)
        {
            m_isFacingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // update gun

        UpdateGunDefault();

        if ((Input.GetAxisRaw ("Horizontal") <= 0 && m_wallJumpWallDirection == 1) || 
            (Input.GetAxisRaw ("Horizontal") >= 0 && m_wallJumpWallDirection == -1))
		{
            // if the stick is defelcted away from wall -> fall

			m_fsm.ChangeState (HeroState.Jump);
		}
		
		else if (Mathf.Sign(m_vh) != m_wallJumpWallDirection && !Input.GetButton("Jump"))
		{
            //let go of jump Button while moving away from wall -> lose velocity away from wall

            m_vh = 0;
		}
		else if (OnWall(m_wallJumpWallDirection) && Mathf.Sign(m_vh) == m_wallJumpWallDirection)
		{
            //hit wall -> wall slide

            // dont slide up the wall after a wall jump

            m_vv = 0;

            m_fsm.ChangeState (HeroState.WallSlide);
		} 
		
		else if(Mathf.Abs(m_vh) > Mathf.Sqrt(2 * WallJumpHorizontalGravity * MaxWallJumpWidth) * 1.3)
		{
            //miss wall -> fall
            // this can happen if we wall jump over the top of a wall

            m_fsm.ChangeState (HeroState.Fall);
		}
	}

    // hurt state

    public void Hurt()
    {
        m_fsm.ChangeState(HeroState.Hurt);
    }

    IEnumerator Hurt_Enter()
    {
        m_audio.PlayOneShot(HurtSound);

        m_vh = 0;
        m_vv = 0;
        m_col.enabled = false;

        float elapsed = 0;

        while (elapsed < HitStun)
        {
            elapsed += Time.deltaTime;
            yield return 0;
        }

        m_col.enabled = true;

        m_fsm.ChangeState(HeroState.Idle);
    }

    // private state

    private float                      	m_vv;
    private float                      	m_vh;
    private Rigidbody2D                	m_rb;
    private BoxCollider2D            	m_col;
    private StateMachine<HeroState>		m_fsm;
	private Transform 					m_tranMuzzle;
	private float						m_timeLastFire;
	private bool						m_isFacingRight = true;
	private AudioSource					m_audio;
    private int                         m_wallJumpWallDirection;
	private float						m_timeWallStickBegin = -1;
}
