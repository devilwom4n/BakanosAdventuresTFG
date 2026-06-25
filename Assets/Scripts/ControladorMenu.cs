using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorMenu : MonoBehaviour
{
    [Header("Configuracion de Escenas")]
    public string nombrePrimerNivel = "SampleScene"; 

    [Header("Paneles de la Interfaz")]
    public GameObject panelAjustes; 
    public UnityEngine.UI.Image fondoBrillo; // Objeto FondoBrillo del Canvas

    [Header("Sliders de Ajustes")]
    public Slider sliderVolumen; 
    public Slider sliderBrillo; // Tu BarraBrillo

    void Start()
    {
        // 1. CARGAR VOLUMEN
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenJuego", 80f);
        if (sliderVolumen != null) sliderVolumen.value = volumenGuardado;
        AudioListener.volume = volumenGuardado / 100f;

        // 2. CARGAR BRILLO (Por defecto 0.5f, que es neutral en medio)
        float brilloGuardado = PlayerPrefs.GetFloat("BrilloJuego", 0.5f);
        if (sliderBrillo != null) sliderBrillo.value = brilloGuardado;
        
        AplicarBrilloFisico(brilloGuardado);

        // Desactivar panel al arrancar solo si existe
        if (panelAjustes != null) panelAjustes.SetActive(false);
    }

    public void Jugar() { SceneManager.LoadScene(nombrePrimerNivel);
    Time.timeScale = 1f; }
    public void AbrirAjustes() { if (panelAjustes != null) panelAjustes.SetActive(true); }
    public void CerrarAjustes() { if (panelAjustes != null) panelAjustes.SetActive(false); }

    public void CambiarVolumen(float valor100)
    {
        AudioListener.volume = valor100 / 100f;
        PlayerPrefs.SetFloat("VolumenJuego", valor100);
        PlayerPrefs.Save(); 
    }

    public void CambiarBrillo(float valorSlider)
    {
        AplicarBrilloFisico(valorSlider);
        PlayerPrefs.SetFloat("BrilloJuego", valorSlider);
        PlayerPrefs.Save();
    }

    private void AplicarBrilloFisico(float valor)
    {
        if (fondoBrillo == null) return;

        // Si el valor está en el centro exacto (0.5), el alpha es 0 (totalmente invisible)
        if (valor >= 0.5f)
        {
            // Mover a la derecha: ACLARA (Filtro Blanco)
            float alphaAclarar = Mathf.Lerp(0f, 0.25f, (valor - 0.5f) * 2f);
            fondoBrillo.color = new Color(1f, 1f, 1f, alphaAclarar);
        }
        else
        {
            // Mover a la izquierda: OSCURECE (Filtro Negro)
            float alphaOscurecer = Mathf.Lerp(0.7f, 0f, valor * 2f);
            fondoBrillo.color = new Color(0f, 0f, 0f, alphaOscurecer);
        }
    }

    public void CerrarJuego() { Application.Quit(); }
}
