﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Griffin.WebServer;
using Griffin.WebServer.Files;
using Griffin.WebServer.Modules;

namespace CameraControl.Core.Classes
{
    public class WebServerModule : IWorkerModule
    {
/*        private string _lineFormat =
            "{image : '@image@', title : 'Image Credit: Maria Kazvan', thumb : '@image_thumb@', url : '@image_url@'},";*/
        private string _lineFormat =
            "            <div>  <img u=\"image\" src=\"@image@\" /><img u=\"thumb\" src=\"@image_thumb@\" /></div>";

        

        #region Implementation of IHttpModule

        public void BeginRequest(IHttpContext context)
        {
            
        }

        public void EndRequest(IHttpContext context)
        {
            
        }

        public void HandleRequestAsync(IHttpContext context, Action<IAsyncModuleResult> callback)
        {
            callback(new AsyncModuleResult(context, HandleRequest(context)));
        }

        #endregion

        public ModuleResult HandleRequest(IHttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request.Uri.AbsolutePath) || context.Request.Uri.AbsolutePath == "/")
            {

                string str = context.Request.Uri.Scheme + "://" + context.Request.Uri.Host;
                if (context.Request.Uri.Port != 80)
                    str = str + (object)":" + context.Request.Uri.Port.ToString();
                string uriString = str + context.Request.Uri.AbsolutePath + "index.html";
                if (!string.IsNullOrEmpty(context.Request.Uri.Query))
                    uriString = uriString + "?" + context.Request.Uri.Query;
                context.Request.Uri = new Uri(uriString);
            }

            if (context.Request.Uri.AbsolutePath.StartsWith("/thumb/large"))
            {
                foreach (FileItem item in ServiceProvider.Settings.DefaultSession.Files)
                {
                    if (Path.GetFileName(item.LargeThumb) == Path.GetFileName(context.Request.Uri.AbsolutePath.Replace("/", "\\")))
                    {
                        //if (!File.Exists(item.LargeThumb) && !item.IsRaw)
                        //{
                        //    BitmapLoader.Instance.GenerateCache(item);
                        //}
                        SendFile(context,
                                 !File.Exists(item.LargeThumb)
                                     ? Path.Combine(Settings.WebServerFolder, "logo.png")
                                     : item.LargeThumb);
                        SendFile(context, item.LargeThumb);
                        return ModuleResult.Continue;
                    }
                }
            }

            if (context.Request.Uri.AbsolutePath.StartsWith("/thumb/small"))
            {
                foreach (FileItem item in ServiceProvider.Settings.DefaultSession.Files)
                {
                    if (Path.GetFileName(item.SmallThumb) == Path.GetFileName(context.Request.Uri.AbsolutePath.Replace("/", "\\")))
                    {
                        SendFile(context,
                                 !File.Exists(item.SmallThumb)
                                     ? Path.Combine(Settings.WebServerFolder, "logo.png")
                                     : item.SmallThumb);
                        return ModuleResult.Continue;
                    }
                }
            }

            if (context.Request.Uri.AbsolutePath.StartsWith("/preview.jpg"))
            {
                SendFile(context, ServiceProvider.Settings.SelectedBitmap.FileItem.LargeThumb);
            }
            if (context.Request.Uri.AbsolutePath.StartsWith("/image/"))
            {
                foreach (FileItem item in ServiceProvider.Settings.DefaultSession.Files)
                {
                    if (Path.GetFileName(item.FileName) == Path.GetFileName(context.Request.Uri.AbsolutePath.Replace("/", "\\")))
                    {
                        SendFile(context, item.FileName);
                        return ModuleResult.Continue;
                    }
                }
            }
            string fullpath = GetFullPath(context.Request.Uri);
            if(!string.IsNullOrEmpty(fullpath) && File.Exists(fullpath))
            {
                if (Path.GetFileName(fullpath) == "slide.html")
                {

                    string file = File.ReadAllText(fullpath);

                    StringBuilder builder = new StringBuilder();
                    foreach (FileItem item in ServiceProvider.Settings.DefaultSession.Files)
                    {
                        string tempStr = _lineFormat.Replace("@image@",
                                                             "/thumb/large/" + Path.GetFileName(item.LargeThumb));
                        tempStr = tempStr.Replace("@image_thumb@", "/thumb/small/" + Path.GetFileName(item.SmallThumb));
                        tempStr = tempStr.Replace("@image_url@", "/image/" + Path.GetFileName(item.FileName));
                        tempStr = tempStr.Replace("@title@", item.Name);
                        tempStr = tempStr.Replace("@desc@", item.FileInfo!=null?( item.FileInfo.InfoLabel ?? ""):"");
                        builder.AppendLine(tempStr);
                    }

                    file = file.Replace("@@image_list@@", builder.ToString());

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(file);

                    //response.ContentLength64 = buffer.Length;
                    context.Response.AddHeader("Content-Length", buffer.Length.ToString());

                    context.Response.Body = new MemoryStream();

                    context.Response.Body.Write(buffer, 0, buffer.Length);
                    context.Response.Body.Position = 0;
                }
                else
                {
                    SendFile(context, fullpath);
                }
            }
            string cmd = context.Request.QueryString["CMD"];
            if(!string.IsNullOrEmpty(cmd))
                ServiceProvider.WindowsManager.ExecuteCommand(cmd);
            
            return ModuleResult.Continue;
        }

        private void SendFile(IHttpContext context, string fullpath)
        {
            if(!File.Exists(fullpath))
                return;
            
            string str = MimeTypeProvider.Instance.Get(fullpath);
            FileStream fileStream = new FileStream(fullpath, FileMode.Open, FileAccess.Read,
                                                   FileShare.Read | FileShare.Write);
            context.Response.AddHeader("Content-Disposition",
                                       "inline;filename=\"" + Path.GetFileName(fullpath) + "\"");
            context.Response.ContentType = str;
            context.Response.ContentLength = (int)fileStream.Length;
            context.Response.Body = fileStream;
            
        }

        private string GetFullPath(Uri uri)
        {
            return Path.Combine(Settings.WebServerFolder,
                                Uri.UnescapeDataString(uri.AbsolutePath.Remove(0, 1)).TrimStart(new char
                                                                                                                       [
                                                                                                                       1
                                                                                                                       ]
                                                                                                                       {
                                                                                                                           '/'
                                                                                                                       })
                                    .Replace('/', '\\'));
        }
    }
}
