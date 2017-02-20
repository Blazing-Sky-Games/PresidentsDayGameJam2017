using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour 
{
	public Transform Hero;
	public float MaxLag;
	public float CameraSpeedMin;
	public float CameraSpeedMax;

	void LateUpdate()
	{
		Vector2 posXY = transform.position;
		Vector2 dPos = transform.position - Hero.position;

		if(dPos.magnitude > 0)
		{
			float speed = Mathf.Lerp(CameraSpeedMin, CameraSpeedMax, dPos.magnitude / MaxLag);
			Vector3 velocity = -dPos.normalized * speed;
			Vector3 dPosFollow = velocity * Time.deltaTime;

			if(dPosFollow.magnitude > dPos.magnitude)
			{
				posXY = Hero.position;
				transform.position = new Vector3(posXY.x, posXY.y, transform.position.z);
			}
			else
			{
				transform.position += dPosFollow;
			}

		}

		if(dPos.magnitude > MaxLag)
		{
			dPos = dPos.normalized * MaxLag;
			posXY = (Vector2)Hero.position + dPos;
			transform.position = new Vector3(posXY.x, posXY.y, transform.position.z);
		}
	}

}
