using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using System;
using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using ReactNative.Collections;
using System.Linq;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZXing;
using static System.FormattableString;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RNImageResizer
{
    class RCTImageResizerModule : ReactContextNativeModuleBase, ILifecycleEventListener
    {

	private static readonly String FIELD_URI = "uri";
	private static readonly String FIELD_NAME = "name";
	private static readonly String FIELD_TYPE = "type";
	private static readonly String FIELD_SIZE = "size";

        public RCTImageResizerModule(ReactContext reactContext)
            : base(reactContext)
        {
        }

        public override string Name
        {
            get
            {
                return "ImageResizer";
            }
        }

        public override void Initialize()
        {
            Context.AddLifecycleEventListener(this);
        }


    [ReactMethod]
    public void createResizedImage(String imagePath, int newWidth, int newHeight, String compressFormat,
                            int quality, int rotation, String outputPath, IPromise promise) {

            //StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            //StorageFile imageFile = null;
            //imageFile = await localFolder.GetFileAsync(imagePath);
            if (!File.Exists(imagePath))
                {
                    RejectFileNotFound(promise, imagePath);
                    return;
                }
            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
    
                    JArray jarrayObj = new JArray();
                foreach (var file in files) {
                    jarrayObj.Add(PrepareFile(file).Result);
                }
                promise.Resolve(jarrayObj);

    }

        private void RejectFileNotFound(IPromise promise, String imagePath)
        {
            promise.Reject("FILE NOT EXIST", "No such image file, '" + imagePath + "'");
        }

private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
{
    using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
    {
        // Create an encoder with the desired format
        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

        // Set the software bitmap
        encoder.SetSoftwareBitmap(softwareBitmap);

        // Set additional encoding parameters, if needed
        encoder.BitmapTransform.ScaledWidth = 320;
        encoder.BitmapTransform.ScaledHeight = 240;
        encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
        encoder.IsThumbnailGenerated = true;

        try
        {
            await encoder.FlushAsync();
        }
        catch (Exception err)
        {
            switch (err.HResult)
            {
                case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
                                                 // If the encoder does not support writing a thumbnail, then try again
                                                 // but disable thumbnail generation.
                    encoder.IsThumbnailGenerated = false;
                    break;
                default:
                    throw err;
            }
        }

        if (encoder.IsThumbnailGenerated == false)
        {
            await encoder.FlushAsync();
        }


    }
}


private BitmapImage ResizedImage(BitmapImage sourceImage, int maxWidth, int maxHeight)
{
    var origHeight = sourceImage.PixelHeight;
    var origWidth = sourceImage.PixelWidth;
    var ratioX = maxWidth/(float) origWidth;
    var ratioY = maxHeight/(float) origHeight;
    var ratio = Math.Min(ratioX, ratioY);
    var newHeight = (int) (origHeight * ratio);
    var newWidth = (int) (origWidth * ratio);

    sourceImage.DecodePixelWidth = newWidth;
    sourceImage.DecodePixelHeight = newHeight;

    return sourceImage;
}

        private static string GetDirectory(String filePath)
        {
            return file.Path.Replace("\\" + file.Name, "");
        }

        private async Task<JObject> PrepareFile(StorageFile file)
        {
            var basicProperties = await file.GetBasicPropertiesAsync();

            return new JObject {
                    { FIELD_URI, file.Path },
                    { FIELD_TYPE, file.ContentType },
                    { FIELD_NAME, file.Name },
                    { FIELD_SIZE, basicProperties.Size}
                };
        }


        private void OnInvoked(Object error, Object success, ICallback callback)
        {
            callback.Invoke(error, success);
        }

        private static async void RunOnDispatcher(DispatchedHandler action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action).AsTask().ConfigureAwait(false);
        }
    }
}
