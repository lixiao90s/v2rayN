namespace v2rayN.ThirdApi;

public interface IRequestCommand
{
    /// <summary>
    /// 地址
    /// </summary>
    /// <returns></returns>
    public string Path();
    /// <summary>
    /// 处理http请求的内容
    /// </summary>
    /// <param name="paramMap"></param>
    /// <returns></returns>
    public Task<ResponseCommand> Execute(Dictionary<string, object> paramMap);
}

public class ResponseCommand
{
    public int code;
    public string message;
    public string data;
}