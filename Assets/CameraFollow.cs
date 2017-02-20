using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour 
{
	public Transform Hero;
	public float MaxLag;
	public float CameraSpeedMin;
	public float CameraSpeedMax;
	public float MaxCameraShake;
	public float ScreenShakeIntensity;

	void Start()
	{
		m_posXYFollow = transform.position;
	}

	void LateUpdate()
	{
		Vector2 dPos = m_posXYFollow - (Vector2)Hero.position;

		if(dPos.magnitude > 0)
		{
			float speed = Mathf.Lerp(CameraSpeedMin, CameraSpeedMax, dPos.magnitude / MaxLag);
			Vector2 velocity = -dPos.normalized * speed;
			Vector2 dPosFollow = velocity * Time.deltaTime;

			if(dPosFollow.magnitude > dPos.magnitude)
			{
				m_posXYFollow = Hero.position;
			}
			else
			{
				m_posXYFollow += dPosFollow;
			}

		}

		if(dPos.magnitude > MaxLag)
		{
			dPos = dPos.normalized * MaxLag;
			m_posXYFollow = (Vector2)Hero.position + dPos;

		}

		Vector2 camPosFinal = m_posXYFollow;

		if(Time.time < m_timeShakeStop)
		{
			float perlinX = Mathf.PerlinNoise(Time.time * ScreenShakeIntensity, 0);
			float perlinY = Mathf.PerlinNoise(0, Time.time * ScreenShakeIntensity);
			perlinX = Mathf.Clamp(perlinX, 0, 1);
			perlinY = Mathf.Clamp(perlinY, 0, 1);
			perlinX = perlinX * 2 - 1;
			perlinY = perlinY * 2 - 1;
			perlinX *= MaxCameraShake;
			perlinY *= MaxCameraShake;

			camPosFinal += new Vector2(perlinX, perlinY);
		}

		transform.position = new Vector3(camPosFinal.x, camPosFinal.y, transform.position.z);

	}

	public void ScreenShakeUntil(float timeShakeStop)
	{
		m_timeShakeStop = timeShakeStop;
	}

	private Vector2 m_posXYFollow;
	private float m_timeShakeStop = -1;
}