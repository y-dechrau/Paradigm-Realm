using System;
using System.Linq;
using UnityEngine;
namespace ParadigmRealm
{
    public partial class RealmGame
    {
        // One screen, three bars: how close each way of winning is.
        void DrawVictoryStatus()
        {
            float[] progress;var lines=world.VictoryText(out progress);
            GUILayout.Label(world.result==""?"Three ways to win. Whichever bar fills first ends the game.":world.result,heading);
            var colours=new[]{new Color(.9f,.45f,.35f),new Color(.45f,.7f,.95f),new Color(.5f,.85f,.5f)};
            string[] names={"Conquest","Transcendence","Concord"};
            for(int i=0;i<3;i++)
            {
                GUILayout.Space(12);GUILayout.Label(names[i],heading);
                var r=GUILayoutUtility.GetRect(900,26);
                Fill(r,UiTrough);Fill(new Rect(r.x+2,r.y+2,(r.width-4)*Mathf.Clamp01(progress[i]),r.height-4),colours[i]);
                GUI.Label(r,Mathf.RoundToInt(progress[i]*100)+"%",small);
                GUILayout.Label(lines[i],body);
            }
        }
    }
}
