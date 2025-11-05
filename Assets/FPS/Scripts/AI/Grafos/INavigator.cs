using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI.Navigation
{
    // Interfaz agnóstica al backend (NavMesh, grafos, etc.)
    public interface INavigator
    {
        // Define el destino de navegación (posición objetivo).
        void SetDestination(Vector3 target);

        // Borra destino y ruta actual.
        void ClearDestination();

        // Si existe una ruta vigente.
        bool HasPath { get; }

        // Ruta en coordenadas del mundo (waypoints).
        IReadOnlyList<Vector3> Path { get; }

        // Velocidad instantánea estimada (para animaciones/audio).
        Vector3 Velocity { get; }

        // Velocidad máxima configurada.
        float MaxSpeed { get; }

        // Avanza la simulación (debe llamarse en Update).
        void Tick(Transform agentTransform, float deltaTime);

        // Permite objetivos dinámicos (p. ej. seguir a un target móvil) con replanificación periódica.
        void SetDynamicTarget(Func<Vector3> targetProvider, float replanningIntervalSeconds = 0f);
    }
}