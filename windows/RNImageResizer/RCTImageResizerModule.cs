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
using System.IO;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Foundation;

namespace RNImageResizer
{
    class RCTImageResizerModule : ReactContextNativeModuleBase
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


    [ReactMethod]
    public async void createResizedImage(String imagePath, int newWidth, int newHeight, String compressFormat,
                            int quality, int rotation, String outputPath, IPromise promise) {

            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = File.OpenRead(imagePath).AsRandomAccessStream())
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            StorageFile outputFile = null;
            if (outputPath == null)
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                String imageName = System.IO.Path.GetFileName(imagePath);
                outputFile = await localFolder.CreateFileAsync(imageName, CreationCollisionOption.ReplaceExisting);
            }
            else {
                outputFile = await StorageFile.GetFileFromPathAsync(outputPath);
            }


            await SaveSoftwareBitmapToFile(softwareBitmap, newWidth, newHeight,
                             quality, rotation, outputFile);

            promise.Resolve(PrepareFile(outputFile).Result);

        }

        private void RejectFileNotFound(IPromise promise, String imagePath)
        {
            promise.Reject("FILE NOT EXIST", "No such image file, '" + imagePath + "'");
        }

        // TODO: Implement rotation
private async 
Task
SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, int newWidth, int newHeight,
                            int quality, int rotation, StorageFile outputFile)
{
    using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
    {
                BitmapPropertySet bitmapPropertiesSet = new BitmapPropertySet();
                double qualitypercent = (quality % 100) / 100.0;
                bitmapPropertiesSet.Add("ImageQuality", new BitmapTypedValue(qualitypercent, PropertyType.Single));
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream, bitmapPropertiesSet);

        // Set the software bitmap
        encoder.SetSoftwareBitmap(softwareBitmap);
        // Set additional encoding parameters, if needed
        encoder.BitmapTransform.ScaledWidth = (uint)newWidth;
        encoder.BitmapTransform.ScaledHeight = (uint)newHeight;
        encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.None;
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

        private async Task<JObject> PrepareFile(StorageFile file)
        {
            var basicProperties = await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);

            return new JObject {
                    { FIELD_URI, file.Path },
                    { FIELD_TYPE, file.ContentType },
                    { FIELD_NAME, file.Name },
                    { FIELD_SIZE, basicProperties.Size}
                };
        }


        private static async void RunOnDispatcher(DispatchedHandler action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action).AsTask().ConfigureAwait(false);
        }
    }
}
