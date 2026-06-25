using UnityEngine;
using UnityEngine.SceneManagement;

public class ContadorGlobal : MonoBehaviour
{
    public static ContadorGlobal instancia;

    [Header("Datos del Jugador")]
    public string nombreUsuario = "";
    public float tiempoTotalJuego = 0f;
    public string idUnicoDispositivo = "";

    private bool juegoTerminado = false;

    void Awake()
{
    if (instancia == null)
    {
        instancia = this;
        DontDestroyOnLoad(gameObject);
        idUnicoDispositivo = SystemInfo.deviceUniqueIdentifier;
    }
    else
    {
        Destroy(gameObject);
    }
}

// NUEVO TRUCO PARA EL TFG: Forzar la creación si se arranca desde cualquier nivel
public static void AsegurarContador()
{
    if (instancia == null)
    {
        GameObject go = new GameObject("GestorGlobalAutocreado");
        instancia = go.AddComponent<ContadorGlobal>();
        DontDestroyOnLoad(go);
        instancia.idUnicoDispositivo = SystemInfo.deviceUniqueIdentifier;
    }
}


    void Update()
    {
        // Si el jugador está jugando niveles (no en el menu), sumamos los segundos reales transcurridos
        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual != "MenuPrincipal" && !juegoTerminado)
        {
            tiempoTotalJuego += Time.deltaTime;
        }
    }

    public void DetenerReloj()
    {
        juegoTerminado = true;
    }
}
