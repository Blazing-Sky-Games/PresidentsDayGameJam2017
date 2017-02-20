using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour {

	void Awake()
	{
		m_rb = GetComponent<Rigidbody2D> ();
        //m_posLast = transform.position;
    }

	public void SetVelocity(Vector2 v)
	{
		m_rb.velocity = v;
	}

    /*
    void FixedUpdate()
    {
        LayerMask mask = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")));
        RaycastHit2D hit = Physics2D.Linecast(m_posLast, transform.position, mask);

        if(hit)
        {
            Destroy(gameObject);
        }
        else
        {
            m_posLast = transform.position;
        }
    }
    */

    void OnTriggerEnter2D(Collider2D other)
	{
		Destroy (this.gameObject, 0.02f);
	}
 
    private Rigidbody2D m_rb;
    //private Vector2     m_posLast;
}
