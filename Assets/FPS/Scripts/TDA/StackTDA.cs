using System;
using UnityEngine;

public class StackTDA : IStackTDA
{
    // arreglo en donde se guarda la informacion
    int[] a;
    // cantidad de anillos de datos de la pila
    int cantidad_datos_max;
    // variable entera en donde se guarda la cantidad de elementos que se tienen guardados
    int indice;

    public void InicializarPila(int cantidad)
    {
        cantidad_datos_max = cantidad;
        a = new int[cantidad];
        indice = 0;
    }

    public int Apilar(int x)
    {
        if (indice < cantidad_datos_max)
        {
            a[indice] = x;
            indice++;
            return indice;
        }
        else
        {
            return 0;
        }

    }

    public int Desapilar()
    {
        if (!PilaVacia())
        {
            indice--;
            return indice;
        }
        else
        {
            return 0;
        }

    }

    public bool PilaVacia()
    {
        return (indice == 0);
    }

    public int Tope()
    {
        return a[indice - 1];
    }

    public void imprimoPila()
    {
        for (int i = indice - 1; i >= 0; i--)
        {
            Console.WriteLine("Elemento: " + a[i]);
        }

    }

    void IStackTDA.Desapilar()
    {
        throw new System.NotImplementedException();
    }
}
