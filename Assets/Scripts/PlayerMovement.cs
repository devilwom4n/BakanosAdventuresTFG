using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Obligatorio para usar imágenes de la interfaz (UI)
using UnityEngine.SceneManagement; // Obligatorio para el sistema de escenas

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    public float speed = 10f;
    public float jumpForce = 19f;

    [Header("Configuracion de Vida y UI")]
    public int vidasActuales = 4; // Cambiado a 4 por la manzana
    public int vidasMaximas = 4;  // Límite máximo para no complicarse
    public Image[] corazonesUI;
    public GameObject panelMuerte; // Arrastra tu objeto PanelMuerte aquí
    public float tiempoInvulnerabilidad = 1.5f;
    private bool esInvulnerable = false;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool moveLeft;
    private bool moveRight;
    private bool isGrounded;
    private bool estaMuerto = false;

    [Header("Configuracion de Combate")]
    public Transform controladorAtaque; // Objeto vacío que indica dónde golpear
    public float radioAtaque = 0.5f; // Qué tan grande es el golpe
    public LayerMask capaEnemigo; // Capa donde está tu enemigo (Enemigo)

    [Header("Variables Guardadas (Puntos y Tiempo)")]
    public int puntosActuales = 0;
    public TMPro.TMP_Text textoPuntosUI; // Si usaste Text clásico, pon: public Text textoPuntosUI;

    public float tiempoTotalJuego = 0f;

    void Start()
    {
        float vol = PlayerPrefs.GetFloat("VolumenJuego", 80f);
        AudioListener.volume = vol / 100f;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // =======================================================
        // GESTIÓN DE DATOS ENTRE ESCENAS
        // =======================================================
        int indexEscena = SceneManager.GetActiveScene().buildIndex;

        // Si es la primera escena del juego (Índice 0), empezamos limpios de cero
        if (indexEscena == 0)
        {
            vidasActuales = vidasMaximas;
            puntosActuales = 0;
            tiempoTotalJuego = 0f;

            // Borramos datos de partidas anteriores para que no se queden guardados
            PlayerPrefs.DeleteKey("VidasGuardadas");
            PlayerPrefs.DeleteKey("PuntosGuardados");
            PlayerPrefs.DeleteKey("TiempoGuardado");
        }
        else
        {
            // Si es la escena 2, cargamos todo lo acumulado en la escena 1
            vidasActuales = PlayerPrefs.GetInt("VidasGuardadas", vidasMaximas);
            puntosActuales = PlayerPrefs.GetInt("PuntosGuardados", 0);
            tiempoTotalJuego = PlayerPrefs.GetFloat("TiempoGuardado", 0f);
        }

        float brilloGuardado = PlayerPrefs.GetFloat("BrilloJuego", 0.5f);
        UnityEngine.UI.Image imgBrillo = GetComponentInChildren<UnityEngine.UI.Image>();

        if (imgBrillo != null)
        {
            if (brilloGuardado >= 0.5f)
            {
                imgBrillo.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 0.25f, (brilloGuardado - 0.5f) * 2f));
            }
            else
            {
                imgBrillo.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.7f, 0f, brilloGuardado * 2f));
            }
            Debug.Log("¡Brillo inyectado con exito!");
        }

        // Asegurar que el panel de muerte empiece oculto al iniciar
        if (panelMuerte != null) panelMuerte.SetActive(false);

        ActualizarCorazonesUI();
        ContadorGlobal.AsegurarContador();
        if (textoPuntosUI != null) textoPuntosUI.text = "PUNTOS: " + puntosActuales;

    }

    void Update()
    {
        if (estaMuerto) return;

        // Sumamos el tiempo real transcurrido fotograma a fotograma
        tiempoTotalJuego += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (estaMuerto) return;

        if (moveLeft)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            // GIRO CORRECTO: Rotamos el objeto a la izquierda (180 grados en Y)
            transform.eulerAngles = new Vector3(0, 180, 0); 
        }
        else if (moveRight)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            // GIRO CORRECTO: Volvemos a la posición original (0 grados en Y)
            transform.eulerAngles = new Vector3(0, 0, 0); 
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
    
    // =======================================================
    // SISTEMA DE DAÑO Y MUERTE
    // =======================================================
    public void RecibirDano()
    {
        // Si el héroe ya está muerto o está parpadeando por daño reciente, no hacer nada
        if (estaMuerto || esInvulnerable) return;

        vidasActuales--;
        ActualizarCorazonesUI();

        // Control estricto: Si la vida llega a 0 o menos, muere de inmediato y corta el script
        if (vidasActuales <= 0)
        {
            Morir();
            return; 
        }
        
        // Si aún le quedan vidas, hace el parpadeo de daño normal
        if (animator != null) animator.SetTrigger("hurt");
        StartCoroutine(EfectoParpadeo());
    }

    IEnumerator EfectoParpadeo()
    {
        esInvulnerable = true;
        float tiempoPasado = 0f;
        float intervaloParpadeo = 0.15f;

        // Si el jugador muere mientras parpadea, este bucle se detiene
        while (tiempoPasado < tiempoInvulnerabilidad && !estaMuerto)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);

            yield return new WaitForSeconds(intervaloParpadeo);

            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            yield return new WaitForSeconds(intervaloParpadeo);
            tiempoPasado += (intervaloParpadeo * 2);
        }

        // Solo vuelve a ser vulnerable si sobrevivió al golpe
        if (!estaMuerto)
        {
            esInvulnerable = false;
        }
    }

    void Morir()
    {
        estaMuerto = true;
        esInvulnerable = true; // Bloquea cualquier otra colisión de daño externa
        
        Debug.Log(gameObject.name + " ha muerto. Activando interfaz...");

        // Detener por completo los movimientos y fuerzas del héroe
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; 
        }

        // Restaurar el color original del sprite por si se quedó transparente al morir
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }

        if (animator != null) animator.SetTrigger("die");

        // ACTIVACIÓN FORZADA: Abre el panel de muerte sí o sí
        if (panelMuerte != null) 
        {
            panelMuerte.SetActive(true);
        }
        else
        {
            Debug.LogError("¡ERROR! No has arrastrado el PanelMuerte al Inspector del Héroe.");
        }
    }

    // =======================================================
    // METODOS DE RECOLECCIÓN (MANZANA Y MONEDA)
    // =======================================================
    public void AñadirVidaManzana()
    {
        if (vidasActuales < vidasMaximas)
        {
            vidasActuales++;
            ActualizarCorazonesUI();
            Debug.Log("¡Manzana comida! Vida aumentada a: " + vidasActuales);
        }
    }

   public void AñadirPuntosMoneda()
    {
        puntosActuales += 10; // Suma los 10 puntos en el código
        Debug.Log("¡Moneda recogida! +10 Puntos. Total: " + puntosActuales);

        // NUEVO: Esto obliga al marcador de la pantalla a actualizar el número al instante
        if (textoPuntosUI != null) 
        {
            textoPuntosUI.text = "PUNTOS: " + puntosActuales;
        }
    }

    // OBLIGATORIO: Llama a esta función desde el script que cambie de nivel antes de cargar la escena 2
    public void GuardarDatosEscena()
    {
        PlayerPrefs.SetInt("VidasGuardadas", vidasActuales);
        PlayerPrefs.SetInt("PuntosGuardados", puntosActuales);
        PlayerPrefs.SetFloat("TiempoGuardado", tiempoTotalJuego);
        PlayerPrefs.Save();
        Debug.Log("¡Datos guardados con éxito para el siguiente nivel!");
    }

    // =======================================================
    // BOTONES UI
    // =======================================================
         public void Atacar()
    {
        if (estaMuerto) return;

        if (animator != null) animator.SetTrigger("attack");

        if (controladorAtaque == null) return;

        // Detectar enemigos en el área
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(controladorAtaque.position, radioAtaque, capaEnemigo);

        foreach (Collider2D enemigo in enemigosGolpeados)
        {
            Enemigo scriptEnemigo = enemigo.GetComponent<Enemigo>();
            if (scriptEnemigo != null)
            {
                // Solo le hacemos daño. NO sumamos puntos aquí.
                scriptEnemigo.Recibir_Dano(10f);
            }
            else
            {
                Destroy(enemigo.gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controladorAtaque != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(controladorAtaque.position, radioAtaque);
        }
    }

    public void MoveLeftDown() { if (!estaMuerto) moveLeft = true; }
    public void MoveLeftUp() { moveLeft = false; }
    public void MoveRightDown() { if (!estaMuerto) moveRight = true; }
    public void MoveRightUp() { moveRight = false; }

    public void Jump()
    {
        if (isGrounded && !estaMuerto)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    // =======================================================
    // DETECCION DE COLISIONES


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("Enemigo") || collision.gameObject.CompareTag("Enemy"))
        {
            RecibirDano();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void ActualizarCorazonesUI()
    {
        for (int i = 0; i < corazonesUI.Length; i++)
        {
            if (corazonesUI[i] != null)
            {
                corazonesUI[i].enabled = i < vidasActuales;
            }
        }
    }
          private void OnTriggerEnter2D(Collider2D collision)
    {
        // Daño del enemigo si tiene Is Trigger activo
        if (collision.CompareTag("Enemigo") || collision.CompareTag("Enemy"))
        {
            RecibirDano();
        }

        // Detectar si lo que atravesamos es una Moneda
        if (collision.CompareTag("Moneda") || collision.CompareTag("Coin"))
        {
            AñadirPuntosMoneda(); // Te suma tus 10 puntos obligatorios
            Destroy(collision.gameObject); // Borra la moneda directamente de la pantalla
        }

        // Detectar si lo que atravesamos es una Manzana/Vida
        if (collision.CompareTag("Vida") || collision.CompareTag("Manzana"))
        {
            AñadirVidaManzana(); // Te suma una vida si te falta para llegar a 4
            Destroy(collision.gameObject); // Borra la manzana directamente de la pantalla
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemigo") || collision.CompareTag("Enemy"))
        {
            RecibirDano();
        }
    }

}
