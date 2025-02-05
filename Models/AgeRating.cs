

namespace Admin.Models
{
    public class AgeRating
    {
        public int Id { get; set; }
        public int Age { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is AgeRating other)
            {
                return this.Id == other.Id;
            }
            return false;
        }
    }
}