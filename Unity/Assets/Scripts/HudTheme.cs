using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParadigmRealm
{
    // The in-game interface in the style of the owner's mockup (30 Oct 2026): the map fills the whole screen and framed navy
    // and gold widgets float over it. Top bar with resources, a navigation rail on the left, research and leaders along the
    // top, notifications on the right, minimap and End Turn bottom right, the unit or city card bottom left, the hovered
    // tile bottom centre. Titles use Cinzel (SIL Open Font Licence, Resources/Fonts); body text stays sans-serif.
    public partial class RealmGame
    {
        // ---------- layout ----------
        public const float MapX=0,MapY=0,MapW=1280,MapH=800;
        static readonly Rect MapView=new Rect(MapX,MapY,MapW,MapH);
        static readonly Rect TopBar=new Rect(0,0,1280,38),ResearchCard=new Rect(174,46,300,52),UnitCard=new Rect(8,592,410,200),
            TileCard=new Rect(571,694,336,52),MiniMap=new Rect(1060,590,178,138),MiniButtons=new Rect(1242,590,32,138),
            EndTurnRect=new Rect(1060,738,214,54),MorePanel=new Rect(8,246,410,340);
        static readonly string[] RailScreens={"Economy","Civics","Research","Diplomacy","Religion","Cities","Military","Atlas","Victory","Chronicle","Identity"};
        static readonly string[] RailLabels={"Empire","Civics","Research","Diplomacy","Religion","Cities","Military","Realms","Victory","Chronicle","Identity"};
        static readonly int[] RailIcons={PixelArt.IconEmpire,PixelArt.IconCivics,PixelArt.IconResearch,PixelArt.IconDiplomacy,PixelArt.IconFaith,PixelArt.IconCities,PixelArt.IconMilitary,PixelArt.IconRealms,PixelArt.IconVictory,PixelArt.IconChronicle,PixelArt.IconIdentity};
        const float RailItem=44;
        static Rect RailRect{get{return new Rect(0,44,166,RailScreens.Length*RailItem+8);}}
        readonly List<Rect> hudRects=new List<Rect>();
        bool moreActions,moveMode,roadMode;
        // A label whose font shrinks (down to minimum) until the whole text fits the box (owner request: nothing cut off).
        void FitLabel(Rect r,string text,GUIStyle style,int minimum=9){var s=new GUIStyle(style){wordWrap=false,clipping=TextClipping.Overflow};while(s.fontSize>minimum&&s.CalcSize(new GUIContent(text)).x>r.width)s.fontSize--;GUI.Label(r,text,s);}
        // Clicks and the mouse wheel reach the map only where no widget is drawn over it.
        bool OverHud(Vector2 p){foreach(var r in hudRects)if(r.Contains(p))return true;return false;}
        bool InMap(Vector2 p){return MapView.Contains(p)&&!OverHud(p);}
        // The interface is laid out and drawn several times a frame; what it reads from the world is worked out at most once a frame,
        // and the costly totals a few times a second.
        List<int> metRivalsMemo;int metRivalsFrame=-1;
        List<int> MetRivals(){if(metRivalsMemo!=null&&metRivalsFrame==Time.frameCount)return metRivalsMemo;metRivalsFrame=Time.frameCount;metRivalsMemo=new List<int>(5);for(int o=1;o<world.civCount&&metRivalsMemo.Count<5;o++)if(world.met[o])metRivalsMemo.Add(o);return metRivalsMemo;}
        int hudBudget,hudOwnCities,hudCalm,hudScience,hudFood,hudFaith,hudResearchTurns,hudEra,hudTurn=-1;float hudAt;World hudWorld;
        void RefreshHudNumbers(){
            if(hudWorld==world&&hudTurn==world.turn&&Time.realtimeSinceStartup-hudAt<.3f)return;hudWorld=world;hudTurn=world.turn;hudAt=Time.realtimeSinceStartup;
            hudBudget=world.Budget(0)[2];hudOwnCities=0;hudCalm=0;hudScience=0;foreach(var c in world.cities)if(c.owner==0){hudOwnCities++;if(!world.Disorder(c))hudCalm++;hudScience+=world.Yield(c)[2];}
            int need;hudFood=world.FoodBalance(0,out need);hudFaith=world.FaithPerTurn(0);hudResearchTurns=world.research>=0?world.ResearchTurns(world.research,0):-1;hudEra=world.Era(0);
        }
        Rect LeaderRect(int i){return new Rect(1274-(i+1)*80,46,74,96);}
        // The current message (what just happened or why an order failed) leads, then the newest log entries of this turn and last.
        List<string> notesMemo;int notesTurn=-1,notesLog=-1;string notesStatus;
        List<string> RecentNotes(){if(notesMemo!=null&&notesTurn==world.turn&&notesLog==world.log.Count&&notesStatus==status)return notesMemo;notesTurn=world.turn;notesLog=world.log.Count;notesStatus=status;notesMemo=WorkOutNotes();return notesMemo;}
        List<string> WorkOutNotes(){string now="Turn "+world.turn+":",last="Turn "+(world.turn-1)+":";var notes=new List<string>();if(!string.IsNullOrEmpty(status))notes.Add(status);foreach(var l in world.log.Where(l=>l.StartsWith(now)||l.StartsWith(last))){if(notes.Count>=3)break;string text=l.Substring(l.IndexOf(':')+1).Trim();if(text!=status)notes.Add(text);}return notes;}
        Rect NoteRect(int i){float top=MetRivals().Count>0?150:46;return new Rect(1040,top+i*52,234,46);}
        bool CardShown(){return Selected()!=null||world.cities.Any(a=>a.owner==0&&a.id==cityId);}
        void BuildHudRects()
        {
            hudRects.Clear();if(world==null)return;
            hudRects.Add(TopBar);hudRects.Add(RailRect);hudRects.Add(ResearchCard);hudRects.Add(MiniMap);hudRects.Add(MiniButtons);hudRects.Add(EndTurnRect);hudRects.Add(TileCard);hudRects.Add(MedalBar);
            if(CardShown())hudRects.Add(UnitCard);if(moreActions&&CardShown())hudRects.Add(MorePanel);
            for(int i=0;i<MetRivals().Count;i++)hudRects.Add(LeaderRect(i));
            for(int i=0;i<RecentNotes().Count;i++)hudRects.Add(NoteRect(i));
            // The tutorial panel takes its own clicks (its Next, Hide and End buttons), not the map underneath.
            if(TutorialShown())hudRects.Add(TutorialRect());
            if(NoticeShown())hudRects.Add(NoticeRect);if(CallShown())hudRects.Add(CallRect);
            if(viewRealm>0)hudRects.Add(new Rect(472,708,336,30));

        }

        // ---------- type and icons ----------
        Font displayFont;GUIStyle hudTitle,hudLabel,hudSmall,hudNumber,hudCentre,hudCaption,endTurnStyle,keyCapStyle;
        void HudStyles()
        {
            if(hudTitle!=null)return;
            displayFont=Resources.Load<Font>("Fonts/Cinzel");
            hudTitle=new GUIStyle(GUI.skin.label){fontSize=17,fontStyle=FontStyle.Bold,wordWrap=false,clipping=TextClipping.Clip};if(displayFont!=null)hudTitle.font=displayFont;hudTitle.normal.textColor=UiGold;
            hudLabel=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=false,clipping=TextClipping.Clip};hudLabel.normal.textColor=UiText;
            hudSmall=new GUIStyle(hudLabel){fontSize=12,wordWrap=true};hudSmall.normal.textColor=UiMuted;
            hudNumber=new GUIStyle(hudLabel){fontSize=14,alignment=TextAnchor.MiddleRight};
            hudCentre=new GUIStyle(hudLabel){fontSize=12,alignment=TextAnchor.MiddleCenter};
            hudCaption=new GUIStyle(hudTitle){fontSize=14};
            endTurnStyle=new GUIStyle(GUI.skin.button){fontSize=22,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(0,0,0,6)};if(displayFont!=null)endTurnStyle.font=displayFont;
            var goldFace=PanelTexture(new Color(.78f,.6f,.3f),new Color(.98f,.86f,.5f),true);var goldHover=PanelTexture(new Color(.88f,.7f,.36f),new Color(1f,.92f,.62f),true);
            foreach(var s in new[]{endTurnStyle.normal,endTurnStyle.focused})s.background=goldFace;endTurnStyle.hover.background=goldHover;endTurnStyle.active.background=goldHover;
            foreach(var s in new[]{endTurnStyle.normal,endTurnStyle.hover,endTurnStyle.active,endTurnStyle.focused})s.textColor=new Color(.16f,.1f,.04f);
            keyCapStyle=new GUIStyle(hudCentre){fontSize=10};keyCapStyle.normal.textColor=UiText;
            // Panel headings across the game use the same lettering.
            if(displayFont!=null){title.font=displayFont;heading.font=displayFont;}title.normal.textColor=UiGold;heading.normal.textColor=UiGold;
        }
        Texture2D Icon(int k){return UiIcon(k)??IsoPicture(8,k,0);}
        void DrawIcon(int k,Rect r){GUI.DrawTexture(r,Icon(k),ScaleMode.ScaleToFit,true);}
        void Bar(Rect r,float fraction,Color ink){Fill(r,UiTrough);Fill(new Rect(r.x,r.y,r.width*Mathf.Clamp01(fraction),r.height),ink);Fill(new Rect(r.x,r.y,r.width*Mathf.Clamp01(fraction),1),Color.Lerp(ink,Color.white,.4f));}
        // A framed button with an icon over a caption; "active" shows the teal pressed state of the mockup.
        bool HudButton(Rect r,int icon,string caption,bool active=false)
        {
            bool clicked=GUI.Button(r,"");
            if(active)Fill(new Rect(r.x+2,r.y+2,r.width-4,r.height-4),new Color(UiButtonActive.r,UiButtonActive.g,UiButtonActive.b,.9f));
            DrawIcon(icon,new Rect(r.center.x-11,r.y+3,22,22));FitLabel(new Rect(r.x+2,r.yMax-20,r.width-4,18),caption,hudCentre);
            return clicked;
        }
        int SciencePerTurn(){RefreshHudNumbers();return hudScience;}

        // ---------- the whole HUD ----------
        void DrawHud()
        {
            bool idle=!guide&&!confirmNew&&pendingUnit==-1&&!PromptOpen()&&!turnRunning;GUI.enabled=idle;
            DrawRail();DrawResearchCard();DrawLeaders();DrawNotices();DrawMiniMap();
            if(CardShown()){if(moreActions)DrawMoreActions();DrawUnitCard();}else moreActions=false;
            DrawTileCard();DrawMedalBar();DrawBuildNotice();DrawRivalCall();

            GUI.enabled=idle&&world.result=="";

            if(turnRunning){GUI.enabled=true;GUI.Box(EndTurnRect,"");GUI.Label(new Rect(EndTurnRect.x,EndTurnRect.y+6,EndTurnRect.width,20),"The world moves...",hudCentre);EraBar(new Rect(EndTurnRect.x+12,EndTurnRect.y+32,EndTurnRect.width-24,10),Mathf.Min(turnStep,world.civCount)/(float)Math.Max(1,world.civCount));GUI.enabled=false;}
            else{
            if(GUI.Button(EndTurnRect,"END TURN",endTurnStyle))EndTurn();
            var cap=new Rect(EndTurnRect.xMax-50,EndTurnRect.yMax-21,42,16);Fill(cap,new Color(.12f,.08f,.03f,.75f));GUI.Label(cap,"Enter",keyCapStyle);}
            GUI.enabled=true;
            DrawTopBar();
            // Full tile details (movement, settlement advice, units present) while Alt is held.
            if(tooltips&&Event.current.alt&&idle)DrawTileTooltip();
        }
        void DrawTopBar()
        {
            HudStyles();RefreshHudNumbers();
            Fill(TopBar,new Color(.035f,.055f,.095f,.96f));Fill(new Rect(0,TopBar.yMax-2,1280,1),UiGold);Fill(new Rect(0,TopBar.yMax-1,1280,1),new Color(0,0,0,.6f));
            // The compass-rose emblem and name on a gold-framed plate, as in the mockup.
            {var plate=new Rect(2,1,228,36);GUI.Box(plate,"");var logo=UiTex("logo");if(logo!=null)GUI.DrawTexture(new Rect(6,0,38,38),logo,ScaleMode.ScaleToFit,true);else DrawIcon(PixelArt.IconRealms,new Rect(12,7,24,24));FitLabel(new Rect(50,8,172,26),"PARADIGM  REALM",hudTitle,11);}
            int budget=hudBudget,ownCities=hudOwnCities,calm=hudCalm;
            // Resources as in the mockup: a large painted icon, the amount, and the change per turn in green (red when falling).
            // Hovering an item names it.
            var items=new[]{
                new KeyValuePair<int,string[]>(PixelArt.IconGold,new[]{world.gold.ToString(),(budget>=0?"+":"")+budget+(world.nations[0].arrears>0?"  IN DEBT":""),"Gold in the treasury and the change per turn"}),
                new KeyValuePair<int,string[]>(PixelArt.IconScience,new[]{"","+"+SciencePerTurn(),"Science per turn"}),
                new KeyValuePair<int,string[]>(PixelArt.IconFood,new[]{world.FoodStateName(0),(hudFood>=0?"+":"")+hudFood,"Food for the army: "+world.FoodStateName(0)}),
                new KeyValuePair<int,string[]>(PixelArt.IconFaith,new[]{world.nations[0].faith.ToString(),"+"+hudFaith,"Faith and faith per turn"}),
                new KeyValuePair<int,string[]>(PixelArt.IconAge,new[]{TechnologyCatalog.EraNames[hudEra]+(world.AgeOf(0)!=World.AgeNormal?" · "+World.AgeNames[world.AgeOf(0)].Replace(" Age",""):""),"","Your age"}),
                new KeyValuePair<int,string[]>(PixelArt.IconOrder,new[]{ownCities==0?"—":(calm*100/ownCities)+"%","","Cities in order"})};
            float x=244;string hoverTip=null;var mouse=Event.current.mousePosition;
            var deltaUp=new GUIStyle(hudLabel){fontSize=12};deltaUp.normal.textColor=new Color(.45f,.85f,.4f);var deltaDown=new GUIStyle(deltaUp);deltaDown.normal.textColor=new Color(.95f,.4f,.3f);
            var amount=new GUIStyle(hudLabel){fontSize=15};if(displayFont!=null)amount.font=displayFont;
            foreach(var item in items)
            {
                float start=x;DrawIcon(item.Key,new Rect(x,4,30,30));x+=34;
                if(item.Value[0]!=""){float w=amount.CalcSize(new GUIContent(item.Value[0])).x;GUI.Label(new Rect(x,8,w+4,24),item.Value[0],amount);x+=w+6;}
                if(item.Value[1]!=""){var st=item.Value[1].StartsWith("-")?deltaDown:deltaUp;float w=st.CalcSize(new GUIContent(item.Value[1])).x;GUI.Label(new Rect(x,11,w+4,20),item.Value[1],st);x+=w+4;}
                if(new Rect(start,2,x-start,34).Contains(mouse))hoverTip=item.Value[2];
                x+=18;Fill(new Rect(x-10,9,1,20),new Color(UiGold.r,UiGold.g,UiGold.b,.45f));
            }
            if(hoverTip!=null){var size=hudSmall.CalcSize(new GUIContent(hoverTip));var tr=new Rect(Mathf.Min(mouse.x,1270-size.x-12),40,size.x+12,size.y+6);Fill(tr,new Color(.05f,.08f,.16f,.95f));Fill(new Rect(tr.x,tr.yMax-1,tr.width,1),UiGold);GUI.Label(new Rect(tr.x+6,tr.y+3,size.x+2,size.y),hoverTip,hudSmall);}
            {float tx=Math.Max(x,1036);DrawIcon(PixelArt.IconTurn,new Rect(tx,8,22,22));GUI.Label(new Rect(tx+26,9,1192-tx-26,22),"Turn "+world.turn+"  •  "+world.Calendar(),hudLabel);}
            bool enabled=GUI.enabled;GUI.enabled=!confirmNew&&pendingUnit==-1&&!turnRunning;
            if(GUI.Button(new Rect(1196,5,34,28),""))guide=true;DrawIcon(PixelArt.IconHelp,new Rect(1201,7,24,24));
            if(GUI.Button(new Rect(1236,5,38,28),"")){pauseMenu=true;GUIUtility.ExitGUI();}DrawIcon(PixelArt.IconMenu,new Rect(1243,7,24,24));
            GUI.enabled=enabled;
        }
        void DrawRail()
        {
            // The left menu after the mockup: a gold-framed column of gold-trimmed bars, each a painted icon and its name.
            var rail=RailRect;GUI.Box(rail,"");
            int offers=world.offers.Count(o=>o.to==0&&o.status==0&&o.expires>world.turn);
            var railLabel=new GUIStyle(hudLabel){fontSize=14,alignment=TextAnchor.MiddleLeft};if(displayFont!=null)railLabel.font=displayFont;
            for(int i=0;i<RailScreens.Length;i++)
            {
                var r=new Rect(rail.x+5,rail.y+5+i*RailItem,rail.width-10,RailItem-3);bool over=r.Contains(Event.current.mousePosition)&&GUI.enabled;
                DrawButtonFace(r,over);var art=RailScreens[i]=="Religion"?UiTex("icon-religion"):UiIcon(RailIcons[i]);
                var iconRect=new Rect(r.x+14,r.y+5,r.height-10,r.height-10);if(art!=null)GUI.DrawTexture(iconRect,art,ScaleMode.ScaleToFit,true);else DrawIcon(RailIcons[i],iconRect);
                railLabel.normal.textColor=over?new Color(1f,.9f,.6f):UiText;FitLabel(new Rect(iconRect.xMax+6,r.y,r.xMax-iconRect.xMax-30,r.height),RailLabels[i],railLabel,10);
                if(GUI.Button(r,"",GUIStyle.none))OpenRailScreen(RailScreens[i]);
                if(RailScreens[i]=="Diplomacy"&&offers>0){var b=new Rect(r.xMax-24,r.y+4,16,16);Fill(b,new Color(.75f,.18f,.14f));GUI.Label(b,offers.ToString(),hudCentre);}
            }
        }
        void DrawResearchCard()
        {
            var r=ResearchCard;bool clicked=GUI.Button(r,"");if(world.research>=0)GlowBorder(r,GlowSilver);
            var well=new Rect(r.x+6,r.y+6,40,40);Fill(well,UiTrough);DrawIcon(PixelArt.IconResearch,new Rect(well.x+8,well.y+8,24,24));
            if(world.research<0){GUI.Label(new Rect(r.x+56,r.y+15,230,22),"Choose research",hudCaption);}
            else
            {
                int cost=world.ResearchCost(world.research,0),done=world.science;RefreshHudNumbers();int turns=hudResearchTurns;
                GUI.Label(new Rect(r.x+56,r.y+6,170,22),World.TechNames[world.research],hudCaption);GUI.Label(new Rect(r.xMax-82,r.y+7,72,20),turns<0?"no science":turns+(turns==1?" turn":" turns"),hudNumber);
                EraBar(new Rect(r.x+56,r.y+32,r.width-68,8),done/(float)Math.Max(1,cost));
            }
            if(clicked){researchScreen=true;detailsScreen="";ResetMapGesture();GUI.FocusControl(null);GUIUtility.ExitGUI();}
        }
        // Leaders you have met, with how they feel about you; click to open Diplomacy with them.
        void DrawLeaders()
        {
            var met=MetRivals();
            for(int i=0;i<met.Count;i++)
            {
                int o=met[i];var r=LeaderRect(i);bool clicked=GUI.Button(r,"");var n=world.nations[o];
                var face=new Rect(r.x+4,r.y+4,r.width-8,r.height-30);var art=Portrait(n.portrait,world.Era(o));
                if(art!=null)DrawPortrait(face,art);else{Fill(face,nations[o%nations.Length]*.6f);GUI.Label(face,world.NationName(o).Substring(0,1),hudTitle);}
                if(world.AtWar(0,o)){Fill(new Rect(r.x,r.y,r.width,2),new Color(.85f,.2f,.15f));Fill(new Rect(r.x,r.yMax-2,r.width,2),new Color(.85f,.2f,.15f));}
                int opinion=world.Opinion(o,0);Color mood=opinion>10?new Color(.35f,.8f,.4f):opinion<-10?new Color(.9f,.3f,.25f):new Color(.95f,.78f,.3f);
                DrawIcon(opinion>10?PixelArt.IconMoodGood:opinion<-10?PixelArt.IconMoodBad:PixelArt.IconMoodNeutral,new Rect(r.x+6,r.yMax-24,20,20));var score=new GUIStyle(hudLabel);score.normal.textColor=mood;GUI.Label(new Rect(r.x+30,r.yMax-23,r.width-34,20),(opinion>0?"+":"")+opinion,score);
                if(clicked){diplomacyPartner=o;diplomacyDirect=true;OpenDetails("Diplomacy");}
            }
        }
        void DrawNotices()
        {
            var notes=RecentNotes();
            for(int i=0;i<notes.Count;i++)
            {
                var r=NoteRect(i);if(GUI.Button(r,""))OpenDetails("Chronicle");
                DrawIcon(i==0&&notes[i]==status?PixelArt.IconHelp:PixelArt.IconChronicle,new Rect(r.x+8,r.y+11,24,24));string text=notes[i];
                GUI.Label(new Rect(r.x+40,r.y+4,r.width-46,r.height-6),text,hudSmall);
            }
        }
        // The known world in miniature with the visible area outlined; click or drag to move the view.
        void DrawMiniMap()
        {
            GUI.Box(MiniMap,"");var inner=new Rect(MiniMap.x+5,MiniMap.y+5,MiniMap.width-10,MiniMap.height-10);
            if(atlasTexture==null||atlasWorld!=world||Time.realtimeSinceStartup-atlasBuilt>3f)BuildAtlasTexture();
            // Staggered rows are a quarter of a tile width apart (16 of 64 pixels), so a row is a quarter as tall as a column is wide.
            float aspect=world.Width/(world.Hex?ViewRows*PixelArt.HR/(float)PixelArt.HW:world.Staggered?ViewRows*.25f:(float)ViewRows),w=inner.width,h=w/aspect;if(h>inner.height){h=inner.height;w=h*aspect;}
            var drawn=new Rect(inner.center.x-w/2,inner.center.y-h/2,w,h);Fill(inner,UiTrough);GUI.DrawTexture(drawn,atlasTexture,ScaleMode.StretchToFill);
            float vx,vy,vw,vh;
            if(diamondView&&world.Rows){float tw=isoWidth,rowH=RowPixels(tw);vx=camI-MapW/2/tw;vw=MapW/tw;vy=camJ-MapH/2/rowH;vh=MapH/rowH;}
            else if(diamondView){// The diagonal view covers a turned square of the grid; outline the grid rectangle around it.
                float lo=float.MaxValue,hi=float.MinValue,top=float.MaxValue,bottom=float.MinValue;foreach(var p in new[]{new Vector2(0,0),new Vector2(MapW,0),new Vector2(0,MapH),new Vector2(MapW,MapH)}){float tw=isoWidth,th=tw*PixelArt.DH/PixelArt.DW,a=(p.x-IsoCentreX)/(tw/2),b=(p.y-IsoCentreY)/(th/2),gi=camI+(a+b)/2,gj=camJ+(b-a)/2;lo=Mathf.Min(lo,gi);hi=Mathf.Max(hi,gi);top=Mathf.Min(top,gj);bottom=Mathf.Max(bottom,gj);}
                vx=lo;vy=top;vw=hi-lo;vh=bottom-top;}
            else{vx=mapScroll.x/tilePixels;vy=mapScroll.y/ClassicRowHeight();vw=MapW/tilePixels;vh=MapH/ClassicRowHeight();}
            vy-=ViewTop;float sx=w/world.Width,sy=h/ViewRows;var view=new Rect(drawn.x+vx*sx,drawn.y+vy*sy,vw*sx,vh*sy);
            GUI.BeginGroup(drawn);var local=new Rect(view.x-drawn.x,view.y-drawn.y,view.width,view.height);var edge=new Color(1,1,1,.9f);
            Fill(new Rect(local.x,local.y,local.width,1),edge);Fill(new Rect(local.x,local.yMax-1,local.width,1),edge);Fill(new Rect(local.x,local.y,1,local.height),edge);Fill(new Rect(local.xMax-1,local.y,1,local.height),edge);
            GUI.EndGroup();
            var e=Event.current;
            if((e.type==EventType.MouseDown||e.type==EventType.MouseDrag)&&e.button==0&&drawn.Contains(e.mousePosition)&&GUI.enabled)
            {int tx=Mathf.Clamp((int)((e.mousePosition.x-drawn.x)/sx),0,world.Width-1),ty=ViewTop+Mathf.Clamp((int)((e.mousePosition.y-drawn.y)/sy),0,ViewRows-1);globeZoom=false;FocusTile(tx,ty);e.Use();}
            float bh=(MiniButtons.height-8)/3;
            if(GUI.Button(new Rect(MiniButtons.x,MiniButtons.y,MiniButtons.width,bh),"")){if(diamondView)IsoZoom(1);else ClassicZoom(8);}DrawIcon(PixelArt.IconZoomIn,new Rect(MiniButtons.x+4,MiniButtons.y+bh/2-12,24,24));
            if(GUI.Button(new Rect(MiniButtons.x,MiniButtons.y+bh+4,MiniButtons.width,bh),"")){if(diamondView)IsoZoom(-1);else ClassicZoom(-8);}DrawIcon(PixelArt.IconZoomOut,new Rect(MiniButtons.x+4,MiniButtons.y+bh*1.5f+4-12,24,24));
            var viewButton=new Rect(MiniButtons.x,MiniButtons.y+bh*2+8,MiniButtons.width,bh);
            if(GUI.Button(viewButton,""))ToggleView();DrawIcon(PixelArt.IconLayers,new Rect(MiniButtons.x+4,MiniButtons.y+bh*2.5f+8-12,24,24));
            // Hovering names the view the button switches to, beside the pointer (kept on screen to its left).
            if(viewButton.Contains(e.mousePosition)){string tip=world.Hex?"Classic view is not available on a hex map":diamondView?"Classic view (V)":"Pixel-art view (V)";var size=small.CalcSize(new GUIContent(tip));var tipRect=new Rect(e.mousePosition.x-size.x-26,e.mousePosition.y-size.y-10,size.x+12,size.y+6);Fill(tipRect,new Color(.05f,.08f,.16f,.95f));Fill(new Rect(tipRect.x,tipRect.yMax-1,tipRect.width,1),new Color(.85f,.68f,.3f));GUI.Label(new Rect(tipRect.x+6,tipRect.y+3,size.x,size.y),tip,small);}
        }
        // The selected unit or city: picture, name, health or growth, key figures and the main actions. "More" opens every
        // other action in a panel above the card.
        void DrawUnitCard()
        {
            var r=UnitCard;GUI.Box(r,"");var u=Selected();var c=u==null?world.cities.Find(a=>a.owner==0&&a.id==cityId):null;
            var picture=new Rect(r.x+10,r.y+10,128,118);Fill(new Rect(picture.x,picture.y,picture.width,picture.height*.55f),new Color(.42f,.6f,.78f));Fill(new Rect(picture.x,picture.y+picture.height*.55f,picture.width,picture.height*.45f),new Color(.32f,.5f,.24f));
            Fill(new Rect(picture.x,picture.y+picture.height*.55f,picture.width,2),new Color(.5f,.66f,.38f));
            float tx=r.x+150,tw=r.width-160;
            if(u!=null)
            {
                var art=UnitPicture(u);GUI.DrawTexture(new Rect(picture.x+4,picture.y+2,picture.width-8,picture.height-4),art,ScaleMode.ScaleToFit,true);
                FitLabel(new Rect(tx,r.y+8,tw-6,24),world.UnitName(u).ToUpper(),hudTitle);
                Bar(new Rect(tx,r.y+36,tw-6,8),u.hp/10f,u.hp>6?UiAccent:u.hp>3?new Color(.95f,.75f,.3f):new Color(.9f,.3f,.25f));
                DrawIcon(PixelArt.IconRoad,new Rect(tx,r.y+52,20,20));GUI.Label(new Rect(tx+26,r.y+52,120,22),"Movement",hudLabel);GUI.Label(new Rect(tx,r.y+52,tw-8,22),u.moves+" / "+world.MaxMoves(u),hudNumber);
                DrawIcon(PixelArt.IconMilitary,new Rect(tx,r.y+76,20,20));GUI.Label(new Rect(tx+26,r.y+76,120,22),"Strength",hudLabel);GUI.Label(new Rect(tx,r.y+76,tw-8,22),World.Attack[u.type]+" / "+World.Defence[u.type],hudNumber);
                // What the unit is doing, on its own line (owner request), with the time left for jobs and journeys; rank below.
                string doing=world.Activity(u);if(!u.unfed&&(u.job>0||u.hasOrder))doing+=" — "+world.Remaining(u);if(u.stack!=0)doing=world.StackOf(u).Count+" in stack: "+doing;
                FitLabel(new Rect(tx,r.y+97,tw-8,20),doing,new GUIStyle(hudLabel){fontSize=13,normal={textColor=UiGold}});
                FitLabel(new Rect(tx,r.y+117,tw-8,16),World.RankText(u)+world.GearText(u),hudSmall,8);
                DrawUnitActions(u,new Rect(r.x+10,r.y+136,r.width-20,56));
            }
            else if(c!=null)
            {
                GUI.DrawTexture(new Rect(picture.x,picture.y,picture.width,picture.height),SettlementTexture(CityVisualEra(c.owner),c.pop,SettlementFlags(c),c.id),ScaleMode.ScaleToFit,true);
                FitLabel(new Rect(tx,r.y+8,tw-6,24),c.name.ToUpper(),hudTitle);
                var y=world.Yield(c);GUI.Label(new Rect(tx,r.y+36,tw,22),"Size "+c.pop,hudLabel);
                DrawIcon(PixelArt.IconFood,new Rect(tx,r.y+60,20,20));GUI.Label(new Rect(tx+24,r.y+60,40,22),(y[0]>=0?"+":"")+y[0],hudLabel);
                DrawIcon(PixelArt.IconShields,new Rect(tx+70,r.y+60,20,20));GUI.Label(new Rect(tx+94,r.y+60,40,22),y[1].ToString(),hudLabel);
                DrawIcon(PixelArt.IconScience,new Rect(tx+140,r.y+60,20,20));GUI.Label(new Rect(tx+164,r.y+60,40,22),y[2].ToString(),hudLabel);
                int b=c.buildingProject>=0?c.buildingProject:c.build;int progress=c.buildingProject>=0?c.buildingProduction:c.production;
                GUI.Label(new Rect(tx,r.y+86,tw,22),b>=0?world.BuildNameFor(c.owner,b):"Nothing in production",hudSmall);if(b>=0)Bar(new Rect(tx,r.y+108,tw-6,6),progress/(float)Math.Max(1,world.CostFor(c,b)),UiAccent);
                var row=new Rect(r.x+10,r.y+136,r.width-20,56);
                if(HudButton(new Rect(row.x,row.y,row.width/2-4,row.height),PixelArt.IconCities,"Open city"))OpenCity(c);
                if(HudButton(new Rect(row.x+row.width/2+4,row.y,row.width/2-4,row.height),PixelArt.IconNext,"Next unit"))NextUnit();
            }
        }
        void DrawUnitActions(Unit u,Rect row)
        {
            var actions=new List<KeyValuePair<int,string>>();bool idle=GUI.enabled&&world.result=="";
            actions.Add(new KeyValuePair<int,string>(PixelArt.IconMove,"Move"));
            if(u.type==0)actions.Add(new KeyValuePair<int,string>(PixelArt.IconFound,"Found"));
            // Workers always show Road, Farm, Mine and Clear (woodland); a job the civilisation lacks the technology for is greyed
            // out (owner request). Fort and the rest are in More.
            if(u.type==1){actions.Add(new KeyValuePair<int,string>(PixelArt.IconRoad,"Road"));actions.Add(new KeyValuePair<int,string>(PixelArt.IconFood,"Farm"));actions.Add(new KeyValuePair<int,string>(PixelArt.IconShields,"Mine"));actions.Add(new KeyValuePair<int,string>(PixelArt.IconAxe,"Clear"));}
            if(u.type==World.PriestUnit)actions.Add(new KeyValuePair<int,string>(PixelArt.IconFaith,"Preach"));
            if(u.type==World.PriestUnit&&world.At(u.x,u.y)!=null&&world.At(u.x,u.y).gateLink>0&&!world.GateOpen(world.At(u.x,u.y)))actions.Add(new KeyValuePair<int,string>(PixelArt.IconRealms,"Unseal"));
            if(World.CanExplore(u.type))actions.Add(new KeyValuePair<int,string>(PixelArt.IconExplore,"Explore"));
            actions.Add(new KeyValuePair<int,string>(PixelArt.IconSkip,"Skip"));
            if(World.Power[u.type]>0&&!World.IsNaval(u.type))actions.Add(new KeyValuePair<int,string>(PixelArt.IconFortify,u.fortified?"Wake":"Fortify"));
            else if(u.type!=1)actions.Add(new KeyValuePair<int,string>(PixelArt.IconSentry,u.sentry?"Wake":"Sentry"));
            bool canStack=!World.IsNaval(u.type)&&world.UnitsToStack(u).Count>1;if(canStack&&u.type!=1)actions.Insert(1,new KeyValuePair<int,string>(PixelArt.IconMilitary,u.stack!=0?"Unstack":"Stack"));
            // On a tile of mixed units, one click gathers every unit of this kind (owner request).
            bool sameKind=canStack&&u.type!=1&&world.SameTypeHere(u)>=2&&world.UnitsToStack(u).Count>world.SameTypeHere(u);if(sameKind)actions.Insert(2,new KeyValuePair<int,string>(PixelArt.IconCities,"All "+world.SameTypeHere(u)));
            actions=actions.Take(u.type==1?5:sameKind?6:canStack?5:4).ToList();actions.Add(new KeyValuePair<int,string>(PixelArt.IconMore,moreActions?"Less":"More"));
            float w=(row.width-(actions.Count-1)*6)/actions.Count;
            for(int i=0;i<actions.Count;i++)
            {
                var a=actions[i];bool active=a.Value=="Explore"&&u.exploring||a.Value=="Wake"||a.Value=="Less"||a.Value=="Move"&&moveMode||a.Value=="Road"&&roadMode||a.Value=="Unstack";
                int barJob=a.Value=="Road"?1:a.Value=="Farm"?2:a.Value=="Mine"?3:a.Value=="Clear"?5:0;
                bool enabled=GUI.enabled;GUI.enabled=enabled&&(idle||a.Key==PixelArt.IconMore)&&(barJob==0||world.JobTechMissing(u.owner,barJob)=="");
                if(HudButton(new Rect(row.x+i*(w+6),row.y,w,row.height),a.Key,a.Value,active))
                {
                    switch(a.Value)
                    {
                        case "Move":moveMode=!moveMode;status=moveMode?"Click a destination for the "+world.UnitName(u)+" (right-click cancels).":"";break;
                        case "Found":Found();break;
                        case "Stack":status=world.JoinStack(u)?"These units now move together. Soldiers protect any civilians with them.":"";break;
                        case "Unstack":world.LeaveStack(u);status="Stack broken up.";break;
                        case var all when all.StartsWith("All "):world.JoinType(u);status="All the "+World.UnitNames[u.type]+" units here now move together.";break;
                        case "Road":RoadKey(u);break;
                        case "Farm":case "Mine":case "Clear":{int job=a.Value=="Farm"?2:a.Value=="Clear"?5:3;string reason=world.WorkerTeam(u).Select(m=>world.JobReason(m,job)).FirstOrDefault(s=>s!="")??"";if(!world.StartJob(u,job))status=reason;break;}
                        case "Preach":if(!world.Preach(u))status=world.PreachReason(u);break;
                        case "Unseal":{var gateTile=world.At(u.x,u.y);string why=world.RiteReason(u);if(why!=""||!world.PerformRite(u))status=why;else{status="The rite is done: the gateway opens.";StartGateScene(gateTile);}break;}
                        case "Explore":if(u.exploring)world.CancelActivity(u);else world.AutoExplore(u);break;
                        case "Skip":world.SkipTurn(u);status=world.UnitName(u)+" skips this turn.";break;
                        case "Fortify":case "Wake":if(World.Power[u.type]>0&&!World.IsNaval(u.type)){bool fortify=!u.fortified;world.CancelActivity(u);u.fortified=fortify;}else{bool watch=!u.sentry;world.CancelActivity(u);u.sentry=watch;}break;
                        case "Sentry":world.CancelActivity(u);u.sentry=true;break;
                        default:moreActions=!moreActions;break;
                    }
                }
                GUI.enabled=enabled;
            }
        }
        // Every other action of the selected unit or city (the former side panel), in a framed panel above the card.
        void DrawMoreActions()
        {
            GUI.Box(MorePanel,"");GUILayout.BeginArea(new Rect(MorePanel.x+14,MorePanel.y+12,MorePanel.width-28,MorePanel.height-24));sideScroll=GUILayout.BeginScrollView(sideScroll);
            var c=world.cities.Find(a=>a.owner==0&&a.id==cityId);var u=Selected();
            if(u!=null)
            {
                GUILayout.Label("All orders",heading);
                bool enabled=GUI.enabled;GUI.enabled=enabled&&world.result=="";
                // Auto develop: the worker picks its own jobs each turn (roads, farms, mines, clearing) until told otherwise.
                if(u.type==1&&Button(u.autoWork?"Stop auto develop":"Auto develop")){bool on=!u.autoWork;world.SetAutoWork(u,on);status=on?"Worker set to auto develop.":"Auto develop stopped.";}
                if(u.type==1&&Button("Road to a destination [R, then right-drag]"))RoadKey(u);
                // Every worker activity stays listed; those needing an unknown technology are greyed out and say which (owner request).
                if(u.type==1){GUILayout.Label("Worker activities",body);int team=world.WorkerTeam(u).Count;foreach(int job in new[]{1,2,3,5,4}){string missing=world.JobTechMissing(u.owner,job);bool was=GUI.enabled;GUI.enabled=was&&missing=="";string label=World.JobNames[job]+(missing!=""?" — "+missing:" ("+((world.JobWork(u,job)+team-1)/team)+" turns)");if(Button(label)){string reason=world.WorkerTeam(u).Select(a=>world.JobReason(a,job)).FirstOrDefault(r=>r!="")??"";if(!world.StartJob(u,job))status=reason;}GUI.enabled=was;}}
                if(u.hasOrder){GUILayout.Label("Destination: "+u.destinationX+", "+u.destinationY,body);if(Button("Cancel movement order [X]"))world.CancelActivity(u);}
                GUI.enabled=enabled;
            }
            DrawFoundationPanel(u);
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
        // The tile under the pointer (or under the selected unit): terrain, river, owner and yields.
        float tileCardGrow;GUIStyle tileCardText;
        void DrawTileCard()
        {
            tileCardGrow=0;Tile t=null;int x,y;var mouse=Event.current.mousePosition;
            if(InMap(mouse)&&MapTileAt(mouse,out x,out y))t=world.KnownTile(world.At(x,y));
            if(t==null){var u=Selected();if(u!=null)t=world.KnownTile(world.At(u.x,u.y));}
            // Units on the tile (owner request): a line each above the tile's own description, growing the card upwards.
            var present=t!=null&&t.seen?world.units.Where(a=>a.x==t.x&&a.y==t.y&&(a.owner==0||world.Visible(t.x,t.y))).OrderByDescending(a=>a.id==selected).ThenByDescending(a=>World.Power[a.type]).ToList():new List<Unit>();
            int unitLines=Math.Min(3,present.Count)+(present.Count>3?1:0);
            if(t==null||!t.seen){GUI.Box(TileCard,"");GUI.Label(new Rect(TileCard.x,TileCard.y+15,TileCard.width,22),t==null?"Point at the map to inspect a tile":"Unexplored",hudCentre);return;}
            var v=world.TileYield(t);bool known=t.resource>0&&world.ResourceKnown(0,t.resource);if(t.resource>0&&!known){v[0]-=World.ResourceFood[t.resource];v[1]-=World.ResourceShields[t.resource];v[2]-=World.ResourceTrade[t.resource];}
            var features=new List<string>{World.TerrainNames[t.terrain]+(World.TerrainDefence(t.terrain)>0?" (defence +"+World.TerrainDefence(t.terrain)+")":"")};if(t.road)features.Add(World.Forested(t.terrain)?"Trail":"Road");if(t.farm)features.Add("Farm");if(t.mine)features.Add("Mine");if(t.fort)features.Add("Fort");if(known)features.Add(World.ResourceNames[t.resource]);
            {var beast=world.wildlife&&world.Visible(t.x,t.y)?world.AnimalAt(t.x,t.y):null;if(beast!=null)features.Add(world.AnimalLabel(beast));}
            // Tribal villages (owner request): named on the card, with the civilisation they joined if any.
            {var ruinTile=world.At(t.x,t.y);if(ruinTile!=null&&ruinTile.ruin)features.Add(ruinTile.realm>0?"Relic site":"Ancient ruins");}
            // Ancient gateways name their world once you have been there.
            {var gateTile=world.At(t.x,t.y);if(gateTile!=null&&gateTile.gateLink>0){int far=world.GateTwin(gateTile).realm;features.Add(world.gatesLocked?world.GateName(gateTile):gateTile.realm>0?"Ancient gateway home":world.RealmDiscovered(0,far)?world.GateName(gateTile):"Ancient gateway (destination unknown)");}}
            {string story=world.StoryText(t.x,t.y);if(story!="")features.Add(story);}
            {var live=world.At(t.x,t.y);if(live!=null&&live.village)features.Add(live.villageOwner>=0?"Village (joined "+world.NationName(live.villageOwner)+")":"Tribal village");}
            // Border influence: how far a rival has swung this tile (it changes hands at 90%).
            {var live=world.At(t.x,t.y);if(live!=null&&live.swayCiv>=0&&live.sway>0)features.Add("Influence "+live.sway+"% to "+world.NationName(live.swayCiv));}
            // The box grows upwards until every word shows: wrapped description lines first, then a line for each unit (owner request).
            if(tileCardText==null)tileCardText=new GUIStyle(hudCentre){wordWrap=true,clipping=TextClipping.Overflow};
            string described=string.Join("  •  ",features.ToArray());float wrapped=tileCardText.CalcHeight(new GUIContent(described),TileCard.width-16),extra=Mathf.Max(0,wrapped-20);tileCardGrow=extra+unitLines*19;
            {var grown=new Rect(TileCard.x,TileCard.y-tileCardGrow,TileCard.width,TileCard.height+tileCardGrow);
             GUI.Box(grown,"");GUI.Label(new Rect(TileCard.x+8,TileCard.y+4-extra,TileCard.width-16,20+extra),described,tileCardText);
             float ly=grown.y+4;foreach(var a in present.Take(3)){string line=(a.owner==0?"":world.NationName(a.owner)+" ")+world.UnitName(a)+"  •  Attack "+World.Attack[a.type]+" / Defence "+World.Defence[a.type]+"  •  Health "+a.hp+"/10"+(a.owner==0?"  •  "+world.Activity(a):"");var ink=new GUIStyle(hudCentre);ink.normal.textColor=a.owner==0?UiGold:nations[a.owner];FitLabel(new Rect(grown.x+6,ly,grown.width-12,18),line,ink,8);ly+=19;}
             if(present.Count>3)GUI.Label(new Rect(grown.x,ly,grown.width,18),"and "+(present.Count-3)+" more",hudCentre);}
            float cx=TileCard.center.x-96;
            DrawIcon(PixelArt.IconFood,new Rect(cx,TileCard.y+26,20,20));GUI.Label(new Rect(cx+22,TileCard.y+26,40,20),"+"+v[0],hudLabel);
            DrawIcon(PixelArt.IconShields,new Rect(cx+64,TileCard.y+26,20,20));GUI.Label(new Rect(cx+86,TileCard.y+26,40,20),"+"+v[1],hudLabel);
            DrawIcon(PixelArt.IconTrade,new Rect(cx+128,TileCard.y+26,20,20));GUI.Label(new Rect(cx+150,TileCard.y+26,40,20),"+"+v[2],hudLabel);
        }
        // City name plates on the map: framed navy plate, crown for a capital, name and size; your own cities also show what
        // they are building and how far it has come.
        GUIStyle plateLine,plateMaking;
        string CityMakingLine(City c){bool u=c.build>=0,b=c.buildingProject>=0;return !u&&!b?"Nothing in production":(u?world.BuildNameFor(c.owner,c.build):"")+(u&&b?"  •  ":"")+(b?world.BuildNameFor(c.owner,c.buildingProject):"");}
        void DrawCityPlate(City c,string name,Vector2 top,Color ink)
        {
            // Owner request (31 Oct): a small see-through plate, name and size on one line plus a thin production bar for
            // your cities; what is being built shows when the pointer is over the plate. Drawn in map order, so units on
            // the tiles in front stay on top of it.
            HudStyles();if(plateLine==null){plateLine=new GUIStyle(hudCaption){fontSize=11,wordWrap=false,clipping=TextClipping.Clip,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(0,0,0,0)};}
            bool own=c!=null&&c.owner==0;string label=name.ToUpper();bool capital=c!=null&&c.owner>=0&&world.nations[c.owner].capital==c.id;
            // The whole name always shows: the plate widens to fit it, and only very long names step the type down (owner request).
            var nameStyle=new GUIStyle(plateLine){clipping=TextClipping.Overflow};float lead=capital?33:22,tail=c!=null?26:6;float textW=nameStyle.CalcSize(new GUIContent(label)).x;
            while(nameStyle.fontSize>8&&lead+textW+tail>240){nameStyle.fontSize--;textW=nameStyle.CalcSize(new GUIContent(label)).x;}
            // What the city is making goes under its name, small, and the plate widens only as far as it must (owner request).
            string making=own?CityMakingLine(c):null;if(plateMaking==null)plateMaking=new GUIStyle(plateLine){fontSize=10,fontStyle=FontStyle.Normal,font=hudLabel.font,normal={textColor=UiText}};
            float w=Mathf.Max(70,lead+textW+tail);if(making!=null)w=Mathf.Min(240,Mathf.Max(w,plateMaking.CalcSize(new GUIContent(making)).x+12));float h=own?36:20;var r=new Rect(top.x-w/2,top.y,w,h);
            Fill(r,new Color(.04f,.06f,.12f,.72f));Fill(new Rect(r.x,r.y,2,r.height),ink);
            if(c!=null){var plateFlag=FlagTexture(c.owner);if(plateFlag!=null)GUI.DrawTexture(new Rect(r.x+4,r.y+5,15,10),plateFlag,ScaleMode.StretchToFill,true);}
            if(capital)DrawIcon(PixelArt.IconCrown,new Rect(r.x+20,r.y+4,12,12));
            GUI.Label(new Rect(r.x+lead,r.y+1,textW+4,19),label,nameStyle);
            if(c!=null)GUI.Label(new Rect(r.xMax-22,r.y+1,18,19),c.pop.ToString(),new GUIStyle(plateLine){alignment=TextAnchor.MiddleRight});
            if(own)
            {
                FitLabel(new Rect(r.x+6,r.y+18,w-10,14),making,plateMaking,7);
                int b=c.buildingProject>=0?c.buildingProject:c.build;int progress=c.buildingProject>=0?c.buildingProduction:c.production;
                if(b>=0)Bar(new Rect(r.x+4,r.yMax-3,w-8,2),progress/(float)Math.Max(1,world.CostFor(c,b)),UiAccent);
                if(r.Contains(Event.current.mousePosition)){int perTurn=b>=0?world.LineOutput(c,b):1;string line=b<0?"Nothing in production":world.BuildNameFor(c.owner,b)+" ("+Math.Max(1,(world.CostFor(c,b)-progress+perTurn-1)/perTurn)+" turns)";var tip=new Rect(r.x,r.yMax+2,Mathf.Max(w,hudSmall.CalcSize(new GUIContent(line)).x+12),18);Fill(tip,new Color(.04f,.06f,.12f,.92f));GUI.Label(new Rect(tip.x+6,tip.y+1,tip.width-8,16),line,new GUIStyle(hudSmall){wordWrap=false});}
            }
        }
    }
}
