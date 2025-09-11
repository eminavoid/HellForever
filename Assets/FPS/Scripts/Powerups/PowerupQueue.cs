using System.Collections;
using UnityEngine;

public class PowerupQueue : MonoBehaviour
{
    PowerupNode head;
    PowerupNode tail;
    bool _running;

    public void Enqueue(PowerupBase p)
    {
        var node = new PowerupNode(p);
        if (tail == null) head = tail = node;
        else { tail.Next = node; tail = node; }

        TryRunNext();
    }

    void TryRunNext()
    {
        if (_running || head == null) return;
        StartCoroutine(RunRoutine());
    }

    IEnumerator RunRoutine()
    {
        _running = true;

        while (head != null)
        {
            var node = head;
            head = head.Next;
            if (head == null) tail = null;

            // Inicialización y ejecución del efecto
            node.Power.Init(gameObject);
            node.Power.Apply();

            // Esperar duración
            float t = node.Power.Duration;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                yield return null;
            }

            // Revertir
            node.Power.Revert();
        }

        _running = false;
    }
}