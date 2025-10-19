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
        if (other.GetComponent<DetectableObject>())
        {
            other.GetComponent<DetectableObject>().Detected();
        }
    }
}
