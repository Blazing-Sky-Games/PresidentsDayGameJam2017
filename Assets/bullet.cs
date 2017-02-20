using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour {

    public int Damage;

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
        m_damageOnDestory = other.gameObject.GetComponent<Mob>();

        Destroy (gameObject, 0.02f);
	}

    void OnDestroy()
    {
        if(m_damageOnDestory != null)
            m_damageOnDestory.HP -= Damage;
    }

    private Rigidbody2D m_rb;
    private Mob         m_damageOnDestory;
}
