using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using TDSouth.ChamCong.Controllers;
using TDSouth.ChamCong.Services;

namespace TDSouth.ChamCong
{
    /// <summary>
    /// Điểm khởi chạy ứng dụng Web C# Hệ Thống Chấm Công (TDSouth Infrastructure)
    /// Có thể chạy độc lập qua C# HttpListener (cổng 8081) hoặc chạy trực tiếp trên IIS qua Default.aspx
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            int port = 8081;
            if (args != null && args.Length > 0)
            {
                int parsedPort;
                if (int.TryParse(args[0], out parsedPort) && parsedPort > 0)
                {
                    port = parsedPort;
                }
            }

            string urlPrefix = string.Format("http://localhost:{0}/", port);
            var service = new ChamCongService();
            var controller = new ChamCongController(service);

            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(urlPrefix);
                listener.Start();

                Console.WriteLine("================================================================");
                Console.WriteLine("  HỆ THỐNG CHẤM CÔNG NHÂN VIÊN (C# .NET) - TDSOUTH INFRASTRUCTURE");
                Console.WriteLine("================================================================");
                Console.WriteLine("  URL truy cập  : " + urlPrefix);
                Console.WriteLine("  Trạng thái DB : " + service.DbStatusMessage);
                Console.WriteLine("  Nhấn Ctrl+C để dừng dịch vụ Web.");
                Console.WriteLine("================================================================");

                while (listener.IsListening)
                {
                    try
                    {
                        HttpListenerContext context = listener.GetContext();
                        ProcessHttpRequest(context, controller);
                    }
                    catch (HttpListenerException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[ERROR] " + ex.Message);
                    }
                }
            }
        }

        private static void ProcessHttpRequest(HttpListenerContext context, ChamCongController controller)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string actionMsg = string.Empty;
            string searchQuery = request.QueryString["q"] ?? string.Empty;

            if (string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) && request.HasEntityBody)
            {
                using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                {
                    string rawBody = reader.ReadToEnd();
                    NameValueCollection form = HttpUtility.ParseQueryString(rawBody, Encoding.UTF8);
                    actionMsg = controller.HandlePostAction(form);
                }
            }

            string html = controller.RenderHtmlPage(actionMsg, searchQuery);
            byte[] buffer = Encoding.UTF8.GetBytes(html);

            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}

