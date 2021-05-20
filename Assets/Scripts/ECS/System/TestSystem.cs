/*
 *深入浅出Systembase  https://zhuanlan.zhihu.com/p/252858463
 */

using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;



[AlwaysSynchronizeSystem]
public class TestSystem : SystemBase
{

    //已经被禁用
    protected override void OnUpdate()
    {
        float deltaTime = this.Time.DeltaTime;
        float angel =0.01f;
        Entities.ForEach((ref Rotation orientation, ref Translation position,
            in LocalToWorld transform,
            in Target target) =>
            {

                // Check to make sure the target Entity still exists and has
                // the needed component
                //if (!HasComponent<LocalToWorld>(target.entity))
                //{
                //   // Debug.Log("Noit Find Target");
                //    return;
                //}

                #region 面向Target物体
                //return;
                // Look up the entity data
                //LocalToWorld targetTransform
                //    = GetComponent<LocalToWorld>(target.entity);


                //float3 targetPosition = targetTransform.Position;

                //// Calculate the rotation
                //float3 displacement = targetPosition - transform.Position;

                ////float3 upReference = new float3(0, 1, 0);
                //float3 upReference = new float3(0, -1, 1);
                //quaternion lookRotation =
                //    quaternion.LookRotationSafe(displacement, upReference);

                //orientation.Value =
                //    math.slerp(orientation.Value, lookRotation, deltaTime);

                #region 固定轴面朝对象飞去的办法

                var rotation = orientation;
                //LocalToWorld targetTransform
                //    = GetComponent<LocalToWorld>(target.entity);


                //float3 targetPosition = targetTransform.Position;
                //var targetDir = targetPosition - position.Value;


                //quaternion temp1 = Quaternion.FromToRotation(Vector3.down, targetDir);

                //orientation.Value = math.slerp(orientation.Value, temp1, deltaTime);

                #endregion





                #endregion

                #region 围绕Target旋转


                //float3 pos = position.Value;
                ////旋转轴和旋转角度
                //quaternion rot = quaternion.AxisAngle(math.up(), angel);
                //float3 dir = pos - targetTransform.Position;

                //dir = math.mul(rot, dir);
                ////
                //position.Value = targetTransform.Position + dir;
                //// position.Value = math.lerp(position.Value, targetTransform.Position + dir,deltaTime);//移动剑的位置
                //var myrot = orientation.Value;
                //orientation.Value = math.mul(rot, myrot);



                #endregion


            }).ScheduleParallel();
            
    }


    //类中的函数调用是不允许的

    /// <summary>
    /// 用某个轴去朝向物体
    /// </summary>
    /// <param name="tr_self">朝向的本体</param>
    /// <param name="lookPos">朝向的目标</param>
    /// <param name="directionAxis">方向轴，取决于你用那个方向去朝向</param>
    public quaternion AxisLookAt(Rotation rot, Translation pos, float3 lookPos, float3 directionAxis)
    {
        var rotation = rot;
        var targetDir = lookPos - pos.Value;
        //指定哪根轴朝向目标,自行修改Vector3的方向
        var fromDir = math.mul(rotation.Value, directionAxis);
        //计算垂直于当前方向和目标方向的轴

        var axis = math.cross(fromDir, targetDir);
        axis = math.normalize(axis);
        // var axis = Vector3.Cross(fromDir, targetDir).normalized;

        //计算当前方向和目标方向的夹角
        //var angle = Vector3.Angle(fromDir, targetDir);
        var angle2 = math.dot(fromDir, targetDir);
        //将当前朝向向目标方向旋转一定角度，这个角度值可以做插值


        quaternion temp = math.mul(quaternion.AxisAngle(axis, angle2), rotation.Value);
        return temp;
        // tr_self.rotation = Quaternion.AngleAxis(angle, axis) * rotation.Value;
        // tr_self.localEulerAngles = new Vector3(0, tr_self.localEulerAngles.y, 90);//后来调试增加的，因为我想让x，z轴向不会有任何变化

    }
}