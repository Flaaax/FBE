using MegaCrit.Sts2.Core.Saves;

namespace FBE.Scripts.Utils;

using System;
using System.Collections.Generic;
using Godot;

public static class AudioHelper
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, AudioStream> LoadedStreams = new();
    private static readonly HashSet<string> FailedPaths = [];
    private static float Volume => SaveManager.Instance.SettingsSave.VolumeMaster;
    private static float SfxVolume => SaveManager.Instance.SettingsSave.VolumeSfx * Volume;

    private static AudioHelperHost? _host;

    public static bool Play(string resPath, float volume = 1f)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resPath))
                return false;

            resPath = NormalizeResPath(resPath);

            AudioStream? stream = GetOrLoadStream(resPath);
            if (stream == null)
                return false;

            AudioHelperHost? host = EnsureHost();
            if (host == null)
            {
                GD.PushWarning("[AudioHelper] Audio host is unavailable.");
                return false;
            }

            var clampedVolume = Math.Clamp(volume, 0f, 4f);
            var finalVolume = clampedVolume * Math.Clamp(SfxVolume, 0f, 1f);

            host.PlayDeferred(stream, finalVolume);
            return true;
        }
        catch (Exception ex)
        {
            GD.PushError($"[AudioHelper] Play failed: {resPath}\n{ex}");
            return false;
        }
    }

    public static bool PlayRandom(string resPath, float volume = 1f)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resPath))
                return false;

            resPath = NormalizeResPath(resPath);

            var candidates = GetRandomAudioCandidates(resPath);
            if (candidates.Count == 0)
            {
                GD.PushWarning($"[AudioHelper] No random audio variants found for: {resPath}");
                return false;
            }

            var selectedPath = candidates[Random.Shared.Next(candidates.Count)];
            return Play(selectedPath, volume);
        }
        catch (Exception ex)
        {
            GD.PushError($"[AudioHelper] PlayRandom failed: {resPath}\n{ex}");
            return false;
        }
    }

    public static bool PlayLoop(string resPath, float volume = 1f, bool restartIfAlreadyPlaying = false)
    {
        try
        {
            var key = resPath;
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(resPath))
                return false;

            key = NormalizeKey(key);
            resPath = NormalizeResPath(resPath);

            AudioStream? stream = GetOrLoadStream(resPath);
            if (stream == null)
                return false;

            AudioStream loopStream = CreateLoopStream(stream);

            AudioHelperHost? host = EnsureHost();
            if (host == null)
            {
                GD.PushWarning("[AudioHelper] Audio host is unavailable.");
                return false;
            }

            var clampedVolume = Math.Clamp(volume, 0f, 4f);
            var finalVolume = clampedVolume * Math.Clamp(SfxVolume, 0f, 1f);

            host.PlayLoopDeferred(key, loopStream, finalVolume, restartIfAlreadyPlaying);
            return true;
        }
        catch (Exception ex)
        {
            GD.PushError($"[AudioHelper] PlayLoop failed: {resPath}\n{ex}");
            return false;
        }
    }

    public static bool StopLoop(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            key = NormalizeKey(key);

            AudioHelperHost? host = EnsureHost();
            if (host == null)
            {
                GD.PushWarning("[AudioHelper] Audio host is unavailable.");
                return false;
            }

            host.StopLoopDeferred(key);
            return true;
        }
        catch (Exception ex)
        {
            GD.PushError($"[AudioHelper] StopLoop failed: {key}\n{ex}");
            return false;
        }
    }

    public static bool IsLoopPlaying(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            key = NormalizeKey(key);

            AudioHelperHost? host = EnsureHost();
            return host != null && host.IsLoopPlaying(key);
        }
        catch (Exception ex)
        {
            GD.PushError($"[AudioHelper] IsLoopPlaying failed: {key}\n{ex}");
            return false;
        }
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim();
    }

    private static string NormalizeResPath(string resPath)
    {
        string normalized = resPath.Replace('\\', '/').Trim();

        if (!normalized.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"AudioHelper only accepts full res:// paths. Got: {resPath}",
                nameof(resPath));

        return normalized;
    }

    private static List<string> GetRandomAudioCandidates(string resPath)
    {
        var slashIndex = resPath.LastIndexOf('/');
        var extensionIndex = resPath.LastIndexOf('.');
        if (extensionIndex <= slashIndex)
            extensionIndex = resPath.Length;

        var digitIndex = extensionIndex - 1;
        if (digitIndex <= slashIndex || !char.IsDigit(resPath[digitIndex]))
            return [];

        var prefix = resPath[..digitIndex];
        var suffix = resPath[extensionIndex..];
        List<string> candidates = [];

        for (var i = 0; i <= 9; i++)
        {
            var candidate = $"{prefix}{i}{suffix}";
            if (ResourceLoader.Exists(candidate))
                candidates.Add(candidate);
        }

        return candidates;
    }

    private static void EnableLoop(AudioStream stream)
    {
        switch (stream)
        {
            case AudioStreamOggVorbis ogg:
                ogg.Loop = true;
                break;
            case AudioStreamMP3 mp3:
                mp3.Loop = true;
                break;
            case AudioStreamWav wav:
                wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
                break;
        }
    }

    private static AudioStream CreateLoopStream(AudioStream stream)
    {
        AudioStream loopStream = stream.Duplicate() as AudioStream ?? stream;
        EnableLoop(loopStream);
        return loopStream;
    }

    private static AudioStream? GetOrLoadStream(string resPath)
    {
        lock (Sync)
        {
            if (FailedPaths.Contains(resPath))
                return null;

            if (LoadedStreams.TryGetValue(resPath, out AudioStream? cached))
            {
                if (GodotObject.IsInstanceValid(cached))
                    return cached;

                LoadedStreams.Remove(resPath);
            }

            if (!ResourceLoader.Exists(resPath))
            {
                FailedPaths.Add(resPath);
                GD.PushWarning($"[AudioHelper] Audio resource not found: {resPath}");
                return null;
            }

            AudioStream? stream = ResourceLoader.Load<AudioStream>(resPath);
            if (stream == null)
            {
                FailedPaths.Add(resPath);
                GD.PushWarning($"[AudioHelper] Failed to load audio stream: {resPath}");
                return null;
            }

            LoadedStreams[resPath] = stream;
            return stream;
        }
    }

    private static AudioHelperHost? EnsureHost()
    {
        lock (Sync)
        {
            if (_host != null && GodotObject.IsInstanceValid(_host))
                return _host;

            if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
                return null;

            AudioHelperHost? existing = tree.Root.GetNodeOrNull<AudioHelperHost>("FBE_AudioHelperHost");
            if (existing != null && GodotObject.IsInstanceValid(existing))
            {
                _host = existing;
                return _host;
            }

            _host = new AudioHelperHost
            {
                Name = "FBE_AudioHelperHost"
            };

            tree.Root.AddChild(_host);
            return _host;
        }
    }
}

