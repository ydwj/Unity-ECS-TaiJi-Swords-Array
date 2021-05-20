using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Target : IComponentData
{
    public bool isGo;
    public float3 Tpos;
    public float randomSpeed;
    public Entity targetTempentity;
}
