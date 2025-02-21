using Market.Market.DataAccess.Models;

namespace Market.DataAccess.Models.Filters
{
    public class FilterCriteria
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string> Categories { get; set; } = [];
        public AlState? State { get; set; }
        public SortOption SortBy { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}