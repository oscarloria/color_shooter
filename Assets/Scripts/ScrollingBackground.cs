using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ScrollingBackground : MonoBehaviour
{
    public float scrollSpeed = 0.1f; 
    private Material myMaterial; 
    private Vector2 offset;

    void Start()
    {
        // Obtenemos el Renderer de este objeto
        Renderer rend = GetComponent<Renderer>();
        
        // Importante: Clonamos el material para no alterar el material original 
        // (si varios objetos usan el mismo material, todos se desplazarían).
        myMaterial = rend.material; 
    }

    void Update()
    {
        // Desplazamos el offset en X (o Y, depende de si quieres scroll horizontal o vertical)
        offset = new Vector2(0f, Time.time * scrollSpeed);

        // Asignamos el offset a la propiedad del material
        myMaterial.mainTextureOffset = offset;
    }
}
