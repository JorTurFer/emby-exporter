using System;
using System.Collections.Generic;

namespace emby_exporter.Entities
{
    public class SessionsResponse
    {
        public List<object> AdditionalUsers { get; set; }
        public string RemoteEndPoint { get; set; }
        public List<string> PlayableMediaTypes { get; set; }
        public int PlaylistIndex { get; set; }
        public int PlaylistLength { get; set; }
        public string Id { get; set; }
        public string ServerId { get; set; }
        public string Client { get; set; }
        public DateTime LastActivityDate { get; set; }
        public string DeviceName { get; set; }
        public string DeviceId { get; set; }
        public string ApplicationVersion { get; set; }
        public List<string> SupportedCommands { get; set; }
        public bool SupportsRemoteControl { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string AppIconUrl { get; set; }
        public string UserPrimaryImageTag { get; set; }
        public NowPlayingItem NowPlayingItem { get; set; }
        public TranscodingInfo TranscodingInfo { get; set; }

        public TranscodeType TranscodeType 
        { 
            get 
            {
                return TranscodingInfo switch
                {                    
                    { IsVideoDirect: true, IsAudioDirect: false } => TranscodeType.Audio,
                    { IsVideoDirect: false, IsAudioDirect: true } => TranscodeType.Video,
                    { IsVideoDirect: false, IsAudioDirect: false } => TranscodeType.Full,
                    _ => TranscodeType.Direct
                };
            } 
        }

    }

    public class NowPlayingItem
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string Id { get; set; }
    }

    public class TranscodingInfo
    {
        public bool IsVideoDirect { get; set; }
        public bool IsAudioDirect { get; set; }        
    }

    public enum TranscodeType
    {
        Direct,
        Audio,
        Video,
        Full
    }
}
