using System;
using UnityEditor;
using UnityEngine;

public static class GraphicsChecks {
 [MenuItem("Paradigm Realm/Run Graphics Checks")]
 public static void Run(){
  int count=0;
  for(int i=0;i<12;i++)Check("Visuals/unit-"+i.ToString("00"),true,ref count);
  // Walking frames from the owner's movement pack (10 Nov 2026): six per unit type, all 256 x 256.
  for(int i=0;i<16;i++)for(int f=0;f<6;f++){string p="Visuals/Movement/unit-"+i.ToString("00")+"-move-"+f.ToString("00");Check(p,true,ref count);var t=Resources.Load<Texture2D>(p);if(t.width!=256||t.height!=256)throw new Exception("Walking frame is not 256 x 256: "+p);}
  for(int era=0;era<4;era++)for(int style=0;style<5;style++)Check("Visuals/city-"+era+"-"+style,true,ref count);
  for(int i=1;i<=5;i++)Check("Visuals/resource-"+i.ToString("00"),true,ref count);
  foreach(var name in new[]{"trees","pines","mountain","farm","mine","fort","village","palms"})Check("Visuals/"+name,true,ref count);
  for(int i=0;i<21;i++)Check("Terrain/terrain-"+i.ToString("00"),false,ref count);
  Check("Visuals/title-world",false,ref count);
  // The owner's technology icons (13 Nov 2026): one for every technology that can be researched, 128 x 128 (UI art, smooth filtering).
  for(int t=0;t<ParadigmRealm.TechnologyCatalog.Count;t++){if(!ParadigmRealm.TechnologyCatalog.Active[t])continue;string p="UI/Tech/tech-"+ParadigmRealm.TechnologyCatalog.Ids[t];var icon=Resources.Load<Texture2D>(p);if(icon==null)throw new Exception("Missing technology icon: "+p);if(icon.width!=128||icon.height!=128)throw new Exception("Technology icon is not 128 x 128: "+p);count++;}
  var names=Resources.Load<TextAsset>("Visuals/gallery-names");
  if(names==null)throw new Exception("Missing artwork gallery index");
  var lines=names.text.Split(new[]{'\n'},StringSplitOptions.RemoveEmptyEntries);
  for(int i=0;i<lines.Length;i++)Check("Visuals/Gallery/"+i.ToString("000"),false,ref count);
  Debug.Log("PASS: "+count+" graphics assets, point filtering, readable terrain and transparent sprite backgrounds. Test Play Mode for layout and interaction.");
 }
 static void Check(string path,bool alpha,ref int count){
  var texture=Resources.Load<Texture2D>(path);
  if(texture==null)throw new Exception("Missing graphics asset: "+path);
  if(texture.filterMode!=FilterMode.Point)throw new Exception("Expected point filtering: "+path);
  if(!texture.isReadable)throw new Exception("Expected readable texture: "+path);
  if(alpha){bool clear=false,solid=false;foreach(var c in texture.GetPixels32()){if(c.a==0)clear=true;if(c.a>128)solid=true;}if(!clear||!solid)throw new Exception("Invalid sprite alpha: "+path);}
  count++;
 }
}