internal partial class AudioHelperHost : Node
{
    private static readonly StringName SfxBus = new("SFX");
    private readonly Dictionary<string, AudioStreamPlayer> _loopPlayers = new();

    public void PlayDeferred(AudioStream stream, float volume)
    {
        CallDeferred(nameof(PlayInternal), stream, volume);
    }

    public void PlayLoopDeferred(string key, AudioStream stream, float volume, bool restartIfAlreadyPlaying)
    {
        CallDeferred(nameof(PlayLoopInternal), key, stream, volume, restartIfAlreadyPlaying);
    }

    public void StopLoopDeferred(string key)
    {
        CallDeferred(nameof(StopLoopInternal), key);
    }

    public bool IsLoopPlaying(string key)
    {
        return _loopPlayers.TryGetValue(key, out AudioStreamPlayer? player) &&
               GodotObject.IsInstanceValid(player) &&
               player.Playing;
    }

    public void PlayInternal(AudioStream stream, float volume)
    {
        try
        {
            if (!GodotObject.IsInstanceValid(stream))
                return;

            AudioStreamPlayer player = new()
            {
                Stream = stream,
                VolumeDb = volume <= 0f ? -80f : Mathf.LinearToDb(volume),
                Bus = SfxBus
            };

            AddChild(player);

            player.Finished += () =>
            {
                if (GodotObject.IsInstanceValid(player))
                    player.QueueFree();
            };

            player.Play();

            if (!player.Playing)
                player.QueueFree();
        }
        catch (Exception ex)
        {
            GD.PushError($"[AudioHelper] PlayInternal failed.\n{ex}");
        }
    }

    public void PlayLoopInternal(string key, AudioStream stream, float volume, bool restartIfAlreadyPlaying)
    {
        try
        {
            if (!IsInstanceValid(stream))
                return;

            if (_loopPlayers.TryGetValue(key, out var existing))
            {
                if (IsInstanceValid(existing))
                {
                    if (existing.Playing && !restartIfAlreadyPlaying)
                        return;

                    existing.Stream = stream;
                    existing.VolumeDb = volume <= 0f ? -80f : Mathf.LinearToDb(volume);

                    existing.Play();
                    return;
                }

                _loopPlayers.Remove(key);
            }

            AudioStreamPlayer player = new()
            {
                Stream = stream,
                VolumeDb = volume <= 0f ? -80f : Mathf.LinearToDb(volume),
                Bus = SfxBus
            };

            _loopPlayers[key] = player;
            AddChild(player);
            player.Play();

            if (player.Playing) return;
            _loopPlayers.Remove(key);
            player.QueueFree();
        }
        catch (Exception ex)
        {
            GD.PushError($"[AudioHelper] PlayLoopInternal failed: {key}\n{ex}");
        }
    }

    public void StopLoopInternal(string key)
    {
        try
        {
            if (!_loopPlayers.Remove(key, out AudioStreamPlayer? player))
                return;

            if (!IsInstanceValid(player))
                return;

            player.Stop();
            player.QueueFree();
        }
        catch (Exception ex)
        {
            GD.PushError($"[AudioHelper] StopLoopInternal failed: {key}\n{ex}");
        }
    }
}