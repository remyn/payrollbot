namespace ReckonTwo.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
        public string MessageTime { get; set; }
        public bool IsBot { get; set; }
    }
}