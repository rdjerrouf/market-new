using System.Diagnostics;

namespace Market.Views;

public partial class PostItemPage : ContentPage
{
	public PostItemPage()
	{
		InitializeComponent();
	}

    private void OnUploadPhotoClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Upload photo button clicked");
    }
}