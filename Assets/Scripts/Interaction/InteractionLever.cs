using UnityEngine;

public class InteractionLever : Interactable
{
    [SerializeField] private GameObject lever;

    private bool leverPulled = false;

    [SerializeField] private float maxRotationDelta = 50f;

    private Vector3 leverPulledRotation;
    private Vector3 startRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() 
    {
        base.Start();
        float leverRotationZ = lever.transform.localEulerAngles.z;
        startRotation = lever.transform.localEulerAngles;
        leverPulledRotation = startRotation;
        leverPulledRotation.z = -leverRotationZ;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (leverPulled)
        {
            Vector3 vec = lever.transform.localEulerAngles;

            if (!vec.Equals(leverPulledRotation))
            {
                vec.z = Mathf.MoveTowardsAngle(vec.z, leverPulledRotation.z, maxRotationDelta);
                lever.transform.localEulerAngles = vec;
                /*vec.z -= maxRotationDelta * Time.deltaTime;
                if (vec.z < leverPulledRotation.z))
                {
                    vec.z = leverPulledRotation.z;
                }
                lever.transform.localEulerAngles = vec;*/
            }
        }
        else
        {
            Vector3 vec = lever.transform.localEulerAngles;

            if (!vec.Equals(startRotation))
            {
                vec.z = Mathf.MoveTowardsAngle(vec.z, startRotation.z, maxRotationDelta);
                lever.transform.localEulerAngles = vec;
                /*vec.z += maxRotationDelta * Time.deltaTime;
                if (vec.z > startRotation.z)
                {
                    vec.z = startRotation.z;
                }
                lever.transform.localEulerAngles = vec;*/
            }
        }
    }
    
    public override void Interact()
    {
        leverPulled = !leverPulled;
    }

    public override void StopInteract()
    {
    }
}
