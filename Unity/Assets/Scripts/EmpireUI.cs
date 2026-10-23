using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm {
public partial class RealmGame {
    int managedCity=-1;
    void DrawEmpireEconomy(){
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
            GUILayout.Label(a.name+" ↔ "+b.name+(world.RouteActive(r)?" — active":" — suspended by war or capture"),body);
        }
    }
    void DrawEmpireDiplomacy(int other){
        GUILayout.Label("Relations: "+world.nations[other].relations+" | Gifts improve willingness to share technology.",small);
        if(!world.war[other]){
            if(Button("Gift 25 gold to "+world.NationName(other)))status=world.GiftGold(other)?"Gift accepted.":"You need 25 gold.";
            foreach(int id in world.AvailableTech().Where(t=>world.nations[other].tech[t]).Take(10))
                if(Button("Buy "+World.TechNames[id]+" — "+world.TechPrice(id)+" gold"))status=world.BuyTechnology(other,id)?"Technology acquired.":"Requires enough gold and non-negative relations.";
        }
        GUILayout.Label(status,small);
    }
    void DrawCitizenManagement(){
        var c=world.cities.Find(a=>a.id==managedCity&&a.owner==0);if(c==null){GUILayout.Label("Select a city first.",body);return;}
        GUILayout.Label(c.name+" — population "+c.pop+" / growth limit "+world.GrowthLimit(c),heading);
        GUILayout.Label("Each specialist replaces one worked tile and still consumes food. Scientists: +3 science; merchants: +3 gold; entertainers: +2 happiness.",body);
        string[] names={"Scientists","Merchants","Entertainers"};int[] values={c.scientists,c.merchants,c.entertainers};
        for(int i=0;i<3;i++){GUILayout.Label(names[i]+": "+values[i],heading);GUILayout.BeginHorizontal();if(GUILayout.Button("- "+names[i]))world.AdjustSpecialist(c,i,-1);if(GUILayout.Button("+ "+names[i]))world.AdjustSpecialist(c,i,1);GUILayout.EndHorizontal();}
        var v=world.Yield(c);GUILayout.Label("Net food "+v[0]+" | Production "+v[1]+" | Science "+v[2]+" | Happiness "+world.Happiness(c),body);
        GUILayout.Label("Aqueduct raises growth limit from 6 to 12. Sewer system raises it to 20. Existing large cities are preserved.",small);
        if(Button("Return to "+c.name)){detailsScreen="";OpenCity(c);}
    }
    void DrawEmpireUnit(Unit u){
        if(u.type==3&&Button(u.exploring?"Stop auto-exploration":"Auto-explore nearby frontier")){if(u.exploring)world.CancelActivity(u);else world.AutoExplore(u);}
        if(Button(u.sentry?"Wake sentry":"Sentry until enemy approaches")){bool active=!u.sentry;world.CancelActivity(u);u.sentry=active;}
        if(u.type==11){
            GUILayout.Label("Caravans: reach a different city to establish trade, or contribute 40 production to a friendly wonder.",small);
            foreach(var c in world.cities.Where(c=>World.Distance(u.x,u.y,c.x,c.y)<=1&&(c.owner==0||world.Visible(c.x,c.y))).ToList()){
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
    void DrawAtlas(){
        GUILayout.Label("Known world — click a tile to centre the main map. Dark tiles are unexplored.",body);
        GUILayout.Space(490);
        for(int y=0;y<World.Height;y++)for(int x=0;x<World.Width;x++){
            var t=world.KnownTile(world.At(x,y));var r=new Rect(x*16,35+y*16,16,16);
            if(!t.seen){Fill(r,new Color(.025f,.04f,.06f));continue;}
            GUI.DrawTexture(r,terrain[t.terrain]);if(t.owner>=0)Fill(new Rect(r.x+5,r.y+5,6,6),nations[t.owner]);
        }
        var e=Event.current;if(e.type==EventType.MouseDown&&e.button==0&&new Rect(0,35,768,480).Contains(e.mousePosition)){
            int x=(int)(e.mousePosition.x/16),y=(int)((e.mousePosition.y-35)/16);
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
