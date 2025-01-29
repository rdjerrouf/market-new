// ViewModels/MainViewModel.cs

using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Market.DataAccess.Models;

namespace Market.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            Items = new ObservableCollection<Item>(GetAllItems());
            SearchCommand = new RelayCommand<string?>(SearchItems);
        }

        public ObservableCollection<Item> Items { get; }

        private string? searchQuery;

        public string? SearchQuery
        {
            get => searchQuery;
            set => SetProperty(ref searchQuery, value);
        }

        public IRelayCommand<string?> SearchCommand { get; }

        private void SearchItems(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                Items.Clear();
                foreach (var item in GetAllItems())
                {
                    Items.Add(item);
                }
            }
            else
            {
                var filteredItems = GetAllItems().Where(item =>
                    item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    item.Description.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
                Items.Clear();
                foreach (var item in filteredItems)
                {
                    Items.Add(item);
                }
            }
        }

        private List<Item> GetAllItems()
        {
            // Replace this with your actual data fetching logic
            return new List<Item>
    {
        new Item
        {
            Id = 1,
            Title = "Item 1",
            Description = "Description 1",
            PhotoUrl = "http://example.com/photo1.jpg",
            Price = 10.99m,
            UserId = 123,
            ListedDate = DateTime.UtcNow,
            Status = "Available"
        },
        new Item
        {
            Id = 2,
            Title = "Item 2",
            Description = "Description 2",
            PhotoUrl = "http://example.com/photo2.jpg",
            Price = 20.99m,
            UserId = 124,
            ListedDate = DateTime.UtcNow,
            Status = "Available"
        },
        new Item
        {
            Id = 3,
            Title = "Item 3",
            Description = "Description 3",
            PhotoUrl = "http://example.com/photo3.jpg",
            Price = 30.99m,
            UserId = 125,
            ListedDate = DateTime.UtcNow,
            Status = "Available"
        }
        // Add more items here
    };
        }


        [RelayCommand]
        private void ForSale()
        {
            // Implement For Sale logic
        }

        [RelayCommand]
        private void Jobs()
        {
            // Implement Jobs logic
        }

        [RelayCommand]
        private void Services()
        {
            // Implement Services logic
        }

        [RelayCommand]
        private void Rentals()
        {
            // Implement Rentals logic
        }

        [RelayCommand]
        private void Home()
        {
            // Implement Home logic
        }

        [RelayCommand]
        private void Inbox()
        {
            // Implement Inbox logic
        }

        [RelayCommand]
        private void Post()
        {
            // Implement Post logic
        }

        [RelayCommand]
        private void MyListings()
        {
            // Implement My Listings logic
        }
    }
}
