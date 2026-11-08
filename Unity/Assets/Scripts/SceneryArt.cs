using System;
using System.Collections.Generic;

// Owner request, 8 Nov 2026: scenery and city pictures must sit seamlessly on the map. The supplied pictures (SpriteCook)
// each stand on their own painted diamond of ground, which showed as a square base around every tree, hill and city.
// StripGround removes that ground at load time and keeps the objects standing on it; the files stay unchanged.
// It also takes the fruit off grove trees, because red apples read as a food resource that is not there.
// Pixels are top-down 0xAARRGGBB, the layout PixelArt uses.
public static class SceneryArt
{
    static int R(uint p){return (int)(p>>16&255);}static int G(uint p){return (int)(p>>8&255);}static int B(uint p){return (int)(p&255);}static int A(uint p){return (int)(p>>24);}
    static int Bright(uint p){return R(p)+G(p)+B(p);}
    static int Diff(uint a,uint b){return Math.Abs(R(a)-R(b))+Math.Abs(G(a)-G(b))+Math.Abs(B(a)-B(b));}
    // Red or orange fruit: strongly red and clearly redder than it is green.
    public static bool IsFruit(uint p){return A(p)>=128&&R(p)>=140&&R(p)>G(p)+60&&R(p)>B(p)+50;}

    // Finds the ground diamond under the picture: as wide as the picture, half as tall, its lower tip on the lowest opaque
    // row. Returns false when the picture has no such base (nothing is removed then).
    public static bool FindBase(uint[] px,int w,int h,out float cx,out float cy,out float hw,out float hh)
    {
        cx=(w-1)/2f;hw=w/2f;hh=w/4f;cy=0;int yb=-1;
        for(int y=h-1;y>=0&&yb<0;y--)for(int x=0;x<w;x++)if(A(px[y*w+x])>=128){yb=y;break;}
        if(yb<0)return false;cy=yb-hh+.5f;
        // A diamond tip widens by about four pixels per row (64-wide picture): check the row a few above the tip.
        int probe=yb-(int)Math.Round(hh*.25f),first=-1,last=-1;if(probe<0)return false;
        for(int x=0;x<w;x++)if(A(px[probe*w+x])>=128){if(first<0)first=x;last=x;}
        if(first<0)return false;float span=last-first+1,expect=hw*.5f;
        return span>=expect*.45f&&span<=expect*1.7f&&Math.Abs((first+last)/2f-cx)<=w*.12f;
    }
    static float DiamondDist(int x,int y,float cx,float cy,float hw,float hh){return Math.Abs(x-cx)/hw+Math.Abs(y-cy)/hh;}

