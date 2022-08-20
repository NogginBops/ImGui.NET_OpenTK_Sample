using System.IO;
using System.Xml.Serialization;
using NAudio.Wave;

namespace Engine;

public class AudioSource : Component
{
	[Show] public AudioClip clip;
	[SliderF(0, 1)] public float volume;

	[XmlIgnore] public Action PlaySoundAction;
	[XmlIgnore] public Action StopSoundAction;

	private WaveOutEvent outputDevice;

	private AudioFileReader audioFile;

	public override void Awake()
	{
		PlaySoundAction = () => { PlaySound(); };
		StopSoundAction = () => { StopSound(); };
		base.Awake();
	}

	private void ValidateDevice()
	{
		if (outputDevice == null)
		{
			outputDevice = new WaveOutEvent();
		}

		if (audioFile == null)
		{
			audioFile = new AudioFileReader(Path.Combine(Folders.Assets, clip.path));
			outputDevice.Init(audioFile);
			outputDevice.DeviceNumber = Scene.I.gameObjects.Count;
		}
	}

	public void PlaySound()
	{
		ValidateDevice();

		outputDevice.Play();
	}

	public void PauseSound()
	{
		ValidateDevice();

		outputDevice.Stop();
	}

	public void StopSound()
	{
		outputDevice.Stop();
		audioFile = null;

		ValidateDevice();

		outputDevice.Stop();
	}

	public override void Update()
	{
		if (outputDevice != null && outputDevice.Volume != volume)
		{
			outputDevice.Volume = volume;
		}

		base.Update();
	}
}