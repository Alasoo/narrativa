using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestEditModeScript
{

    [Test]
    public void TestRecibirDanoReduceVida()
    {
        GameObject go = new GameObject();
        SistemaVida sistema = go.AddComponent<SistemaVida>();
        sistema.vidaMaxima = 100;
        sistema.vidaActual = 100; // Simulamos Start manual porque en EditMode Start no corre solo

        sistema.RecibirDano(20);

        Assert.AreEqual(80, sistema.vidaActual);

        GameObject.DestroyImmediate(go);
    }

    [Test]
    public void TestCuracionNoExcedeMaximo()
    {
        GameObject go = new GameObject();
        SistemaVida sistema = go.AddComponent<SistemaVida>();
        sistema.vidaMaxima = 100;
        sistema.vidaActual = 90;

        sistema.Curar(50); // Intentamos curar 50, debería topar en 100

        Assert.AreEqual(100, sistema.vidaActual);

        GameObject.DestroyImmediate(go);
    }

    [Test]
    public void TestVidaNoBajaDeCero()
    {
        GameObject go = new GameObject();
        SistemaVida sistema = go.AddComponent<SistemaVida>();
        sistema.vidaActual = 10;

        sistema.RecibirDano(50); // Daño excesivo

        Assert.AreEqual(0, sistema.vidaActual);
Debug.Log($"dsdgf");
        GameObject.DestroyImmediate(go);
    }


    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestEditModeScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
