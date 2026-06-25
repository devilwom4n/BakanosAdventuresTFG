using UnityEngine;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public float vidaMaxima = 2f;
    private float vidaActual;
    private bool estaMuerto = false;

    [Header("Configuración de Movimiento y Detección")]
    public bool tieneMovimiento = true;
    public float velocidad = 2f;
    public Transform detectar_Suelo; // GameObject vacío para raycast de suelo al frente (detecta precipicios)
    public Transform detectar_Pared; // GameObject vacío para raycast de pared al frente
    public float distancia_Deteccion = 0.1f; // Distancia para ambos raycasts
    public LayerMask capa_Suelo; // ¡IMPORTANTE! Esta debe ser la capa de tus plataformas

    private bool mirando_Derecha = true;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider; // Referencia al collider del enemigo

    [Header("Ataque al Contacto")]
    public float danoContacto = 1f;

    void Start()
    {
        vidaActual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();

        // Validaciones importantes
        if (rb == null) Debug.LogError("Enemigo requiere un Rigidbody2D para funcionar correctamente. Asegúrate de añadir uno.", this);
        if (enemyCollider == null) Debug.LogError("Enemigo requiere un Collider2D para detectar colisiones y el suelo. Asegúrate de añadir uno.", this);
        if (spriteRenderer == null) Debug.LogError("Enemigo requiere un SpriteRenderer.", this);
        if (animator == null) Debug.LogWarning("Enemigo no tiene Animator. Las animaciones no funcionarán.", this);

        if (!tieneMovimiento)
        {
            if (rb != null)
            {
                rb.isKinematic = true; // Si no tiene movimiento, no le afecta la física
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            // Advertencias si los detectores no están asignados, crucial para el patrullaje
            if (detectar_Suelo == null) Debug.LogError("Detectar_Suelo no asignado en Enemigo con movimiento. Asigna un GameObject vacío un poco más adelante y abajo de los pies del enemigo para detectar precipicios.", this);
            if (detectar_Pared == null) Debug.LogError("Detectar_Pared no asignado en Enemigo con movimiento. Asigna un GameObject vacío un poco adelante del centro del enemigo para detectar paredes.", this);
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        if (tieneMovimiento)
        {
            Patrullar();
        }
        
        // Actualizar animación de caminar/correr si el Animator y sus parámetros existen
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (ContainsAnimatorParameter("isWalking", AnimatorControllerParameterType.Bool))
            {
                animator.SetBool("isWalking", Mathf.Abs(rb.velocity.x) > 0.1f);
            }
            else if (ContainsAnimatorParameter("isRunning", AnimatorControllerParameterType.Bool))
            {
                animator.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0.1f);
            }
        }

        DibujarRayosDePrueba(); // Para visualizar los rayos en el Editor
    }

        // 1. AUMENTA ESTA VARIABLE EN EL INSPECTOR (Prueba con 0.5 o 1.0 si es necesario)
    // public float distancia_Deteccion = 0.5f; 

    void Patrullar()
    {
        // Mover el enemigo horizontalmente de forma fluida
        rb.velocity = new Vector2(velocidad * (mirando_Derecha ? 1 : -1), rb.velocity.y);

        // Detección de suelo al frente (precipicios)
        bool suelo_Adelante_Detectado = false; 
        if (detectar_Suelo != null)
        {
            // Lanzamos el rayo hacia abajo
            suelo_Adelante_Detectado = Physics2D.Raycast(detectar_Suelo.position, Vector2.down, distancia_Deteccion, capa_Suelo);
        }
       
        // Detección de pared al frente
        bool pared_Detectada = false;
        Vector2 direccion_pared = mirando_Derecha ? Vector2.right : Vector2.left;
        if (detectar_Pared != null)
        {
            pared_Detectada = Physics2D.Raycast(detectar_Pared.position, direccion_pared, distancia_Deteccion, capa_Suelo);
        }

        // CONDICIÓN CORREGIDA: Si detecta pared O si NO detecta suelo adelante.
        // Quitamos "IsGroundedPhysicsCheck()" temporalmente de aquí para evitar que se quede atrapado al pasar el borde.
        if (pared_Detectada || !suelo_Adelante_Detectado) 
        {
            Cambiar_Direccion();
        }
    }

    void Cambiar_Direccion()
    {
        mirando_Derecha = !mirando_Derecha;

        // CORRECCIÓN DE GIRO: En vez de usar flipX, rotamos el objeto en el eje Y (180 grados).
        // Esto hace que los GameObjects hijos (los detectores) también cambien de lado automáticamente.
        transform.Rotate(0f, 180f, 0f);
    }


    // Método para detectar si el enemigo está en el suelo usando un BoxCast
    // Un BoxCast es más robusto que un Raycast simple para la detección de suelo bajo un collider.
    bool IsGroundedPhysicsCheck()
    {
        if (rb == null || enemyCollider == null) return false;
        
        // Define la posición y tamaño del BoxCast.
        // Lo hacemos un poco más pequeño que el collider real para evitar falsas detecciones en esquinas.
        // El BoxCast debe estar ligeramente por debajo de los pies del enemigo.
        Vector2 boxCastOrigin = enemyCollider.bounds.center;
        Vector2 boxCastSize = new Vector2(enemyCollider.bounds.size.x * 0.8f, 0.05f); // Un "pie" delgado
        float boxCastDistance = 0.1f; // Pequeña distancia para asegurar contacto con el suelo

        // Mover el origen del BoxCast un poco hacia abajo para que esté al nivel de los pies
        // Ajustamos la posición y. Esto hace que el "pie" quede justo debajo del collider.
        boxCastOrigin.y = enemyCollider.bounds.min.y - boxCastDistance / 2f; 

        // Realiza el BoxCast
        RaycastHit2D hit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, boxCastDistance, capa_Suelo);
        
        // Si el BoxCast golpea algo en la capa del suelo, está en el suelo.
        return hit.collider != null;
    }


 
    public void Recibir_Dano(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        Debug.Log(gameObject.name + " recibió " + cantidad + " de daño. Vida actual: " + vidaActual);

        if (animator != null && animator.runtimeAnimatorController != null && ContainsAnimatorParameter("hurt", AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger("hurt");
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

        void Morir()
    {
        estaMuerto = true;
        Debug.Log(gameObject.name + " ha muerto.");

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Apagamos el colisionador para que no estorbe ni cause lagazos
        Collider2D colliderEnemigo = GetComponent<Collider2D>();
        if (colliderEnemigo != null) colliderEnemigo.enabled = false;

        // =======================================================
        // OBLIGATORIO: SUMAR PUNTOS AL JUGADOR AL MORIR REALMENTE
        // =======================================================
        PlayerMovement jugador = FindObjectOfType<PlayerMovement>();
        if (jugador != null)
        {
            jugador.puntosActuales += 20; // Suma los 20 puntos en el código del héroe
            
            // Refrescamos el texto de la pantalla inmediatamente
            if (jugador.textoPuntosUI != null)
            {
                jugador.textoPuntosUI.text = "PUNTOS: " + jugador.puntosActuales;
            }
            Debug.Log("¡Enemigo eliminado legalmente! +20 Puntos añadidos.");
        }
        // =======================================================

        if (animator != null && animator.runtimeAnimatorController != null && ContainsAnimatorParameter("die", AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger("die");
            StartCoroutine(DestruirEnemigoDespuesDeAnimacion(2f)); 
        }
        else
        {
            Destroy(gameObject);
        }
    }


    IEnumerator DestruirEnemigoDespuesDeAnimacion(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (estaMuerto) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemigo colisionó con el jugador. Debería hacerle " + danoContacto + " de daño.");
            // Aquí es donde llamarías a un método en el script del jugador para infligir daño
            // Por ejemplo: collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(danoContacto);
        }
    }

    // Helper para comprobar si un parámetro existe en el Animator
    private bool ContainsAnimatorParameter(string name, AnimatorControllerParameterType type)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == name && param.type == type) return true;
        }
        return false;
    }

    // Dibuja los rayos y BoxCast para depuración en el Editor
    void DibujarRayosDePrueba()
{
    if (detectar_Suelo != null)
    {
        // Dibuja una línea roja hacia abajo desde los pies del enemigo
        Debug.DrawRay(detectar_Suelo.position, Vector2.down * distancia_Deteccion, Color.red);
    }
}

}