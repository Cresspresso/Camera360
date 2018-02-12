using UnityEngine;

// Displays the screen position of a world-space object, accounting for Camera360.
public class CameraRectLog : MonoBehaviour
{
	// public fields

	[Tooltip("Where on the screen to display this message.")]
	public Rect rect = new Rect(20, 20, 200, 30);

	[Tooltip("The world-space GameObject.")]
	public Transform target;

	// private fields

	private Camera360 cam360;
	private Vector3 screenPosition;

	// methods

	void Awake()
	{
		cam360 = FindObjectOfType<Camera360>();
	}

	void OnGUI()
	{
		cam360.WorldToScreenPoint(target.position, out screenPosition);

		GUI.Label(rect, screenPosition.ToString());
	}
}
