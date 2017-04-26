using System;
using System.IO;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;
using SharpDX.WIC;
using Device = SharpDX.Direct3D11.Device;

namespace CrpViewer.Renderer {
    class TextureLoader {
        private ImagingFactory factory;
        private Device device;

        public TextureLoader(Device device) {
            factory = new ImagingFactory();
            this.device = device;
        }

        public Texture2D LoadFromFile(string filename) {
/*            if (filename.EndsWith(".tga")) {
                var image = new TargaImage(filename);
                var format = Format.Unknown;
                var imageData = image.Image;
                if (image.Header.PixelDepth == 24) {
                    format = Format.B8G8R8A8_UNorm;
                    imageData = image.Image.Clone(new System.Drawing.Rectangle(0, 0, image.Image.Width, image.Image.Height), System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                }
                var bitmapData = imageData.LockBits(new System.Drawing.Rectangle(0, 0, image.Image.Width, image.Image.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, imageData.PixelFormat);
                var bufferSize = bitmapData.Height * bitmapData.Stride;

                // copy our buffer to the texture
                int stride = image.Image.Width * 4;
                var tex = new Texture2D(device, new Texture2DDescription() {
                    Width = image.Header.Width,
                    Height = image.Header.Height,
                    Format = format,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    MipLevels = GetNumMipLevels(image.Header.Width, image.Header.Height),
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                    SampleDescription = new SampleDescription(1, 0),
                });
                device.ImmediateContext.UpdateSubresource(tex, 0, null, bitmapData.Scan0, stride, 0);

                // unlock the bitmap data
                imageData.UnlockBits(bitmapData);
                return tex;
            } else {*/
                var bitmap = LoadBitmap(filename);
                return CreateTexture2DFromBitmap(bitmap);
//            }
        }

        public Texture2D loadFromByteArray(byte[] raw) {
            using (var stream = new MemoryStream(raw)) {
                var bitmapDecoder = new BitmapDecoder(factory,stream,DecodeOptions.CacheOnDemand);
                var formatConverter = new FormatConverter(factory);

                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    PixelFormat.Format32bppPRGBA,
                    BitmapDitherType.None,
                    null,
                    0.0,
                        BitmapPaletteType.Custom);
                return CreateTexture2DFromBitmap(formatConverter);
            }
        }    

        private BitmapSource LoadBitmap(string filename) {
            var bitmapDecoder = new BitmapDecoder(
                factory,
                filename,
                DecodeOptions.CacheOnDemand
                );

            var formatConverter = new FormatConverter(factory);

            formatConverter.Initialize(
                bitmapDecoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            return formatConverter;
        }

        /// <summary>
        /// Creates a <see cref="SharpDX.Direct3D11.Texture2D"/> from a WIC <see cref="SharpDX.WIC.BitmapSource"/>
        /// </summary>
        /// <param name="device">The Direct3D11 device</param>
        /// <param name="bitmapSource">The WIC bitmap source</param>
        /// <returns>A Texture2D</returns>
        private Texture2D CreateTexture2DFromBitmap(BitmapSource bitmapSource) {
            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmapSource.Size.Width * 4;
            using (var buffer = new SharpDX.DataStream(bitmapSource.Size.Height * stride, true, true)) {
                // Copy the content of the WIC to the buffer
                bitmapSource.CopyPixels(stride, buffer);
                var tex = new Texture2D(device, new Texture2DDescription() {
                    Width = bitmapSource.Size.Width,
                    Height = bitmapSource.Size.Height,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    MipLevels = GetNumMipLevels(bitmapSource.Size.Width, bitmapSource.Size.Height),
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                    SampleDescription = new SampleDescription(1, 0),
                });
                device.ImmediateContext.UpdateSubresource(tex, 0, null, buffer.DataPointer, stride, 0);
                return tex;
            }
        }

        private int GetNumMipLevels(int width, int height) {
            var numLevels = 1;
            while (width > 1 && height > 1) {
                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);
                numLevels++;
            }

            return numLevels;
        }
    }
}
