using UnityEngine;

public class SkyCameraRotation : MonoBehaviour {

    public float Speed;

	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(Time.deltaTime*Speed, 0, 0));
	}
}
