using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public class GetPixel : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    //像素点相对位置
    public List<int2> posList;

    [Header("Drawing")]

    //剑阵密度
    public int drawDensity ;
    //剑阵离散程度
    public int disperseMin;
    public static GetPixel Instance;
    //图片宽高
    private int width;
    private int height;
    void Start()
    {
        Instance = this;
        width = spriteRenderer.sprite.texture.width;
        height = spriteRenderer.sprite.texture.height;
        Debug.Log("图片宽度" + width + "图片高度" + height);
        GetPixelPos();
     
    }

  
    public void GetPixelPos()
    {
        int halfHeight= height / 2;
        int halfWidth = width / 2;
        int2 tempPos;
        for (int i = 0; i < height; i += drawDensity)
        {
            for (int j = 0; j < width; j += drawDensity)
            {
                //获取每个位置像素点的颜色
                Color32 c = spriteRenderer.sprite.texture.GetPixel(j, i);
                tempPos.y = (j-halfHeight)*disperseMin; 
               // Debug.Log("RGBA:" + c);
               //如果对应位置颜色不为透明，则记录坐标到List中
                if (c.a != 0)
                {
                    tempPos.x = (i-halfWidth)* disperseMin;
                    posList.Add(tempPos);
                }

            }
        }

        Debug.Log("位置List长度"+posList.Count);
    }
}
