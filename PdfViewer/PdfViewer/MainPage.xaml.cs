using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Pdf;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using System.Diagnostics;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace PdfViewer
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        //main method to set up a picker in order to load pdf
        private async void Button_Click_Open_PDF(object sender, RoutedEventArgs e)
        {
            //picker black magic
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".pdf");

            var file = await picker.PickSingleFileAsync();

            if (file == null) return;

            await OpenPDFAsync(file); //open pdf
            await LoadPages(); //load into xml

        }

        private PdfDocument myPdfDoc { get; set; } //loaded pdf
        private uint currentPage = 0; //index for single pages loading

        public ObservableCollection<BitmapImage> PdfPages //collection to hold currenrly displayed pdf images
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        private async Task OpenPDFAsync(StorageFile file) //opens specified pdf and stores it in local var for later use
        {
            if (file == null) throw new ArgumentNullException();

            myPdfDoc = await PdfDocument.LoadFromFileAsync(file);
        }

        private async Task LoadPages() //loads all pages in the current pdf into xml scroll view
        {
            if (myPdfDoc == null) //TODO better error checking - now we just crash the program
            {
                throw new Exception("No document open.");
            }

            PdfPages.Clear();//yeetus deletus

            for(uint i = 0; i < myPdfDoc.PageCount; i++) //iterate over all pages and blend them into the scroll view
            {
                BitmapImage image = new BitmapImage(); //image to blend current pdf page into
                var curPage = myPdfDoc.GetPage(i); 

                using (var stream = new InMemoryRandomAccessStream())//blend content of pdf page into the image
                {
                    await curPage.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }
                PdfPages.Add(image); //add new image to collection in order to display it in the xml

            }
            Debug.WriteLine("PDF loading done, yip yip!");
        }

        //stuff for single page loading OUTDATED
        private async Task DisplayPage(uint pageIndex) //render a single page specified by param
        {
            if (myPdfDoc == null)
            {
                throw new Exception("No document open.");
            }

            if (pageIndex >= myPdfDoc.PageCount || pageIndex < 0)
            {
                throw new ArgumentOutOfRangeException($"Document has only {myPdfDoc.PageCount} pages.");
            }

            var page = myPdfDoc.GetPage(pageIndex); //grab page to render

            var image = new BitmapImage(); //image to render into

            using (var stream = new InMemoryRandomAccessStream()) //blend pdf page into image
            {
                await page.RenderToStreamAsync(stream);
                await image.SetSourceAsync(stream);

                PdfImage.Source = image; //set cml-image source to the new image
            }
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            this.currentPage++;
            await DisplayPage(this.currentPage);
        }

        private async void Prev_Click(object sender, RoutedEventArgs e)
        {
            this.currentPage--;
            await DisplayPage(this.currentPage);
        }
    }
}
