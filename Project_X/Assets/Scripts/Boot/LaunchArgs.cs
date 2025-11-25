/// <summary>
/// Parâmetros de "lançamento" entre cenas. Simples e estático.
/// LevelSelect seta o índice e 02_Game consome.
/// </summary>
public static class LaunchArgs
{
    // null = nenhum level especificado; GameBootstrapper carrega o level 0
    public static int? PendingLevel = null;
}