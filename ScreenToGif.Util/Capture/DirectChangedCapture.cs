using ScreenToGif.Domain.Models.Project.Recording;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Runtime.InteropServices;
using System.Windows;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace ScreenToGif.Util.Capture;

public class DirectChangedCapture : DirectCapture
{
    public override int Capture(RecordingFrame frame)
    {
        var res = new Result(-1);
        var wasCaptured = false;

        try
        {
            //Try to get the duplicated output frame within given time.
            res = DuplicatedOutput.TryAcquireNextFrame(0, out var info, out var resource);

            if (res.Failure || resource == null || info.TotalMetadataBufferSize == 0)
            {
                //Somehow, it was not possible to retrieve the resource, frame or metadata.
                resource?.Dispose();
                return FrameCount;
            }

            #region Process changes

            //Copy resource into memory that can be accessed by the CPU.
            using (var screenTexture = resource.QueryInterface<Texture2D>())
            {
                #region Moved rectangles

                var movedRectangles = new OutputDuplicateMoveRectangle[info.TotalMetadataBufferSize];
                DuplicatedOutput.GetFrameMoveRects(movedRectangles.Length, movedRectangles, out var movedRegionsLength);

                for (var movedIndex = 0; movedIndex < movedRegionsLength / Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle)); movedIndex++)
                {
                    //Crop the destination rectangle to the scree area rectangle.
                    var left = Math.Max(movedRectangles[movedIndex].DestinationRect.Left, Left);
                    var right = Math.Min(movedRectangles[movedIndex].DestinationRect.Right, Left + Width);
                    var top = Math.Max(movedRectangles[movedIndex].DestinationRect.Top, Top);
                    var bottom = Math.Min(movedRectangles[movedIndex].DestinationRect.Bottom, Top + Height);

                    //Copies from the screen texture only the area which the user wants to capture.
                    if (right > left && bottom > top)
                    {
                        //Limit the source rectangle to the available size within the destination rectangle.
                        var sourceWidth = movedRectangles[movedIndex].SourcePoint.X + (right - left);
                        var sourceHeight = movedRectangles[movedIndex].SourcePoint.Y + (bottom - top);

                        Device.ImmediateContext.CopySubresourceRegion(screenTexture, 0, new ResourceRegion(movedRectangles[movedIndex].SourcePoint.X, movedRectangles[movedIndex].SourcePoint.Y, 0, sourceWidth, sourceHeight, 1),
                            StagingTexture, 0, left - Left, top - Top);
                        wasCaptured = true;
                    }
                }

                #endregion

                #region Dirty rectangles

                var dirtyRectangles = new RawRectangle[info.TotalMetadataBufferSize];
                DuplicatedOutput.GetFrameDirtyRects(dirtyRectangles.Length, dirtyRectangles, out var dirtyRegionsLength);

                for (var dirtyIndex = 0; dirtyIndex < dirtyRegionsLength / Marshal.SizeOf(typeof(RawRectangle)); dirtyIndex++)
                {
                    //Crop screen positions and size to frame sizes.
                    var left = Math.Max(dirtyRectangles[dirtyIndex].Left, Left);
                    var right = Math.Min(dirtyRectangles[dirtyIndex].Right, Left + Width);
                    var top = Math.Max(dirtyRectangles[dirtyIndex].Top, Top);
                    var bottom = Math.Min(dirtyRectangles[dirtyIndex].Bottom, Top + Height);

                    //Copies from the screen texture only the area which the user wants to capture.
                    if (right > left && bottom > top)
                    {
                        Device.ImmediateContext.CopySubresourceRegion(screenTexture, 0, new ResourceRegion(left, top, 0, right, bottom, 1), StagingTexture, 0, left - Left, top - Top);
                        wasCaptured = true;
                    }
                }

                #endregion

                if (!wasCaptured)
                {
                    //Nothing was changed within the capture region, so ignore this frame.
                    resource.Dispose();
                    return FrameCount;
                }
            }

            #endregion

            #region Gets the image data

            //Gets the staging texture as a stream.
            var data = Device.ImmediateContext.MapSubresource(StagingTexture, 0, MapMode.Read, MapFlags.None, out var stream);

            if (data.IsEmpty)
            {
                Device.ImmediateContext.UnmapSubresource(StagingTexture, 0);
                stream?.Dispose();
                resource.Dispose();
                return FrameCount;
            }

