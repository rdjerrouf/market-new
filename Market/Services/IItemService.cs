using Market.DataAccess.Models;
using Market.DataAccess.Models.Dtos;
using Market.Market.DataAccess.Models;
using Market.Market.DataAccess.Models.Dtos;

namespace Market.Services
{
    public interface IItemService
    {
        Task<bool> AddItemAsync(Item item);
        Task<int?> AddItemAsync(int userId, CreateItemDto itemDto);
        Task<bool> UpdateItemAsync(int userId, int itemId, ItemUpdateDto updateDto);
        Task<bool> DeleteItemAsync(int id);
        Task<Item?> GetItemAsync(int id);
        Task<IEnumerable<Item>> GetItemsAsync();
        Task<IEnumerable<Item>> GetUserItemsAsync(int userId);
        Task<IEnumerable<Item>> GetItemsByUserAsync(int userId);
        Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm, string? category = null);
        Task<IEnumerable<Item>> SearchByStateAsync(AlState state);
        Task<IEnumerable<Item>> SearchByLocationAsync(double latitude, double longitude, double radiusKm);
        Task<IEnumerable<Item>> SearchByCategoryAndStateAsync(string category, AlState state);
        Task<bool> AddFavoriteAsync(int userId, int itemId);
        Task<bool> RemoveFavoriteAsync(int userId, int itemId);
        Task<bool> AddRatingAsync(int userId, int itemId, int score, string review);
        Task<IEnumerable<Item>> GetUserFavoriteItemsAsync(int userId);
        Task<IEnumerable<Rating>> GetUserRatingsAsync(int userId);
        Task<UserProfileStatistics> GetUserProfileStatisticsAsync(int userId);
        Task<bool> UpdateItemStatusAsync(int userId, int itemId, ItemStatus status);
        Task<bool> IsItemAvailableAsync(int itemId);
        Task<bool> AddItemPhotoAsync(int userId, int itemId, string photoUrl);
        Task<bool> RemoveItemPhotoAsync(int userId, int photoId);
        Task<bool> SetPrimaryPhotoAsync(int userId, int photoId);
        Task<IEnumerable<ItemPhoto>> GetItemPhotosAsync(int itemId);
        Task<IEnumerable<ItemPerformanceDto>> GetTopPerformingItemsAsync(int count);
        Task<bool> IncrementItemViewAsync(int itemId);
        Task<ItemStatistics?> GetItemStatisticsAsync(int itemId);
        Task<bool> RecordItemInquiryAsync(int itemId);
    }
}