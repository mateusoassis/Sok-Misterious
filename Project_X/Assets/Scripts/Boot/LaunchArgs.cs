/// <summary>
/// Parâmetros de "lançamento" entre cenas. Simples e estático.
/// LevelSelect seta o índice e 02_Game consome.
/// </summary>
public static class LaunchArgs
{
    // null = não foi definido; GameBootstrapper pode cair no nível 0
    public static int? PendingLevel = null;
}
