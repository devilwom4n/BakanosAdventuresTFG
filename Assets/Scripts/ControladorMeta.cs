using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class ControladorMeta : MonoBehaviour
{
    [Header("Configuracion del Viaje")]
    public string siguienteEscena = "Scene2"; 
    public GameObject pantallaNegra;           

    private bool yaHaPasado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Player") || collision.name.Contains("hero")) && !yaHaPasado)
        {
            yaHaPasado = true;
            StartCoroutine(SecuenciaCambioNivel());
        }
    }IEnumerator SecuenciaCambioNivel()
{
    if (pantallaNegra != null)
    {
        pantallaNegra.SetActive(true);
        Image img = pantallaNegra.GetComponent<Image>();
        TextMeshProUGUI textoTMP = pantallaNegra.GetComponentInChildren<TextMeshProUGUI>();

        float tiempoFundido = 1.0f; 
        float tiempoTranscurrido = 0f;

        // CAMBIO: Usamos unscaledDeltaTime para ignorar la pausa del juego
        while (tiempoTranscurrido < tiempoFundido)
        {
            tiempoTranscurrido += Time.unscaledDeltaTime; 
            float porcentaje = tiempoTranscurrido / tiempoFundido;

            if (img != null) img.color = new Color(0f, 0f, 0f, porcentaje);
            if (textoTMP != null) textoTMP.color = new Color(1f, 1f, 1f, porcentaje);

            yield return null;
        }

        if (img != null) img.color = Color.black;
        if (textoTMP != null) textoTMP.color = Color.white;

        float tiempoEspera = 2.0f;
        float relojEspera = 0f;

        // CAMBIO: Usamos unscaledDeltaTime y unscaledTime para el parpadeo
        while (relojEspera < tiempoEspera)
        {
            relojEspera += Time.unscaledDeltaTime;

            if (textoTMP != null)
            {
                // Usamos Time.unscaledTime para que el texto parpadee en pausa
                float alphaParpadeo = Mathf.PingPong(Time.unscaledTime * 2.5f, 1f); 
                textoTMP.color = new Color(1f, 1f, 1f, alphaParpadeo);
            }

            yield return null;
        }
    }

    // Antes de cambiar de escena, asegúrate de restablecer el tiempo si estaba pausado
    Time.timeScale = 1f; 
    SceneManager.LoadScene(siguienteEscena);
}
}
