namespace HTApi.Models
{
    public class ValidationResult
    {
        public bool IsValid {  get; set; }
        public List<string> Messages { get; set; } = [];


        public void Invalidate(string message)
        {
            this.IsValid = false;
            this.Messages.Add(message);
        }
    }
}
