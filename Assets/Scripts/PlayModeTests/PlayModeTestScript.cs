using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayModeTestScript
{
    [UnityTest]
    public IEnumerator TestInicializacionCorrecta()
    {
        GameObject go = new GameObject();
        SistemaVida sistema = go.AddComponent<SistemaVida>();
        sistema.vidaMaxima = 150;

        yield return null;

        Assert.AreEqual(150, sistema.vidaActual);

        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator TestMuerteDelPersonaje()
    {
        GameObject go = new GameObject();
        SistemaVida sistema = go.AddComponent<SistemaVida>();
        yield return null; 

        sistema.RecibirDano(1000); 

        Assert.IsTrue(sistema.estaMuerto, "El booleano estaMuerto debería ser true");
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator TestNoSePuedeCurarMuerto()
    {
        GameObject go = new GameObject();
        SistemaVida sistema = go.AddComponent<SistemaVida>();
        yield return null;

        sistema.RecibirDano(sistema.vidaMaxima);
        yield return null;

        sistema.Curar(100);

        Assert.AreEqual(0, sistema.vidaActual);
        Assert.IsTrue(sistema.estaMuerto);

        Object.Destroy(go);
    }



    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator PlayModeTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
