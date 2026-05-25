using Godot;
using System;

public partial class MusicManager : AudioStreamPlayer
{
    public override void _Ready()
    {
        Stream = GD.Load<AudioStream>("res://zene.mp3"); 
        Autoplay = true;
    }

    public void ToggleMusic()
    {
        if (Playing)
            Stop();
        else
            Play();
    }
}