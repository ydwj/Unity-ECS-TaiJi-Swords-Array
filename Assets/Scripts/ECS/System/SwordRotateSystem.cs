using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Physics;

[UpdateAfter(typeof(TempEntityRotateSystem))]
public class SwordRotateSystem : SystemBase
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
        bool isGo = false;
        float3 hitpos = float3.zero;
        float deltaTime = Time.DeltaTime;

        // 请求一个ECS并且转换成可并行的
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
        if (Input.GetMouseButtonDown(0))
        {
            //获取物理世界
            BuildPhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
            NativeArray<RigidBody> rigidBodies = new NativeArray<RigidBody>(1, Allocator.TempJob);
            NativeArray<float3> rayHitPos = new NativeArray<float3>(1, Allocator.TempJob);
            //获取射线发射位置
            UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastJobHandle raycastJonHande = new RaycastJobHandle()
            {
                mStartPos = ray.origin,
                mEndPos = ray.direction * 10000,
                physicsWorld = physicsWorld.PhysicsWorld,
                Bodies = rigidBodies,
                rayHitpos = rayHitPos
            };

            //需要依赖当前Job
            JobHandle jobHandle = raycastJonHande.Schedule(this.Dependency);
            jobHandle.Complete();

            if (rigidBodies[0].Entity != null)
            {
                Debug.Log("目标坐标：" + rayHitPos[0]);
                Debug.Log("射线击中目标" + rigidBodies[0].Entity);
                hitpos = rayHitPos[0];
                isGo = true;
            }

            rigidBodies.Dispose();
            rayHitPos.Dispose();

        }

        Entities.
         WithAll<SwordTag>().
         WithNone<GoTag>().
         ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation orientation, ref Target target) =>
         {
             #region 飞剑群出击！
             if (isGo && entityInQueryIndex < 10000)
             {

                 GoTag tag = new GoTag
                 {
                     targetPos = hitpos,
                     TempEntity = target.targetTempentity,
                     originPos = translation.Value,
                     isBack = false
                 };

                 // 将entityInQueryIndex传给操作，这样ECS回放时能保证正确的顺序
                 ecb.AddComponent(entityInQueryIndex, entity, tag);
             }
             #endregion

             if (!HasComponent<LocalToWorld>(target.targetTempentity))
             {
                 return;
             }

             var rotation = orientation;

             float3 targetPosition = target.Tpos;

             var targetDir = targetPosition - translation.Value;

             //飞剑垂直向下面向中心点
             quaternion temp1 = Quaternion.FromToRotation(Vector3.left, targetDir);

             orientation.Value = temp1;

             LocalToWorld tempEntityPos = GetComponent<LocalToWorld>(target.targetTempentity);
             translation.Value = tempEntityPos.Position;

         }).ScheduleParallel();

        // 保证ECB system依赖当前这个Job
        m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }

    //发射射线Job
    public struct RaycastJobHandle : IJob
    {

        public NativeArray<RigidBody> Bodies;
        public NativeArray<float3> rayHitpos;
        public float3 mStartPos;
        public float3 mEndPos;
        public PhysicsWorld physicsWorld;

        public void Execute()
        {
            //创建输入
            RaycastInput raycastInput = new RaycastInput()
            {
                Start = mStartPos,
                End = mEndPos * 100,
                //声明碰撞过滤器，用来过滤某些层级下的物体是否进行射线检测
                Filter = new CollisionFilter() { BelongsTo = ~0u, CollidesWith = ~0u, GroupIndex = 0, }
            };
            Unity.Physics.RaycastHit raycastHit = new Unity.Physics.RaycastHit();

            // 发射射线去检测Entity实体
            if (physicsWorld.CollisionWorld.CastRay(raycastInput, out raycastHit))
            {
                //拿到我们射线击中的entity
                Bodies[0] = physicsWorld.Bodies[raycastHit.RigidBodyIndex];
                //拿到击中点的位置信息
                rayHitpos[0] = raycastHit.Position;
            }
        }
    }
}

