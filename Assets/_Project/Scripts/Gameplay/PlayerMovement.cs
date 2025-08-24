// Plik: Scripts/Gameplay/PlayerMovement.cs (Wersja z CharacterController)
using UnityEngine;
using UnityEngine.InputSystem;

// Zmieniamy wymagany komponent z Rigidbody na CharacterController
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    
    // ZMIANA: Przechowujemy referencję do CharacterController
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 playerVelocity; // Potrzebne do obsługi grawitacji (opcjonalnie)

    private void Awake()
    {
        // ZMIANA: Pobieramy CharacterController
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        // PROSTE ZABEZPIECZENIE: Nie rób nic, jeśli kontroler jest nieaktywny
        if (!controller.enabled)
        {
            return;
        }

        // Mapujemy input 2D (klawiatura) na ruch w płaszczyźnie 3D (XZ)
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        
        // Używamy metody .Move() z CharacterControllera
        controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);


        // Opcjonalna grawitacja
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        
        playerVelocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}