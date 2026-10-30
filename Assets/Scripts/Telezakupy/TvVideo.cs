using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace UnityEngine.Rendering.Universal {
/// <summary>
/// Plays TV videos from StreamingAssets/Video by file name.
/// Web builds cannot use VideoClip assets (no VideoClipImporter support) and only support
/// Direct audio output, so everything goes through URL playback. On desktop/editor the same
/// path works via file URLs, and audio still routes through the AudioSource for 3D attenuation.
/// </summary>
public static class TvVideo {
    public const string Folder = "Video";

#if UNITY_WEBGL && !UNITY_EDITOR
    public const bool UsesDirectAudio = true;
#else
    public const bool UsesDirectAudio = false;
#endif

    public static string GetUrl(string fileName) {
        if (string.IsNullOrEmpty(fileName))
            return null;
        // StreamingAssets on Web is already an http(s) URL; on desktop it is a plain path.
        return Application.streamingAssetsPath + "/" + Folder + "/" + fileName;
    }

    public static void Play(VideoPlayer player, string fileName) {
        if (player == null)
            return;

        var url = GetUrl(fileName);
        if (url == null) {
            player.Stop();
            return;
        }

        player.source = VideoSource.Url;
        if (player.url == url && player.isPlaying)
            return;

        player.audioOutputMode = UsesDirectAudio ? VideoAudioOutputMode.Direct : VideoAudioOutputMode.AudioSource;
        player.url = url;
        player.Play();
    }

    /// <summary>
    /// Approximates an AudioSource with linear rolloff for Direct audio output,
    /// which is the only mode available on Web.
    /// </summary>
    public static void ApplyDirectVolume(VideoPlayer player, Vector3 sourcePosition, float volume,
                                         float minDistance, float maxDistance) {
        if (player == null || !UsesDirectAudio)
            return;

        var listener = Object.FindAnyObjectByType<AudioListener>();
        float attenuation = 1f;
        if (listener != null) {
            float d = Vector3.Distance(listener.transform.position, sourcePosition);
            attenuation = maxDistance <= minDistance ? 1f : 1f - Mathf.Clamp01((d - minDistance) / (maxDistance - minDistance));
        }

        ushort tracks = player.audioTrackCount;
        for (ushort i = 0; i < tracks; i++)
            player.SetDirectAudioVolume(i, volume * attenuation);
    }
}
}
