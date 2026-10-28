namespace ParadigmRealm {
 // A fixed diagnostic map independent of the player's campaign and save files.
 public static class TerrainDemo {
  public const int Width=16,Height=12,Seed=8173;
  static readonly int[,] Map={
   {14,14,15,15,13,13,8,8,0,0,0,3,3,3,3,3},
   {14,15,15,5,13,8,8,0,0,0,0,20,3,3,3,3},
   {8,13,5,5,2,2,0,0,0,7,0,20,20,3,3,3},
   {13,1,5,2,2,1,1,0,0,7,7,7,20,20,3,3},
   {1,1,2,2,1,1,0,0,0,0,0,7,0,20,3,3},
   {1,1,1,0,0,0,7,7,7,7,7,7,7,7,6,6},
   {0,0,0,1,0,0,7,0,0,9,9,0,0,0,6,6},
   {0,0,1,1,0,0,7,0,9,9,11,11,16,16,10,10},
   {18,18,0,0,18,18,7,19,19,11,11,16,16,17,17,10},
   {4,4,18,18,4,4,4,4,19,19,16,16,17,17,17,10},
   {4,4,4,4,4,0,4,4,12,12,4,4,4,4,4,4},
   {4,4,4,4,4,4,4,4,12,12,4,4,4,4,4,4}
  };
  public static int At(int x,int y){x=System.Math.Max(0,System.Math.Min(Width-1,x));y=System.Math.Max(0,System.Math.Min(Height-1,y));return Map[y,x];}
 }
}
