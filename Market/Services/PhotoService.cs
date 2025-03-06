using Market.DataAccess.Models;
using Market.Market.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Market.DataAccess.Data;

namespace Market.Services
{
    public class PhotoService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMediaService _mediaService;

        public PhotoService(AppDbContext dbContext, IMediaService mediaService)
        {
            _dbContext = dbContext;
            _mediaService = mediaService;
        }

        // Get all photos for an item
        public async Task<List<ItemPhoto>> GetItemPhotosAsync(int itemId)
        {
            return await _dbContext.ItemPhotos
                .Where(p => p.ItemId == itemId)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        // Add a new photo
        public async Task<ItemPhoto> AddPhotoAsync(int itemId, FileResult photoFile)
        {
            // Get the item
            var item = await _dbContext.Items
                .Include(i => i.Photos)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
                throw new Exception($"Item with ID {itemId} not found");

            // Upload the image to storage/file system
            string photoUrl = await _mediaService.UploadImageAsync(photoFile);

            // Check if this is the first photo
            bool isPrimary = !item.Photos.Any();

            // Get the highest display order
            int maxDisplayOrder = item.Photos.Any()
                ? item.Photos.Max(p => p.DisplayOrder)
                : -1;

            // Create new photo record
            var photo = new ItemPhoto
            {
                ItemId = itemId,
                Item = item,
                PhotoUrl = photoUrl,
                IsPrimaryPhoto = isPrimary,
                UploadedAt = DateTime.UtcNow,
                DisplayOrder = maxDisplayOrder + 1
            };

            // Add to database
            _dbContext.ItemPhotos.Add(photo);

            // If this is the first/primary photo, update the item's main photo URL
            if (isPrimary)
            {
                item.PhotoUrl = photoUrl;
                item.ImageUrl = photoUrl;
                _dbContext.Items.Update(item);
            }

            await _dbContext.SaveChangesAsync();

            return photo;
        }

        // Delete a photo
        public async Task DeletePhotoAsync(int photoId)
        {
            var photo = await _dbContext.ItemPhotos
                .Include(p => p.Item)
                .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo == null)
                throw new Exception("Photo not found");

            bool wasPrimary = photo.IsPrimaryPhoto;

            // Delete the file from storage
            await _mediaService.DeleteImageAsync(photo.PhotoUrl);

            // Remove from database
            _dbContext.ItemPhotos.Remove(photo);

            // If this was the primary photo, set a new one
            if (wasPrimary)
            {
                var newPrimary = await _dbContext.ItemPhotos
                    .Where(p => p.ItemId == photo.ItemId && p.Id != photoId)
                    .OrderBy(p => p.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (newPrimary != null)
                {
                    newPrimary.IsPrimaryPhoto = true;
                    photo.Item.PhotoUrl = newPrimary.PhotoUrl;
                    photo.Item.ImageUrl = newPrimary.PhotoUrl;
                }
                else
                {
                    // No photos left
                    photo.Item.PhotoUrl = null;
                    photo.Item.ImageUrl = null;
                }

                _dbContext.Update(photo.Item);
            }

            await _dbContext.SaveChangesAsync();
        }

        // Set a photo as primary
        public async Task SetPrimaryPhotoAsync(int photoId)
        {
            var photo = await _dbContext.ItemPhotos
                .Include(p => p.Item)
                .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo == null)
                throw new Exception("Photo not found");

            // Get all photos for this item
            var allPhotos = await _dbContext.ItemPhotos
                .Where(p => p.ItemId == photo.ItemId)
                .ToListAsync();

            // Update primary status
            foreach (var p in allPhotos)
            {
                p.IsPrimaryPhoto = (p.Id == photoId);
            }

            // Update the item's main photo
            photo.Item.PhotoUrl = photo.PhotoUrl;
            photo.Item.ImageUrl = photo.PhotoUrl;
            _dbContext.Update(photo.Item);

            await _dbContext.SaveChangesAsync();
        }

        // Reorder photos
        public async Task ReorderPhotosAsync(int itemId, List<int> photoIds)
        {
            var photos = await _dbContext.ItemPhotos
                .Where(p => p.ItemId == itemId)
                .ToListAsync();

            // Update display order based on the provided sequence
            for (int i = 0; i < photoIds.Count; i++)
            {
                var photo = photos.FirstOrDefault(p => p.Id == photoIds[i]);
                if (photo != null)
                {
                    photo.DisplayOrder = i;
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}