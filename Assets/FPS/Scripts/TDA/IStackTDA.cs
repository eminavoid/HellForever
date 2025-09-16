public interface IStackTDA
{
    void InicializarPila(int cantidad);
    // siempre que la pila este inicializada
    int Apilar(int x);
    // siempre que la pila este inicializada y no este vacıa
    void Desapilar();
    // siempre que la pila este inicializada
    bool PilaVacia();
    // siempre que la pila este inicializada y no este vacıa
    int Tope();
}
