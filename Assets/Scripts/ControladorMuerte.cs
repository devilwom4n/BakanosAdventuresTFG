using UnityEngine;
using UnityEngine.SceneManagement;


public class ControladorMuerte : MonoBehaviour
{
    public GameObject panelMuerte; // Arrastra aqui el PanelMuerte

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detectamos si lo que cae es el heroe
        if (collision.CompareTag("Player") || collision.name.Contains("hero"))
        {
            // Desactivamos el movimiento del heroe para que no siga cayendo
            collision.gameObject.SetActive(false);

            // Activamos el menu de muerte
            if (panelMuerte != null)
            {
                panelMuerte.SetActive(true);
            }
            Time.timeScale = 0f;
        }
    }

    // Funcion publica para el boton de reiniciar
    public void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    // Añade esto al final de tu script ControladorMuerte.cs, antes de la última llave }
    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

}
