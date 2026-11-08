using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParadigmRealm
{
    // Painted interface art (owner request, 8 Nov 2026: "make the UI like the picture"). SpriteCook pictures made in the style of
    // the owner's Religion mockup live in Resources/UI: icon-<name> (128 x 128) replaces each drawn interface icon by its id,
    // logo is the compass-rose emblem, panel is the gold-cornered navy frame every GUI.Box uses, button is the gold-trimmed
    // menu face of the left menu and medallion is the round button of the bottom bar. Missing files fall back to the drawn art.
    public partial class RealmGame
    {
        static readonly string[] IconArtNames={"empire","research","diplomacy","cities","military","realms","victory","chronicle","identity",
            "gold","science","age","order","turn","menu","crown","food","shields","explore","skip","fortify","found","more","zoomin","zoomout","layers",
            "sentry","road","trade","help","next","moodgood","moodneutral","moodbad","move","faith","axe","civics"};
        readonly Dictionary<string,Texture2D> uiArt=new Dictionary<string,Texture2D>();
        Texture2D UiTex(string name){Texture2D t;if(!uiArt.TryGetValue(name,out t)){t=Resources.Load<Texture2D>("UI/"+name);if(t!=null)t.filterMode=FilterMode.Trilinear;uiArt[name]=t;}return t;}
        Texture2D UiIcon(int k){return k>=0&&k<IconArtNames.Length?UiTex("icon-"+IconArtNames[k]):null;}

        // The left menu face: a gold-trimmed navy bar stretched sideways, its pointed ends kept whole.
        void DrawButtonFace(Rect r,bool lit)
        {
            // Menu-sized bars use one ready-made picture (button-menu, 3 x 156 x 41): drawing three stretched pieces left a
            // visible seam running down every bar at some screen sizes (owner report, 8 Nov 2026).
            var whole=UiTex("button-menu");
            if(whole!=null&&Mathf.Abs(r.width/r.height-whole.width/(float)whole.height)<.6f){var keep=GUI.color;if(lit)GUI.color=new Color(1.25f,1.12f,.85f,keep.a);GUI.DrawTexture(r,whole,ScaleMode.StretchToFill,true);GUI.color=keep;return;}
            var t=UiTex("button");if(t==null){GUI.Box(r,"");return;}
            float endTex=40f/t.width,endW=Mathf.Min(r.width/2,r.height*40f/t.height);
            var old=GUI.color;if(lit)GUI.color=new Color(1.25f,1.12f,.85f,old.a);
            GUI.DrawTextureWithTexCoords(new Rect(r.x,r.y,endW,r.height),t,new Rect(0,0,endTex,1));
            GUI.DrawTextureWithTexCoords(new Rect(r.x+endW,r.y,r.width-2*endW,r.height),t,new Rect(endTex,0,1-2*endTex,1));
            GUI.DrawTextureWithTexCoords(new Rect(r.xMax-endW,r.y,endW,r.height),t,new Rect(1-endTex,0,endTex,1));
            GUI.color=old;
        }
        void DrawMedallion(Rect r,bool lit){var t=UiTex("medallion");if(t==null){GUI.Box(r,"");return;}var old=GUI.color;if(lit)GUI.color=new Color(1.25f,1.15f,.9f,old.a);GUI.DrawTexture(r,t,ScaleMode.ScaleToFit,true);GUI.color=old;}

        // Bottom bar of round medallions (after the mockup): quick ways into the main screens.
        static readonly string[] MedalScreens={"Economy","Research","Civics","Chronicle","Victory","Diplomacy","Military","Atlas","Religion"};
        static readonly string[] MedalTips={"Empire","Research","Civics","Chronicle","Victory","Diplomacy","Units: active and idle","Realms","Religion"};
        static readonly string[] MedalArt={"crown","research2","civics","chronicle","victory","diplomacy","military2","realms","faith"};
        const float MedalSize=46,MedalGap=50;
        static Rect MedalBar{get{float w=MedalScreens.Length*MedalGap;return new Rect(739-w/2,750,w,MedalSize);}}
        void DrawMedalBar()
        {
            var bar=MedalBar;var e=Event.current;string tip=null;
            for(int i=0;i<MedalScreens.Length;i++)
            {
                var r=new Rect(bar.x+i*MedalGap+(MedalGap-MedalSize)/2,bar.y,MedalSize,MedalSize);bool over=r.Contains(e.mousePosition);
                DrawMedallion(r,over);var art=UiTex("icon-"+MedalArt[i]);
                if(art!=null)GUI.DrawTexture(new Rect(r.x+9,r.y+9,r.width-18,r.height-18),art,ScaleMode.ScaleToFit,true);
                if(over)tip=MedalTips[i];
                if(GUI.Button(r,"",GUIStyle.none))OpenRailScreen(MedalScreens[i]);
            }
            if(tip!=null){var size=hudCentre.CalcSize(new GUIContent(tip));var tr=new Rect(e.mousePosition.x-size.x/2-6,bar.y-size.y-12,size.x+12,size.y+6);Fill(tr,new Color(.05f,.08f,.16f,.95f));Fill(new Rect(tr.x,tr.yMax-1,tr.width,1),UiGold);GUI.Label(tr,tip,hudCentre);}
        }
        void OpenRailScreen(string screen)
        {
            if(screen=="Research"){researchScreen=true;detailsScreen="";ResetMapGesture();GUI.FocusControl(null);GUIUtility.ExitGUI();}
            else OpenDetails(screen);
        }
    }
}
