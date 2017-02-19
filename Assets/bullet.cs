using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour {

	void Awake()
	{
		m_rb = GetComponent<Rigidbody2D> ();
	}


	public void SetVelocity(Vector2 v)
	{
		m_rb.velocity = v;
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		Destroy (this.gameObject);
	}

	private Rigidbody2D m_rb;
}
