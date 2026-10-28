using System;
namespace ParadigmRealm {
 // Periodic texture preparation retains the lighting orientation of supplied art.
 // Blend opposite phases near their seams, then quantise; never mirror the artwork.
 public static class TerrainPalette {
  public const int Size=64;
  static float Frac(float x){return x-(float)Math.Floor(x);}
  static float Edge(float x){float t=Math.Min(x,1-x)/.20f;t=Math.Min(1,t);return t*t*(3-2*t);}
  public static TerrainInk[] Build(Func<float,float,TerrainInk> source){
   var pixels=new TerrainInk[Size*Size];
   for(int y=0;y<Size;y++)for(int x=0;x<Size;x++){
    float u=(x+.5f)/Size,v=(y+.5f)/Size,s=Frac(u+.5f),t=Frac(v+.5f),ex=Edge(u),ey=Edge(v);
    var a=TerrainInk.Mix(source(s,t),source(u,t),ex);var b=TerrainInk.Mix(source(s,v),source(u,v),ex);
    pixels[y*Size+x]=TerrainInk.Mix(a,b,ey);
   }
   return pixels;
  }
  public static TerrainInk Sample(TerrainInk[] pixels,float x,float y){return pixels[Math.Min(Size-1,(int)(Frac(y)*Size))*Size+Math.Min(Size-1,(int)(Frac(x)*Size))];}
 }
}
