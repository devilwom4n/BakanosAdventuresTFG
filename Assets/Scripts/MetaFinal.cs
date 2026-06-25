using UnityEngine;

public class MetaFinalJuego : MonoBehaviour
{
    [Header("Interfaz de Victoria")]
    public GameObject panelVictory; // Arrastra aquí tu PanelVictory principal

    private bool metaAlcanzada = false;

    // Detecta cuando el héroe pisa la rampa del final
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos si es el jugador y que no se active dos veces
        if ((collision.CompareTag("Player") || collision.name.Contains("hero")) && !metaAlcanzada)
        {
            metaAlcanzada = true;
            ActivarVictoriaFinal(collision.gameObject);
        }
    }

    void ActivarVictoriaFinal(GameObject heroe)
    {
        // 1. Apagamos el cuerpo del héroe para que no pise el panel ni se vea de fondo
        if (heroe != null) heroe.SetActive(false);

        // 2. Encendemos el panel de victoria y base de datos en el iPhone
        if (panelVictory != null)
        {
            panelVictory.SetActive(true);
        }

        // 3. Forzamos a que aparezca el cursor del ratón/táctil para poder clicar
        Cursor.visible = true;

        // 4. TRUCO VITAL: Congelamos las físicas y los botones de movimiento en seco
        // Esto frena el cronómetro y los controles, dejando el menú quieto para siempre
        Time.timeScale = 0f; 

        Debug.Log("¡Juego Completado! Panel de victoria activado de forma fija.");
    }
}
