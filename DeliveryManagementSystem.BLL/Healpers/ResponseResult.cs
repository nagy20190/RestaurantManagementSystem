using System.Net;

public class ResponseResult<T>
{
    public HttpStatusCode _StatusCode { get; set; }
    public bool _IsSuccess { get; set; }
    public string _Message { get; set; }
    public object _Data { get; set; }
    public T Result { get; set; }
    public ResponseResult(bool isSuccess, string message, object data = null)
    {
        _IsSuccess = isSuccess;
        _Message = message;
        _Data = data;
    }
    public ResponseResult()
    {
    }
    public ResponseResult(bool isSuccess, string message)
    {
        _IsSuccess = isSuccess;
        _Message = message;
    }
    public ResponseResult(bool isSuccess, T data)
    {
        _IsSuccess = isSuccess;
        Result = data;
    }
}