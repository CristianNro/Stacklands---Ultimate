// Define como termina una transformacion temporal de carta.
// La idea es mantener el cierre explicito y evitar muchos bools
// mezclados dentro de la regla principal.
public enum CardTransformationCompletionMode
{
    DestroyOnly,
    ReplaceWithSingleResult,
    SpawnMultipleResults
}
