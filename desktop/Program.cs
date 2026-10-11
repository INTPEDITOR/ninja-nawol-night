using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
class NawolApp : Form {
 WebView2 view = new WebView2();
 bool test; int testPhase=0; Dictionary<string,object> report=new Dictionary<string,object>();
 string baseDir=AppDomain.CurrentDomain.BaseDirectory;
 NawolApp(bool testing) {
  test=testing; Text="수련해라! 닌자 나월!";
  Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);
  StartPosition=FormStartPosition.CenterScreen;
  AutoScaleMode=AutoScaleMode.None;
  ClientSize=new Size(1920,1080);
  MinimumSize=new Size(640,400);
  MaximizeBox=true;
  BackColor=Color.FromArgb(6,12,23);
  view.DefaultBackgroundColor=Color.FromArgb(6,12,23);view.Dock=DockStyle.Fill; Controls.Add(view);
  if(test){Opacity=0;ShowInTaskbar=false;}
  Shown+=async (a,b)=>{
   try {
    string profile=test?Path.Combine(baseDir,"test-profile"):Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"NawolNight","Profile");
    var env=await CoreWebView2Environment.CreateAsync(null,profile,new CoreWebView2EnvironmentOptions("--autoplay-policy=no-user-gesture-required"));
    await view.EnsureCoreWebView2Async(env);
    await view.CoreWebView2.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.DiskCache);
    view.CoreWebView2.Settings.AreDefaultContextMenusEnabled=false;
    view.CoreWebView2.Settings.AreDevToolsEnabled=false;
    view.CoreWebView2.Settings.IsZoomControlEnabled=false;
    view.CoreWebView2.SetVirtualHostNameToFolderMapping("nawol.local",Path.Combine(baseDir,"game"),CoreWebView2HostResourceAccessKind.DenyCors);
    view.CoreWebView2.NewWindowRequested+=(sender,e)=>{e.Handled=true;if(e.Uri.StartsWith("https://nawol.local/"))view.CoreWebView2.Navigate(e.Uri);};
    view.CoreWebView2.WebMessageReceived+=(sender,e)=>{
     try{var msg=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(e.WebMessageAsJson);
      if((string)msg["type"]=="window"){
       bool max=Convert.ToBoolean(msg["maximized"]);
       WindowState=FormWindowState.Normal;
       int w=Convert.ToInt32(msg["width"]),h=Convert.ToInt32(msg["height"]);
       if(w>=640&&w<=3840&&h==w*9/16)ClientSize=new Size(w,h);
       if(max)WindowState=FormWindowState.Maximized;
      }
     }catch{}
    };
    view.CoreWebView2.NavigationCompleted+=async(sender,e)=>{
     if(!test)return;
     if(testPhase==0){testPhase=1;report["mainMusic"]=await view.CoreWebView2.ExecuteScriptAsync("JSON.stringify({exists:!!document.getElementById('menuBgm'),loop:document.getElementById('menuBgm').loop,playing:!document.getElementById('menuBgm').paused,source:document.getElementById('menuBgm').getAttribute('src')})");await view.CoreWebView2.ExecuteScriptAsync("document.getElementById('gameSound').click()");report["menuMute"]=await view.CoreWebView2.ExecuteScriptAsync("document.getElementById('menuBgm').paused");await view.CoreWebView2.ExecuteScriptAsync("document.getElementById('gameSound').click()");report["main"]=await view.CoreWebView2.ExecuteScriptAsync("JSON.stringify({title:document.title,start:!!document.getElementById('launch'),options:!!document.getElementById('settingsDialog')})");await view.CoreWebView2.ExecuteScriptAsync("document.getElementById('launch').click()");return;}
     if(testPhase==1){testPhase=2;report["selection"]=await view.CoreWebView2.ExecuteScriptAsync("JSON.stringify({cards:document.querySelectorAll('.stage-card').length,characters:document.querySelectorAll('.menu-character').length,menuMusicAbsent:!document.getElementById('menuBgm'),actionsOverlap:(()=>{const a=document.getElementById('returnMain').getBoundingClientRect(),b=document.getElementById('settingsButton').getBoundingClientRect();return a.left<b.right&&a.right>b.left&&a.top<b.bottom&&a.bottom>b.top})()})");await view.CoreWebView2.ExecuteScriptAsync("document.querySelector('.stage-card:not(:disabled)').click();window.chrome.webview.postMessage({type:'window',width:1280,height:720,maximized:false})");
      var timer=new System.Windows.Forms.Timer();timer.Interval=350;timer.Tick+=async(ts,te)=>{timer.Stop();report["clientWidth"]=ClientSize.Width;report["clientHeight"]=ClientSize.Height;report["game"]=await view.CoreWebView2.ExecuteScriptAsync("JSON.stringify({state:NAWOL.state,aspect:document.getElementById('game').getBoundingClientRect().width/document.getElementById('game').getBoundingClientRect().height,optionsHidden:document.getElementById('settingsButton').hidden})");File.WriteAllText(Path.Combine(baseDir,"self-test.json"),new JavaScriptSerializer().Serialize(report));Close();};timer.Start();
     }
    };
    view.CoreWebView2.Navigate("https://nawol.local/index.html");
   }catch(Exception ex){if(test){File.WriteAllText(Path.Combine(baseDir,"self-test-error.txt"),ex.ToString());Close();}else MessageBox.Show("게임을 실행하려면 Microsoft Edge WebView2 Runtime이 필요합니다.\n"+ex.Message,"닌자 나월");}
  };
 }
 [STAThread] static void Main(string[] args){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new NawolApp(Array.IndexOf(args,"--self-test")>=0));}
}