using Market.DataAccess.Data;
using Market.DataAccess.Models;
using Market.DataAccess.Models.Dtos;
using Market.DataAccess.Models.Filters;
using Market.Market.DataAccess.Models;
using Market.Market.DataAccess.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Market.Services
{
    public class ItemService : IItemService
    {
        private readonly AppDbContext _context;

        public ItemService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            try
            {
                // Get the user before adding the item
                var user = await _context.Users.FindAsync(item.PostedByUserId);
                if (user == null) return false;

                // Set the required PostedByUser property
                item.PostedByUser = user;

                await _context.Items.AddAsync(item);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding item: {ex.Message}");
                return false;
            }
        }

        public async Task<int?> AddItemAsync(int userId, CreateItemDto itemDto)
        {
            try
            {
                // Get the user first
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return null;

                // Create the item with the required PostedByUser property
                var item = new Item
                {
                    Title = itemDto.Title,
                    Description = itemDto.Description,
                    Price = itemDto.Price,
                    Category = itemDto.Category,
                    PostedByUserId = userId,
                    PostedByUser = user,  // Set the required property
                    JobType = itemDto.JobType,
                    ServiceType = itemDto.ServiceType,
                    JobCategory = itemDto.JobCategory,
                    CompanyName = itemDto.CompanyName,
                    JobLocation = itemDto.JobLocation,
                    ApplyMethod = itemDto.ApplyMethod,
                    ApplyContact = itemDto.ApplyContact
                };

                await _context.Items.AddAsync(item);
                var result = await _context.SaveChangesAsync();

                if (result <= 0) return null;

                if (itemDto.PhotoUrls?.Any() == true)
                {
                    foreach (var photoUrl in itemDto.PhotoUrls.Take(2))
                    {
                        await AddItemPhotoAsync(userId, item.Id, photoUrl);
                    }
                }

                return item.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating item: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateItemAsync(int userId, int itemId, ItemUpdateDto updateDto)
        {
            try
            {
                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.PostedByUserId == userId);

                if (item == null) return false;

                item.Title = updateDto.Title ?? item.Title;
                item.Description = updateDto.Description ?? item.Description;
                item.Price = updateDto.Price;
                item.Category = updateDto.Category ?? item.Category;
                item.JobType = updateDto.JobType;
                item.ServiceType = updateDto.ServiceType;
                item.RentalPeriod = updateDto.RentalPeriod;
                item.AvailableFrom = updateDto.AvailableFrom;
                item.AvailableTo = updateDto.AvailableTo;
                item.JobCategory = updateDto.JobCategory;
                item.CompanyName = updateDto.CompanyName;
                item.JobLocation = updateDto.JobLocation;
                item.ApplyMethod = updateDto.ApplyMethod;
                item.ApplyContact = updateDto.ApplyContact;
                item.ServiceCategory = updateDto.ServiceCategory;
                item.ServiceAvailability = updateDto.ServiceAvailability;
                item.YearsOfExperience = updateDto.YearsOfExperience;
                item.ServiceLocation = updateDto.ServiceLocation;
                item.ForSaleCategory = updateDto.ForSaleCategory;
                item.ForRentCategory = updateDto.ForRentCategory;
                item.State = updateDto.State;
                item.Latitude = updateDto.Latitude;
                item.Longitude = updateDto.Longitude;

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null) return false;

                _context.Items.Remove(item);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting item: {ex.Message}");
                return false;
            }
        }

        public async Task<Item?> GetItemAsync(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item != null)
                {
                    await IncrementItemViewAsync(id);
                }
                return item;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving item: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            try
            {
                return await _context.Items
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving items: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        public async Task<IEnumerable<Item>> GetUserItemsAsync(int userId)
        {
            try
            {
                return await _context.Items
                    .Where(i => i.PostedByUserId == userId)
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving user items: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        public Task<IEnumerable<Item>> GetItemsByUserAsync(int userId) => GetUserItemsAsync(userId);

        public async Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm, string? category = null)
        {
            try
            {
                var query = _context.Items.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(i =>
                        i.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        i.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(i => i.Category == category);
                }

                return await query.OrderByDescending(i => i.ListedDate).ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching items: {ex.Message}");
                return [];  // Use collection expression
            }
        }

        public async Task<IEnumerable<Item>> SearchByStateAsync(AlState state)
        {
            try
            {
                return await _context.Items
                    .Where(i => i.State == state)
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching by state: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        public async Task<IEnumerable<Item>> SearchByLocationAsync(double latitude, double longitude, double radiusKm)
        {
            try
            {
                var lat1 = latitude * Math.PI / 180;
                var lon1 = longitude * Math.PI / 180;

                return await _context.Items
                    .Where(i => i.Latitude != null && i.Longitude != null)
                    .Select(i => new
                    {
                        Item = i,
                        Distance = 6371 * Math.Acos(
                            Math.Sin(lat1) * Math.Sin(i.Latitude!.Value * Math.PI / 180) +
                            Math.Cos(lat1) * Math.Cos(i.Latitude!.Value * Math.PI / 180) *
                            Math.Cos((i.Longitude!.Value * Math.PI / 180) - lon1)
                        )
                    })
                    .Where(x => x.Distance <= radiusKm)
                    .OrderBy(x => x.Distance)
                    .Select(x => x.Item)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching by location: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        public async Task<IEnumerable<Item>> SearchByCategoryAndStateAsync(string category, AlState state)
        {
            try
            {
                return await _context.Items
                    .Where(i => i.Category == category && i.State == state)
                    .OrderByDescending(i => i.ListedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching by category and state: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        public async Task<IEnumerable<Item>> GetItemsWithFiltersAsync(FilterCriteria criteria)
        {
            var query = _context.Items.AsQueryable();

            if (criteria.MinPrice.HasValue)
                query = query.Where(i => i.Price >= criteria.MinPrice.Value);

            if (criteria.MaxPrice.HasValue)
                query = query.Where(i => i.Price <= criteria.MaxPrice.Value);

            if (criteria.State.HasValue)
                query = query.Where(i => i.State == criteria.State.Value);

            if (criteria.Categories?.Any() == true)
                query = query.Where(i => criteria.Categories.Contains(i.Category));

            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                query = query.Where(i =>
                    i.Title.Contains(criteria.SearchText) ||
                    i.Description.Contains(criteria.SearchText));

            if (criteria.DateFrom.HasValue)
                query = query.Where(i => i.ListedDate >= criteria.DateFrom.Value);

            if (criteria.DateTo.HasValue)
                query = query.Where(i => i.ListedDate <= criteria.DateTo.Value);

            query = criteria.SortBy switch
            {
                SortOption.PriceLowToHigh => query.OrderBy(i => i.Price),
                SortOption.PriceHighToLow => query.OrderByDescending(i => i.Price),
                SortOption.DateNewest => query.OrderByDescending(i => i.ListedDate),
                SortOption.DateOldest => query.OrderBy(i => i.ListedDate),
                _ => query.OrderByDescending(i => i.ListedDate)
            };

            return await query.ToListAsync();
        }

        public async Task<bool> AddFavoriteAsync(int userId, int itemId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                var item = await _context.Items.FindAsync(itemId);
                if (user == null || item == null) return false;

                user.FavoriteItems.Add(item);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding favorite: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int itemId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                var item = await _context.Items.FindAsync(itemId);
                if (user == null || item == null) return false;

                user.FavoriteItems.Remove(item);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing favorite: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddRatingAsync(int userId, int itemId, int score, string review)
        {
            try
            {
                var rating = new Rating
                {
                    UserId = userId,
                    ItemId = itemId,
                    Score = score,
                    Review = review
                };

                await _context.Ratings.AddAsync(rating);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding rating: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Item>> GetUserFavoriteItemsAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.FavoriteItems)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                return user?.FavoriteItems ?? Enumerable.Empty<Item>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving user favorites: {ex.Message}");
                return Enumerable.Empty<Item>();
            }
        }

        public async Task<IEnumerable<Rating>> GetUserRatingsAsync(int userId)
        {
            try
            {
                return await _context.Ratings
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Item)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving user ratings: {ex.Message}");
                return Enumerable.Empty<Rating>();
            }
        }

        public async Task<UserProfileStatistics> GetUserProfileStatisticsAsync(int userId)
        {
            try
            {
                var postedItemsCount = await _context.Items.CountAsync(i => i.PostedByUserId == userId);
                var favoriteItemsCount = await _context.Users
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.FavoriteItems)
                    .CountAsync();

                var averageRating = await _context.Ratings
                    .Where(r => r.UserId == userId)
                    .AverageAsync(r => (double?)r.Score) ?? 0;

                return new UserProfileStatistics
                {
                    UserId = userId,
                    PostedItemsCount = postedItemsCount,
                    FavoriteItemsCount = favoriteItemsCount,
                    AverageRating = Math.Round(averageRating, 2)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving user statistics: {ex.Message}");
                return new UserProfileStatistics { UserId = userId };
            }
        }

        public async Task<bool> UpdateItemStatusAsync(int userId, int itemId, ItemStatus status)
        {
            try
            {
                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.PostedByUserId == userId);

                if (item == null) return false;

                item.Status = status;
                if (status == ItemStatus.Sold || status == ItemStatus.Rented)
                {
                    item.AvailableTo = DateTime.UtcNow;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating item status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsItemAvailableAsync(int itemId)
        {
            try
            {
                var item = await _context.Items.FindAsync(itemId);
                return item?.Status == ItemStatus.Active;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking item availability: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddItemPhotoAsync(int userId, int itemId, string photoUrl)
        {
            try
            {
                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.PostedByUserId == userId);

                if (item == null) return false;

                var currentPhotoCount = await _context.ItemPhotos
                    .CountAsync(p => p.ItemId == itemId);

                if (currentPhotoCount >= 2) return false;

                var newPhoto = new ItemPhoto
                {
                    ItemId = itemId,
                    Item = item,  // Add this line to satisfy the required property
                    PhotoUrl = photoUrl,
                    IsPrimaryPhoto = currentPhotoCount == 0,
                    DisplayOrder = currentPhotoCount + 1
                };

                await _context.ItemPhotos.AddAsync(newPhoto);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding photo: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> RemoveItemPhotoAsync(int userId, int photoId)
        {
            try
            {
                var photo = await _context.ItemPhotos
                    .Include(p => p.Item)
                    .FirstOrDefaultAsync(p =>
                        p.Id == photoId &&
                        p.Item.PostedByUserId == userId);

                if (photo == null) return false;

                _context.ItemPhotos.Remove(photo);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing photo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetPrimaryPhotoAsync(int userId, int photoId)
        {
            try
            {
                var photo = await _context.ItemPhotos
                    .Include(p => p.Item)
                    .FirstOrDefaultAsync(p =>
                        p.Id == photoId &&
                        p.Item.PostedByUserId == userId);

                if (photo == null) return false;

                var itemPhotos = await _context.ItemPhotos
                    .Where(p => p.ItemId == photo.ItemId)
                    .ToListAsync();

                foreach (var itemPhoto in itemPhotos)
                {
                    itemPhoto.IsPrimaryPhoto = itemPhoto.Id == photoId;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting primary photo: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<ItemPhoto>> GetItemPhotosAsync(int itemId)
        {
            try
            {
                return await _context.ItemPhotos
                    .Where(p => p.ItemId == itemId)
                    .OrderBy(p => p.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving photos: {ex.Message}");
                return Enumerable.Empty<ItemPhoto>();
            }
        }

        public async Task<IEnumerable<ItemPerformanceDto>> GetTopPerformingItemsAsync(int count)
        {
            try
            {
                return await _context.ItemStatistics
                    .OrderByDescending(s => s.ViewCount)
                    .Take(count)
                    .Select(s => new ItemPerformanceDto
                    {
                        ItemId = s.ItemId,
                        ViewCount = s.ViewCount,
                        InquiryCount = s.InquiryCount,
                        Title = s.Item.Title,
                        Category = s.Item.Category
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving top items: {ex.Message}");
                return Enumerable.Empty<ItemPerformanceDto>();
            }
        }

        public async Task<bool> IncrementItemViewAsync(int itemId)
        {
            try
            {
                var statistics = await _context.ItemStatistics
                    .FirstOrDefaultAsync(s => s.ItemId == itemId);

                if (statistics == null)
                {
                    var item = await _context.Items.FindAsync(itemId);
                    if (item == null) return false;

                    statistics = new ItemStatistics
                    {
                        ItemId = itemId,
                        Item = item,  // Add the required Item
                        ViewCount = 1,
                        FirstViewedAt = DateTime.UtcNow,
                        LastViewedAt = DateTime.UtcNow
                    };
                    await _context.ItemStatistics.AddAsync(statistics);
                }
                else
                {
                    statistics.ViewCount++;
                    statistics.LastViewedAt = DateTime.UtcNow;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error incrementing view: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> RecordItemInquiryAsync(int itemId)
        {
            try
            {
                var statistics = await _context.ItemStatistics
                    .FirstOrDefaultAsync(s => s.ItemId == itemId);

                if (statistics == null)
                {
                    var item = await _context.Items.FindAsync(itemId);
                    if (item == null) return false;

                    statistics = new ItemStatistics
                    {
                        ItemId = itemId,
                        Item = item,  // Add the required Item
                        InquiryCount = 1,
                        FirstViewedAt = DateTime.UtcNow,
                        LastViewedAt = DateTime.UtcNow
                    };
                    await _context.ItemStatistics.AddAsync(statistics);
                }
                else
                {
                    statistics.InquiryCount++;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error recording inquiry: {ex.Message}");
                return false;
            }
        }
        public async Task<ItemStatistics?> GetItemStatisticsAsync(int itemId)
        {
            try
            {
                return await _context.ItemStatistics
                    .FirstOrDefaultAsync(s => s.ItemId == itemId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving statistics: {ex.Message}");
                return null;
            }
        }
    }
}