    public static uint[] StripGround(uint[] src,int w,int h,bool dropFruit)
    {
        var px=(uint[])src.Clone();
        if(dropFruit)RemoveFruit(px,w,h);
        float cx,cy,hw,hh;if(!FindBase(px,w,h,out cx,out cy,out hw,out hh))return px;
        // Ground colours: what lies along the diamond's lower edges and in its lower tip, where objects rarely stand.
        var counts=new Dictionary<uint,int>();
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            uint p=px[y*w+x];if(A(p)<128||Bright(p)<90)continue;float d=DiamondDist(x,y,cx,cy,hw,hh);
            bool tip=y>cy+hh*.6f&&d<=1.05f,edge=y>cy&&d>=.86f&&d<=1.06f;
            if(!tip&&!edge)continue;uint rgb=p&0xFFFFFFu;int n;counts.TryGetValue(rgb,out n);counts[rgb]=n+(tip?3:1);
        }
        var palette=new List<uint>();foreach(var kv in counts)if(kv.Value>=3)palette.Add(kv.Key);
        if(palette.Count==0)return px;
        Func<uint,bool> groundLike=p=>{if(A(p)<128||Bright(p)<90)return false;uint rgb=p&0xFFFFFFu;foreach(var c in palette)if(c==rgb||Diff(c,rgb)<=20)return true;return false;};
        // Flood from the diamond's edges through ground-coloured pixels; object outlines stop it.
        var ground=new bool[w*h];var queue=new Queue<int>();
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            float d=DiamondDist(x,y,cx,cy,hw,hh);if(d<.82f||d>1.08f)continue;int k=y*w+x;
            if(groundLike(px[k])){ground[k]=true;queue.Enqueue(k);}
        }
        while(queue.Count>0)
        {
            int k=queue.Dequeue(),x=k%w,y=k/w;
            for(int n=0;n<4;n++)
            {
                int nx=x+(n==0?1:n==1?-1:0),ny=y+(n==2?1:n==3?-1:0);if(nx<0||ny<0||nx>=w||ny>=h)continue;int q=ny*w+nx;
                if(ground[q]||DiamondDist(nx,ny,cx,cy,hw,hh)>1.08f||!groundLike(px[q]))continue;ground[q]=true;queue.Enqueue(q);
            }
        }
        for(int k=0;k<px.Length;k++)if(ground[k])px[k]=0;
        // The cut leaves tufts, pebbles and the base's own dark rim: small loose pieces inside the diamond are dropped.
        DropSpecks(px,w,h,cx,cy,hw,hh,Math.Max(10,w*w/320));
        // The base's thin dark rim survives the flood: a line pixel near the edge with few opaque neighbours is erased.
        for(int pass=0;pass<4;pass++)
        {
            var clear=new List<int>();
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                int k=y*w+x;uint p=px[k];if(A(p)<40)continue;float d=DiamondDist(x,y,cx,cy,hw,hh);if(d<.84f||d>1.12f)continue;
                int n=0;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if((dx!=0||dy!=0)&&nx>=0&&ny>=0&&nx<w&&ny<h&&A(px[ny*w+nx])>=40)n++;}
                if(n<=2)clear.Add(k);
            }
            if(clear.Count==0)break;foreach(int k in clear)px[k]=0;
        }
        DropSpecks(px,w,h,cx,cy,hw,hh,Math.Max(10,w*w/320));
        return px;
    }
    // Mountains, hills, volcanoes and glaciers stand on ground that is part of their shape, so only the hard outer edge of
    // the base is softened: alpha rises from nothing at the diamond's edge to full a few pixels inside.
    public static uint[] FeatherRim(uint[] src,int w,int h)
    {
        var px=(uint[])src.Clone();float cx,cy,hw,hh;if(!FindBase(px,w,h,out cx,out cy,out hw,out hh))return px;
        float band=Math.Max(2.5f,w/16f);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            int k=y*w+x;uint p=px[k];if(A(p)==0)continue;
            // Distance inside the diamond in pixels, measured vertically (the diamond is half as tall as it is wide).
            float inside=(1-DiamondDist(x,y,cx,cy,hw,hh))*hh;if(y<cy-hh*.2f||inside>=band)continue;
            float f=Math.Max(0,inside)/band;f=f*f*(3-2*f);px[k]=((uint)(A(p)*f)<<24)|(p&0xFFFFFFu);
        }
        return px;
    }
    // Which treatment a supplied picture gets, by its slot name; null leaves it as it is.
    public static bool IsLandform(string slot){return slot.StartsWith("hills")||slot.StartsWith("terrain-05")||slot.StartsWith("terrain-15")||slot.StartsWith("terrain-17");}
    public static bool IsScenery(string slot){return slot.StartsWith("terrain-")||slot.StartsWith("hills")||slot.StartsWith("hamlet-")||slot.StartsWith("city-");}
    public static uint[] Prepare(string slot,uint[] px,int w,int h){return IsLandform(slot)?FeatherRim(px,w,h):StripGround(px,w,h,slot.StartsWith("terrain-00"));}
    static void DropSpecks(uint[] px,int w,int h,float cx,float cy,float hw,float hh,int minSize)
    {
        var seen=new bool[w*h];var part=new List<int>();var stack=new Stack<int>();
        for(int s=0;s<px.Length;s++)
        {
            if(seen[s]||A(px[s])<40)continue;part.Clear();stack.Push(s);seen[s]=true;bool inBase=true;
            while(stack.Count>0)
            {
                int k=stack.Pop(),x=k%w,y=k/w;part.Add(k);if(DiamondDist(x,y,cx,cy,hw,hh)>1.1f)inBase=false;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int q=ny*w+nx;if(seen[q]||A(px[q])<40)continue;seen[q]=true;stack.Push(q);}
            }
            if(inBase&&part.Count<minSize)foreach(int k in part)px[k]=0;
        }
    }
    // Fruit pixels take the leaf colour around them.
    static void RemoveFruit(uint[] px,int w,int h)
    {
        var fruit=new bool[w*h];bool any=false;
        for(int k=0;k<px.Length;k++)if(IsFruit(px[k])){fruit[k]=true;any=true;}
        if(!any)return;
        for(int pass=0;pass<6;pass++)
        {
            bool left=false;var next=(uint[])px.Clone();
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                int k=y*w+x;if(!fruit[k])continue;int r=0,g=0,b=0,n=0;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h||dx==0&&dy==0)continue;int q=ny*w+nx;if(fruit[q]||A(px[q])<128||Bright(px[q])<70)continue;r+=R(px[q]);g+=G(px[q]);b+=B(px[q]);n++;}
                if(n==0){left=true;continue;}
                next[k]=(px[k]&0xFF000000u)|((uint)(r/n)<<16)|((uint)(g/n)<<8)|(uint)(b/n);fruit[k]=false;
            }
            Array.Copy(next,px,px.Length);if(!left)break;
        }
        for(int k=0;k<px.Length;k++)if(fruit[k])px[k]=(px[k]&0xFF000000u)|0x3E7A44u;
    }
}
