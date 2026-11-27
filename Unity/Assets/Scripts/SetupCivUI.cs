using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
// New Game, first page, after the owner's picture of 27 Nov 2026 ("Choose your civilisation"): the title under a landscape, the
// leaders as framed cards in two centred rows, then the chosen civilisation's name, leader and epithet in gold capitals with its
// traits, ability, unit, building and weakness on the left and strengths, weaknesses and its rival agenda on the right, and the two
// buttons along the foot. The picture with its cards and words cleared is Resources/UI/Setup/setup2-civ (1672 x 941), laid out in
// its own pixels like the world page (SetupMatrix). "Create your own" opens its name, leader, portrait and traits in the same place.
public partial class RealmGame {
 const float CivCardW=145,CivCardH=192;
 GUIStyle civRich,civCard;
 void CivSetupStyles(){
  SetupStyles();if(civRich!=null)return;
  civRich=new GUIStyle(setText){fontSize=19,richText=true,wordWrap=true};
  civCard=new GUIStyle(setText){fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=true};civCard.normal.textColor=civCard.hover.textColor=new Color(.95f,.93f,.88f);
 }
 // A gold small-capital label for rich text: a larger first letter, as the picture's "Trait", "Strengths".
 static string CivLabel(string word,int size){string up=word.ToUpperInvariant();return "<color=#E3B25E>"+(up.Length>0?"<size="+(size+3)+">"+up.Substring(0,1)+"</size>":"")+"<size="+size+">"+(up.Length>1?up.Substring(1):"")+"</size></color>";}
 // Gold capitals on one line, made smaller until they fit.
 void SetupCapsFit(float x,float y,string text,int size,float width){var s=new GUIStyle(setCap){fontSize=size};while(s.fontSize>12&&s.CalcSize(new GUIContent(text.ToUpperInvariant())).x>width)s.fontSize--;SetupCaps(x,y+(size-s.fontSize)*.5f,text,s.fontSize);}
 void Line(Rect r,Color c){if(Event.current.type==EventType.Repaint)GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,c,0,0);}
 void CivCardFrame(Rect r,bool chosen,bool hover){
  var gold=new Color(.72f,.56f,.3f);
  if(chosen){Line(new Rect(r.x-6,r.y-6,r.width+12,r.height+12),new Color(.25f,.75f,.7f,.18f));Line(new Rect(r.x-4,r.y-4,r.width+8,r.height+8),new Color(.36f,.85f,.78f,.55f));Line(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),new Color(.55f,.95f,.88f));}
  Line(r,hover&&!chosen?new Color(.9f,.74f,.42f):gold);Line(new Rect(r.x+1.5f,r.y+1.5f,r.width-3,r.height-3),new Color(.05f,.08f,.14f));
 }
 void DrawCivSetup(){
  CivSetupStyles();Wide();Fill(FullCanvas,new Color(.035f,.06f,.12f));
  GUI.matrix=SetupMatrix();var frame=SetupTex("setup2-civ");if(frame!=null)GUI.DrawTexture(new Rect(0,0,SetupW,SetupH),frame,ScaleMode.StretchToFill,false);
  var e=Event.current;
  // Two groups only while the real-world civilisations are offered; otherwise one set of cards, as in the picture.
  if(!CivilisationCatalog.RealWorldOffered)civGroupReal=false;
  else for(int g=0;g<2;g++){var r=new Rect(1100+g*268,96,260,28);if(SetupOption(r,g==0?"Fantasy Kingdoms":"Real World Civilisations",civGroupReal==(g==1))&&civGroupReal!=(g==1)){civGroupReal=g==1;Centred();SelectCivilisationGroup();}}
  var cards=CivilisationCatalog.Offered(civGroupReal).Concat(new[]{CivilisationCatalog.Count}).ToList();
  if(setupCiv<CivilisationCatalog.Count&&(!CivilisationCatalog.Enabled[setupCiv]||CivilisationCatalog.IsReal(setupCiv)!=civGroupReal))setupCiv=cards[0];
  if(CivilisationCatalog.RealPortrait(customPortrait)!=civGroupReal)customPortrait=NextPortrait(civGroupReal?CivilisationCatalog.FirstReal-1:CivilisationCatalog.PortraitCount-1,1);
  // ---------- the cards: the first row takes the larger half, both centred ----------
  int perRow=(cards.Count+1)/2;float w=CivCardW,h=CivCardH,gap=20,top=cards.Count>perRow*2?100:107;
  if(perRow*(w+gap)-gap>1580){float k=1580/(perRow*(w+gap)-gap);w*=k;h*=k;gap*=k;}
  for(int n=0;n<cards.Count;n++){
   int row=n<perRow?0:1,inRow=row==0?perRow:cards.Count-perRow,col=row==0?n:n-perRow;float rowGap=row==0?gap:gap+6;
   float x=836-(inRow*(w+rowGap)-rowGap)/2+col*(w+rowGap),y=top+row*(h+18);var r=new Rect(x,y,w,h);int i=cards[n];
   bool hover=r.Contains(e.mousePosition);CivCardFrame(r,setupCiv==i,hover);
   float face=h*.735f;var tex=Portrait(i<CivilisationCatalog.Count?CivilisationCatalog.Portraits[i]:customPortrait);
   if(tex!=null)DrawPortrait(new Rect(x+3,y+3,w-6,face-3),tex);
   Line(new Rect(x+3,y+face,w-6,1.5f),new Color(.72f,.56f,.3f));Line(new Rect(x+3,y+face+1.5f,w-6,h-face-4.5f),new Color(.04f,.07f,.13f));
   // Two lines as in the picture: the first word, then the rest ("Azure / Crown", "Create / your own").
   string name=i<CivilisationCatalog.Count?CivilisationCatalog.Names[i]:"Create your own";{int sp=name.IndexOf(' ');if(sp>0)name=name.Substring(0,sp)+"\n"+name.Substring(sp+1);}
   SetupWrite(new Rect(x+6,y+face+2,w-12,h-face-6),name,civCard,12);
   if(GUI.Button(r,"",GUIStyle.none)){setupCiv=i;if(i<CivilisationCatalog.Count)proposedName=CivilisationCatalog.Names[i];civLoreOpen=false;}
  }
  // ---------- the chosen civilisation ----------
  bool lore=false;
  if(setupCiv<CivilisationCatalog.Count){
   int civ=setupCiv;var entry=CivLore.Of(civ);var traits=CivilisationCatalog.Traits[civ];
   SetupCapsFit(48,540,CivilisationCatalog.Names[civ]+" — led by "+CivilisationCatalog.Leaders[civ]+(entry!=null?"  ·  "+entry.Epithet:""),26,1250);
   if(entry!=null&&SetupOption(new Rect(1331,543,300,34),"History and lore",true))lore=true;
   var left=new System.Collections.Generic.List<string>();
   for(int k=0;k<2;k++)left.Add(CivLabel("Trait",15)+" — "+CivilisationCatalog.TraitNames[traits[k]]+": "+CivilisationCatalog.TraitTexts[traits[k]]);
   if(entry!=null){left.Add(CivLabel("Ability",15)+" — "+entry.Ability+": "+entry.AbilityText);left.Add(CivLabel("Unique unit",15)+" — "+entry.Unit+": "+entry.UnitText);
    left.Add(CivLabel("Unique building",15)+" — "+entry.Building+": "+entry.BuildingText);left.Add(CivLabel("Weakness",15)+" — "+entry.Weakness+": "+entry.WeaknessText);}
   SetupFlow(new Rect(60,594,790,196),left,6);
   var right=new System.Collections.Generic.List<string>();
   if(entry!=null){right.Add(CivLabel("Strengths:",15)+"  "+entry.Strengths);right.Add(CivLabel("Weaknesses:",15)+"  "+entry.Weaknesses);
    right.Add(CivLabel("As a rival,",15)+"  "+CivilisationCatalog.Leaders[civ]+" is a "+CivLore.AgendaNames[entry.Agenda]+": "+CivLore.AgendaTexts[entry.Agenda]);}
   else right.Add("Rivals are drawn from the other civilisations, each with its own leader and traits.");
   SetupFlow(new Rect(916,594,712,196),right,12);
  }else DrawCustomCivSetup();
  // ---------- next, or back ----------
  bool ok=CustomValid();bool was=GUI.enabled;GUI.enabled=was&&ok;
  bool next=SetupBig(new Rect(127,830,694,56),"Next: choose the world");GUI.enabled=was;
  if(!ok)Line(new Rect(131,834,686,48),new Color(.02f,.04f,.08f,.45f));
  bool back=SetupBig(new Rect(854,830,692,56),"Back");
  Centred();
  if(lore){civLoreOpen=true;civLoreScroll=Vector2.zero;GUIUtility.ExitGUI();}
  if(setupCiv<CivilisationCatalog.Count&&civLoreOpen)DrawCivilisationLore(setupCiv);
  if(next&&ok){setupPage=1;GUIUtility.ExitGUI();}
  if(back){nameSetup=false;setupPage=0;GUIUtility.ExitGUI();}
 }
 // Rich-text paragraphs one under another; every paragraph steps down together until all of them fit the place.
 void SetupFlow(Rect r,System.Collections.Generic.List<string> parts,float gap){
  var s=new GUIStyle(civRich);
  Func<float> total=()=>parts.Sum(p=>s.CalcHeight(new GUIContent(p),r.width))+gap*(parts.Count-1);
  while(s.fontSize>12&&total()>r.height){s.fontSize--;gap=Mathf.Max(2,gap-1);}
  float y=r.y;var shade=new GUIStyle(s);shade.normal.textColor=shade.hover.textColor=new Color(0,0,0,.5f);
  foreach(var p in parts){float ph=s.CalcHeight(new GUIContent(p),r.width);GUI.Label(new Rect(r.x,y,r.width,ph),p,s);y+=ph+gap;}
 }
 // "Create your own": name and leader on the left with the portrait chooser; the two traits on the right.
 void DrawCustomCivSetup(){
  SetupCapsFit(48,540,"Create your own civilisation",26,1250);
  var label=new GUIStyle(setText){fontSize=17};label.normal.textColor=label.hover.textColor=new Color(.89f,.7f,.37f);
  // Each field's words get smaller until the whole name shows (no text is ever cut; the longest allowed is 32 letters).
  Func<string,GUIStyle> fit=t=>{var f=new GUIStyle(setField){fontSize=18,padding=new RectOffset(10,10,0,0)};while(f.fontSize>11&&f.CalcSize(new GUIContent(t)).x>370)f.fontSize--;return f;};
  SetupWrite(new Rect(60,594,380,22),"Civilisation name",label);SetupBar(new Rect(58,617,392,34),"setup2-button",18);GUI.SetNextControlName("customname");proposedName=GUI.TextField(new Rect(62,619,384,30),proposedName,32,fit(proposedName));
  SetupWrite(new Rect(468,594,380,22),"Leader",label);SetupBar(new Rect(466,617,392,34),"setup2-button",18);customLeader=GUI.TextField(new Rect(470,619,384,30),customLeader,32,fit(customLeader));
  SetupWrite(new Rect(60,664,790,22),"Portrait: "+CivilisationCatalog.PortraitTitles[customPortrait],label);
  if(SetupOption(new Rect(60,690,60,30),"<",false))customPortrait=NextPortrait(customPortrait,-1);
  {var group=Enumerable.Range(0,CivilisationCatalog.PortraitCount).Where(p=>CivilisationCatalog.RealPortrait(p)==civGroupReal&&Portrait(p)!=null).ToList();SetupWrite(new Rect(126,690,90,30),(group.IndexOf(customPortrait)+1)+" / "+group.Count,new GUIStyle(setOption){fontSize=18});}
  if(SetupOption(new Rect(222,690,60,30),">",false))customPortrait=NextPortrait(customPortrait,1);
  SetupWrite(new Rect(60,732,790,54),customTraitA==customTraitB?"Choose two different traits.":CivilisationCatalog.TraitLine(new[]{customTraitA,customTraitB}),new GUIStyle(setText){fontSize=17});
  SetupWrite(new Rect(916,594,340,22),"First trait",label);SetupWrite(new Rect(1280,594,340,22),"Second trait",label);
  var names=CivilisationCatalog.TraitNames;
  customTraitA=SetupGrid(customTraitA,names,3,916,346,620,26,30,5);customTraitB=SetupGrid(customTraitB,names,3,1280,346,620,26,30,5);
 }
}
}
