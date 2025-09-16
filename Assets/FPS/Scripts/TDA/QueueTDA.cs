public class QueueTDA : IQueueTDA
{
    int[] a; // arreglo en donde se guarda la informacion
    int indice; // variable entera en donde se guarda la cantidad de elementos que se tienen guardados

    public void InicializarCola()
    {
        a = new int[100];
        indice = 0;
    }

    public void Acolar(int x)
    {
        for (int i = indice - 1; i >= 0; i--)
        {
            a[i + 1] = a[i];
        }
        a[0] = x;

        indice++;
    }
    public void Desacolar()
    {
        indice--;
    }

    public bool ColaVacia()
    {
        return (indice == 0);
    }

    public int Primero()
    {
        return a[indice - 1];
    }
}
