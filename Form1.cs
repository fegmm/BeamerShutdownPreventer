using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net;

namespace BeamerShutdownPreventer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        public extern static bool ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string pwszReason);

        protected override void WndProc(ref Message aMessage)
        {
            const int WM_QUERYENDSESSION = 0x0011;
            const int WM_ENDSESSION = 0x0016;

            if ((aMessage.Msg == WM_QUERYENDSESSION || aMessage.Msg == WM_ENDSESSION))
            {
                if (IsBeamerOn().Result)
                {
                    return;
                }
            }
            base.WndProc(ref aMessage);
        }

        private async Task<bool> IsBeamerOn()
        {
            try
            {
                var client = new HttpClient(new HttpClientHandler() { Credentials = new NetworkCredential("EPSONWEB", "FegMM", "Web Control"), ServerCertificateCustomValidationCallback = (_, _, _, _) => true });
                var result = await client.PostAsync("https://192.168.1.31/cgi-bin/webconf", new FormUrlEncodedContent(new Dictionary<string, string>() { { "page", "70" } }));
                var resultString = await result.Content.ReadAsStringAsync();
                return resultString.Contains("\"laserstatus\": \"On\"");
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShutdownBlockReasonCreate(this.Handle, "BEAMER IST NOCH AN! - Dennoch herunterfahren?");
        }
    }
}
