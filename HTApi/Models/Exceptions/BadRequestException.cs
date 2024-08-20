namespace HTApi.Models.Exceptions
{
    public class BadRequestException : Exception
    {
        public int StatusCode { get; set; }
        public BadRequestException(string message, int statusCode) : base(message)
        {
            this.StatusCode = statusCode;
        }
    }
}
