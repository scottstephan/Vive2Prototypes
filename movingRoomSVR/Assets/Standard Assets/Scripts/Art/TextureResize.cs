// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable
using UnityEngine;
using System.Threading;

public class ThreadData {
    public int start;
    public int end;
    
    public ThreadData(int s, int e) 
    {
        start = s;
        end = e;
    }
}


public class TextureResize 
{
    private static Color[] texColors;
    private static Color[] newColors;
    private static int w;
    private static float ratioX;
    private static float ratioY;
    private static int w2;
    private static int finishCount;
    
    public static void Point(ref Texture2D tex,int newWidth, int newHeight) {
        ThreadedScale(ref tex, newWidth, newHeight, false);
    }
    
  
/* has a bug.
     * public static void Bilinear(ref Texture2D tex, int newWidth, int newHeight) {
        ThreadedScale(ref tex, newWidth, newHeight, true);
    }*/
    
    private static void ThreadedScale(ref Texture2D tex, int newWidth, int newHeight, bool useBilinear) {
        texColors = tex.GetPixels();
        newColors = new Color[newWidth * newHeight];
        if (useBilinear) {
            ratioX = 1.0f / ((float)(newWidth)) / ((float)(tex.width-1));
            ratioY = 1.0f / ((float)(newHeight)) / ((float)(tex.height-1));
        }
        else {
            ratioX = ((float)(tex.width)) / ((float)newWidth);
            ratioY = ((float)(tex.height)) / ((float)newHeight);
        }
        w = tex.width;
        w2 = newWidth;
//        int cores = Mathf.Min(SystemInfo.processorCount, newHeight);
//        int slice = newHeight/cores;
//        finishCount = 0;
        
        ThreadData threadData = null;        
/*        if (cores > 1) {
            int i = 0;
            for (i = 0; i < cores-1; i++) {
                threadData = new ThreadData(slice*i, slice*(i+1));
                Thread thread = useBilinear? new Thread(BilinearScale) : new Thread(PointScale);
                thread.Start(threadData);
            }
            threadData = new ThreadData(slice*i, newHeight);
            if (useBilinear) {
                BilinearScale(threadData);
            }
            else {
                PointScale(threadData);
            }
            while (finishCount < cores);
        }
        else */
        {
            threadData = new ThreadData(0, newHeight);
            if (useBilinear) {
                BilinearScale(threadData);
            }
            else {
                PointScale(threadData);
            }
        }
        
        tex.Resize(newWidth, newHeight);
        tex.SetPixels(newColors);
        tex.Apply();
    }
    
    private static void BilinearScale (ThreadData threadData) {
        for (int y = threadData.start; y < threadData.end; y++) {
            int yFloor = Mathf.FloorToInt(y * ratioY);
            int y1 = yFloor * w;
            int y2 = (yFloor+1) * w;
            int yw = y * w2;
            
            for (int x = 0; x < w2; x++) {
                int xFloor = Mathf.FloorToInt(x * ratioX);
                int xLerp = (int)(x * ratioX-xFloor);
                newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor+1], xLerp),
                                                       ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor+1], xLerp),
                                                       y*ratioY-yFloor);
            }
        }
        
        finishCount++;
    }
    
    private static void PointScale(ThreadData threadData) {
        for (int y = threadData.start; y < threadData.end; y++) {
            int thisY = (int)(ratioY * y) * w;
            int yw = y * w2;
            for (int x = 0; x < w2; x++) {
                newColors[yw + x] = texColors[thisY + (int)(ratioX*x)];
            }
        }
        
        finishCount++;
    }
    
    private static Color ColorLerpUnclamped (Color c1, Color c2, float value) {
        return new Color (c1.r + (c2.r - c1.r)*value, 
                          c1.g + (c2.g - c1.g)*value, 
                          c1.b + (c2.b - c1.b)*value, 
                          c1.a + (c2.a - c1.a)*value);
    }
}