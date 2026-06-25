using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario si mantienes funciones como Morir() o ReiniciarEscena()

public class PlayerMove : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 10f;
    public float fuerza_Salto = 19f;
    public float saltos_Maximos = 1f; // 1 para salto simple, 2 para doble salto
    private float saltos_Actuales;

    // Tu lógica de botones
    private bool moveLeft;
    private bool moveRight;
    private bool isGrounded; // Detecta si está en el suelo

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // Para animaciones básicas

    // --- Componentes adicionales del compañero (mantener si existen en el prefab) ---
    // Si tu compañero tiene detecciones de suelo/pared más complejas o el BoxCollider2D para agacharse
    [Header("Detección de Suelo (del compañero, si aplica)")]
    public LayerMask capa_Suelo;
    public Transform controlador_Suelo;
    public Vector2 dimensiones_Caja_Suelo;

    // --- Variables para funcionalidades futuras (inicialmente no usadas, pero listas) ---
    private bool mirando_Derecha = true; // Para voltear el sprite correctamente

    /* Inicialización del personaje */
    void Start()
    {
        Application.targetFrameRate = 50; // De tu script original

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Obtener el Animator
        saltos_Actuales = saltos_Maximos;

        // Aquí iría tu lógica de carga de brillo y volumen si aún la necesitas,
        // pero para el movimiento básico, la omitimos para simplificar.
        // float vol = PlayerPrefs.GetFloat("VolumenJuego", 80f);
        // AudioListener.volume = vol / 100f;
        // ... Lógica de brillo ...

        // Asegúrate de que los parámetros del Animator existen para evitar warnings
        if (animator != null)
        {
            // Solo para evitar el warning 'isGrounded' en este script simplificado.
            // Los otros parámetros (isRunning, salto, etc.) se añadirán cuando los uses.
            if (!ContainsAnimatorParameter("isGrounded", AnimatorControllerParameterType.Bool))
                Debug.LogWarning("Animator parameter 'isGrounded' not found. Please add a Bool parameter named 'isGrounded' to your Animator Controller.");
            if (!ContainsAnimatorParameter("velocityY", AnimatorControllerParameterType.Float))
                Debug.LogWarning("Animator parameter 'velocityY' not found. Please add a Float parameter named 'velocityY' to your Animator Controller.");
        }
    }

    // Función auxiliar para verificar parámetros del Animator
    private bool ContainsAnimatorParameter(string name, AnimatorControllerParameterType type)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == name && param.type == type) return true;
        }
        return false;
    }

    void Update()
    {
        // Solo para debug y unificar tu isGrounded con la lógica de saltos
        DeteccionesDeSuelo(); // Usa la detección de tu compañero si está configurada, o tu OnCollision...

        Procesar_Movimiento_Basico();
        Actualizar_Animaciones_Movimiento();
    }

    // Usa la detección de suelo de tu compañero si el controlador_Suelo está asignado
    void DeteccionesDeSuelo()
    {
        if (controlador_Suelo != null && capa_Suelo != 0) // Si la detección de tu compañero está configurada
        {
            isGrounded = Physics2D.BoxCast(controlador_Suelo.position, dimensiones_Caja_Suelo, 0f, Vector2.down, 0.1f, capa_Suelo);
        }
        // Si no, tu OnCollision... seguirá actualizando 'isGrounded'
        
        if (isGrounded)
        {
            saltos_Actuales = saltos_Maximos; // Reinicia los saltos al tocar el suelo
        }
    }

    void Procesar_Movimiento_Basico()
    {
        float input_X = 0;
        if (moveLeft)
        {
            input_X = -1;
            mirando_Derecha = false; // Actualiza la dirección para el volteo de sprite
        }
        else if (moveRight)
        {
            input_X = 1;
            mirando_Derecha = true; // Actualiza la dirección para el volteo de sprite
        }

        rb.velocity = new Vector2(input_X * velocidad, rb.velocity.y);

        Gestionar_Orientacion_Sprite(input_X);
    }

    void Actualizar_Animaciones_Movimiento()
    {
        if (animator == null) return;

        bool isRunning = Mathf.Abs(rb.velocity.x) > 0.1f; // Si se está moviendo horizontalmente
        animator.SetBool("isRunning", isRunning); // Asume que tienes un parámetro "isRunning" (Bool)

        animator.SetBool("isGrounded", isGrounded); // Asume que tienes un parámetro "isGrounded" (Bool)
        animator.SetFloat("velocityY", rb.velocity.y); // Asume que tienes un parámetro "velocityY" (Float)
    }

    void Gestionar_Orientacion_Sprite(float input_Movimiento)
    {
        if (spriteRenderer == null) return;

        if (input_Movimiento < 0 && mirando_Derecha)
        {
            spriteRenderer.flipX = true;
            mirando_Derecha = false;
        }
        else if (input_Movimiento > 0 && !mirando_Derecha)
        {
            spriteRenderer.flipX = false;
            mirando_Derecha = true;
        }
    }

    // =======================================================
    // BOTONES UI (Conectar en el componente Event Trigger o On Click)
    // =======================================================

    // Conectar al boton izquierdo en POINTER DOWN
    public void MoveLeftDown()
    {
        moveLeft = true;
    }

    // Conectar al boton izquierdo en POINTER UP
    public void MoveLeftUp()
    {
        moveLeft = false;
    }

    // Conectar al boton derecho en POINTER DOWN
    public void MoveRightDown()
    {
        moveRight = true;
    }

    // Conectar al boton derecho en POINTER UP
    public void MoveRightUp()
    {
        moveRight = false;
    }

    // Conectar al boton de saltar (Se puede usar un boton normal con ON CLICK)
    public void Jump()
    {
        if (isGrounded || saltos_Actuales > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, fuerza_Salto);
            saltos_Actuales--;
            if (animator != null) animator.SetTrigger("salto"); // Asume que tienes un parámetro "salto" (Trigger)
        }
    }

    // =======================================================
    // DETECCION DE SUELO (Tus sistemas de colision de fisica 2D)
    // =======================================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
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

    // --- Funciones del compañero para futuras integraciones ---
    // (Puedes dejarlas comentadas o eliminarlas si no las vas a usar)

    /*public void Tomar_Dano(float cantidad_Dano) { // Lógica de daño }
    void Morir() { // Lógica de muerte }
    public void Aplicar_Golpe() { // Lógica de empuje al ser golpeado }
    public void Curar(float cantidad) { // Lógica de curación }*/

    // Puedes mantener el OnDrawGizmos para visualizar la detección de suelo si usas controlador_Suelo
    private void OnDrawGizmos()
    {
        if (controlador_Suelo != null && dimensiones_Caja_Suelo != Vector2.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(controlador_Suelo.position, dimensiones_Caja_Suelo);
        }
    }
}