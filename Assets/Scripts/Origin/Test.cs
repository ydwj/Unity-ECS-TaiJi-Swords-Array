using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.UI;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics.Systems;


public class Test : MonoBehaviour
{
    public static Test Instance;

    public GameObject swordPrefab;
    public int swordGroupAmount;
    public float RotateSpeed;

    private EntityManager _manager;
    //blobAssetStore是一个提供缓存的类，缓存能让你对象创建时更快。
    private BlobAssetStore _blobAssetStore;
    private GameObjectConversionSettings _settings;

    private Entity swordEntity;
    public Transform TargetPos;

    Unity.Physics.RaycastHit raycastHit;
    private bool isGo = false;
    EntityArchetype tempAchetype;
    void Start()
    {
        Instance = this;
        raycastHit = new Unity.Physics.RaycastHit();
        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _blobAssetStore = new BlobAssetStore();
        _settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
        swordEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(swordPrefab, _settings);

        ForbidSystem();

     tempAchetype = _manager.CreateArchetype(
      typeof(Translation),
      typeof(LocalToWorld),
      typeof(Rotation),
      typeof(RotateTag),
      typeof(Target),
      typeof(TempEntityTag)

      );
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            BurstGenerateSword();
        }

    }

    private void OnDestroy()
    {
        _blobAssetStore.Dispose();
    }

    #region 生成飞剑Entity 和TempEntity

    public void SpawnNewSword(float2 pos,Entity prefabEntity)
    {
        Entity newSword = _manager.Instantiate(swordEntity);
        Translation ballTrans = new Translation
        {
            Value = new float3(pos.x, 0f, pos.y)
        };

        float3 temp;
        float randomSpeed = UnityEngine.Random.Range(4f, 7f);
        temp = float3.zero;

        Target target = new Target
        {
            isGo = false,
            Tpos = temp,
            randomSpeed = randomSpeed,
            targetTempentity = prefabEntity
        };

        _manager.AddComponentData(newSword, ballTrans);
        _manager.AddComponentData(newSword, target);
    }


    private Entity SpawnTempEntity(float2 aa)
    {

        Entity tempEntity = _manager.CreateEntity(tempAchetype);

        Target target2 = new Target
        {
            isGo = false,
            Tpos = float3.zero,
        };
        
        Translation tempTrans = new Translation
        {
            Value = new float3(aa.x, 0f, aa.y)
        };

        _manager.SetComponentData(tempEntity, target2);
        _manager.SetComponentData(tempEntity, tempTrans);

        return tempEntity;

    } 
    #endregion


    public void ForbidSystem()
    {
        TestSystem system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TestSystem>();
        system.Enabled = false;
    }
    public void BurstGenerateSword()
    {
        Debug.Log("生成数量:" + GetPixel.Instance.posList.Count);
  
        //遍历位置列表，生成对应数量的飞剑Entity
        for (int i = 0; i < GetPixel.Instance.posList.Count; i++)
        {
          Entity temp = SpawnTempEntity(GetPixel.Instance.posList[i]);
          SpawnNewSword(GetPixel.Instance.posList[i],temp );
        }
    }


    #region obsolete

    public Entity Raycast(float3 startPos, float3 endPos)
    {
        //首先获取物理世界
        BuildPhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        //然后获取碰撞世界
        CollisionWorld collisionWorld = physicsWorld.PhysicsWorld.CollisionWorld;

        RaycastInput raycastInput = new RaycastInput()
        {
            Start = startPos,
            End = endPos,
            //声明碰撞过滤器，用来过滤某些层级下的物体是否进行射线检测
            Filter = CollisionFilter.Default,
            //Filter = new CollisionFilter()
            //{
            //    BelongsTo = ~0u,
            //    CollidesWith = ~0u,
            //    GroupIndex = 0,
            //}
        };

        //发射射线去检测Entity实体 
        if (collisionWorld.CastRay(raycastInput, out raycastHit))
        {
            isGo = true;
            //拿到我们射线击中的entity
            Entity entity = physicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            return entity;
        }
        else
        {
            Debug.Log("Notthing Found");
            return Entity.Null;
        }
    }

    // 飞剑前往目标点
    public void GetTargetModelPos(float3 pos)
    {
        Debug.Log("11");
        EntityQueryDesc description = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(GoTag), typeof(TempEntityTag) },
            All = new ComponentType[] { typeof(Rotation), ComponentType.ReadOnly<SwordTag>() }
        };
        //获取飞剑群
        EntityQuery entityQuery = _manager.CreateEntityQuery(description);

        NativeArray<Entity> newgroupArray = entityQuery.ToEntityArray(Allocator.Persistent);

        if (newgroupArray.Length < swordGroupAmount)
        {
            Debug.Log("当前长度：" + newgroupArray.Length);
            entityQuery.Dispose();
            newgroupArray.Dispose();
            return;
        }

        for (int i = 0; i < swordGroupAmount; i++)
        {
            //Translation aaa = _manager.GetComponentData<Translation>(newgroupArray[i]);
            //Entity temp = SpawnTempEntity(aaa.Value);
            float randomSpeed = UnityEngine.Random.Range(6f, 10f);
            Target target = new Target
            {
                isGo = true,
                Tpos = pos,

                // entity=temp 
            };
        }
        entityQuery.Dispose();
        newgroupArray.Dispose();
    }

    #endregion
}
