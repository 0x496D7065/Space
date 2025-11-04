using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] float rotationSpeed = 1;

    public void Update()
    {
        root.transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered");
        if (other.GetComponent<DetectableObject>())
        {
            Debug.Log("Detectable Object found");
            other.GetComponent<DetectableObject>().Detected();
        }
    }
}
