using System;

[Serializable]
public struct LevelStats
{
    public int moves;       // quantos GameEvents.OnMove
    public int pushes;      // quantos GameEvents.OnPush
    public int undos;       // quantos GameEvents.OnUndo
    public int restarts;    // quantos GameEvents.OnRestart (vamos criar)
    public float timeSec;   // tempo desde o carregamento do nível

    public override string ToString()
    {
        return $"moves={moves}, pushes={pushes}, undos={undos}, restarts={restarts}, time={timeSec:F1}s";
    }
}