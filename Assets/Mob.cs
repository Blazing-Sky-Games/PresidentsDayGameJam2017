using UnityEngine;

public class Mob : MonoBehaviour
{
	// editor properties

	public float        WalkSpeed;
    public float        SensorRange;
    public LayerMask    PlayerLayer;
	public LayerMask    WallLayer;
    public int          HP;

    void Awake()
	{
		// get components

		m_rb = GetComponent<Rigidbody2D>();
    }

    Vector2 DirectionToTarget()
    {
        if (m_targetCurrent != null)
        {
            return m_targetCurrent.position - transform.position;
        }
        else
        {
			return new Vector2(0, 0);
        }
    }

	RaycastHit2D WallOnLeft()
	{
		return Physics2D.Linecast(transform.position, (Vector2)transform.position + (Vector2.right * -8), WallLayer);
	}
	RaycastHit2D WallOnRight()
	{
		return Physics2D.Linecast(transform.position, (Vector2)transform.position + (Vector2.right * 8), WallLayer);
	}
	
	void Update()
	{
        if(HP <= 0)
        {
            Destroy(gameObject);
        }
        
        // clear target

        m_targetCurrent = null;

        // check for new current target

        Collider2D[] sensorHits = Physics2D.OverlapCircleAll(transform.position, SensorRange, PlayerLayer);

        if (sensorHits.Length > 0)
        {
            m_targetCurrent = sensorHits[0].transform;
        }

        // set vh. always walk towards target

        if (DirectionToTarget().x < 0 || WallOnRight())
        {
            m_walkDirection = -1;
        }
        else if (DirectionToTarget().x > 0 || WallOnLeft())
        {
            m_walkDirection = 1;
        }

        m_vh = WalkSpeed * m_walkDirection;

        // make sure the mob is facing the right direction

        if (m_walkDirection < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // set rb velocity

        m_rb.velocity = new Vector2(m_vh, m_rb.velocity.y);
    }

    // debug drawing of sesor range

    void OnDrawGizmos()
	{
        // save the gizmo color so we can restor it when we are done

        Color gizomColorOld = Gizmos.color;

        // draw red circle to show sensor range

        Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, SensorRange);

		if (m_targetCurrent != null)
		{
			// if we have a target ...

			// draw a line from us to the target

			Gizmos.DrawLine(transform.position, m_targetCurrent.position);
			Gizmos.DrawLine(transform.position, transform.position + Vector3.right * -1);
			Gizmos.DrawLine(transform.position, transform.position + Vector3.right * 1);
		}

        // restore the old gizmo color

        Gizmos.color = gizomColorOld;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Hero hero = collision.gameObject.GetComponent<Hero>();
        if(hero != null)
        {
            hero.Hurt();
            m_targetCurrent = null;
        }
    }

    // private state

    private Rigidbody2D     m_rb;
    private Transform       m_tranGun;
    private float           m_vh;
    private Transform       m_targetCurrent;
    int                     m_walkDirection = 1;
}
