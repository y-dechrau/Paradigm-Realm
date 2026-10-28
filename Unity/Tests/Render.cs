using System;
using System.IO;
using ParadigmRealm;
class Render {
 static byte[][] art=new byte[21][];static TerrainInk[][] palette=new TerrainInk[21][];
 static float Mirror(float f){f=f-(float)Math.Floor(f/2)*2;return f<=1?f:2-f;}
 static TerrainInk Raw(int id,float x,float y){int i=(Math.Min(255,Math.Max(0,(int)(y*256)))*256+Math.Min(255,Math.Max(0,(int)(x*256))))*3;return new TerrainInk(art[id][i]/255f,art[id][i+1]/255f,art[id][i+2]/255f);}
 static TerrainInk Sample(int id,float x,float y){return TerrainPalette.Sample(palette[id],x,y);}
 static TerrainInk Old(float wx,float wy){
  int tx=(int)wx,ty=(int)wy;float u=wx-tx,v=wy-ty;int id=TerrainDemo.At(tx,ty),dx=u<.5f?-1:1,dy=v<.5f?-1:1,h=TerrainDemo.At(tx+dx,ty),vert=TerrainDemo.At(tx,ty+dy),d=TerrainDemo.At(tx+dx,ty+dy);
  bool water=TerrainPainter.Water(id),sx=water!=TerrainPainter.Water(h),sy=water!=TerrainPainter.Water(vert);
  float bx=sx?.12f:.18f,by=sy?.12f:.18f,fx=Math.Max(0,(bx-Math.Min(u,1-u))/(2*bx)),fy=Math.Max(0,(by-Math.Min(v,1-v))/(2*by));
  var c=TerrainInk.Mix(TerrainInk.Mix(Raw(id,u,v),Raw(h,1-u,v),fx),TerrainInk.Mix(Raw(vert,u,1-v),Raw(d,1-u,1-v),fx),fy);float coast=Math.Max(sx?fx:0,sy?fy:0);
  if(coast>0)c=TerrainInk.Mix(c,water?new TerrainInk(.26f,.59f,.62f):new TerrainInk(.71f,.66f,.44f),Math.Min(.68f,coast*1.5f));
  return new TerrainInk((float)Math.Round(c.r*31)/31,(float)Math.Round(c.g*31)/31,(float)Math.Round(c.b*31)/31);
 }
 static void Main(){
  for(int i=0;i<21;i++)art[i]=File.ReadAllBytes("gameplay/Tests/pixels/"+i+".rgb");
  for(int i=0;i<21;i++){int id=i==6?4:i;palette[i]=TerrainPalette.Build((x,y)=>Raw(id,x,id==18?.12f+y*.3f:y));}
  foreach(bool legacy in new[]{true,false}){
   int size=legacy?96:64,w=TerrainDemo.Width*size,h=TerrainDemo.Height*size;using(var f=File.Create("gameplay/Tests/"+(legacy?"before":"after")+".ppm")){
    var head=System.Text.Encoding.ASCII.GetBytes("P6\n"+w+" "+h+"\n255\n");f.Write(head,0,head.Length);var row=new byte[w*3];
    for(int y=0;y<h;y++){for(int x=0;x<w;x++){var c=legacy?Old((x+.5f)/size,(y+.5f)/size):TerrainPainter.Pixel((x+.5f)/size,(y+.5f)/size,TerrainDemo.Seed,TerrainDemo.At,Sample);row[x*3]=(byte)(c.r*255);row[x*3+1]=(byte)(c.g*255);row[x*3+2]=(byte)(c.b*255);}f.Write(row,0,row.Length);}
   }
  }
  Console.WriteLine("Rendered exact legacy and new terrain CPU pixels; not Unity screenshots.");
 }
}
