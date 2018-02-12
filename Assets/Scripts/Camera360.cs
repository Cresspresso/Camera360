using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera360 : MonoBehaviour
{
	public Camera prefab;

	[Tooltip("Number of cameras.")]
	[Range(1, 20)]
	public int count = 11;

	[Tooltip("The total horizontal field of view.")]
	[Range(1f, 360f)]
	public float arc = 360f;

	// private fields

	// All cameras, ordered from left to right.
	private Camera[] cameras;
	
	// logic

	void Awake()
	{
		cameras = new Camera[count];

		// get constants
		float oneOverCount = 1f / count;
		float arcOverCount = arc / count;

		float rotationOffset = -0.5f;
		if (count % 2 != 0)
			rotationOffset += 0.5f / count;

		Rect rect = new Rect(
			0f,
			0f,
			oneOverCount,
			1f
		);

		// set up all cameras
		// MainCamera is in the middle, facing forwards.
		int middle = count / 2;
		for (int index = 0; index < count; ++index)
		{
			// get camera
			Camera cam;
			if (index == middle)
			{
				cam = Camera.main;
				cam.transform.SetParent(this.transform);
				cam.transform.SetAsLastSibling();
			}
			else
			{
				cam = ((GameObject)Instantiate(prefab.gameObject, this.transform)).GetComponent<Camera>();
				cam.name = prefab.name + " (" + index + ")";
			}
			cameras[index] = cam;

			// get constants
			float indexOverCount = (float)index / count;
			rect.x = indexOverCount;

			// set up camera
			cam.transform.localPosition = Vector3.zero;
			cam.transform.localEulerAngles = new Vector3(
				0,
				(indexOverCount + rotationOffset) * arc,
				0
			);

			cam.rect = rect;

			cam.fieldOfView = FOV_HorzToVert(arcOverCount, cam.aspect);
		}
	}
	
	// Converts vertical field of view to horizontal field of view.
	float FOV_VertToHorz(float vertFOV, float aspect)
	{
		return Mathf.Atan(Mathf.Tan((vertFOV * Mathf.Deg2Rad) / 2) * aspect) * 2 * Mathf.Rad2Deg;
	}

	// Converts horizontal field of view to vertical field of view.
	float FOV_HorzToVert(float horzFOV, float aspect)
	{
		return Mathf.Atan(Mathf.Tan((horzFOV * Mathf.Deg2Rad) / 2) / aspect) * 2 * Mathf.Rad2Deg;
	}

	

	public bool WorldToScreenPoint(Vector3 position, out Vector3 screenPosition)
	{
		// get vector on plane
		Vector3 vec = position - transform.position;
		vec = Vector3.ProjectOnPlane(vec, transform.up);

		// get signed angle
		float angle = Vector3.Angle(vec, transform.forward);
		if (Vector3.Dot(vec, transform.right) < 0f)
			angle = -angle;

		// get constants
		float arcOverCount = arc / count;
		float arcOverCountHalved = arcOverCount / 2;

		float rotationOffset = -0.5f;
		if (count % 2 != 0)
			rotationOffset += 0.5f / count;

		// find camera whose view contains that world position
		Camera cam = null;
		for (int index = 0; index < count; ++index)
		{
			// get constants
			float indexOverCount = (float)index / count;
			float camAngle = (indexOverCount + rotationOffset) * arc;
			float minAngle = camAngle - arcOverCountHalved;
			float maxAngle = camAngle + arcOverCountHalved;

			// if same direction
			if (angle >= minAngle && angle <= maxAngle)
			{
				cam = cameras[index];
				break;
			}
		}

		if (cam == null)
		{
			// could not find camera (position is outside of arc range)
			screenPosition = Camera.main.WorldToScreenPoint(position);
			return false;
		}

		screenPosition = cam.WorldToScreenPoint(position);
		return true;
	}

	public bool ScreenToWorldPoint(Vector3 position, out Vector3 worldPosition)
	{
		position.x /= Screen.width;
		position.y /= Screen.height;
		return ViewportToWorldPoint(position, out worldPosition);
	}

	public bool ViewportToWorldPoint(Vector3 position, out Vector3 worldPosition)
	{
		// clamp position x to screen by repeat/overflow
		if (arc == 360f)
		{
			while (position.x > 1f)
				position.x -= 1f;
			while (position.x < 0f)
				position.x += 1f;
		}

		// find camera whose rect contains that viewport position
		Camera cam = null;
		for (int index = 0; index < count; ++index)
		{
			// get constants
			float indexOverCount = (float)index / count;
			float indexP1OverCount = (float)(index + 1) / count;

			// if within rect
			if (position.x >= indexOverCount && position.x <= indexP1OverCount)
			{
				cam = cameras[index];
				break;
			}
		}

		// get world point

		position.x *= Screen.width;
		position.y *= Screen.height;

		if (cam == null)
		{
			// could not find camera (x is out of range)
			worldPosition = Camera.main.ScreenToWorldPoint(position);
			return false;
		}

		worldPosition = cam.ScreenToWorldPoint(position);
		return true;
	}
}
