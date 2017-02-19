using UnityEngine;
using DG.Tweening;
public class Mob : MonoBehaviour
{
	// editor properties

	public float MaxJumpHeight;
	public float WalkSpeed;
	public float TimeBetweenBullets;
	public bullet BulletPrefab;
	public float BulletSpeed;
	public float BulletDelay;
	public Ease BulletEase;
	public Transform BulletPath;

	public AudioClip FireSound;
	public AudioClip JumpSound;

	void Awake()
	{
		// get components

		m_rb = GetComponent<Rigidbody2D>();

		m_col = GetComponent<BoxCollider2D>();

		//m_fsm = StateMachine<HeroState>.Initialize(this);

		m_audio = GetComponent<AudioSource>();
		m_tranGun = transform.FindChild("gun");
		m_tranMuzzle = transform.FindChild("gun").FindChild("muzzle");
	}

	// default root motion when nothing special is happening

	void Update()
	{
		UpdateVDefault();
	}


	public Transform targetCurrent;
	public Vector2 targetDirection
	{
		get
		{
			Vector2 _dir = transform.position;
			if (targetCurrent != null)
			{
				_dir =  (Vector2)targetCurrent.position - _dir ;
			}
			return _dir;
		}
	}
	public bool targetInRange
	{
		get
		{
			bool _range = false;
			if (targetCurrent != null)
			{
				Vector2 _dir = targetCurrent.position - transform.position;
				if (_dir.magnitude < attackRange)
				{
					_range = true;
				}
			}
			return _range;
		}
	}
	public float attackRange = 5;
	public float ai_Smarts = 1.2f;
	public float sensorRange = 10;
	public float sensorAngle = 0;
	public LayerMask whatIsPlayer;
	Collider2D[] sensorHits;
	Vector3[] bulletPath;
	float moveTimer = 1;
	float attackTimer = 1;
	void UpdateVDefault()
	{
		// are we deflecting the stick left, right, or not at all
		sensorHits = Physics2D.OverlapCircleAll(transform.position, sensorRange, whatIsPlayer);
		float xAxis = 0;
		if (sensorHits.Length > 0 && sensorHits != null)
		{
			targetCurrent = sensorHits[0].transform;
			xAxis = targetDirection.x;
			sensorAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
			float pathAngle = Mathf.Atan2(targetDirection.y * -1, targetDirection.x * -1)  * Mathf.Rad2Deg;
			m_tranGun.rotation = Quaternion.AngleAxis(sensorAngle, Vector3.forward);
			BulletPath.rotation = Quaternion.AngleAxis(pathAngle, Vector3.forward);


		}
		else
		{
			targetCurrent = null;
		}
		int walkDirection = 0;
		if (xAxis < 0)
		{
			walkDirection = -1;
		}
		else if (xAxis > 0)
		{
			walkDirection = 1;
		}

		// set vh

		m_vh = WalkSpeed * walkDirection;

		// make sure the character is facing the right direction

		if (xAxis < 0)
		{
			m_isFacingRight = false;
			transform.localScale = new Vector3(-1, 1, 1);
			BulletPath.localScale = new Vector3(1, 1, 1);
		}
		else if (xAxis > 0)
		{
			m_isFacingRight = true;
			transform.localScale = new Vector3(1, 1, 1);
			BulletPath.localScale = new Vector3(-1, 1, 1);

		}

		// set rb velocity
		moveTimer -= Time.deltaTime;
		
		if (targetInRange )
		{
			AttackThatHoe();
			if (moveTimer < 0)
			{
				if (!IsInvoking("ResetMoveTimer"))
					Invoke("ResetMoveTimer", ai_Smarts);
				m_vh = m_vh * -1;
			}
			
		}
		m_rb.velocity = new Vector3(m_vh, m_vv, 0);
	}
	void AttackThatHoe()
	{
		attackTimer -= Time.deltaTime;
		if (attackTimer < 0)
		{
			attackTimer = ai_Smarts;
			GameObject _newBullet = Instantiate(BulletPrefab.gameObject, GetPath()[0], Quaternion.identity);
			_newBullet.transform.DOPath(GetPath(), BulletSpeed, PathType.CatmullRom, PathMode.Full3D, 10, Color.yellow)
				.SetSpeedBased(true)
				.SetDelay(BulletDelay)
				.SetEase(BulletEase)
				.OnStart(() =>
				{
					_newBullet.SetActive(true);
				})
				.OnComplete(() =>
				{
					Destroy(_newBullet);
				});
		}

	}
	void ResetMoveTimer()
	{
		moveTimer = ai_Smarts;

	}
	Vector3[] GetPath()
	{
		if (BulletPath != null && BulletPath.childCount > 0)
		{
			bulletPath = new Vector3[BulletPath.childCount];
			for (int ai = 0; ai < BulletPath.childCount; ai++)
			{
				bulletPath[ai] = BulletPath.GetChild(ai).position;
			}
		}
		return bulletPath;
	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, sensorRange);
		if (targetCurrent != null)
		{
			Gizmos.DrawLine(transform.position, targetCurrent.position);
			if (targetDirection.magnitude < attackRange)
			{
				Gizmos.color = Color.green;
			}
			Gizmos.DrawWireSphere(transform.position, attackRange);

		}

	}

	// default bullet shooting behavior

	void UpdateGunDefault()
	{
		if (Input.GetAxisRaw("Fire") == 0)
		{
			// if you let go of the fire button you can fire again as soon as you press trigger again

			m_timeLastFire = 0;
		}
		else if (Time.time - m_timeLastFire > TimeBetweenBullets)
		{
			// keep track of when we fired so we know when to fire again

			m_timeLastFire = Time.time;

			// spawn the bullet, and set its position and velocity

			bullet newBullet = Instantiate(BulletPrefab);
			//newBullet.SetVelocity(new Vector2(m_isFacingRight ? BulletSpeed : -BulletSpeed, 0));
			newBullet.transform.position = m_tranMuzzle.position;

			m_audio.PlayOneShot(FireSound);
		}
	}

	// check if we are on ground

	bool Grounded()
	{
		float height = m_col.size.y;
		float width = m_col.size.x;

		// cast three rays down. one from the middle bottom, the left bottom, and the right bottom

		Vector2 rayStartCenter = transform.position + new Vector3(0, -height / 2, 0);
		Vector2 rayStartLeft = rayStartCenter + new Vector2(-width / 2, 0);
		Vector2 rayStartRight = rayStartCenter + new Vector2(width / 2, 0);

		// BB (matthew) not sure how big to make this. needs to be big enough to actually hit the ground, but small enough so it doesnt look like the hero is floating

		float raycastDistance = 0.2f;

		RaycastHit2D centerHit = Physics2D.Raycast(rayStartCenter, Vector2.down, raycastDistance);
		RaycastHit2D leftHit = Physics2D.Raycast(rayStartLeft, Vector2.down, raycastDistance);
		RaycastHit2D rightHit = Physics2D.Raycast(rayStartRight, Vector2.down, raycastDistance);

		if (centerHit.collider == null &&
			leftHit.collider == null &&
			rightHit.collider == null)
			return false;
		else
			return true;
	}

	// private state

	private float m_vv;
	private float m_vh;
	private Rigidbody2D m_rb;
	private BoxCollider2D m_col;
	private Transform m_tranMuzzle;
	private Transform m_tranGun;
	private float m_timeLastFire;
	private bool m_isFacingRight = true;
	private AudioSource m_audio;
}
