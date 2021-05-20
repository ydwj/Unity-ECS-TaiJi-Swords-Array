using Unity.Entities;
using Unity.Mathematics;

public struct GoTag : IComponentData
{
    //飞剑群脱离剑阵要飞向的目标点
    public float3 targetPos;

    //对应的TempEntity
    public Entity TempEntity;

    //原本位置，用来计算速度
    public float3 originPos;

    public bool isBack ;

}