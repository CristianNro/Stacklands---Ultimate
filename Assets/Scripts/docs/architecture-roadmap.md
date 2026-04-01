# Roadmap de Arquitectura

## Objetivo

Este documento define la hoja de ruta de cambios necesarios para llevar el proyecto a una base mas solida, mantenible y escalable.

No es una lista de ideas sueltas. Es una guia de decisiones tecnicas para ordenar futuros refactors y extensiones.

## Estado actual resumido

El proyecto funciona, pero hoy depende de:

- Logica de dominio repartida entre `MonoBehaviours`
- Demasiado acoplamiento entre interaccion, board, stacks, recetas, mercado y contenedores
- Uso excesivo de `string` tags e ids para reglas criticas
- Clases con demasiadas responsabilidades
- Datos declarados que no gobiernan comportamiento real
- Persistencia runtime incompleta
- Escaneos globales del mundo en lugar de eventos y ownership claro

## Objetivos arquitectonicos

1. Separar dominio, estado runtime, presentacion e interaccion.
2. Reducir el acoplamiento entre sistemas.
3. Reemplazar reglas blandas por contratos mas explicitos.
4. Hacer que las cartas se definan por capacidades, no por jerarquias infladas.
5. Hacer que board, stack, receta, economia y contenedores tengan ownership claro.
6. Dejar una base donde agregar contenido sea barato y agregar sistemas no implique tocar todo.

## Principios de refactor

1. No profundizar herencia salvo que haya una razon muy fuerte.
2. Preferir composicion y capacidades explicitas.
3. Sacar logica de negocio de componentes de UI/interaccion.
4. Evitar que un solo script concentre demasiado poder.
5. Toda fase debe dejar invariantes mas claras que antes.
6. Antes de ampliar contenido, primero consolidar los contratos base.

## Problemas raiz

### 1. Modelo de carta demasiado ambiguo

`CardData` concentra campos generales, flags y categorias, pero gran parte de ese modelo no gobierna el comportamiento real.

Consecuencias:

- El modelo parece mas completo de lo que realmente es
- Se agregan campos sin sistema que los consuma
- El proyecto crece con deuda silenciosa

### 2. Runtime de carta inconsistente

`CardInstance` no es una verdadera fuente de verdad de estado runtime.
Parte del comportamiento esta en runtimes paralelos (`UnitRuntime`, `BuildingRuntime`, `ContainerRuntime`, `MarketPackRuntime`) activados por `CardInitializer`.

Consecuencias:

- Extension dificil
- Dependencia de switches manuales
- Estado runtime fragmentado

### 3. `CardStack` tiene demasiadas responsabilidades

Hoy hace:

- Estructura del stack
- Layout visual
- Barra de progreso
- Consumo de recetas
- Ejecucion parcial del crafting
- Helpers de consultas

Consecuencia:

- El stack no es un agregado limpio, es un centro de logica heterogenea

### 4. Interaccion acoplada a logica de negocio

`CardDrag` decide demasiadas cosas del juego:

- Partir stacks
- Mover objetos
- Guardar en contenedores
- Comprar
- Vender
- Mergear stacks

Consecuencia:

- Cada nueva mecanica de drop obliga a tocar el mismo script

### 5. Sistema de recetas dependiente de strings y scans globales

Problemas:

- Tags string para matching critico
- Reglas de consumo por `cardId` string
- `RecipeSystem` escaneando todos los stacks cada frame

Consecuencia:

- Mala base para crecimiento
- Fragilidad de contenido
- Dificultad para validar

### 6. Economia duplicada y sin capa propia

La logica economica esta repartida entre `MarketSellSlot`, `MarketPackPurchaseSlot`, `ContainerRuntime` y tags monetarios.

Consecuencia:

- Reglas duplicadas
- Inconsistencias futuras
- Dificultad para introducir nuevas monedas, tasas, descuentos o vendors

### 7. Persistencia runtime pobre

Los contenedores guardan una proyeccion incompleta de la carta.

Consecuencia:

- Futuras mecanicas de estado se rompen o se pierden al guardar

### 8. Board poco autoritativo

`BoardRoot` hoy registra cartas y clampa posiciones, pero no es el orquestador espacial y sistemico del tablero.

Consecuencia:

- Ownership difuso
- Sistemas laterales consultando estado de forma ad hoc

### 9. Datos muertos o prematuros

Hay enums y campos que hoy no participan realmente en el gameplay actual.

Consecuencia:

- Ruido de modelo
- Falsa sensacion de robustez
- Mayor costo mental para mantener el proyecto

## Roadmap por fases

## Fase 0. Congelar direccion tecnica

