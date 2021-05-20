using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(SwordRotateSystem))]
public class GroupSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        // 从World中获取ECS系统并且存起来
        m_EndSimulationEcbSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
  
    }

    protected override void OnUpdate()
    {

        // 请求一个ECS并且转换成可并行的
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        float deltaTime = Time.DeltaTime;

        float angel = 0.01f;

        Entities
            .WithName("Group").
            ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation orientation, ref GoTag goTag, ref Target target) =>
        {

            var rotation = orientation;
            float3 targetPosition = goTag.targetPos;
            float distance = math.distance(targetPosition, translation.Value);
            LocalToWorld targetTransform = GetComponent<LocalToWorld>(goTag.TempEntity);

            //距离目标点位置小于30，则返回剑阵
            if (distance < 30f)
            {
                if (goTag.TempEntity != null)
                {
                    goTag.isBack = true;
                }
            }

            //追上自己对应的Tempentity
            if (goTag.isBack)
            {
                float3 newPos = targetTransform.Position;
                var a = newPos - translation.Value;
                //飞剑剑头指向目标点
                quaternion b = Quaternion.FromToRotation(Vector3.down, a);
                orientation.Value = b;

                float d1 = math.distance(translation.Value, newPos);
                translation.Value += math.normalizesafe(a);
                float d2 = math.distance(translation.Value, newPos);
                float c = math.distance(newPos, float3.zero) / 100f;
                float d = d1 - d2;

                if (d1 >10+c)
                {
                    int loop = (int )((10 + c) / d);
                    for (int i = 0; i < loop; i++)
                    {
                        translation.Value += math.normalizesafe(a);
                    }
                }
                else
                {
                    target.Tpos = float3.zero;
                    translation.Value = targetTransform.Position;
                    float distance3 = math.distance(newPos, translation.Value);
                    ecb.RemoveComponent(entityInQueryIndex, entity, ComponentType.ReadWrite<GoTag>());
                }

                return;
            }

            #region 飞向目标点

            var targetDir = targetPosition - translation.Value;
            quaternion temp1 = Quaternion.FromToRotation(Vector3.down, targetDir);
            orientation.Value = temp1;
            float3 distancePos = goTag.targetPos - goTag.originPos;
            translation.Value += distancePos * deltaTime * target.randomSpeed / 5f; 
            #endregion

        }).ScheduleParallel();

        // 保证ECB system依赖当前这个Job
        m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
       
    }
}