            //Set frame details.
            FrameCount++;

            frame.TimeStampInTicks = Stopwatch.GetElapsedTicks();
            //frame.Delay = Stopwatch.GetMilliseconds(); //Resets the stopwatch. Messes up the editor.
            frame.Pixels = new byte[stream.Length];

            //BGRA32 is 4 bytes.
            for (var height = 0; height < Height; height++)
            {
                stream.Position = height * data.RowPitch;
                Marshal.Copy(new IntPtr(stream.DataPointer.ToInt64() + height * data.RowPitch), frame.Pixels, height * Width * 4, Width * 4);
            }

            FrameCollection.Add(frame);

            #endregion

            Device.ImmediateContext.UnmapSubresource(StagingTexture, 0);

            resource.Dispose();
            return FrameCount;
        }
        catch (SharpDXException se) when (se.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
        {
            return FrameCount;
        }
        catch (SharpDXException se) when (se.ResultCode.Code == SharpDX.DXGI.ResultCode.DeviceRemoved.Result.Code || se.ResultCode.Code == SharpDX.DXGI.ResultCode.DeviceReset.Result.Code)
        {
            //When the device gets lost or reset, the resources should be instantiated again.
            ExtraDisposeInternal();
            Initialize();

            return FrameCount;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "It was not possible to finish capturing the frame with DirectX.");

            Application.Current.Dispatcher.Invoke(() => OnError.Invoke(ex));
            return FrameCount;
        }
        finally
        {
            try
            {
                //Only release the frame if there was a sucess in capturing it.
                if (res.Success)
                    DuplicatedOutput.ReleaseFrame();
            }
            catch (Exception e)
            {
                LogWriter.Log(e, "It was not possible to release the frame.");
            }
        }
    }

    public override int CaptureWithCursor(RecordingFrame frame)
    {
        var res = new Result(-1);
        var wasCaptured = false;

        try
        {
            //Try to get the duplicated output frame within given time.
            res = DuplicatedOutput.TryAcquireNextFrame(0, out var info, out var resource);

            //Checks how to proceed with the capture. It could have failed, or the screen, cursor or both could have been captured.
            if ((res.Failure || resource == null) && info.TotalMetadataBufferSize == 0 && info.LastMouseUpdateTime == 0)
            {
                //Somehow, it was not possible to retrieve the resource, frame or metadata.
                resource?.Dispose();
                return FrameCount;
            }
            else if (FrameCount == 0 && info.TotalMetadataBufferSize == 0 && info.LastMouseUpdateTime > 0)
            {
                //Sometimes, the first frame has cursor info, but no screen changes.
                GetCursor(null, info, frame);
                resource?.Dispose();
                return FrameCount;
            }

            #region Process changes

            //Something on screen was moved or changed.
            if (info.TotalMetadataBufferSize > 0 && resource != null)
            {
                //Copies the screen data into memory that can be accessed by the CPU.
                using (var screenTexture = resource.QueryInterface<Texture2D>())
                {
                    #region Moved rectangles

                    var movedRectangles = new OutputDuplicateMoveRectangle[info.TotalMetadataBufferSize];
                    DuplicatedOutput.GetFrameMoveRects(movedRectangles.Length, movedRectangles, out var movedRegionsLength);

                    for (var movedIndex = 0; movedIndex < movedRegionsLength / Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle)); movedIndex++)
                    {
                        //Crop the destination rectangle to the scree area rectangle.
                        var left = Math.Max(movedRectangles[movedIndex].DestinationRect.Left, Left);
                        var right = Math.Min(movedRectangles[movedIndex].DestinationRect.Right, Left + Width);
                        var top = Math.Max(movedRectangles[movedIndex].DestinationRect.Top, Top);
                        var bottom = Math.Min(movedRectangles[movedIndex].DestinationRect.Bottom, Top + Height);

                        //Copies from the screen texture only the area which the user wants to capture.
                        if (right > left && bottom > top)
                        {
                            //Limit the source rectangle to the available size within the destination rectangle.
                            var sourceWidth = movedRectangles[movedIndex].SourcePoint.X + (right - left);
                            var sourceHeight = movedRectangles[movedIndex].SourcePoint.Y + (bottom - top);

                            Device.ImmediateContext.CopySubresourceRegion(screenTexture, 0, new ResourceRegion(movedRectangles[movedIndex].SourcePoint.X, movedRectangles[movedIndex].SourcePoint.Y, 0, sourceWidth, sourceHeight, 1),
                                StagingTexture, 0, left - Left, top - Top);

                            wasCaptured = true;
                        }
                    }

                    #endregion

                    #region Dirty rectangles

                    var dirtyRectangles = new RawRectangle[info.TotalMetadataBufferSize];
                    DuplicatedOutput.GetFrameDirtyRects(dirtyRectangles.Length, dirtyRectangles, out var dirtyRegionsLength);

                    for (var dirtyIndex = 0; dirtyIndex < dirtyRegionsLength / Marshal.SizeOf(typeof(RawRectangle)); dirtyIndex++)
                    {
                        //Crop screen positions and size to frame sizes.
                        var left = Math.Max(dirtyRectangles[dirtyIndex].Left, Left);
                        var right = Math.Min(dirtyRectangles[dirtyIndex].Right, Left + Width);
                        var top = Math.Max(dirtyRectangles[dirtyIndex].Top, Top);
                        var bottom = Math.Min(dirtyRectangles[dirtyIndex].Bottom, Top + Height);

                        //Copies from the screen texture only the area which the user wants to capture.
                        if (right > left && bottom > top)
                        {
                            Device.ImmediateContext.CopySubresourceRegion(screenTexture, 0, new ResourceRegion(left, top, 0, right, bottom, 1), StagingTexture, 0, left - Left, top - Top);
                            wasCaptured = true;
                        }
                    }

                    #endregion
                }
            }
            
            //Gets the cursor image and merges with the staging texture.
            if (!GetCursor(StagingTexture, info, frame) && !wasCaptured)
            {
                //Nothing was changed within the capture region, so ignore this frame.
                resource?.Dispose();
                return FrameCount;
            }

            //Saves the most recent capture time.
            LastProcessTime = Math.Max(info.LastPresentTime, info.LastMouseUpdateTime);

            #endregion

            #region Gets the image data

            //Gets the staging texture as a stream.
            var data = Device.ImmediateContext.MapSubresource(StagingTexture, 0, MapMode.Read, MapFlags.None, out var stream);

            if (data.IsEmpty)
            {
                Device.ImmediateContext.UnmapSubresource(StagingTexture, 0);
                stream?.Dispose();
                resource?.Dispose();
                return FrameCount;
            }

            //Sets the frame details.
            FrameCount++;

            frame.TimeStampInTicks = Stopwatch.GetElapsedTicks();
            //frame.Delay = Stopwatch.GetMilliseconds(); //Resets the stopwatch. Messes up the editor.
            frame.Pixels = new byte[stream.Length];

            //BGRA32 is 4 bytes.
            for (var height = 0; height < Height; height++)
            {
                stream.Position = height * data.RowPitch;
                Marshal.Copy(new IntPtr(stream.DataPointer.ToInt64() + height * data.RowPitch), frame.Pixels, height * Width * 4, Width * 4);
            }

            FrameCollection.Add(frame);

            #endregion

            Device.ImmediateContext.UnmapSubresource(StagingTexture, 0);
            stream.Dispose();
            resource?.Dispose();

            return FrameCount;
        }
        catch (SharpDXException se) when (se.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
        {
            return FrameCount;
        }
        catch (SharpDXException se) when (se.ResultCode.Code == SharpDX.DXGI.ResultCode.DeviceRemoved.Result.Code || se.ResultCode.Code == SharpDX.DXGI.ResultCode.DeviceReset.Result.Code)
        {
            //When the device gets lost or reset, the resources should be instantiated again.
            ExtraDisposeInternal();
            Initialize();

            return FrameCount;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "It was not possible to finish capturing the frame with DirectX.");

            MajorCrashHappened = true;
            Application.Current.Dispatcher.Invoke(() => OnError.Invoke(ex));
            return FrameCount;
        }
        finally
        {
            try
            {
                //Only release the frame if there was a sucess in capturing it.
                if (res.Success)
                    DuplicatedOutput.ReleaseFrame();
            }
            catch (Exception e)
            {
                LogWriter.Log(e, "It was not possible to release the frame.");
            }
        }
    }
}