Objetivo:

Definir el lenguaje comun del refactor antes de tocar sistemas grandes.

Cambios:

1. Establecer una politica de arquitectura:
   dominio, runtime, presentacion e interaccion deben vivir separados.
2. Definir que una carta tendra:
   definicion de datos, estado runtime y capacidades.
3. Definir que `CardStack` deja de ser responsable de crafting y UI.
4. Definir que la economia sera un sistema propio.
5. Definir que tags string no pueden seguir siendo la unica forma de modelar reglas criticas.

Entregables:

- Este roadmap
- Convenciones de nombres y ownership por sistema

## Fase 1. Limpiar el modelo base de cartas

Objetivo:

Convertir la carta en una base delgada y confiable.

Cambios:

1. Revisar `CardData` y separar:
   campos base obligatorios, metadata opcional y capacidades reales.
2. Eliminar o deprecatear flags que hoy no gobiernan comportamiento.
3. Reemplazar categorias genericas por capacidades mas concretas donde haga falta.
4. Decidir que campos son de definicion y cuales son estrictamente runtime.
5. Crear un inventario de campos muertos o prematuros y retirarlos del modelo activo.

Resultado buscado:

- `CardData` mas chico
- Menos campos decorativos
- Menos promesas falsas en el modelo

## Fase 2. Rediseñar el runtime de carta

Objetivo:

Hacer que el estado runtime sea coherente y extensible.

Cambios:

1. Definir una fuente central de verdad de estado runtime por carta.
2. Reemplazar el encendido/apagado manual de runtimes por composicion clara.
3. Separar identidad runtime, estado mutable y capacidades activas.
4. Modelar los estados con datos y contratos, no con bools sueltos.
5. Eliminar estado runtime no usado o no confiable.

Resultado buscado:

- Menos dependencia de `CardInitializer` como switch central
- Menos fragmentacion entre runtimes
- Capacidad real de crecer a nuevas mecanicas

## Fase 3. Separar stack logico, stack visual y crafting

Objetivo:

Reducir a `CardStack` a un agregado claro.

Cambios:

1. Definir `CardStack` solo como owner de relaciones entre cartas apiladas.
2. Mover layout visual a un componente o servicio de presentacion de stack.
3. Mover barra de progreso a una capa visual aparte.
4. Mover consumo de recetas y resolucion de crafting fuera del stack.
5. Conservar en stack solo consultas que sean verdaderamente propias del agregado.

Resultado buscado:

- `CardStack` mas chico y legible
- Menos riesgo de romper varias cosas con un solo cambio

## Fase 4. Crear una capa de interaccion y drop resolution

Objetivo:

Quitarle a `CardDrag` la responsabilidad de decidir reglas de negocio.

Cambios:

1. Dejar `CardDrag` solo como emisor de intencion:
   comienzo, arrastre, fin y contexto.
2. Crear un resolvedor de drop o pipeline de interaccion.
3. Mover a handlers separados:
   merge de stacks, almacenamiento en contenedor, compra, venta, drop sobre board.
4. Hacer que las reglas de interaccion sean componibles y ordenables.
5. Evitar dependencias directas de UI con negocio.

Resultado buscado:

- Menos ifs gigantes
- Nuevas mecanicas de drop sin tocar el mismo archivo cada vez

## Fase 5. Rehacer recetas alrededor de contratos fuertes

Objetivo:

Volver el sistema de recetas declarativo, validable y menos fragil.

Cambios:

1. Reemplazar tags string criticos por identificadores o categorias validadas.
2. Reemplazar `cardId` string de reglas de consumo por referencias mas fuertes.
3. Separar:
   matching, prioridad, consumo, resultado y ejecucion temporal.
4. Crear un matcher de receta independiente del `MonoBehaviour`.
5. Eliminar escaneos globales y pasar a evaluacion por eventos de stack.
6. Definir bien que significa especificidad y prioridad.

Resultado buscado:

- Recetas mas faciles de testear
- Menos errores por contenido mal configurado
- Mejor escalabilidad

## Fase 6. Reestructurar el sistema de tareas

Objetivo:

Hacer que la ejecucion temporal sea un sistema reusable, no una lista procedural minima.

Cambios:

1. Separar scheduler, tarea activa, progreso y resolucion.
2. Definir estados de tarea explicitamente.
3. Integrar las tareas al sistema de recetas mediante contratos, no acoplamiento directo.
4. Permitir cancelacion, refresco y reemplazo de forma clara.
5. Preparar la base para futuras tareas no relacionadas a crafting.

Resultado buscado:

- Base para workers, produccion y automatizacion futura

