using LitJson;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;


namespace v2rayN.ThirdApi;

public class SelectedUpdate : IRequestCommand
{
    public string Path()
    {
        return "/SelectedUpdate";
    }

    public async Task<ResponseCommand> Execute(Dictionary<string, object> paramMap)
    {
        ResponseCommand responseCommand = new ResponseCommand();
        responseCommand.code = 0;
        try
        {
            object list=null;
            if (paramMap.ContainsKey("list"))
            {
                list = paramMap["list"];
            }

            List<string> usedIndex = new();
            if (list != null)
            {
                usedIndex =  JsonMapper.ToObject<List<string>>(list.ToString());
            }

            ThirdApp.Instance.RandomProfileItem();

            var selected = ThirdApp.Instance.GetSelectedServer();
            if (selected != null)
            {
                responseCommand.data = selected.port.ToString();
            }

        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }

        return responseCommand;
    }

    

}