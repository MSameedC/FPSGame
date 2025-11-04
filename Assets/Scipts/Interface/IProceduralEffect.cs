public interface IProceduralEffect
{
    void Initialize(ProceduralRuntimeContext ctx);
    void Apply(ProceduralRuntimeContext ctx);
    void ResetEffect();
}