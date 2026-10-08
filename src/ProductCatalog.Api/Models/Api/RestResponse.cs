namespace ProductCatalog.Api.Models.Api;


public class RestResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static RestResponse<T> CreateSuccessResponse(T data) => new()
    {
        Data = data,
        IsSuccess = true
    };

    public static RestResponse<T> CreateErrorResponse(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
