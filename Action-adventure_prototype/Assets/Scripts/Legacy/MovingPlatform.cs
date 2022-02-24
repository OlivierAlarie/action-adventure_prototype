using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    // Transforms to act as start and end markers for the journey.
    public Transform endMarker;

    // Movement speed in units per second.
    public float speed = 1.0F;

    private Vector3 startPosition;
    private Vector3 endPosition;
    // Time when the movement started.
    private float distCovered = 0;
    private float direction = 1;
    // Total distance between the markers.
    private float journeyLength;

    void Start()
    {
        // Keep a note of the time the movement started.
        startPosition = transform.position;
        endPosition = endMarker.position;

        // Calculate the journey length.
        journeyLength = Vector3.Distance(startPosition, endPosition);
    }

    // Move to the target end position.
    void FixedUpdate()
    {
        // Distance moved equals elapsed time times speed..
        distCovered += direction * Time.deltaTime * speed;

        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / journeyLength;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);

        if (fractionOfJourney >= 1)
        {
            direction = -1;
        }
        else if (fractionOfJourney <= 0)
        {
            direction = 1;
        }
    }
}
