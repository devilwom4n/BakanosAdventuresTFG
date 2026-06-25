using UnityEngine;

public class ItemManzana : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement jugador = collision.GetComponent<PlayerMovement>();
            if (jugador != null)
            {
                // Cambiado al nombre exacto de tu script principal
                jugador.AñadirVidaManzana(); 
                Destroy(gameObject); 
            }
        }
    }
}