## Fase 7. Crear una capa de economia

Objetivo:

Sacar la logica economica del Market y de los contenedores.

Cambios:

1. Crear un `EconomyService` o equivalente.
2. Centralizar:
   valuacion, combinacion de pago, cambio, venta y restricciones de moneda.
3. Reemplazar tags monetarios criticos por capacidades o tipos de moneda.
4. Hacer que Market y contenedores consuman reglas economicas, no las implementen.
5. Unificar algoritmos duplicados de combinacion exacta.

Resultado buscado:

- Menos duplicacion
- Menos inconsistencia futura
- Soporte real para ampliar economia

## Fase 8. Rediseñar contenedores y persistencia

Objetivo:

Hacer que guardar una carta preserve su estado real.

Cambios:

1. Diseñar un snapshot runtime de carta.
2. Hacer que contenedores almacenen snapshots completos, no solo `CardData` + pocos campos.
3. Definir ownership claro entre contenedor, escena interna y board.
4. Revisar el flujo de escenas de contenedor para que no capture todo el board ciegamente.
5. Preparar compatibilidad con cartas complejas y contenido anidado.

Resultado buscado:

- Persistencia mas confiable
- Menos deuda al agregar estado runtime nuevo

## Fase 9. Hacer al board autoritativo

Objetivo:

Convertir el tablero en el eje de ownership espacial y de registro runtime.

Cambios:

1. Formalizar registro y eventos de entidades del board.
2. Evitar que cada sistema consulte el mundo por su cuenta.
3. Definir mejor spawn, occupancy, clamp y ownership espacial.
4. Hacer que stacks y cartas se integren al board mediante contratos claros.
5. Reducir dependencia de singletons ad hoc.

Resultado buscado:

- Mejor trazabilidad del estado del juego
- Menos consultas globales

## Fase 10. Limpiar datos muertos y deuda de contenido

Objetivo:

Reducir ruido y dejar solo modelo que realmente sostenga gameplay.

Cambios:

1. Auditar enums y campos no usados.
2. Eliminar o marcar explicitamente lo que es futuro y todavia no forma parte del juego.
3. Corregir nombres inconsistentes y errores de estructura.
4. Alinear nombres de carpetas, sistemas y contratos.

Resultado buscado:

- Menor costo cognitivo
- Modelo mas honesto

## Fase 11. Agregar validacion y testing de configuracion

Objetivo:

Evitar que la escalabilidad dependa de inspeccion manual.

Cambios:

1. Crear validadores de assets:
   recetas, cartas, packs, contenedores.
2. Agregar tests de dominio para matching de recetas, consumo, economia y snapshots.
3. Agregar chequeos de consistencia para ids, categorias y referencias.
4. Detectar contenido roto antes de entrar a play.

Resultado buscado:

- Menos fragilidad por data entry
- Mayor confianza para iterar

## Orden recomendado de ejecucion

1. Fase 0
2. Fase 1
3. Fase 2
4. Fase 3
5. Fase 4
6. Fase 5
7. Fase 6
8. Fase 7
9. Fase 8
10. Fase 9
11. Fase 10
12. Fase 11

## Prioridad real

### Prioridad alta

- Fase 1
- Fase 2
- Fase 3
- Fase 4
- Fase 5

### Prioridad media

- Fase 6
- Fase 7
- Fase 8
- Fase 9

### Prioridad de consolidacion

- Fase 10
- Fase 11

## Criterios de exito

Vamos a considerar que la base mejoro de verdad cuando:

1. Una nueva mecanica de carta no obligue a tocar `CardDrag`, `CardStack` y `CardInitializer` al mismo tiempo.
2. Las recetas se puedan evaluar sin depender del scene graph.
3. El market no implemente reglas economicas por su cuenta.
4. Un contenedor pueda guardar y restaurar cartas sin perder estado importante.
5. El board sea el lugar claro para consultar entidades activas y reglas espaciales.
6. El modelo de datos deje de tener campos que nadie usa.

## Propuesta de primer tramo de trabajo

Si se sigue este roadmap, el mejor primer bloque concreto es:

1. Auditar y adelgazar `CardData`
2. Diseñar el nuevo modelo runtime de carta
3. Separar `CardStack` en dominio y presentacion
4. Diseñar el pipeline de drops
5. Rehacer la evaluacion de recetas basada en eventos

## Uso de este documento

Este archivo debe actualizarse cuando:

- una fase se complete
- una fase cambie de alcance
- aparezca un problema raiz nuevo
- decidamos romper una decision previa por una mejor

No debe convertirse en un changelog tecnico fino. Su funcion es preservar direccion arquitectonica.
