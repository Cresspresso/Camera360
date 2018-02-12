using UnityEngine;

public class FollowTouch360 : MonoBehaviour
{
	Camera360 cam360;

	void Awake()
	{
		cam360 = FindObjectOfType<Camera360>();
	}

	void Update ()
	{
		if (Input.GetMouseButton(0) || Input.touchCount > 0)
		{
			Vector3 screenPos = Input.mousePosition;
			screenPos.z = 10.0f;

			Vector3 worldPos;
			cam360.ScreenToWorldPoint(screenPos, out worldPos);

			transform.position = worldPos;
		}
	}
}
