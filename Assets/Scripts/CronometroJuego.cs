using UnityEngine;
using TMPro; // OBLIGATORIO para controlar el texto TextMeshPro
using UnityEngine.SceneManagement;

public class CronometroJuego : MonoBehaviour
{
    [Header("Configuracion del Tiempo")]
    public float tiempoRestante = 300f; //

    [Header("Componentes de la Interfaz")]
    public TextMeshProUGUI textoVisual; // Arrastra aqui tu TextoCronometro
    public GameObject panelMuerte;      // Arrastra aqui tu panel de muerte principal (el del FondoNegroMuerte)

    private bool juegoTerminado = false;

    void Update()
 {
    if (juegoTerminado) return;

    // NUEVO: Si el panel de muerte está encendido en la pantalla, detenemos el reloj
    if (panelMuerte != null && panelMuerte.activeSelf)
    {
        return; // Rompe el script y congela los números
    }

    if (tiempoRestante > 0)
    {
        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 30f && textoVisual != null)
        {
            textoVisual.color = Color.red; 
        }

        ActualizarTextoVisual();
    }
    else
    {
        tiempoRestante = 0;
        ActualizarTextoVisual();
        ActivarMuertePorTiempo();
    }
 }


    void ActualizarTextoVisual()
    {
        if (textoVisual == null) return;

        // Convertimos los segundos a un formato limpio de Minutos:Segundos
        int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60f);

        // El string.Format hace que si queda 1 segundo salga "01" en vez de "1"
        textoVisual.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    void ActivarMuertePorTiempo()
    {
        juegoTerminado = true;

        // Buscamos al heroe en el mapa y lo desactivamos
        GameObject heroe = GameObject.Find("hero1_0");
        if (heroe != null) heroe.SetActive(false);

        // Encendemos tu panel de muerte de golpe
        if (panelMuerte != null)
        {
            panelMuerte.SetActive(true);
        }
    }
}
