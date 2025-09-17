public interface IQueueTDA<TEntity>
{
    void InicializarCola(int cantidad);
    // siempre que la cola este inicializada
    void Acolar(TEntity x);
    // siempre que la cola este inicializada y no este vacıa
    void Desacolar();
    // siempre que la cola este inicializada
    bool ColaVacia();
    // siempre que la cola este inicializada y no este vacıa
    TEntity Primero();
}
