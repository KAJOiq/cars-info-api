namespace ApiAppPay.Models
{
    public class ApiResponse<T>
    { 
        public bool IsSuccess { get; set; }
        public T Results { get; set; }
        public List<string> Errors { get; set; }
        public ApiResponse(bool isSuccess, T results, List<string> errors = null)
        {
            IsSuccess = isSuccess;
            Results = results;
            Errors = errors ?? new List<string>();
        }
    }
}
