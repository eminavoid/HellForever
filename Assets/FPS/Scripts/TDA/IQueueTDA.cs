public interface IQueueTDA
{
    void InicializarCola();
    // siempre que la cola este inicializada
    void Acolar(int x);
    // siempre que la cola este inicializada y no este vacıa
    void Desacolar();
    // siempre que la cola este inicializada
    bool ColaVacia();
    // siempre que la cola este inicializada y no este vacıa
    int Primero();
}
