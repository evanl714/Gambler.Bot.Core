using DoormatCore.Helpers;
using DoormatCore.Sites;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.Code
{
    public class BrowserBypass:Form
    {
        WebView2 view2;
        
        public BrowserBypass(string site)
        {

            view2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)view2).BeginInit();
            SuspendLayout();
            // 
            // webView21
            // 
            view2.AllowExternalDrop = true;
            view2.CreationProperties = null;
            view2.DefaultBackgroundColor = Color.White;
            view2.Dock = DockStyle.Fill;
            view2.Location = new Point(0, 0);
            view2.Name = "webView21";
            view2.Size = new Size(800, 450);
            view2.Source = new Uri(site, UriKind.Absolute);
            view2.TabIndex = 0;
            view2.ZoomFactor = 1D;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(view2);
            Name = "Form1";
            Text = "Form1";
            this.Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)view2).EndInit();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeAsync();
        }
        private void InitializeAsync()
        {
            Debug.WriteLine("InitializeAsync");
            view2.EnsureCoreWebView2Async(null).Wait();
            Debug.WriteLine("WebView2 Runtime version: " + view2.CoreWebView2.Environment.BrowserVersionString);
            loaded = true;
        }
        public bool loaded = false;
        static string agent = "";
        async Task GetAgent()
        {
            if (string.IsNullOrWhiteSpace(agent))
            {
                agent = await view2.ExecuteScriptAsync("navigator.userAgent");
                if (agent == null)
                    return;
                if (agent.StartsWith("\\"))
                    agent = agent.Substring(1);
                if (agent.EndsWith("\\"))
                    agent = agent.Substring(0, agent.Length - 1);
                if (agent.StartsWith("\""))
                    agent = agent.Substring(1);
                if (agent.EndsWith("\""))
                    agent = agent.Substring(0, agent.Length - 1);
            }
        }
        public void nav (string url)
        {
            
        }
        static BrowserConfig _conf = null;

        internal async Task internalGetBypass(BypassRequiredArgs e)
        {

           
            if (InvokeRequired)
            {
                Invoke(new Action(async () => await internalGetBypass(e)));
            }
            else
            {
                var bc = new BrowserConfig();
                CookieContainer cs = new CookieContainer();
                try
                {

                   
                    await Task.Delay(5000);
                    

                    await GetAgent();
                    string result = agent;

                    var tmp = await view2.ExecuteScriptAsync("document.cookie");
                    
                   
                    object CookieMan = view2.GetType().GetProperty("CookieManager").GetValue(view2);
                    var method = CookieMan.GetType().GetMethod("GetCookiesAsync");//.Invoke(CookieMan, null);
                    var cookies = await (method.Invoke(CookieMan, new object[] { e.URL }) as Task<IList>);
                    foreach (object c in cookies as IList)
                    {
                        System.Net.Cookie svalue = (System.Net.Cookie)c.GetType().GetMethod("ToSystemNetCookie").Invoke(c, null);
                        cs.Add(svalue);
                    }
                    
                    bc.UserAgent = agent;
                    bc.Cookies = cs;
                    _conf = bc;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    //view2.IsVisible = false;
                }
                _conf = new BrowserConfig { Cookies = cs, UserAgent = agent };

            }


            /* wvBypass.ZIndex = -1;

             return new BrowserConfig { Cookies = wvBypass.Cookies, UserAgent = agent };*/
        }

        internal BrowserConfig GetBypass(BypassRequiredArgs e)
        {
            _conf = null;
            internalGetBypass(e);
            while (_conf == null) { Thread.Sleep(100); }
            return _conf;
        }
    }
}
