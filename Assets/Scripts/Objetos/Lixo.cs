using UnityEngine;

public class Lixo : MonoBehaviour
{
    public Transform tpVila;

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            collision.gameObject.GetComponent<PlayerController>().transform.position = tpVila.position;
        }
    }
}
