using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ServidorFirebase : MonoBehaviour
{
    [Header("Configuracion del Servidor")]
    private string urlServidor = "https://bakanosadventure-default-rtdb.firebaseio.com/";

    [Header("Componentes de la Interfaz")]
    public GameObject subcontenedorRegistro; 
    public TMP_InputField cajaNombreInput;
    public TextMeshProUGUI textoTablaTop5;

    private float miTiempoFinal = 0f;
    private string myIDDevice = "";
    public int score_puntos;

    void Start()
    {
        // Al llegar a la meta final, frenamos el reloj global de juego
        if (ContadorGlobal.instancia != null)
        {
            ContadorGlobal.instancia.DetenerReloj();
            miTiempoFinal = ContadorGlobal.instancia.tiempoTotalJuego;
            myIDDevice = ContadorGlobal.instancia.idUnicoDispositivo;
        }

        // Escondemos el cuadro de registro por defecto hasta comprobar el Top
        if (subcontenedorRegistro != null)
        {
            subcontenedorRegistro.SetActive(false);
             if (cajaNombreInput != null)
        {
          
            TouchScreenKeyboard.hideInput = false; 

            cajaNombreInput.Select();             
            cajaNombreInput.ActivateInputField(); 
        }
        }

        StartCoroutine(DescargarYVerificarTop5());
    }

    [System.Serializable]
    public class EstructuraJugador
    {
        public string username;
        public float score_tiempo;
    }

    // SE ACTIVARÁ AL PULSAR EL BOTÓN "GUARDAR PUNTUACIÓN"
   public void EnviarPuntuacionAlTop()
{
    if (cajaNombreInput == null || string.IsNullOrEmpty(cajaNombreInput.text)) return;

    // 1. Bloqueamos la caja de texto para que no puedan editar el texto
    cajaNombreInput.interactable = false;

    // 2. Buscamos el botón "GUARDAR" y lo ponemos en modo DISABLE (Inactivable)
    GameObject botonGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
    if (botonGameObject != null)
    {
        UnityEngine.UI.Button botonComponente = botonGameObject.GetComponent<UnityEngine.UI.Button>();
        if (botonComponente != null)
        {
            botonComponente.interactable = false; // El botón se vuelve gris y se bloquea por completo
        }
    }

    // 3. Ejecutamos la subida normal a Firebase
    StartCoroutine(SubirRecordNube(myIDDevice, cajaNombreInput.text, miTiempoFinal));
}


    // --- CORRECCIÓN UNICA: SUBIDA A TIEMPO REAL REALTIME ---
    IEnumerator SubirRecordNube(string id, string usuario, float tiempo)
{
    EstructuraJugador pack = new EstructuraJugador { username = usuario, score_tiempo = tiempo };
    string json = JsonUtility.ToJson(pack);

    // =======================================================================
    // SOLUCIÓN DEFINITIVA PUNTO 2: Combinamos el usuario con el ID del movil
    // y los segundos para crear una ruta unica imposible de duplicar.
    // De esta forma, dos personas llamadas "MJ" con el mismo tiempo guardaran
    // dos filas separadas en Firebase y el marcador mostrara ambas en empate.
    // =======================================================================
    string claveUnicaFila = usuario + "_" + id + "_" + Mathf.FloorToInt(tiempo);
    string urlDestino = urlServidor + "leaderboard/" + claveUnicaFila + ".json";

    using (UnityWebRequest webRequest = UnityWebRequest.Put(urlDestino, json))
    {
        webRequest.method = "PUT";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            if (subcontenedorRegistro != null) subcontenedorRegistro.SetActive(false);
            StartCoroutine(DescargarYVerificarTop5());
        }
    }
}


    // --- CORRECCIÓN UNICA: DESCARGA A TIEMPO REAL REALTIME ---
    IEnumerator DescargarYVerificarTop5()
    {
        string urlDescarga = urlServidor + "leaderboard.json?orderBy=\"score_tiempo\"&limitToFirst=5";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlDescarga))
        {
            webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonRecibido = webRequest.downloadHandler.text;

                // Pintamos el Top 5 en la pantalla del iPhone
                if (textoTablaTop5 != null)
                {
                    textoTablaTop5.text = "--- TOP 5 MEJORES TIEMPOS ---\n" + FormatearJSONATexto(jsonRecibido);
                }

                VerificarSiEsTop5(jsonRecibido);
            }
        }
    }

    void VerificarSiEsTop5(string json)
    {
        if (json == "{}" || string.IsNullOrEmpty(json) || json == "null")
        {
            ActivarCajaTeclado();
            return;
        }

        int cantidadRegistros = 0;
        float peorTiempoTop = 0f;

        string[] particiones = json.Split(new string[] { "\"score_tiempo\":" }, System.StringSplitOptions.None);

        for (int i = 1; i < particiones.Length; i++)
        {
            cantidadRegistros++;
            string textoNumero = particiones[i].Split(',')[0].Split('}')[0].Trim();
            if (float.TryParse(textoNumero, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float tiempoDetectado))
            {
                if (tiempoDetectado > peorTiempoTop) peorTiempoTop = tiempoDetectado;
            }
        }

        if (cantidadRegistros < 5 || miTiempoFinal < peorTiempoTop)
        {
            ActivarCajaTeclado();
        }
    }

    void ActivarCajaTeclado()
    {
        if (subcontenedorRegistro != null)
        {
            subcontenedorRegistro.SetActive(true);
            
            if (cajaNombreInput != null)
            {
                cajaNombreInput.Select();             
                cajaNombreInput.ActivateInputField(); // Llama al teclado del iPhone al instante
            }
        }
    }

    string FormatearJSONATexto(string json)
    {
        if (json == "{}" || string.IsNullOrEmpty(json) || json == "null") return "¡Sé el primero en marcar un récord!";
        
        string resultado = "";
        string[] filas = json.Split(new string[] { "}," }, System.StringSplitOptions.None);
        int puesto = 1;

        foreach (string fila in filas)
        {
            if (!fila.Contains("\"username\":")) continue;
            string usuario = fila.Split(new string[] { "\"username\":\"" }, System.StringSplitOptions.None)[1].Split('"')[0];
            string tiempo = fila.Split(new string[] { "\"score_tiempo\":" }, System.StringSplitOptions.None)[1].Split('}')[0].Split(',')[0];
            
            if (float.TryParse(tiempo, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float t))
            {
                int min = Mathf.FloorToInt(t / 60f);
                int seg = Mathf.FloorToInt(t % 60f);
                resultado += puesto + ". " + usuario + " - " + string.Format("{0:00}:{1:00}", min, seg) + "\n";
                puesto++;
            }
        }
        return resultado;
    }
}
