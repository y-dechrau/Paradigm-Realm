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
    // Red fruit, dark or bright: clearly red rather than the brown of bark (red well above both green and blue).
    public static bool IsFruit(uint p){return A(p)>=128&&R(p)>=90&&R(p)*10>G(p)*18&&R(p)*10>B(p)*15;}
    // The pale highlight on a fruit: bright, warm, next to fruit.
    static bool FruitShine(uint p){return A(p)>=128&&R(p)>=200&&R(p)>G(p)+40&&R(p)>B(p)+60;}

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

    // Along the back edges only ground-coloured or pale rim pixels go, so trees standing at the back keep their trunks.
    static bool RimLike(uint p,List<uint> palette){if(A(p)<40)return false;if(Bright(p)>=540)return true;uint rgb=p&0xFFFFFFu;foreach(var c in palette)if(Diff(c,rgb)<=60)return true;return false;}
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
        // No straight edge may survive (owner request, 9 Nov 2026): a thin band along the diamond's outline is cleared outright.
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){float d=DiamondDist(x,y,cx,cy,hw,hh);if(d<=1.2f&&(y>=cy&&d>=.97f||d>=.93f&&RimLike(px[y*w+x],palette)))px[y*w+x]=0;}
        ThinLines(px,w,h);
        DropSpecks(px,w,h,cx,cy,hw,hh,Math.Max(10,w*w/320));
        return px;
    }
    // Mountains, hills, volcanoes and glaciers stand on ground that is part of their shape, so only the hard outer edge of
    // the base is softened: alpha rises from nothing at the diamond's edge to full a few pixels inside.
    public static uint[] FeatherRim(uint[] src,int w,int h)
    {
        var px=(uint[])src.Clone();float cx,cy,hw,hh;if(!FindBase(px,w,h,out cx,out cy,out hw,out hh))return px;
        float band=Math.Max(3f,w/10f);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            int k=y*w+x;uint p=px[k];if(A(p)==0)continue;
            // Distance inside the diamond in pixels, measured vertically (the diamond is half as tall as it is wide).
            float jitter=(((x*73856093)^(y*19349663))&7)/7f*2f-1f;float inside=(1-DiamondDist(x,y,cx,cy,hw,hh))*hh+jitter*1.5f;if(y<cy-hh*.2f||inside>=band)continue;
            float f=Math.Max(0,inside)/band;f=f*f*(3-2*f);px[k]=((uint)(A(p)*f)<<24)|(p&0xFFFFFFu);
        }
        return px;
    }
    // Hills (owner report, 10 Nov 2026: "the squaring of the hills at the base ... is not realistic"). The supplied pictures stand on
    // a diamond of ground, so every hill ended in four straight edges and corners. A real hill has no edge: below the convex crest
    // and the steeper backslope comes a concave footslope, where soil washed and crept from above (colluvium) thins out onto the
    // plain in a rounded, uneven skirt (the summit, shoulder, backslope, footslope and toeslope of the standard hillslope model).
    // Here the diamond is cut to a rounded footprint with a few shallow lobes, and the foot fades out over a wide band, slowly at
    // first and then quickly, so the ground of the tile shows through more and more toward the edge and no corner is left.
    public static uint[] RoundFoot(uint[] src,int w,int h)
    {
        var px=(uint[])src.Clone();float cx,cy,hw,hh;if(!FindBase(px,w,h,out cx,out cy,out hw,out hh))return px;
        // Each picture gets its own lobes, from its own pixels, so the three hill pictures do not share one outline.
        int seed=0;for(int k=0;k<px.Length;k+=97)seed=seed*31+(int)(px[k]&0xFFFFu);double p1=(seed&255)/255.0*6.283,p2=(seed>>8&255)/255.0*6.283,p3=(seed>>16&255)/255.0*6.283;
        float band=Math.Max(5f,w/8f),reach=hh*1.4f;
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            int k=y*w+x;uint p=px[k];if(A(p)==0)continue;
            double u=(x-cx)/hw,v=(y-cy)/hh;
            // The back edges of the old diamond, where no hill stands in front of them: a strip along the edge with open air a few
            // rows above it is bare ground, and goes. Where the hill rises through the edge it is left alone.
            if(v<-.05){float d=DiamondDist(x,y,cx,cy,hw,hh);if(d>=.8f&&d<=1.08f){bool body=false;for(int up=3;up<=5&&!body;up++)if(y-up>=0&&A(src[(y-up)*w+x])>=128&&DiamondDist(x,y-up,cx,cy,hw,hh)>1.08f)body=true;if(!body){float e=(d-.8f)/.16f;if(e>1)e=1;px[k]=((uint)(A(p)*(1-e)*(1-e))<<24)|(p&0xFFFFFFu);}}}
            if(v<-.4)continue;   // the body of the hill, above its base
            p=px[k];if(A(p)==0)continue;
            double angle=Math.Atan2(v,u),edge=.69+.07*Math.Sin(3*angle+p1)+.045*Math.Sin(5*angle+p2)+.03*Math.Sin(9*angle+p3);
            float jitter=(((x*73856093)^(y*19349663))&7)/7f*2f-1f;
            double inside=(edge-Math.Sqrt(u*u+v*v))*reach+jitter*1.2;if(inside>=band)continue;
            // The back of the base is hidden by the hill itself: the cut eases in over the rows just above the centre line.
            double keep=v<-.1?(-.1-v)/.3:0;
            // The dark line the artist drew round the base goes first, so no outline of the old diamond is left in the fade.
            bool rim=Bright(p)<150&&DiamondDist(x,y,cx,cy,hw,hh)>=.86f&&v>-.1;
            double f=Math.Max(0,inside)/band;f=f*f*(3-2*f);f*=f;if(rim)f*=.25;f=f+(1-f)*keep;px[k]=((uint)(A(p)*Math.Min(1,f))<<24)|(p&0xFFFFFFu);
        }
        return px;
    }
    // Which treatment a supplied picture gets, by its slot name; null leaves it as it is.
    // Cities and farmsteads keep their ground (their walls stand on its edge) and only lose the hard rim, like landforms.
    // Only open scenery (grassland groves, savanna, bog) is cut out of its ground; dense forest, jungle, rocks, mountains, cities and
    // farmsteads keep their ground, faded softly at the edge, because their trees and walls share colours with it (9 Nov 2026).
    public static bool IsLandform(string slot){return !(slot.StartsWith("terrain-00")||slot.StartsWith("terrain-16")||slot.StartsWith("terrain-09"));}
    public static bool IsScenery(string slot){return slot.StartsWith("field-")||slot.StartsWith("improve-")||slot.StartsWith("terrain-")||slot.StartsWith("hills")||slot.StartsWith("hamlet-")||slot.StartsWith("city-");}
    public static uint[] Prepare(string slot,uint[] px,int w,int h){return slot.StartsWith("hills")?RoundFoot(px,w,h):IsLandform(slot)?FeatherRim(px,w,h):StripGround(px,w,h,slot.StartsWith("terrain-00"));}
    // Thin dark lines left from the old base (1-2 pixels wide) go; outlines of solid shapes stay, having more of themselves around.
    static void ThinLines(uint[] px,int w,int h)
    {
        for(int pass=0;pass<3;pass++){var clear=new List<int>();
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){int k=y*w+x;uint p=px[k];if(A(p)<40||Bright(p)>=300)continue;int n=0;
                for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){int nx=x+dx,ny=y+dy;if(nx>=0&&ny>=0&&nx<w&&ny<h&&A(px[ny*w+nx])>=40)n++;}
                if(n<=9)clear.Add(k);}
            if(clear.Count==0)break;foreach(int k in clear)px[k]=0;}
    }
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
        // Grow the fruit twice over its dark rim and pale shine, so no red edge is left behind.
        for(int pass=0;pass<2;pass++){var add=new List<int>();
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){int k=y*w+x;uint p=px[k];if(fruit[k]||A(p)<128)continue;bool warm=R(p)>G(p)+20&&R(p)>B(p)+10||FruitShine(p);if(!warm)continue;
                for(int d=0;d<4;d++){int nx=x+(d==0?1:d==1?-1:0),ny=y+(d==2?1:d==3?-1:0);if(nx>=0&&ny>=0&&nx<w&&ny<h&&fruit[ny*w+nx]){add.Add(k);break;}}}
            foreach(int k in add)fruit[k]=true;}
        // Each fruit pixel takes the colour of the nearest leaf.
        var result=(uint[])px.Clone();
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){int k=y*w+x;if(!fruit[k])continue;uint best=0xFF3E7A44u;int least=99;
            for(int dy=-5;dy<=5;dy++)for(int dx=-5;dx<=5;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int q=ny*w+nx;uint c=px[q];if(fruit[q]||A(c)<128||G(c)<=R(c)+10||Bright(c)<90)continue;int d=dx*dx+dy*dy;if(d<least){least=d;best=c;}}
            result[k]=(px[k]&0xFF000000u)|(best&0xFFFFFFu);}
        Array.Copy(result,px,px.Length);
    }
}
