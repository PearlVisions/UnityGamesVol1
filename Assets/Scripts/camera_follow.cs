using UnityEngine;

public class camera_follow : MonoBehaviour
{
    public Transform target;      // Pelaaja
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        // Haluttu sijainti
        Vector3 desiredPosition = target.position + offset;

        // Pehmeä siirtymä
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // Aseta kameran uusi sijainti
        transform.position = smoothedPosition;
    }
}
