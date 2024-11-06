using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Rotación de la nave hacia la posición del mouse
    public void RotatePlayer()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Posición del mouse en el mundo
        Vector2 direction = (mousePosition - transform.position).normalized; // Dirección hacia el mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Ángulo de rotación en grados
        transform.rotation = Quaternion.Euler(0f, 0f, angle); // Aplicar la rotación
    }
}