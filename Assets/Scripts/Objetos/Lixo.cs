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
            // 1. Avisa o script do Timer para parar de rodar e sumir da tela imediatamente
            CountdownTimer.PararEDesativarTimer();

            // Invoca qualquer outro evento placeholder que você já tinha configurado
            if (pararTimer != null)
            {
                pararTimer.Invoke(); // FUNCAO PLACEHOLDER PRO J1
            }

            // 2. Desativa temporariamente o CharacterController do jogador (se houver) para o teletransporte funcionar
            CharacterController controller = collision.gameObject.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            // 3. Teletransporta o jogador para a Vila
            collision.gameObject.transform.position = tpVila.position;

            // Reativa o controller após mover
            if (controller != null) controller.enabled = true;

            // 4. Destrói o objeto do lixo por último para evitar que o código pare antes da hora
            Destroy(gameObject);
        }
    }
}