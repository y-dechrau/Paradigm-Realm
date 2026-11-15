using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
namespace ParadigmRealm {
// Development aid (owner request, 15 Nov 2026: every civilisation's characters in one place to draw new art from).
// "-listCivs <folder>" writes civilisations.csv (civilisation, leader, portrait number and file, the people's unit art files) and,
// for portraits the game draws in code (no picture file), a PNG of what it draws now. Then quits. The picture files themselves
// are copied from Resources by the packaging step.
public partial class RealmGame {
 System.Collections.IEnumerator ListCivs(string folder){
  Directory.CreateDirectory(folder);yield return null;
  var csv=new StringBuilder("index,civilisation,leader,leader_title,portrait,portrait_file,unit_art\n");
  for(int c=0;c<CivilisationCatalog.Count;c++){
   int p=CivilisationCatalog.Portraits[c];string file="leader-"+p.ToString("00");
   bool painted=Resources.Load<Texture2D>("Leaders/"+file)!=null;
   if(!painted){Texture2D drawn=null;
    try{if(p>=SealedPeoples.First)drawn=PixelTexture(PixelArt.PeoplePortrait(SealedPeoples.Of(p)));else if(p>=CivilisationCatalog.FirstNative)drawn=PixelTexture(PixelArt.NativePortrait(p-CivilisationCatalog.FirstNative));else if(p>=17)drawn=PixelTexture(PixelArt.LeaderPlaceholder(p-17));}catch(Exception){drawn=null;}
    if(drawn!=null){file="drawn-portrait-"+p.ToString("000");File.WriteAllBytes(Path.Combine(folder,file+".png"),Png(drawn));}else file="";}
   string art=SpeciesPeoples.ArtOf(c);var arts=new StringBuilder();
   if(art!=null)foreach(int type in new[]{0,1,2,3,4,8,9}){string f=SpeciesPeoples.ArtFile(art,type);if(f!=null&&Resources.Load<Texture2D>("Visuals/Species/"+f)!=null&&arts.ToString().IndexOf(f+".png",StringComparison.Ordinal)<0)arts.Append((arts.Length>0?" ":"")+f+".png");}
   string title=CivilisationCatalog.PortraitTitles!=null&&c<CivilisationCatalog.PortraitTitles.Length?CivilisationCatalog.PortraitTitles[c]:"";
   csv.Append(c+","+Q(CivilisationCatalog.Names[c])+","+Q(CivilisationCatalog.Leaders[c])+","+Q(title)+","+p+","+(file==""?"":file+".png")+","+Q(arts.ToString())+"\n");
  }
  File.WriteAllText(Path.Combine(folder,"civilisations.csv"),csv.ToString());
  Application.Quit();
 }
 static string Q(string s){return "\""+(s??"").Replace("\"","\"\"")+"\"";}
 // A plain RGBA PNG writer (the project has no image-encoding module).
 static byte[] Png(Texture2D tex){
  int w=tex.width,h=tex.height;var px=tex.GetPixels32();var raw=new MemoryStream();
  for(int y=h-1;y>=0;y--){raw.WriteByte(0);for(int x=0;x<w;x++){var c=px[y*w+x];raw.WriteByte(c.r);raw.WriteByte(c.g);raw.WriteByte(c.b);raw.WriteByte(c.a);}}
  var data=raw.ToArray();var z=new MemoryStream();z.WriteByte(0x78);z.WriteByte(0x9C);
  using(var d=new DeflateStream(z,System.IO.Compression.CompressionLevel.Optimal,true))d.Write(data,0,data.Length);
  uint a=1,b=0;foreach(byte v in data){a=(a+v)%65521;b=(b+a)%65521;}uint adler=(b<<16)|a;z.WriteByte((byte)(adler>>24));z.WriteByte((byte)(adler>>16));z.WriteByte((byte)(adler>>8));z.WriteByte((byte)adler);
  var o=new MemoryStream();o.Write(new byte[]{137,80,78,71,13,10,26,10},0,8);
  var ihdr=new MemoryStream();Be(ihdr,(uint)w);Be(ihdr,(uint)h);ihdr.WriteByte(8);ihdr.WriteByte(6);ihdr.WriteByte(0);ihdr.WriteByte(0);ihdr.WriteByte(0);
  Chunk(o,"IHDR",ihdr.ToArray());Chunk(o,"IDAT",z.ToArray());Chunk(o,"IEND",new byte[0]);return o.ToArray();
 }
 static void Be(Stream s,uint v){s.WriteByte((byte)(v>>24));s.WriteByte((byte)(v>>16));s.WriteByte((byte)(v>>8));s.WriteByte((byte)v);}
 static uint[] crcTable;
 static void Chunk(Stream o,string type,byte[] body){
  if(crcTable==null){crcTable=new uint[256];for(uint n=0;n<256;n++){uint c=n;for(int k=0;k<8;k++)c=(c&1)!=0?0xEDB88320u^(c>>1):c>>1;crcTable[n]=c;}}
  Be(o,(uint)body.Length);var t=Encoding.ASCII.GetBytes(type);o.Write(t,0,4);o.Write(body,0,body.Length);
  uint crc=0xFFFFFFFFu;foreach(byte v in t)crc=crcTable[(crc^v)&0xFF]^(crc>>8);foreach(byte v in body)crc=crcTable[(crc^v)&0xFF]^(crc>>8);Be(o,crc^0xFFFFFFFFu);
 }
}
}
