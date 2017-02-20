﻿using UnityEngine;
using System.Linq;

public class Mob : MonoBehaviour
{
	// editor properties

	public float        WalkSpeed;

    // bullet

    public GameObject   BulletPrefab;
	public float        BulletSpeed;
	public float        BulletDelay;

    // ai

    public float        AttackRange;
    public float        TimeBetweenBullets;
    public float        SensorRange;
    public LayerMask    PlayerLayer;
	public LayerMask whatIsWall;


    void Awake()
	{
		// get components

		m_rb = GetComponent<Rigidbody2D>();

		m_tranGun = transform.FindChild("gun");
    }

    Vector2 DirectionToTarget()
    {
        if (m_targetCurrent != null)
        {
            return m_targetCurrent.position - transform.position;
        }
        else
        {
			return new Vector2(walkDirection, 0);
        }
    }

    Vector3[] GetBulletPath()
    {
        return m_tranGun
                .FindChild("Path")
                .Cast<Transform>()
                .Select(child => child.position)
                .ToArray();
    }

	RaycastHit2D WallOnLeft()
	{
		return Physics2D.Linecast(transform.position, (Vector2)transform.position + (Vector2.right * -1), whatIsWall);
	}
	RaycastHit2D WallOnRight()
	{
		return Physics2D.Linecast(transform.position, (Vector2)transform.position + (Vector2.right * 1), whatIsWall);
	}
	void UpdateAttack()
    {
        m_attackTimer -= Time.deltaTime;

        if (m_targetCurrent != null && 
            DirectionToTarget().magnitude < AttackRange &&
            m_attackTimer < 0)
        {
            // reset the attack timer

            m_attackTimer = TimeBetweenBullets;

            // fire a bullet

            //Vector3[] bulletPath = GetBulletPath();

            //GameObject newBullet = Instantiate(BulletPrefab, bulletPath[0], Quaternion.identity);
			
		}
    }
	int walkDirection = 1;

	void Update()
	{
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
            walkDirection = -1;
        }
        else if (DirectionToTarget().x > 0 || WallOnLeft())
        {
            walkDirection = 1;
        }

        m_vh = WalkSpeed * walkDirection;

        // make sure the mob is facing the right direction

        if (walkDirection < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // rotate the gun to point at the target

        if (m_targetCurrent != null)
        {
            float angleToTarget = Mathf.Atan2(DirectionToTarget().y, DirectionToTarget().x) * Mathf.Rad2Deg;
            m_tranGun.rotation = Quaternion.AngleAxis(walkDirection < 0 ? angleToTarget - 180 : angleToTarget, Vector3.forward);
        }

        // attack if we can

        UpdateAttack();

        // set rb velocity

        m_rb.velocity = new Vector2(m_vh, m_rb.velocity.y);
    }

    // debug drawing of sesor range and attack range

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

			if (DirectionToTarget().magnitude < AttackRange)
			{
				Gizmos.color = Color.green;
			}

            // draw a circle for the attack range
            // red if the target is not in range, green if it is

			Gizmos.DrawWireSphere(transform.position, AttackRange);
		}

        // restore the old gizmo color

        Gizmos.color = gizomColorOld;
    }

    // private state

    private Rigidbody2D     m_rb;
    private Transform       m_tranGun;

    private float           m_vh;
    private float           m_attackTimer = 1;
    private Transform       m_targetCurrent;
}
