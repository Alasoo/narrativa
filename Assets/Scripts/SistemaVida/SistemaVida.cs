using UnityEngine;

public class SistemaVida : MonoBehaviour
{
    public int vidaMaxima = 100;
    public int vidaActual;
    public bool estaMuerto = false;

    private void Start()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDano(int cantidad)
    {
        vidaActual -= cantidad;
        if (vidaActual < 0) vidaActual = 0;
        
        if (vidaActual == 0)
        {
            estaMuerto = true;
        }
    }

    public void Curar(int cantidad)
    {
        if (estaMuerto) return; // No se puede curar a un muerto

        vidaActual += cantidad;
        if (vidaActual > vidaMaxima) vidaActual = vidaMaxima;
    }
}