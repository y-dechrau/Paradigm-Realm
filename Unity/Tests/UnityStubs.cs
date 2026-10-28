// Minimal API doubles for compilation and input simulation, not Unity rendering.
using System;
namespace UnityEngine {
public class Object { public static T Instantiate<T>(T value)=>value; public static T FindFirstObjectByType<T>() where T:class=>null;public static void Destroy(object o){} }
public class MonoBehaviour:Object{}
public class GameObject {public GameObject(string name){} public T AddComponent<T>() where T:new()=>new T();}
public class Camera {public CameraClearFlags clearFlags;public Color backgroundColor;public bool orthographic;}
public enum CameraClearFlags{SolidColor} public enum RuntimeInitializeLoadType{AfterSceneLoad}
public class RuntimeInitializeOnLoadMethodAttribute:Attribute{public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t){}}
public struct Color{public float r,g,b,a;public Color(float r,float g,float b,float a=1){this.r=r;this.g=g;this.b=b;this.a=a;}public static Color white=>new Color(1,1,1);public static Color green=>new Color(0,1,0);public static Color yellow=>new Color(1,1,0);public static Color gray=>new Color(.5f,.5f,.5f);public static Color black=>new Color(0,0,0);public static Color clear=>new Color(0,0,0,0);public static Color operator*(Color c,float f)=>new Color(c.r*f,c.g*f,c.b*f,c.a*f);}
public struct Vector2 {public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}public static Vector2 zero=>new Vector2();public float sqrMagnitude=>x*x+y*y;public static Vector2 operator+(Vector2 a,Vector2 b)=>new Vector2(a.x+b.x,a.y+b.y);public static Vector2 operator-(Vector2 a,Vector2 b)=>new Vector2(a.x-b.x,a.y-b.y);public static Vector2 operator*(Vector2 a,float f)=>new Vector2(a.x*f,a.y*f);}
public struct Vector3{public Vector3(float x,float y,float z){}}
public struct Quaternion{public static Quaternion identity=>new Quaternion();}
public struct Matrix4x4{public static Matrix4x4 TRS(Vector3 a,Quaternion b,Vector3 c)=>new Matrix4x4();}
public struct Rect{public float x,y,width,height;public Rect(float x,float y,float w,float h){this.x=x;this.y=y;width=w;height=h;}public float xMax=>x+width;public float yMax=>y+height;public bool Contains(Vector2 p)=>p.x>=x&&p.x<xMax&&p.y>=y&&p.y<yMax;}
public static class Mathf {public static float Min(float a,float b)=>Math.Min(a,b);public static float Max(float a,float b)=>Math.Max(a,b);public static int Clamp(int v,int a,int b)=>Math.Clamp(v,a,b);public static float Clamp(float v,float a,float b)=>Math.Clamp(v,a,b);public static float Clamp01(float v)=>Math.Clamp(v,0,1);public static int FloorToInt(float v)=>(int)Math.Floor(v);public static float Round(float v)=>(float)Math.Round(v);public static float Sin(float v)=>(float)Math.Sin(v);}
public enum TextureFormat{RGBA32}public enum FilterMode{Point}public enum TextureWrapMode{Clamp}public enum ScaleMode{StretchToFill,ScaleToFit,ScaleAndCrop}
public class Texture2D{public int width,height;Color[] data;public Texture2D(int w,int h,TextureFormat f,bool m){width=w;height=h;data=new Color[w*h];}public static Texture2D whiteTexture=>null;public FilterMode filterMode;public TextureWrapMode wrapMode;public Color GetPixel(int x,int y)=>data[y*width+x];public byte[] EncodeToPNG()=>new byte[0];public Color[] GetPixels()=>data;public void SetPixel(int x,int y,Color c){data[y*width+x]=c;}public void SetPixels(Color[] c){data=c;}public void Apply(){}}
public class RectOffset{public RectOffset(int a,int b,int c,int d){}}
public class TextAsset{public string text;}
public enum FontStyle{Bold}public enum TextAnchor{MiddleCenter}
public class GUIStyleState{public Texture2D background;public Color textColor;}
public class GUIStyle{public RectOffset border;public GUIStyleState hover=new GUIStyleState(),active=new GUIStyleState(),focused=new GUIStyleState(),onNormal=new GUIStyleState(),onHover=new GUIStyleState(),onActive=new GUIStyleState();public GUIStyle(){}public GUIStyle(GUIStyle o){}public int fontSize;public FontStyle fontStyle;public bool wordWrap;public TextAnchor alignment;public GUIStyleState normal=new GUIStyleState();public float CalcHeight(GUIContent c,float width)=>20;}
public class GUIContent{public GUIContent(string text){}}
public class GUISkin{public GUIStyle box=new GUIStyle(),textField=new GUIStyle(),textArea=new GUIStyle();public GUIStyle label=new GUIStyle(),button=new GUIStyle();}
public static class GUI{public static GUISkin skin=new GUISkin();public static Matrix4x4 matrix;public static Color color=Color.white;public static bool enabled=true;public static string ClickText="";public static System.Collections.Generic.List<string> Labels=new System.Collections.Generic.List<string>();static string focus="";public static string GetNameOfFocusedControl()=>focus;public static void FocusControl(string n){focus=n;}public static void SetNextControlName(string n){}public static void DrawTexture(Rect r,Texture2D t){}public static void DrawTexture(Rect r,Texture2D t,ScaleMode m){}public static void DrawTexture(Rect r,Texture2D t,ScaleMode m,bool a){}public static void DrawTexture(Rect r,Texture2D t,ScaleMode mode,bool alpha,float aspect,Color c,float border,float radius){}public static string TextField(Rect r,string s,int n)=>s;public static int SelectionGrid(Rect r,int n,string[] labels,int count)=>n;public static void Label(Rect r,string s,GUIStyle style){Labels.Add(s);}public static void Box(Rect r,string s){}public static bool Button(Rect r,string s,GUIStyle style){Labels.Add(s);if(enabled&&ClickText==s){ClickText="";return true;}return false;}public static Vector2 BeginScrollView(Rect r,Vector2 pos,Rect content)=>pos;public static void EndScrollView(){}}
public class GUILayoutOption{}
public static class GUILayout{public static GUILayoutOption Width(float w)=>new GUILayoutOption();public static void Label(string s){}public static void Label(Texture2D t,params GUILayoutOption[] o){}public static GUILayoutOption Height(float h)=>new GUILayoutOption();public static bool Button(string s,params GUILayoutOption[] o)=>GUI.Button(new Rect(),s,null);public static bool Button(string s,GUIStyle st,params GUILayoutOption[] o)=>GUI.Button(new Rect(),s,st);public static void BeginArea(Rect r){}public static void EndArea(){}public static void Label(string s,GUIStyle st){GUI.Labels.Add(s);}public static void Space(float n){}public static string TextField(string s,int n)=>s;public static int SelectionGrid(int n,string[] labels,int count)=>n;public static Vector2 BeginScrollView(Vector2 s,params GUILayoutOption[] o)=>s;public static void EndScrollView(){}public static void BeginHorizontal(){}public static void EndHorizontal(){}public static void FlexibleSpace(){}public static bool Toggle(bool b,string s)=>b;public static float HorizontalSlider(float v,float min,float max)=>v;}
public class ExitGUIException:Exception{}
public static class GUIUtility{public static void ExitGUI(){throw new ExitGUIException();}}
public enum EventType{KeyDown,MouseDown,MouseUp,MouseDrag,ScrollWheel,Used,Repaint,Layout}
public enum KeyCode{W,S,A,D,F1,Tab,Return,B,LeftArrow,RightArrow,UpArrow,DownArrow,X,Escape,Space,KeypadEnter}
public class Event{public static Event current=new Event();public EventType type;public KeyCode keyCode;public Vector2 mousePosition,delta;public int button,clickCount;public bool control,alt,command;public void Use(){type=EventType.Used;}}
public static class Application{public static string persistentDataPath=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"paradigm-realm-harness-saves");public static int targetFrameRate;public static void Quit(){}}
public static class Screen{public static int width=1280,height=800;}
public static class Time{public static float realtimeSinceStartup;}
public static class PlayerPrefs{public static float GetFloat(string s,float v)=>v;public static int GetInt(string s,int v)=>v;public static void SetFloat(string s,float v){}public static void SetInt(string s,int v){}public static void Save(){}}
public static class JsonUtility{static System.Text.Json.JsonSerializerOptions options=new System.Text.Json.JsonSerializerOptions{IncludeFields=true};public static string ToJson(object o,bool pretty=false)=>System.Text.Json.JsonSerializer.Serialize(o,options);public static T FromJson<T>(string s)=>System.Text.Json.JsonSerializer.Deserialize<T>(s,options);}
}

namespace UnityEngine { public static class Resources {
 public static void UnloadAsset(object o){}
 public static T Load<T>(string path) where T:class {
  string root=System.IO.Directory.Exists("Assets/Resources")?"Assets/Resources/":"diplomacy/Assets/Resources/";string p=root+path;
  if(typeof(T)==typeof(TextAsset))return (System.IO.File.Exists(p+".txt")?new TextAsset{text=System.IO.File.ReadAllText(p+".txt")}:null) as T;
  if(!System.IO.File.Exists(p+".png"))return null;return new Texture2D(256,256,TextureFormat.RGBA32,false) as T;
 } } }
namespace UnityEditor {
 public class MenuItem: System.Attribute{public MenuItem(string s){}}
 public class EditorWindow:UnityEngine.Object{public static T GetWindow<T>(string s) where T:new()=>new T();public static void DestroyImmediate(object o){}}
 public static class EditorUtility{public static string SaveFilePanel(string a,string b,string c,string d)=>"";}
}
