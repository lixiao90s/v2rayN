using ReactiveUI;

namespace v2rayN.ThirdApi;

public sealed class ThirdApp
{
    private static readonly Lazy<ThirdApp> _lazyInstance =
        new Lazy<ThirdApp>(() => new ThirdApp());

    private ThirdApp() { }

    public static ThirdApp Instance
    {
        get
        {
            return _lazyInstance.Value;
        }
    }

    public ApiHttpServer ApiHttpServer { get; private set; }=new ApiHttpServer();

    public ProfilesViewModel ViewModel { get; set; }



    /// <summary>
    /// 随机服务器物品
    /// </summary>
    public void RandomProfileItem()
    {
        MessageBus.Current.SendMessage("","RandomServer");
    }

    public ProfileItemModel GetSelectedServer()
    {
        return ViewModel.SelectedProfile;
    }

    //public static async Task CheckRebot()
    //{
    //    using (HttpClient client = new HttpClient())
    //    {
    //        try
    //        {
    //            //await Task.Delay(TimeSpan.FromSeconds(5));
    //            client.Timeout = TimeSpan.FromSeconds(15);
    //            string requst = "https://webapi-pc.meitu.com/common/ip_location?ip=";
    //            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, requst));
    //            string result = await response.Content.ReadAsStringAsync();

    //            JsonData jsonData = JsonMapper.ToObject(result);
    //            JsonData data = jsonData["data"];
    //            string ip;
    //            if (data != null && data.Keys != null && data.Keys.Count > 0) ;
    //            {
    //                ip = data.Keys.ToList()[0];
    //            }
    //            //iP是否处于风险中
    //            if (ip == null)
    //            {
    //                return;
    //            }

    //            if (!String.IsNullOrEmpty(result) && result.Contains("重庆"))
    //            {
    //                MainWindow.httpServer.viewModel.RebootAsAdminCmd.Execute().Subscribe();
    //                return;
    //            }

    //            if (!string.IsNullOrEmpty(HttpServer.lastIP) && HttpServer.lastIP.Equals(ip))
    //            {

    //                MainWindow.httpServer.viewModel.RebootAsAdminCmd.Execute().Subscribe();
    //                return;
    //            }

    //            //记录IP
    //            HttpServer.lastIP = ip;
    //        }
    //        catch (Exception e)
    //        {
    //            MainWindow.httpServer.viewModel.RebootAsAdminCmd.Execute().Subscribe();
    //        }
    //    }
    //}
}