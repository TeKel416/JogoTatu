using UnityEngine;
using UnityEngine.Events;

public class Lixo : MonoBehaviour
{
    public Transform tpVila;
    public UnityEvent pararTimer; // FUNCAO PLACEHOLDER PRO J1

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            collision.gameObject.GetComponent<PlayerController>().transform.position = tpVila.position;

            pararTimer.Invoke(); // FUNCAO PLACEHOLDER PRO J1
        }
    }
}
