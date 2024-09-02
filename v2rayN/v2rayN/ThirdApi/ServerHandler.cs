using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using LitJson;
using v2rayN.Views;

namespace v2rayN.ThirdApi;

public class ServerHandler : SimpleChannelInboundHandler<IFullHttpRequest>
{
    static readonly AsciiString TypePlain = AsciiString.Cached("text/plain");
    static readonly AsciiString TypeJson = AsciiString.Cached("application/json");
    static readonly AsciiString TypeXForm = AsciiString.Cached("application/x-www-form-urlencoded");
    static readonly AsciiString ServerName = AsciiString.Cached("Netty");

    protected override async void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
    {
        // 在这里处理 HTTP 请求
        var uri = msg.Uri;
        var method = msg.Method;
        var contentType = msg.Headers.Get(HttpHeaderNames.ContentType, TypePlain);
        var content = msg.Content.ToString(Encoding.UTF8);

        string path = null;
        Dictionary<string, object> paramMap = null;

        if (method.Equals(HttpMethod.Post))
        {
            if (contentType != null && contentType.ToString().Contains(TypeJson.ToString()))
            {
                // 解析 JSON 内容
                paramMap = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            }
            else if (contentType != null && contentType.ToString().Contains(TypeXForm.ToString()))
            {
                // 解析表单内容
                paramMap = ParseFormData(content);
            }
            else
            {
                // 解析文本内容
                paramMap = new Dictionary<string, object> { { "text", content } };
            }

            path = uri;
        }
        else if (method.Equals(HttpMethod.Get))
        {
            // 解析查询参数
            var uriParts = uri.Split('?');
            path = uriParts[0];
            paramMap = uriParts.Length > 1 ? ParseFormData(uriParts[1]) : new Dictionary<string, object>();
        }

        // 打印字典内容（仅用于调试）
        if (paramMap != null)
        {
            foreach (var kvp in paramMap)
            {
                Debug.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }
        ResponseCommand responseContent = null;
        var handler = ThirdApp.Instance.ApiHttpServer.GetRequestCommand(path);
        if (handler != null)
        {
            responseContent = await handler.Execute(paramMap);
        }
        else
        {
            responseContent.code = 1;
            responseContent.message = "path not found";
        }

        var json = JsonMapper.ToJson(responseContent);
        byte[] respBytes = Encoding.UTF8.GetBytes(json);
        var respLength = respBytes.Length;
        var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, Unpooled.WrappedBuffer(respBytes), false);
        HttpHeaders headers = response.Headers;
        headers.Set(HttpHeaderNames.ContentType, TypeJson);
        headers.Set(HttpHeaderNames.Server, ServerName);
        headers.Set(HttpHeaderNames.ContentLength, AsciiString.Cached($"{respLength}"));
        headers.Set(HttpHeaderNames.Date, AsciiString.Cached(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()));

        await ctx.WriteAndFlushAsync(response);
        await ctx.CloseAsync();
    }

    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
    {
        Debug.WriteLine($"Exception: {e}");
        ctx.CloseAsync();
    }
    private Dictionary<string, object> ParseFormData(string formData)
    {
        var dict = new Dictionary<string, object>();
        var pairs = formData.Split('&');
        foreach (var pair in pairs)
        {
            var kvp = pair.Split('=');
            if (kvp.Length == 2)
            {
                dict[Uri.UnescapeDataString(kvp[0])] = Uri.UnescapeDataString(kvp[1]);
            }
        }
        return dict;
    }
}