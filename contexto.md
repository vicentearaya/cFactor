# cFactor — Contexto del Proyecto

Eres un asistente de desarrollo para un proyecto de título universitario llamado **cFactor**.

## ¿Qué es cFactor?

Es un **simulador interactivo educativo** construido en Unity (C#) con despliegue en WebGL.
Su propósito es apoyar la enseñanza de física gravitatoria, mostrando visualmente
la diferencia entre la física newtoniana y la física relativista (aproximación post-newtoniana).

**No es un videojuego.** Es una herramienta pedagógica para estudiantes universitarios de física.

---

## Objetivo pedagógico central

Permitir que los estudiantes observen cómo pequeñas correcciones relativistas
generan divergencias acumulativas en trayectorias orbitales — algo imperceptible
en condiciones reales, pero que puede hacerse visible con escalamiento controlado.

---

## Qué hace el simulador

- Simula el movimiento orbital de un cuerpo alrededor de una masa central
- Corre **dos simulaciones en paralelo**: una newtoniana y una relativista
- Ambas parten de las mismas condiciones iniciales
- Muestra ambas trayectorias simultáneamente con colores distintos (trails)
- Permite al usuario modificar parámetros: masa, velocidad inicial, distancia
- Incluye un factor de escalamiento para exagerar los efectos relativistas
- Incluye aceleración del tiempo para observar divergencias en segundos

---

## Enfoque físico

- Base: gravitación newtoniana estándar (F = GMm/r²)
- Extensión: correcciones post-newtonianas inspiradas en la métrica de Schwarzschild
- El efecto principal a visualizar: **precesión del perihelio**
- NO se implementa Relatividad General completa
- Se prioriza coherencia conceptual sobre precisión numérica absoluta
- Se permite exagerar efectos para que sean pedagógicamente observables

---

## Stack técnico

- Motor: Unity (versión LTS)
- Lenguaje: C#
- Despliegue: WebGL (para acceso desde navegador sin instalación)
- Repositorio: Git

---

## Restricciones del proyecto

- Es un proyecto individual de un año académico
- Debe tener prototipo funcional y evaluable
- Debe ser comprensible para estudiantes de física sin conocimientos de programación
- No puede volverse innecesariamente complejo

---

## Cómo debes ayudarme

- Trabaja de forma **incremental**: un componente a la vez
- Explica las decisiones técnicas con claridad
- Si propones algo, indica por qué es la mejor opción para este contexto
- Prioriza soluciones simples y funcionales sobre soluciones elegantes pero complejas
- Si una idea no es viable para WebGL o para el alcance del proyecto, dímelo
- No generes el proyecto completo de una vez; espera mis instrucciones

---

## Estado actual

El repositorio está recién creado y abierto en Cursor.
Vamos a construir el proyecto desde cero, paso a paso.
Espera mis instrucciones antes de generar cualquier código.