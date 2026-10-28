using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
public partial class RealmGame {
    int managedCity=-1;
    void DrawEmpireEconomy(){
        GUILayout.Label("Diplomatic payments: +"+world.DiplomaticIncome(0)+" income / -"+world.DiplomaticExpense(0)+" expense per turn (included above). War weariness: -"+world.WarUnhappiness(0)+" happiness/city.",body);
        var n=world.nations[0];GUILayout.Space(12);GUILayout.Label("Government: "+World.Governments[n.government]+(n.revolution>0?" — revolution, "+n.revolution+" turns":""),heading);
        GUILayout.Label("Each government has different support and output. Changing causes two turns of half production/science and reduced happiness.",small);
        for(int i=0;i<World.Governments.Length;i++){
            bool old=GUI.enabled;GUI.enabled=old&&world.result==""&&i!=n.government&&n.revolution==0&&(i==0||world.HasTech(0,World.GovernmentTech[i]));
            if(Button("Adopt "+World.Governments[i]))world.ChangeGovernment(0,i);GUI.enabled=old;
            GUILayout.Label(World.GovernmentEffects[i]+(i>0?" | Requires "+World.TechNames[TechnologyCatalog.Index(World.GovernmentTech[i])]:""),small);
        }
        GUILayout.Label("Luxury spending: costs 1 gold/city for each level; +2 happiness/level",heading);
        n.luxury=GUILayout.SelectionGrid(n.luxury,new[]{"None","1 gold/city","2 gold/city"},3);
        GUILayout.Label("Trade income: "+world.TradeIncome(0)+" gold/turn",heading);
        foreach(var r in world.tradeRoutes.Where(r=>r.originOwner==0||r.destinationOwner==0)){
            var a=world.cities.Find(c=>c.id==r.origin);var b=world.cities.Find(c=>c.id==r.destination);
            GUILayout.Label(a.name+" ↔ "+b.name+(world.RouteActive(r)?" — active":" — suspended by war, embargo or capture"),body);
        }
    }
    void DrawEmpireDiplomacy(int other){
        GUILayout.Label("Relations: "+world.Opinion(other,0)+" | Gifts improve willingness to share technology.",small);
        if(!world.war[other]){
            if(Button("Gift 25 gold to "+world.NationName(other)))status=world.GiftGold(other)?"Gift accepted.":"You need 25 gold.";
            foreach(int id in world.AvailableTech().Where(t=>world.nations[other].tech[t]).Take(10))
                if(Button("Buy "+World.TechNames[id]+" — "+world.TechPrice(id)+" gold"))status=world.BuyTechnology(other,id)?"Technology acquired.":"Requires enough gold, non-negative opinion and no embargo.";
        }
        GUILayout.Label(status,small);
    }
    void DrawCitizenManagement(){
        var c=world.cities.Find(a=>a.id==managedCity&&a.owner==0);if(c==null){GUILayout.Label("Select a city first.",body);return;}
        GUILayout.Label(c.name+" — population "+c.pop+" / growth limit "+world.GrowthLimit(c),heading);
        GUILayout.Label("Each specialist replaces one worked tile and still consumes food. Scientists: +3 science; merchants: +3 gold; entertainers: +2 happiness.",body);
        string[] names={"Scientists","Merchants","Entertainers"};int[] values={c.scientists,c.merchants,c.entertainers};
        for(int i=0;i<3;i++){GUILayout.Label(names[i]+": "+values[i],heading);GUILayout.BeginHorizontal();if(GUILayout.Button("- "+names[i]))world.AdjustSpecialist(c,i,-1);if(GUILayout.Button("+ "+names[i]))world.AdjustSpecialist(c,i,1);GUILayout.EndHorizontal();}
        GUILayout.Label(world.CityEconomyReport(c),body);
        GUILayout.Label("Automatic workers try to feed the city before following its focus. Science focus adds 2 science and costs up to 2 production. Specialists can still cause a food deficit.",small);
        var v=world.Yield(c);GUILayout.Label("Net food "+v[0]+" | Production "+v[1]+" | Science "+v[2]+" | Happiness "+world.Happiness(c),body);
        GUILayout.Label("Early population limit: 8 beside fresh water, otherwise 6. Aqueduct: 12. Sewer: 20. River centres add 1 food; hill centres add 1 production. Existing large cities are preserved.",small);
        if(Button("Return to "+c.name)){detailsScreen="";OpenCity(c);}
    }
    void DrawEmpireUnit(Unit u){
        if(u.type==0)DrawSettlementUnit(u);
        if(u.type==3&&Button(u.exploring?"Stop auto-exploration":"Auto-explore nearby frontier")){if(u.exploring)world.CancelActivity(u);else world.AutoExplore(u);}
        if(Button(u.sentry?"Wake sentry":"Sentry until enemy approaches")){bool active=!u.sentry;world.CancelActivity(u);u.sentry=active;}
        if(u.type==11){
            GUILayout.Label("Caravans: reach a different city to establish trade, or contribute 40 production to a friendly wonder.",small);
            foreach(var c in world.cities.Where(c=>world.Distance(u.x,u.y,c.x,c.y)<=1&&(c.owner==0||world.Visible(c.x,c.y))).ToList()){
                string reason=world.TradeReason(u,c);if(Button("Trade with "+c.name)){status=world.DeliverCaravan(u,c)?"Trade route established.":reason;if(!world.units.Contains(u)){selected=-1;GUIUtility.ExitGUI();}}
                if(c.owner==0&&World.IsWonder(c.buildingProject)&&Button("Supply wonder in "+c.name)){status=world.HelpWonder(u,c)?"40 production supplied.":"Caravan needs movement.";if(!world.units.Contains(u)){selected=-1;GUIUtility.ExitGUI();}}
            }
        }
    }
    void DrawEmpireAdvisors(){
        if(Button("World atlas"))OpenDetails("Atlas");
        if(Button("Military adviser"))OpenDetails("Military");
        GUILayout.Label("World wonders",heading);
        foreach(int id in World.BuildingIds.Where(World.IsWonder)){
            var city=world.cities.Find(c=>world.HasBuilding(c,id));
            GUILayout.Label(World.BuildName(id)+": "+(city==null?"Not completed":city.owner==0||world.Visible(city.x,city.y)?city.name+" — "+world.NationName(city.owner):"Completed in a distant city"),body);
        }
    }
    void DrawMilitaryAdviser(){
        GUILayout.Label("Select a unit to centre the map. Upkeep depends on government support.",body);
        foreach(var u in world.units.Where(u=>u.owner==0).ToList())
            if(Button(World.UnitNames[u.type]+" #"+u.id+" | HP "+u.hp+" | "+world.Activity(u)+" | upkeep "+World.Upkeep[u.type])){selected=u.id;cityId=-1;detailsScreen="";cityScreen=false;Center();GUIUtility.ExitGUI();}
    }
    Texture2D atlasTexture;World atlasWorld;float atlasBuilt;readonly Color[] atlasColours=new Color[64];readonly bool[] atlasColourReady=new bool[64];
    Color AtlasColour(int id){
        id=Math.Max(0,Math.Min(atlasColours.Length-1,id));
        if(!atlasColourReady[id]){var px=terrain[id].GetPixels();float r=0,g=0,b=0;foreach(var c in px){r+=c.r;g+=c.g;b+=c.b;}int n=Math.Max(1,px.Length);atlasColours[id]=new Color(r/n,g/n,b/n,1);atlasColourReady[id]=true;}
        return atlasColours[id];
    }
    // Large worlds draw the known map as one small texture instead of thousands of rectangles.
    void BuildAtlasTexture(){
        if(atlasTexture!=null)Destroy(atlasTexture);
        atlasTexture=new Texture2D(world.Width,world.Height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        var pixels=new Color[world.Width*world.Height];
        foreach(var source in world.tiles){
            var t=world.KnownTile(source);var c=new Color(.025f,.04f,.06f,1);
            if(t.seen){c=AtlasColour(t.terrain);if(t.owner>=0){var n=nations[t.owner];c=new Color(c.r+(n.r-c.r)*.45f,c.g+(n.g-c.g)*.45f,c.b+(n.b-c.b)*.45f,1);}}
            pixels[(world.Height-1-t.y)*world.Width+t.x]=c;
        }
        atlasTexture.SetPixels(pixels);atlasTexture.Apply();atlasWorld=world;atlasBuilt=Time.realtimeSinceStartup;
    }
    void DrawAtlas(){
        GUILayout.Label("Known world — click a tile to centre the main map. Dark tiles are unexplored.",body);
        GUILayout.Space(490);
        float cell=Mathf.Min(768f/world.Width,480f/world.Height);
        var area=new Rect(0,35,world.Width*cell,world.Height*cell);
        if(world.tiles.Length<=4000){
            for(int y=0;y<world.Height;y++)for(int x=0;x<world.Width;x++){
                var t=world.KnownTile(world.At(x,y));var r=new Rect(x*cell,35+y*cell,cell,cell);
                if(!t.seen){Fill(r,new Color(.025f,.04f,.06f));continue;}
                GUI.DrawTexture(r,terrain[t.terrain]);if(t.owner>=0)Fill(new Rect(r.x+cell*.31f,r.y+cell*.31f,cell*.38f,cell*.38f),nations[t.owner]);
            }
        }else{
            if(atlasTexture==null||atlasWorld!=world||Time.realtimeSinceStartup-atlasBuilt>1f)BuildAtlasTexture();
            GUI.DrawTexture(area,atlasTexture,ScaleMode.StretchToFill,false);
        }
        var e=Event.current;if(e.type==EventType.MouseDown&&e.button==0&&area.Contains(e.mousePosition)){
            int x=Math.Max(0,Math.Min(world.Width-1,(int)(e.mousePosition.x/cell))),y=Math.Max(0,Math.Min(world.Height-1,(int)((e.mousePosition.y-35)/cell)));
            mapScroll=new Vector2(x*tilePixels-418,y*tilePixels-304);ClampMapScroll();detailsScreen="";cityScreen=false;e.Use();GUIUtility.ExitGUI();
        }
    }
    void DrawEmpireGlyph(Rect r,int id){
        Fill(r,new Color(.1f,.17f,.2f));float cell=r.width/8;
        for(int y=0;y<8;y++)for(int x=0;x<8;x++)if(y==7||x==0||x==7||y==1&&(id&(1<<x))!=0||y>=3&&y<=5&&x==(id+y)%6+1)
            Fill(new Rect(r.x+x*cell,r.y+y*cell,cell,cell),new Color(.85f,.72f,.35f));
    }
}
}
