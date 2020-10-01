using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace PdfViewer
{
    class PdfLoader
    {
        //attributes
        private PdfDocument myPdfDoc { get; set; } //loaded pdf

        public ObservableCollection<BitmapImage> PdfPages //collection to hold currenrly displayed pdf images
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        //constructor
        public PdfLoader(StorageFile file, ObservableCollection<BitmapImage> PdfPages)
        {
            if (file == null)
                return;
            this.PdfPages = PdfPages;
            loadPdf(file);
        }

        private async void loadPdf(StorageFile file)
        {
            await OpenPDFAsync(file); //open pdf
            await LoadPages(); //load into xml
        }

        //methods
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

            for (uint i = 0; i < myPdfDoc.PageCount; i++) //iterate over all pages and blend them into the scroll view
            {
                BitmapImage image = new BitmapImage(); //image to blend current pdf page into
                var curPage = myPdfDoc.GetPage(i);

                using (var stream = new InMemoryRandomAccessStream())//blend content of pdf page into the image
                {
                    await curPage.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }
                PdfPages.Add(image); //add new image to collection in order to display it in the xml
               // mp.PdfPages.Add(image);
            }
            Debug.WriteLine("PDF loading done, yip yip!");
        }

    }
}
