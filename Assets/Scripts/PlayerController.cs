using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour // Controlador sinxelo para o xogador con soporte para ambos sistemas de input de Unity
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;           // Velocidade de movemento cara adiante/atrás
    public float rotationSpeed = 180f;     // Velocidade de rotación (graos por segundo)

    [Header("Input System Selection")]
    public bool useNewInputSystem = true;  // True = novo Input System, False = Input Manager clásico

    private Vector2 moveInput; // Input actual do xogador

    void Update()
    {
        if (useNewInputSystem) // Obtén o input do sistema seleccionado
        {
            GetNewInput();
        }
        else
        {
            GetOldInput();
        }

        MovePlayer(); // Move o xogador baseándose no input
    }

    void GetOldInput() // Le o input usando o Input Manager clásico (Input.GetAxis)
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D ou frechas esquerda/dereita (eixos configurados en Edit > Project Settings > Input Manager)
        float vertical = Input.GetAxis("Vertical");     // W/S ou frechas arriba/abaixo
        moveInput = new Vector2(horizontal, vertical);
    }

    void GetNewInput() // Le o input usando o novo Input System (Keyboard.current)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) // Comproba se hai un teclado conectado
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.aKey.isPressed) horizontal -= 1f;  // A = esquerda
            if (Keyboard.current.dKey.isPressed) horizontal += 1f;  // D = dereita
            if (Keyboard.current.sKey.isPressed) vertical -= 1f;    // S = atrás
            if (Keyboard.current.wKey.isPressed) vertical += 1f;    // W = adiante

            moveInput = new Vector2(horizontal, vertical);
        }
#else
        GetOldInput(); // Se o novo sistema non está dispoñible, usa o antigo como fallback
#endif
    }

    void MovePlayer() // Move e rota o xogador baseándose no moveInput
    {
        if (Mathf.Abs(moveInput.x) > 0.01f) // Rota con A/D (input horizontal)
        {
            transform.Rotate(0f, moveInput.x * rotationSpeed * Time.deltaTime, 0f);
        }

        if (Mathf.Abs(moveInput.y) > 0.01f) // Move cara adiante/atrás con W/S (input vertical)
        {
            transform.position += transform.forward * moveInput.y * moveSpeed * Time.deltaTime;
        }
    }
}
