using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.ProjectOxford.Face;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;


namespace Klitech
{

    public sealed partial class MainPage : Page
    {

        FaceServiceClient faceclient = new FaceServiceClient("4704c7c9b1414380980ae4aee725bc52"); //klienshez tartozó kulcs

        private const string ApiUri = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";
        private const string SubscriptionKey = "fb1fbf3f52614f87acaf1071b63b3b18";
        private static readonly HttpClient Client = GetClient();

        /*
         * Http kliens inicializálása
         */
        private static HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            client.DefaultRequestHeaders.Add("ContentType", "application/json");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }

        /*
         * A detect függvény paraméterként van egy byte tömböt és azt továbbítja a szerverre, ahol kiértékelődik az adat
         * A visszaérkező adatokat JSON segítségével deszerializáljuk
         */
        private static FaceDetectResponse[] Detect(Byte[] bytestream)
        {
            using (var content = new ByteArrayContent(bytestream))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                using (var httpResponse = Client.PostAsync($"{ApiUri}/detect?returnFaceAttributes=age,gender,emotion", content).Result)
                {
                    httpResponse.EnsureSuccessStatusCode();
                    var json = httpResponse.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<FaceDetectResponse[]>(json);
                }
            }
        }


        /*
         * A page componens inicializálása 
         */
        public MainPage()
        {
            this.InitializeComponent();
        }

        /*
         * A függvény megkapja az érzékelt arcokat és az ezekhez tartozó információkat kiírja a képernyőre
         */
        private void writeFaceData(FaceDetectResponse[] faces)
        {
            String infotext = "";

            for (int faceIndex = 0; faceIndex < faces.Count(); faceIndex++)
            {
                var face = faces[faceIndex];
                infotext = String.Concat(infotext, "Face Detected #" + (faceIndex + 1) + "\n");
                infotext = String.Concat(infotext, "Age: " + face.FaceAttributes.Age + "\n");
                infotext = String.Concat(infotext, "Gender: " + face.FaceAttributes.Gender + "\n");
                infotext = String.Concat(infotext, "Main Emotion: " + face.FaceAttributes.Emotion.OrderByDescending(j => j.Value).First().Key + "\n\n");
            };
            Infos.Text = infotext;
            drawRect(faces);
        }

        /*
         * Ezzel a függvénnyel történik az arcok köré a téglalapok kirajzolása. Minden téglalapot 4 vonalból építek fel
         * és ezeket az információkat az érzékelt arcok tömbjéből szerzek meg
         */
        private void drawRect(FaceDetectResponse[] faces)
        {
            foreach (var face in faces)
            {
                Line top = new Line();
                top.X1 = face.FaceRectangle.Left;
                top.Y1 = face.FaceRectangle.Top;
                top.X2 = face.FaceRectangle.Left + face.FaceRectangle.Width;
                top.Y2 = face.FaceRectangle.Top;

                Line right = new Line();
                right.X1 = face.FaceRectangle.Left + face.FaceRectangle.Width;
                right.Y1 = face.FaceRectangle.Top;
                right.X2 = face.FaceRectangle.Left + face.FaceRectangle.Width;
                right.Y2 = face.FaceRectangle.Top + face.FaceRectangle.Height;

                Line bottom = new Line();
                bottom.X1 = face.FaceRectangle.Left;
                bottom.Y1 = face.FaceRectangle.Top + face.FaceRectangle.Height;
                bottom.X2 = face.FaceRectangle.Left + face.FaceRectangle.Width;
                bottom.Y2 = face.FaceRectangle.Top + face.FaceRectangle.Height;

                Line left = new Line();
                left.X1 = face.FaceRectangle.Left;
                left.Y1 = face.FaceRectangle.Top;
                left.X2 = face.FaceRectangle.Left;
                left.Y2 = face.FaceRectangle.Top + face.FaceRectangle.Height;

                top.Stroke = new SolidColorBrush(Windows.UI.Colors.Red);
                right.Stroke = new SolidColorBrush(Windows.UI.Colors.Red);
                bottom.Stroke = new SolidColorBrush(Windows.UI.Colors.Red);
                left.Stroke = new SolidColorBrush(Windows.UI.Colors.Red);

                Canvas.Children.Add(top);
                Canvas.Children.Add(right);
                Canvas.Children.Add(bottom);
                Canvas.Children.Add(left);
            }
        }

        /*
         * Kép kirajzolása amit kiválaszottunk a fájlrendszerből vagy csináltunk az eszköz kamerájával. A képet hozzáadja a canvashoz
         */
        private void drawImage(Image image)
        {
            Canvas.Children.Add(image);
        }

        /*
         * Függvény ami letakarítja a canvast, kiírja az arcok adatait, kirajzolja a képet és rájuk a téglalapokat
         */
        private void displayFaces(FaceDetectResponse[] faces, BitmapImage image)
        {
            Canvas.Children.Clear();
            writeFaceData(faces);
            drawImage(convertBitmapImageToImage(image));
            drawRect(faces);
        }

        /*
         * Ezzel a függvénnyel történik a konverzió Bitmapimage és Image között. Azért van rá szükség, mert a vászonra 
         * sima imaget tudunk csak rakni
         */
        public Image convertBitmapImageToImage(BitmapImage bitmapimage)
        {
            Image image = new Image();
            image.Source = bitmapimage;
            image.Width = bitmapimage.PixelWidth;
            image.Height = bitmapimage.PixelHeight;
            return image;
        }
        /*
         * Ez a függvény kezeli le azt az eseményt amikor a "open file" gombra kattintunk.
         * Megnyit egy file explorert és a kiválaszott fájlt beolvassa bytearraybe, amit pedig át is alakítunk bitmapimaggé.
         * A byte arrayt átadjuk a detect függvénynek, a képet és a detektált arcokat pedig a kiíró függvénynek.
         */
        public async void openfileButton(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (file == null) return;

            byte[] imagearray = null;

            using (var stream = await file.OpenReadAsync())
            {
                imagearray = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(imagearray);
                }
            }
            BitmapImage image = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(imagearray.AsBuffer());
                stream.Seek(0);
                await image.SetSourceAsync(stream);
            }

            FaceDetectResponse[] faces = Detect(imagearray);
            displayFaces(faces, image); 
        }
        /*
         * Ezzel a függvénnyel tudjuk meghívni az eszköz kameráját és fényképet készíteni vele.
         * A fényképet átkonvertáljuk byte arraybe és bitmap imagebe is, ezek után pedig átadjuk őket arcdetektálásra és 
         * a talált információk kiírására
         */
        public async void captureBtn(object sender, RoutedEventArgs e)
        {
            var captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(Canvas.Height, Canvas.Height);

            var photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (photo != null)
            {
                Stream stream = await photo.OpenStreamForReadAsync();

                Byte[] imagearray;

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    imagearray = memoryStream.ToArray();
                };

                BitmapImage image = new BitmapImage();
                using (InMemoryRandomAccessStream randomstream = new InMemoryRandomAccessStream())
                {
                    await randomstream.WriteAsync(imagearray.AsBuffer());
                    randomstream.Seek(0);
                    await image.SetSourceAsync(randomstream);
                }

                FaceDetectResponse[] faces = Detect(imagearray);
                displayFaces(faces, image);
            }
        }

        /*
         * Kilépést kezelő gomb függvénye
         */
        public void quitBtn(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
